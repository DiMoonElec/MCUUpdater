using PolyBootCore.PolyBootProtocol;
using PolyBootCore.PolyBootProtocol.Bootloader;
using PolyBootCore.Transport;
using PolyBootCore.UpdateFile;
using System;
using System.Threading;

namespace PolyBootCore
{
  public delegate void BootloaderEventDelegate();
  public delegate void BootloaderProgressDelegate(int percent);

  public enum BootloaderWorkflowResult
  {
    OK = 0,
    ConnectionError,
    ConnectionLost,
    ErasingError,
    IncompatibleDeviceError,
    UpdateError,
    Cancel,
  }

  public class BootloaderWorkflow
  {
    IBootloaderTransport Transport;

    public event BootloaderEventDelegate EraseBegin;
    public event BootloaderEventDelegate EraseEnd;

    public event BootloaderEventDelegate UserDataEraseBegin;
    public event BootloaderEventDelegate UserDataEraseEnd;

    public event BootloaderEventDelegate UploadBegin;
    public event BootloaderEventDelegate UploadEnd;

    public event BootloaderProgressDelegate EraseProgress;
    public event BootloaderProgressDelegate UserDataEraseProgress;
    public event BootloaderProgressDelegate UploadProgress;


    private readonly CancellationToken cancellationToken;
    private readonly bool hasCancellation = false;

    private int connectionTimeout = 60;

    public BootloaderWorkflow(ConnectionConfig config)
    {
      connectionTimeout = config.DeviceWaitTimeout;
      var transport = config.CreateTransport();
      Transport = transport;
    }

    public BootloaderWorkflow(ConnectionConfig config, CancellationToken token)
    {
      connectionTimeout = config.DeviceWaitTimeout;

      var transport = config.CreateTransport();
      Transport = transport;
      transport.SetCancellationToken(token);
      hasCancellation = true;
      cancellationToken = token;
    }

    public static string GetDescription(BootloaderWorkflowResult result)
    {
      switch (result)
      {
        case BootloaderWorkflowResult.OK:
          return "Operation completed successfully";
        case BootloaderWorkflowResult.ConnectionError:
          return "Failed to connect to device";
        case BootloaderWorkflowResult.ConnectionLost:
          return "Connection to device lost";
        case BootloaderWorkflowResult.ErasingError:
          return "Error while erasing device memory";
        case BootloaderWorkflowResult.IncompatibleDeviceError:
          return "Device is incompatible";
        case BootloaderWorkflowResult.UpdateError:
          return "Firmware update failed";
        case BootloaderWorkflowResult.Cancel:
          return "Operation was canceled";
        default:
          return "Unknown result";
      }
    }

    private void bootloaderUserDataErasureProgress(int numBlocks, int currentBlock)
    {
      if (UserDataEraseProgress != null)
      {
        int progress = (currentBlock * 100) / numBlocks;
        UserDataEraseProgress(progress);
      }
    }

    private void bootloaderMemoryErasureProgress(int numBlocks, int currentBlock)
    {
      if (EraseProgress != null)
      {
        int progress = (currentBlock * 100) / numBlocks;
        EraseProgress(progress);
      }
    }

    /// <summary>
    /// Запустить процесс обновления прошивки
    /// </summary>
    /// <param name="updateFile">Файл обновления</param>
    /// <param name="waitTimeoutSec">Тайм-аут на подключение к обновляемому устройству</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public BootloaderWorkflowResult Update(FirmwareUpdateFileV2 updateFile)
    {
      try
      {
        if (updateFile.FormatVersion == 0)
        {
          /*
           * Версия формата файла обновления 0.
           * Для этих файлов всегда используется протокол обновления версии 0
           */
          var bootloader = new BootloaderProtocolV0(Transport);

          bootloader.BootloaderMemoryErasureProgress += bootloaderMemoryErasureProgress;
          var result = UpdateProtocolVersion0(bootloader, updateFile);
          bootloader.BootloaderMemoryErasureProgress -= bootloaderMemoryErasureProgress;

          return result;
        }
        else if (updateFile.FormatVersion == 1)
        {
          /*
           * Версия формата файла обновления 1.
           * Для этих файлов всегда используется протокол обмена версии 1
           */
          var bootloader = new BootloaderProtocolV1(Transport);

          bootloader.BootloaderMemoryErasureProgress += bootloaderMemoryErasureProgress;
          var result = UpdateProtocolVersion1(bootloader, updateFile);
          bootloader.BootloaderMemoryErasureProgress -= bootloaderMemoryErasureProgress;

          return result;
        }
        else
          throw new Exception($"Protocol version {updateFile.FormatVersion} is not supported.");
      }
      catch (OperationCanceledException)
      {
        return BootloaderWorkflowResult.Cancel;
      }
      finally
      {
        Transport.Disconnect();
      }
    }

    private BootloaderWorkflowResult UpdateProtocolVersion1(IBootloaderProtocolV1 bootloader,
      FirmwareUpdateFileV2 updateFile)
    {
      int connectionIterations = connectionTimeout * 2;
      int i;
      for (i = 0; i < connectionIterations; i++)
      {
        if (hasCancellation)
          cancellationToken.ThrowIfCancellationRequested();

        if (InitializeConnection(bootloader))
          break;

        System.Threading.Thread.Sleep(500);
      }

      if (i == connectionIterations)
        return BootloaderWorkflowResult.ConnectionError;

      PolyBootActionResult result;

      /**** Очистка flash-памяти ****/

      EraseBegin?.Invoke();

      result = ExecuteWithReconnectRetry(() => bootloader.BootloaderBegin(updateFile.RootFirmware.HeaderChunkBase64.Trim()));

      if (result == PolyBootActionResult.IncompatibleDeviceError)
        return BootloaderWorkflowResult.IncompatibleDeviceError;
      else if (result == PolyBootActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;
      if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.ErasingError;

      EraseEnd?.Invoke();

      /**** Отправка обновления ****/

      UploadBegin?.Invoke();

      var dataChunks = updateFile.RootFirmware.DataChunksBase64;

      for (int b = 0; b < dataChunks.Count; b++)
      {
        string u = dataChunks[b].Trim();

        if (u != "")
        {
          result = ExecuteWithReconnectRetry(() => bootloader.BootloaderSend(u));
          if (result == PolyBootActionResult.ConnectionLost)
            return BootloaderWorkflowResult.ConnectionLost;
          else if (result != PolyBootActionResult.OK)
            return BootloaderWorkflowResult.UpdateError;


          result = ExecuteWithReconnectRetry(() => bootloader.BootloaderWrite());

          if (result == PolyBootActionResult.ConnectionLost)
            return BootloaderWorkflowResult.ConnectionLost;
          else if (result != PolyBootActionResult.OK)
            return BootloaderWorkflowResult.UpdateError;
        }

        UploadProgress?.Invoke((b * 100) / dataChunks.Count);
      }

      UploadEnd?.Invoke();

      /**** Завершаем процесс обновления ****/

      result = ExecuteWithReconnectRetry(() => bootloader.BootloaderEnd());

      if (result == PolyBootActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;
      else if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.ConnectionError;

      /**** Проверяем CRC прошивки ****/

      bool crcOK = false;

      result = ExecuteWithReconnectRetry(() => bootloader.BootloaderCheckApplicationCRC(out crcOK));

      if (result == PolyBootActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;
      else if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.ConnectionError;
      else if (crcOK == false)
        return BootloaderWorkflowResult.UpdateError;

      /**** Запускаем прошивку ****/

      result = bootloader.BootloaderApplicationRun();
      if (result == PolyBootActionResult.OK)
        return BootloaderWorkflowResult.OK;
      else
        return BootloaderWorkflowResult.ConnectionLost;
    }

    private BootloaderWorkflowResult UpdateProtocolVersion0(IBootloaderProtocolV0 bootloader,
      FirmwareUpdateFileV2 updateFile)
    {
      int connectionIterations = connectionTimeout * 2;
      int i;
      for (i = 0; i < connectionIterations; i++)
      {
        if (hasCancellation)
          cancellationToken.ThrowIfCancellationRequested();

        if (InitializeConnection(bootloader))
          break;

        System.Threading.Thread.Sleep(500);
      }

      if (i == connectionIterations)
        return BootloaderWorkflowResult.ConnectionError;

      PolyBootActionResult result;

      //Очистка flash-памяти
      if (EraseBegin != null)
        EraseBegin();

      result = bootloader.BootloaderBegin();

      if (EraseEnd != null)
        EraseEnd();

      if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.ErasingError;

      //Отправка обновления
      if (UploadBegin != null)
        UploadBegin();

      var dataChunks = updateFile.RootFirmware.DataChunksBase64;

      for (int b = 0; b < dataChunks.Count; b++)
      {
        string u = dataChunks[b].Trim();

        if (u != "")
        {
          result = bootloader.BootloaderSend(u);
          if (result != PolyBootActionResult.OK)
            return BootloaderWorkflowResult.UpdateError;

          result = bootloader.BootloaderWrite();
          if (result != PolyBootActionResult.OK)
            return BootloaderWorkflowResult.UpdateError;
        }

        if (UploadProgress != null)
        {
          int progress = (b * 100) / dataChunks.Count;
          UploadProgress(progress);
        }
      }

      if (UploadEnd != null)
        UploadEnd();

      //Завершаем процесс обновления
      result = bootloader.BootloaderEnd();
      if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.ConnectionError;

      //Проверяем CRC прошивки
      bool crcOK;
      result = bootloader.BootloaderCheckApplicationCRC(out crcOK);
      if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.ConnectionError;
      if (crcOK == false)
        return BootloaderWorkflowResult.UpdateError;

      //Запускаем прошивку
      result = bootloader.BootloaderApplicationRun();
      if (result == PolyBootActionResult.OK)
        return BootloaderWorkflowResult.OK;
      else
        return BootloaderWorkflowResult.UpdateError;
    }

#if false
    public BootloaderWorkflowResult EraseUserData()
    {
      if (UserDataEraseBegin != null)
        UserDataEraseBegin();

      var result = Bootloader.BootloaderEraseUserData();

      if (UserDataEraseEnd != null)
        UserDataEraseEnd();

      if (result == BootloaderProtocolActionResult.OK)
        return BootloaderWorkflowResult.OK;

      return BootloaderWorkflowResult.ErasingError;
    }
#endif

    private bool InitializeConnection(IBootloaderBase bootloader)
    {
      if (Transport.Connect() == false)
        return false;

      if (bootloader.BootloaderActivate() != PolyBootActionResult.OK)
      {
        Transport.Disconnect();
        return false;
      }

      return true;
    }

    // Обобщённый ретрай с переподключением
    PolyBootActionResult ExecuteWithReconnectRetry(
        Func<PolyBootActionResult> operation,
        int maxAttempts = 10,
        int delayMsOnReconnectFail = 1000)
    {
      if (operation == null) throw new ArgumentNullException(nameof(operation));
      if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
      if (delayMsOnReconnectFail < 0) throw new ArgumentOutOfRangeException(nameof(delayMsOnReconnectFail));

      // Первая попытка отправки запроса
      PolyBootActionResult result = operation();

      // Если результат не требует повторения — возвращаем его
      if (result != PolyBootActionResult.ConnectionLost)
        return result;

      // В результате первой попытки вызова operation()
      // получили ConnectionLost, поэтому запускаем
      // повтор попыток вызова operation() с предварительным
      // Reconnect()
      for (int attempt = 1; attempt < maxAttempts; attempt++)
      {
        if (Reconnect())
        {
          // В случае успешного переподключения
          // повторяем отправку команды
          result = operation();

          // Если результат не требует повторения — возвращаем его
          if (result != PolyBootActionResult.ConnectionLost)
            return result;
        }
        else
        {
          // Если не удалось переподключиться, то не отправляем команду,
          // однако, это все равно защитывается как попытка.
          // Пауза перед следующим переподключением
          System.Threading.Thread.Sleep(delayMsOnReconnectFail);
        }
      }

      // Если попали сюда, то все попытки повторной отправки команды
      // были исчерпаны, возвращаем ошибку потери связи
      Transport.Disconnect();
      return PolyBootActionResult.ConnectionLost;
    }


    private bool Reconnect()
    {
      Transport.Disconnect();
      return Transport.Connect();
    }
  }
}
