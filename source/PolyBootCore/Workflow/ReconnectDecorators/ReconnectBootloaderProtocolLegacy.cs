using PolyBootCore.PolyBootProtocol;
using PolyBootCore.PolyBootProtocol.Bootloader;
using PolyBootCore.Transport;

namespace PolyBootCore.Workflow
{
  internal class ReconnectBootloaderProtocolLegacy : ReconnectBootloaderBase, IBootloaderProtocolLegacy
  {
    IBootloaderProtocolLegacy _inner { get; set; }

    public ReconnectBootloaderProtocolLegacy(IBootloaderTransportChannel transportChannel,
      IBootloaderTransportConnection transportConnection)
      : this(new BootloaderProtocolV0(transportChannel), transportChannel, transportConnection)
    {
    }

    private ReconnectBootloaderProtocolLegacy(IBootloaderProtocolLegacy inner,
          IBootloaderTransportChannel transportChannel, IBootloaderTransportConnection transportConnection)
          : base(inner, transportChannel, transportConnection)
    {
      _inner = inner;
      _inner.BootloaderMemoryErasureProgress += BootloaderMemoryErasureProgress;
    }

    //------------------------------------------------------------

    public event BootloaderErasureProgressDelegate BootloaderMemoryErasureProgress;

    public PolyBootActionResult BootloaderBegin() => Retry(_inner.BootloaderBegin);

    //------------------------------------------------------------
  }
}
