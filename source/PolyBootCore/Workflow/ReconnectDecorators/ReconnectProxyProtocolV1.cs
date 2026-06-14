using PolyBootCore.PolyBootProtocol;
using PolyBootCore.PolyBootProtocol.Proxy;
using PolyBootCore.Transport;
using System;

namespace PolyBootCore.Workflow
{
  internal class ReconnectProxyProtocolV1 : IProxyProtocolV1
  {
    IProxyProtocolV1 _inner { get; set; }
    IBootloaderTransportConnection TransportConnection;
    public ReconnectProxyProtocolV1(IBootloaderTransportChannel transportChannel,
      IBootloaderTransportConnection transportConnection)
    {
      _inner = new ProxyProtocolV1(transportChannel);
      TransportConnection = transportConnection;
    }

    //------------------------------------------------------------

    public PolyBootActionResult ProxyClose() => Retry(_inner.ProxyClose);

    public PolyBootActionResult ProxyGetSlaveList(out byte[] slaveIds)
    {
      byte[] localSlaveIds = null;

      var result = Retry(() => _inner.ProxyGetSlaveList(out localSlaveIds));

      slaveIds = localSlaveIds;

      return result;
    }

    public PolyBootActionResult ProxyInitSlaves() => Retry(_inner.ProxyInitSlaves);

    public PolyBootActionResult ProxyOpen(byte slaveId) => Retry(() => _inner.ProxyOpen(slaveId));

    public byte[] Receive() => _inner.Receive();

    public bool Send(byte[] data) => _inner.Send(data);

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
