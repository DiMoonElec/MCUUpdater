using PolyBootCore.PolyBootProtocol;
using PolyBootCore.PolyBootProtocol.Bootloader;
using PolyBootCore.Transport;

namespace PolyBootCore.Workflow
{
  internal class ReconnectBootloaderProtocolV1 : ReconnectBootloaderBase, IBootloaderProtocolV1
  {
    IBootloaderProtocolV1 _inner { get; set; }

    public ReconnectBootloaderProtocolV1(IBootloaderTransportChannel transportChannel,
      IBootloaderTransportConnection transportConnection)
      : this(new BootloaderProtocolV1(transportChannel), transportChannel, transportConnection)
    {
    }

    protected internal ReconnectBootloaderProtocolV1(IBootloaderProtocolV1 inner,
          IBootloaderTransportChannel transportChannel, IBootloaderTransportConnection transportConnection)
          : base(inner, transportChannel, transportConnection)
    {
      _inner = inner;
      _inner.BootloaderMemoryErasureProgress += BootloaderMemoryErasureProgress;
    }

    //------------------------------------------------------------

    public event BootloaderErasureProgressDelegate BootloaderMemoryErasureProgress;

    public PolyBootActionResult BootloaderBegin(string header) => Retry(() => _inner.BootloaderBegin(header));

    //------------------------------------------------------------
  }
}
