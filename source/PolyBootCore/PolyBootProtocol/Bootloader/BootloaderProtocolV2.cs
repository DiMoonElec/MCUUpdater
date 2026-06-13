using PolyBootCore.Transport;
using System;
using System.Text;

namespace PolyBootCore.PolyBootProtocol.Bootloader
{
  internal class BootloaderProtocolV2 : BootloaderProtocolV1, IBootloaderProtocolV2
  {
    const byte CMD_BOOTLOADER_GET_FIRMWARE_VERSION = 0x79;
    const byte CMD_BOOTLOADER_GET_DEVICE_ID = 0x7A;


    const int DEVICE_ID_LENGTH = 32;

    public BootloaderProtocolV2(IBootloaderTransportChannel transportChannel) : base(transportChannel)
    {
    }

    public PolyBootActionResult BootloaderGetFirmwareVersion(out FirmwareVersion firmwareVersion)
    {
      firmwareVersion = null;

      if (TransportChannel.Send(new byte[] { CMD_BOOTLOADER_GET_FIRMWARE_VERSION }) == false)
        return PolyBootActionResult.ConnectionLost;

      var resp = TransportChannel.Receive();

      if (resp == null)
        return PolyBootActionResult.ConnectionLost;

      if (resp[0] != CMD_BOOTLOADER_GET_FIRMWARE_VERSION)
        return PolyBootActionResult.InternalError;

      // Прошивка отсутствует — возвращаем объект с FirmwareIsPresent=false
      if (resp[1] == 0x01)
      {
        firmwareVersion = new FirmwareVersion(0, 0, 0, firmwareIsPresent: false);
        return PolyBootActionResult.OK;
      }

      if (resp[1] != 0x00)
        return PolyBootActionResult.Error;

      if (resp.Length < 8)
        return PolyBootActionResult.InternalError;

      ushort major = BitConverter.ToUInt16(resp, 2);
      ushort minor = BitConverter.ToUInt16(resp, 4);
      ushort patch = BitConverter.ToUInt16(resp, 6);

      firmwareVersion = new FirmwareVersion(major, minor, patch, firmwareIsPresent: true);
      return PolyBootActionResult.OK;
    }

    public PolyBootActionResult BootloaderGetDeviceID(out string deviceId)
    {
      deviceId = null;

      if (TransportChannel.Send(new byte[] { CMD_BOOTLOADER_GET_DEVICE_ID }) == false)
        return PolyBootActionResult.ConnectionLost;

      var resp = TransportChannel.Receive();

      if (resp == null)
        return PolyBootActionResult.ConnectionLost;

      if (resp[0] != CMD_BOOTLOADER_GET_DEVICE_ID)
        return PolyBootActionResult.InternalError;

      if (resp[1] != 0x00)
        return PolyBootActionResult.Error;

      if (resp.Length < 2 + DEVICE_ID_LENGTH)
        return PolyBootActionResult.InternalError;

      // Преобразуем байты в строку, обрезая нулевой суффикс
      deviceId = Encoding.ASCII.GetString(resp, 2, DEVICE_ID_LENGTH)
                               .TrimEnd('\0');

      return PolyBootActionResult.OK;
    }
  }
}
