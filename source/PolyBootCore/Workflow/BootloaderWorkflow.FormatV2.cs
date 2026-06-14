using PolyBootCore.PolyBootProtocol;
using PolyBootCore.PolyBootProtocol.Bootloader;
using PolyBootCore.UpdateFile;

namespace PolyBootCore.Workflow
{
  public partial class BootloaderWorkflow
  {
    private BootloaderWorkflowResult UpdateFormatV2(FirmwareUpdateFileV2 updateFile)
    {
      return updateFile.Type == UpdateFileType.Single
        ? UpdateFormatV2Single(updateFile, false)
        : UpdateFormatV2Complex(updateFile, false);
    }

    // ---------------------------------------------------------------
    // Вспомогательные методы V2
    // ---------------------------------------------------------------

    /// <summary>
    /// Запрашивает DeviceID у устройства и сверяет с ожидаемым из файла.
    /// </summary>
    private BootloaderWorkflowResult CheckDeviceIdentity(
      IBootloaderProtocolV2 bootloader, FirmwareData firmware)
    {
      if (firmware.DeviceId == null)
        return BootloaderWorkflowResult.OK;

      string deviceId = null;
      var result = bootloader.BootloaderGetDeviceID(out deviceId);

      if (result == PolyBootActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;
      if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.IncompatibleDeviceError;

      if (deviceId != firmware.DeviceId)
        return BootloaderWorkflowResult.IncompatibleDeviceError;

      return BootloaderWorkflowResult.OK;
    }

    /// <summary>
    /// Проверяет версию прошивки на устройстве.
    /// Возвращает true если прошивка уже установлена — обновление можно пропустить.
    /// </summary>
    private BootloaderWorkflowResult IsVersionAlreadyFlashed(
      IBootloaderProtocolV2 bootloader, FirmwareData firmware,
      out bool alreadyFlashed)
    {
      alreadyFlashed = false;

      if (firmware.FirmwareVersion == null)
        return BootloaderWorkflowResult.OK;

      // Проверяем версию прошивки
      FirmwareVersion deviceVersion;
      var result = bootloader.BootloaderGetFirmwareVersion(out deviceVersion);

      if (result == PolyBootActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;
      else if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.ConnectionError;

      if (!deviceVersion.Equals(
            firmware.FirmwareVersion.Major,
            firmware.FirmwareVersion.Minor,
            firmware.FirmwareVersion.Patch))
        return BootloaderWorkflowResult.OK;

      // Версия совпала — дополнительно проверяем целостность
      bool crcOK = false;
      result = bootloader.BootloaderCheckApplicationCRC(out crcOK);
      if (result == PolyBootActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;
      if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.ConnectionError;

      alreadyFlashed = crcOK;
      return BootloaderWorkflowResult.OK;
    }
  }
}