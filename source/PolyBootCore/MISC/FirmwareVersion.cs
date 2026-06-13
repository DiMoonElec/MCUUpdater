using System;

namespace PolyBootCore.MISC
{
  public class FirmwareVersion : IComparable<FirmwareVersion>
  {
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }

    public FirmwareVersion(int major, int minor, int patch)
    {
      Major = major;
      Minor = minor;
      Patch = patch;
    }

    public int CompareTo(FirmwareVersion other)
    {
      if (other == null) return 1;

      if (Major != other.Major) return Major.CompareTo(other.Major);
      if (Minor != other.Minor) return Minor.CompareTo(other.Minor);
      return Patch.CompareTo(other.Patch);
    }

    public static bool operator <(FirmwareVersion v1, FirmwareVersion v2) => v1.CompareTo(v2) < 0;
    public static bool operator >(FirmwareVersion v1, FirmwareVersion v2) => v1.CompareTo(v2) > 0;
    public static bool operator <=(FirmwareVersion v1, FirmwareVersion v2) => v1.CompareTo(v2) <= 0;
    public static bool operator >=(FirmwareVersion v1, FirmwareVersion v2) => v1.CompareTo(v2) >= 0;

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
  }

}
