using PolyBootCore.Transport;
using System;

namespace PolyBootCore.PolyBootProtocol.Bootloader
{
  internal class BootloaderProtocolV2 : BootloaderProtocolV1, IBootloaderProtocolV2
  {
    const byte CMD_BOOTLOADER_GET_FIRMWARE_VERSION = 0x79;

    public BootloaderProtocolV2(IBootloaderTransport transport) : base(transport)
    {
    }

    public BootloaderProtocolActionResult BootloaderGetFirmwareVersion(out FirmwareVersion firmwareVersion)
    {
      throw new NotImplementedException();
    }
  }
}
