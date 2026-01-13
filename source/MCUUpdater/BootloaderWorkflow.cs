using System;
using MCUUpdater.Bootloader;

namespace MCUUpdater
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
  }

  class BootloaderWorkflow
  {
    IBootloaderProtocol Bootloader;

    public event BootloaderEventDelegate EraseBegin;
    public event BootloaderEventDelegate EraseEnd;

    public event BootloaderEventDelegate UserDataEraseBegin;
    public event BootloaderEventDelegate UserDataEraseEnd;

    public event BootloaderEventDelegate UploadBegin;
    public event BootloaderEventDelegate UploadEnd;

    public event BootloaderProgressDelegate EraseProgress;
    public event BootloaderProgressDelegate UserDataEraseProgress;
    public event BootloaderProgressDelegate UploadProgress;


    public BootloaderWorkflow(IBootloaderProtocol bootloader)
    {
      Bootloader = bootloader;
      Bootloader.BootloaderMemoryErasureProgress += bootloaderMemoryErasureProgress;
      Bootloader.BootloaderUserDataErasureProgress += bootloaderUserDataErasureProgress;
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
    public BootloaderWorkflowResult Update(FirmwareUpdateFile updateFile, int waitTimeoutSec)
    {
      if (updateFile.ProtocolVersion == 0)
        return UpdateProtocolVersion0(updateFile, waitTimeoutSec);
      else if (updateFile.ProtocolVersion == 1)
        return UpdateProtocolVersion1(updateFile, waitTimeoutSec);
      else
        throw new Exception($"Protocol version {updateFile.ProtocolVersion} is not supported.");
    }

    private BootloaderWorkflowResult UpdateProtocolVersion1(FirmwareUpdateFile updateFile, int connectionTimeout)
    {
      int connectionIterations = connectionTimeout * 2;
      int i;
      for (i = 0; i < connectionIterations; i++)
      {
        if (InitializeConnection())
          break;

        System.Threading.Thread.Sleep(500);
      }

      if (i == connectionIterations)
        return BootloaderWorkflowResult.ConnectionError;

      BootloaderProtocolActionResult result;

      /**** Очистка flash-памяти ****/

      EraseBegin?.Invoke();

      result = ExecuteWithReconnectRetry(() => Bootloader.BootloaderBegin_V1(updateFile.HeaderChunkBase64.Trim()));

      if (result == BootloaderProtocolActionResult.IncompatibleDeviceError)
        return BootloaderWorkflowResult.IncompatibleDeviceError;
      else if (result == BootloaderProtocolActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;
      if (result != BootloaderProtocolActionResult.OK)
        return BootloaderWorkflowResult.ErasingError;

      EraseEnd?.Invoke();

      /**** Отправка обновления ****/

      UploadBegin?.Invoke();

      var dataChunks = updateFile.DataChunksBase64;

      for (int b = 0; b < dataChunks.Count; b++)
      {
        string u = dataChunks[b].Trim();

        if (u != "")
        {
          result = ExecuteWithReconnectRetry(() => Bootloader.BootloaderSend(u));
          if (result == BootloaderProtocolActionResult.ConnectionLost)
            return BootloaderWorkflowResult.ConnectionLost;
          else if (result != BootloaderProtocolActionResult.OK)
            return BootloaderWorkflowResult.UpdateError;


          result = ExecuteWithReconnectRetry(() => Bootloader.BootloaderWrite());

          if (result == BootloaderProtocolActionResult.ConnectionLost)
            return BootloaderWorkflowResult.ConnectionLost;
          else if (result != BootloaderProtocolActionResult.OK)
            return BootloaderWorkflowResult.UpdateError;
        }

        UploadProgress?.Invoke((b * 100) / dataChunks.Count);
      }

      UploadEnd?.Invoke();

      /**** Завершаем процесс обновления ****/

      result = ExecuteWithReconnectRetry(() => Bootloader.BootloaderEnd());

      if (result == BootloaderProtocolActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;
      else if (result != BootloaderProtocolActionResult.OK)
        return BootloaderWorkflowResult.ConnectionError;

      /**** Проверяем CRC прошивки ****/

      bool crcOK = false;

      result = ExecuteWithReconnectRetry(() => Bootloader.BootloaderCheckApplicationCRC(out crcOK));

      if (result == BootloaderProtocolActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;
      else if (result != BootloaderProtocolActionResult.OK)
        return BootloaderWorkflowResult.ConnectionError;
      else if (crcOK == false)
        return BootloaderWorkflowResult.UpdateError;

      /**** Запускаем прошивку ****/

      result = Bootloader.BootloaderApplicationRun();
      if (result == BootloaderProtocolActionResult.OK)
        return BootloaderWorkflowResult.OK;
      else
        return BootloaderWorkflowResult.ConnectionLost;
    }

    private BootloaderWorkflowResult UpdateProtocolVersion0(FirmwareUpdateFile updateFile, int connectionTimeout)
    {
      int connectionIterations = connectionTimeout * 2;
      int i;
      for (i = 0; i < connectionIterations; i++)
      {
        if (InitializeConnection())
          break;

        System.Threading.Thread.Sleep(500);
      }

      if (i == connectionIterations)
        return BootloaderWorkflowResult.ConnectionError;

      BootloaderProtocolActionResult result;

      //Очистка flash-памяти
      if (EraseBegin != null)
        EraseBegin();

      result = Bootloader.BootloaderBegin_V0();

      if (EraseEnd != null)
        EraseEnd();

      if (result != BootloaderProtocolActionResult.OK)
        return BootloaderWorkflowResult.ErasingError;

      //Отправка обновления
      if (UploadBegin != null)
        UploadBegin();

      var dataChunks = updateFile.DataChunksBase64;

      for (int b = 0; b < dataChunks.Count; b++)
      {
        string u = dataChunks[b].Trim();

        if (u != "")
        {
          result = Bootloader.BootloaderSend(u);
          if (result != BootloaderProtocolActionResult.OK)
            return BootloaderWorkflowResult.UpdateError;

          result = Bootloader.BootloaderWrite();
          if (result != BootloaderProtocolActionResult.OK)
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
      result = Bootloader.BootloaderEnd();
      if (result != BootloaderProtocolActionResult.OK)
        return BootloaderWorkflowResult.ConnectionError;

      //Проверяем CRC прошивки
      bool crcOK;
      result = Bootloader.BootloaderCheckApplicationCRC(out crcOK);
      if (result != BootloaderProtocolActionResult.OK)
        return BootloaderWorkflowResult.ConnectionError;
      if (crcOK == false)
        return BootloaderWorkflowResult.UpdateError;

      //Запускаем прошивку
      result = Bootloader.BootloaderApplicationRun();
      if (result == BootloaderProtocolActionResult.OK)
        return BootloaderWorkflowResult.OK;
      else
        return BootloaderWorkflowResult.UpdateError;
    }

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

    private bool InitializeConnection()
    {
      if (Bootloader.Connect() == false)
        return false;

      if (Bootloader.BootloaderActivate() != BootloaderProtocolActionResult.OK)
      {
        Bootloader.Disconnect();
        return false;
      }

      return true;
    }

    // Обобщённый ретрай с переподключением
    BootloaderProtocolActionResult ExecuteWithReconnectRetry(
        Func<BootloaderProtocolActionResult> operation,
        int maxAttempts = 10,
        int delayMsOnReconnectFail = 1000)
    {
      if (operation == null) throw new ArgumentNullException(nameof(operation));
      if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
      if (delayMsOnReconnectFail < 0) throw new ArgumentOutOfRangeException(nameof(delayMsOnReconnectFail));

      // Первая попытка отправки запроса
      BootloaderProtocolActionResult result = operation();

      // Если результат не требует повторения — возвращаем его
      if (result != BootloaderProtocolActionResult.ConnectionLost)
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
          if (result != BootloaderProtocolActionResult.ConnectionLost)
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
      Bootloader.Disconnect();
      return BootloaderProtocolActionResult.ConnectionLost;
    }


    private bool Reconnect()
    {
      Bootloader.Disconnect();
      return Bootloader.Connect();
    }
  }
}
