using PolyBootCore.PolyBootProtocol;
using PolyBootCore.PolyBootProtocol.Bootloader;
using PolyBootCore.UpdateFile;

namespace PolyBootCore.Workflow
{
  public partial class BootloaderWorkflow
  {
    /// <summary>
    /// Универсальный метод прошивки одного устройства через протокол V1+.
    /// Не знает ничего о мастере/слейве/прокси — работает через переданный интерфейс.
    /// Для прошивки девайса достаточно команд версии V1, поэтому, этот метод
    /// можно сделать универсальным для прошивки девайсов с bootloader-ом версии V1+.
    /// </summary>
    /// <param name="bootloader">Интерфейс протокола загрузчика</param>
    /// <param name="firmware">Данные прошивки из файла обновления</param>
    private BootloaderWorkflowResult FlashDevice(
     IBootloaderProtocolV1 bootloader,
     FirmwareData firmware)
    {
      PolyBootActionResult result;

      /**** Стирание flash ****/
      EraseBegin?.Invoke();
      result = bootloader.BootloaderBegin(firmware.HeaderChunkBase64.Trim());
      if (result == PolyBootActionResult.IncompatibleDeviceError)
        return BootloaderWorkflowResult.IncompatibleDeviceError;
      if (result == PolyBootActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;
      if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.ErasingError;
      EraseEnd?.Invoke();

      /**** Передача чанков ****/
      UploadBegin?.Invoke();
      var chunks = firmware.DataChunksBase64;
      for (int i = 0; i < chunks.Count; i++)
      {
        string chunk = chunks[i].Trim();
        if (chunk.Length == 0)
          continue;

        result = bootloader.BootloaderSend(chunk);
        if (result == PolyBootActionResult.ConnectionLost)
          return BootloaderWorkflowResult.ConnectionLost;
        if (result != PolyBootActionResult.OK)
          return BootloaderWorkflowResult.UpdateError;

        result = bootloader.BootloaderWrite();
        if (result == PolyBootActionResult.ConnectionLost)
          return BootloaderWorkflowResult.ConnectionLost;
        if (result != PolyBootActionResult.OK)
          return BootloaderWorkflowResult.UpdateError;

        UploadProgress?.Invoke((i * 100) / chunks.Count);
      }
      UploadEnd?.Invoke();

      /**** Финализация ****/
      result = bootloader.BootloaderEnd();
      if (result == PolyBootActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;
      if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.UpdateError;

      /**** Проверка CRC ****/
      bool crcOK = false;
      result = bootloader.BootloaderCheckApplicationCRC(out crcOK);
      if (result == PolyBootActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;
      if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.UpdateError;
      if (!crcOK)
        return BootloaderWorkflowResult.UpdateError;

      return BootloaderWorkflowResult.OK;
    }

    /// <summary>
    /// Отправить команду передачи управления основной прошивки девайса
    /// </summary>
    /// <param name="bootloader">Интерфейс протокола загрузчика</param>
    private BootloaderWorkflowResult RunApplication(IBootloaderProtocolV1 bootloader)
    {
      var result = bootloader.BootloaderApplicationRun();
      return result == PolyBootActionResult.OK
        ? BootloaderWorkflowResult.OK
        : BootloaderWorkflowResult.ConnectionLost;
    }
  }
}