using PolyBootCore.PolyBootProtocol;
using PolyBootCore.PolyBootProtocol.Bootloader;
using PolyBootCore.Transport;

namespace PolyBootCore.Workflow
{
  internal class ReconnectBootloaderProtocolV2 : ReconnectBootloaderProtocolV1, IBootloaderProtocolV2
  {
    IBootloaderProtocolV2 _inner { get; set; }

    public ReconnectBootloaderProtocolV2(IBootloaderTransportChannel transportChannel,
      IBootloaderTransportConnection transportConnection)
      : this(new BootloaderProtocolV2(transportChannel), transportChannel, transportConnection)
    {
    }

    private ReconnectBootloaderProtocolV2(IBootloaderProtocolV2 inner,
          IBootloaderTransportChannel transportChannel, IBootloaderTransportConnection transportConnection)
          : base(inner, transportChannel, transportConnection)
    {
      _inner = inner;
    }

    //------------------------------------------------------------

    public PolyBootActionResult BootloaderGetDeviceID(out string deviceId)
    {
      string localDeviceId = null;

      var result = Retry(() => _inner.BootloaderGetDeviceID(out localDeviceId));

      deviceId = localDeviceId;

      return result;
    }

    public PolyBootActionResult BootloaderGetFirmwareVersion(out FirmwareVersion firmwareVersion)
    {
      FirmwareVersion localFirmwareVersion = null;

      var result = Retry(() => _inner.BootloaderGetFirmwareVersion(out localFirmwareVersion));

      firmwareVersion = localFirmwareVersion;

      return result;
    }

    //------------------------------------------------------------
  }
}
