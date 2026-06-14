using PolyBootCore.Transport;
using PolyBootCore.UpdateFile;
using System;
using System.Threading;

namespace PolyBootCore.Workflow
{
  public partial class BootloaderWorkflow
  {
    private readonly IBootloaderTransport Transport;
    private readonly CancellationToken CancellationToken;
    private readonly bool HasCancellation;
    private readonly int ConnectionTimeout;

    // ---------------------------------------------------------------
    // События для UI
    // ---------------------------------------------------------------

    public event BootloaderEventDelegate EraseBegin;
    public event BootloaderEventDelegate EraseEnd;
    public event BootloaderEventDelegate UserDataEraseBegin;
    public event BootloaderEventDelegate UserDataEraseEnd;
    public event BootloaderEventDelegate UploadBegin;
    public event BootloaderEventDelegate UploadEnd;
    public event BootloaderProgressDelegate EraseProgress;
    public event BootloaderProgressDelegate UserDataEraseProgress;
    public event BootloaderProgressDelegate UploadProgress;

    // ---------------------------------------------------------------
    // Конструкторы
    // ---------------------------------------------------------------

    public BootloaderWorkflow(ConnectionConfig config)
    {
      ConnectionTimeout = config.DeviceWaitTimeout;
      Transport = config.CreateTransport();
    }

    public BootloaderWorkflow(ConnectionConfig config, CancellationToken token)
    {
      ConnectionTimeout = config.DeviceWaitTimeout;
      Transport = config.CreateTransport();
      Transport.SetCancellationToken(token);
      HasCancellation = true;
      CancellationToken = token;
    }

    // ---------------------------------------------------------------
    // Точка входа
    // ---------------------------------------------------------------

    public BootloaderWorkflowResult Update(FirmwareUpdateFileV2 updateFile)
    {
      try
      {
        switch (updateFile.FormatVersion)
        {
          case 0: return UpdateFormatLegacy(updateFile);
          case 1: return UpdateFormatV1(updateFile);
          case 2: return UpdateFormatV2(updateFile);
          default:
            throw new NotSupportedException(
              string.Format("File format version {0} is not supported",
                            updateFile.FormatVersion));
        }
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

    // ---------------------------------------------------------------
    // Progress-хелперы — пробрасывают события от протокола наружу
    // ---------------------------------------------------------------

    private void OnMemoryErasureProgress(int numBlocks, int currentBlock)
    {
      if (EraseProgress != null && numBlocks > 0)
        EraseProgress((currentBlock * 100) / numBlocks);
    }

    private void OnUserDataErasureProgress(int numBlocks, int currentBlock)
    {
      if (UserDataEraseProgress != null && numBlocks > 0)
        UserDataEraseProgress((currentBlock * 100) / numBlocks);
    }
  }
}