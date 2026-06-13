namespace PolyBootCore.PolyBootProtocol.Bootloader
{
  public class FirmwareVersion
  {
    public ushort Major {  get; private set; }
    public ushort Minor { get; private set; }
    public ushort Patch { get; private set; }
    public bool FirmwareIsPresent { get; private set; }

    public FirmwareVersion(ushort major, ushort minor, ushort patch, bool firmwareIsPresent )
    {
      Major = major;
      Minor = minor;
      Patch = patch;
      FirmwareIsPresent = firmwareIsPresent;
    }

    public bool Equals(FirmwareVersion other)
      => Major == other.Major && Minor == other.Minor && Patch == other.Patch;

    public override string ToString()
      => $"{Major}.{Minor}.{Patch}";
  }

  internal interface IBootloaderProtocolV2 : IBootloaderProtocolV1
  {
    PolyBootActionResult BootloaderGetFirmwareVersion(out FirmwareVersion firmwareVersion);
  }
}
