namespace PolyBootCore.PolyBootProtocol.Bootloader
{
  public class FirmwareVersion
  {
    public ushort Major { get; private set; }
    public ushort Minor { get; private set; }
    public ushort Patch { get; private set; }
    public bool FirmwareIsPresent { get; private set; }

    public FirmwareVersion(ushort major, ushort minor, ushort patch, bool firmwareIsPresent)
    {
      Major = major;
      Minor = minor;
      Patch = patch;
      FirmwareIsPresent = firmwareIsPresent;
    }

    public bool Equals(ushort major, ushort minor, ushort patch)
    {
      if (!FirmwareIsPresent)
        return false;

      return Major == major && Minor == minor && Patch == patch;
    }

    public bool Equals(FirmwareVersion other)
    {
      if (other == null)
        return false;

      if (!FirmwareIsPresent || !other.FirmwareIsPresent)
        return false;

      return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
    }

    public override string ToString()
      => $"{Major}.{Minor}.{Patch}";
  }

  internal interface IBootloaderProtocolV2 : IBootloaderProtocolV1
  {
    PolyBootActionResult BootloaderGetFirmwareVersion(out FirmwareVersion firmwareVersion);
    PolyBootActionResult BootloaderGetDeviceID(out string deviceId);
  }
}
