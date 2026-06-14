using PolyBootCore.PolyBootProtocol;
using PolyBootCore.PolyBootProtocol.Bootloader;
using PolyBootCore.Transport;
using System;

namespace PolyBootCore.Workflow
{
  internal abstract class ReconnectBootloaderBase : IBootloaderBase
  {
    public event BootloaderErasureProgressDelegate BootloaderUserDataErasureProgress;

    IBootloaderBase _inner { get; set; }
    IBootloaderTransportConnection TransportConnection;

    public ReconnectBootloaderBase(IBootloaderBase inner,
      IBootloaderTransportChannel transportChannel,
      IBootloaderTransportConnection transportConnection)
    {
      TransportConnection = transportConnection;

      _inner = inner;
      _inner.BootloaderUserDataErasureProgress += BootloaderUserDataErasureProgress;
    }

    //------------------------------------------------------------

    /// <summary>
    /// Для BootloaderActivate() не делаем обвертку Retry
    /// </summary>
    public PolyBootActionResult BootloaderActivate() => _inner.BootloaderActivate();

    public PolyBootActionResult BootloaderApplicationRun() => Retry(_inner.BootloaderApplicationRun);

    public PolyBootActionResult BootloaderCheckApplicationCRC(out bool crcOk)
    {
      bool localCrcOk = false;

      var result = Retry(() => _inner.BootloaderCheckApplicationCRC(out localCrcOk));

      crcOk = localCrcOk;

      return result;
    }

    public PolyBootActionResult BootloaderEnd() => Retry(_inner.BootloaderEnd);

    public PolyBootActionResult BootloaderSend(string frame) => Retry(() => _inner.BootloaderSend(frame));

    public PolyBootActionResult BootloaderWrite() => Retry(_inner.BootloaderWrite);

    public PolyBootActionResult BootloaderEraseUserData() => Retry(_inner.BootloaderEraseUserData);

    //------------------------------------------------------------

    protected internal PolyBootActionResult Retry(Func<PolyBootActionResult> action)
    {
      return ExecuteWithReconnectRetry.Retry(action,
        Reconnect,
        TransportConnection.Disconnect);
    }

    private bool Reconnect()
    {
      TransportConnection.Disconnect();
      return TransportConnection.Connect();
    }
  }
}
