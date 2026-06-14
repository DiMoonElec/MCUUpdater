using PolyBootCore.UpdateFile;

namespace PolyBootCore.Workflow
{
  public partial class BootloaderWorkflow
  {
    private BootloaderWorkflowResult UpdateFormatV1(FirmwareUpdateFileV2 updateFile)
    {
      /*
       * Для файлов формата V1 использовался протокол версии V1
       */
      var bootloader = new ReconnectBootloaderProtocolV1(Transport, Transport);

      if (!WaitForConnection(bootloader))
        return BootloaderWorkflowResult.ConnectionError;

      bootloader.BootloaderMemoryErasureProgress += OnMemoryErasureProgress;
      var result = FlashDevice(bootloader, updateFile.RootFirmware);
      bootloader.BootloaderMemoryErasureProgress -= OnMemoryErasureProgress;

      if (result != BootloaderWorkflowResult.OK)
        return result;

      result = RunApplication(bootloader);

      return result;
    }
  }
}