using PolyBootCore.UpdateFile;
using System;

namespace PolyBootCore.Workflow
{
  public partial class BootloaderWorkflow
  {
    private BootloaderWorkflowResult UpdateFormatV2Single(FirmwareUpdateFileV2 updateFile, bool forceUpdate)
    {
      var firmware = updateFile.RootFirmware;

      if (firmware.BootloaderVersion >= 2)
      {
        BootloaderWorkflowResult result;

        var bootloader = new ReconnectBootloaderProtocolV2(Transport, Transport);

        if (!WaitForConnection(bootloader))
          return BootloaderWorkflowResult.ConnectionError;

        if (forceUpdate == false)
        {
          bool alreadyFlashed;
          
          result = IsVersionAlreadyFlashed(bootloader, firmware, out alreadyFlashed);
          if (result != BootloaderWorkflowResult.OK)
            return result;

          // Если в девайс уже прошита актуальная версия, то пропускаем прошивку,
          // и сразу запускаем основное приложение
          if (alreadyFlashed == true)
            return RunApplication(bootloader);
        }

        bootloader.BootloaderMemoryErasureProgress += OnMemoryErasureProgress;
        result = FlashDevice(bootloader, firmware);
        bootloader.BootloaderMemoryErasureProgress -= OnMemoryErasureProgress;

        if (result != BootloaderWorkflowResult.OK)
          return result;

        result = RunApplication(bootloader);

        return result;
      }
      else
      {
        throw new NotSupportedException(
          string.Format("The file format {0} is incompatible with the bootloader version {1}.",
            updateFile.FormatVersion, firmware.FirmwareVersion));
      }
    }
  }
}
