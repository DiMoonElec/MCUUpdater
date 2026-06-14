using PolyBootCore.PolyBootProtocol;
using PolyBootCore.UpdateFile;

namespace PolyBootCore.Workflow
{
  public partial class BootloaderWorkflow
  {
    private BootloaderWorkflowResult UpdateFormatLegacy(FirmwareUpdateFileV2 updateFile)
    {
      var bootloader = new ReconnectBootloaderProtocolLegacy(Transport, Transport);
      bootloader.BootloaderMemoryErasureProgress += OnMemoryErasureProgress;

      if (!WaitForConnection(bootloader))
        return BootloaderWorkflowResult.ConnectionError;

      PolyBootActionResult result;

      EraseBegin?.Invoke();

      result = bootloader.BootloaderBegin();

      EraseEnd?.Invoke();

      if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.ErasingError;

      UploadBegin?.Invoke();

      var chunks = updateFile.RootFirmware.DataChunksBase64;

      for (int i = 0; i < chunks.Count; i++)
      {
        string chunk = chunks[i].Trim();

        if (chunk.Length == 0)
          continue;

        result = bootloader.BootloaderSend(chunk);
        if (result != PolyBootActionResult.OK)
          return BootloaderWorkflowResult.UpdateError;

        result = bootloader.BootloaderWrite();
        if (result != PolyBootActionResult.OK)
          return BootloaderWorkflowResult.UpdateError;

        UploadProgress?.Invoke((i * 100) / chunks.Count);
      }

      UploadEnd?.Invoke();

      result = bootloader.BootloaderEnd();
      if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.UpdateError;

      bool crcOK;
      result = bootloader.BootloaderCheckApplicationCRC(out crcOK);
      if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.UpdateError;
      if (!crcOK)
        return BootloaderWorkflowResult.UpdateError;

      result = bootloader.BootloaderApplicationRun();
      return result == PolyBootActionResult.OK
        ? BootloaderWorkflowResult.OK
        : BootloaderWorkflowResult.UpdateError;
    }
  }
}