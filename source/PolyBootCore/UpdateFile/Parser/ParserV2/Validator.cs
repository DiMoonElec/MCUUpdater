using System.Collections.Generic;
using System.IO;

namespace PolyBootCore.UpdateFile.Parser.ParserV2
{
  internal static class Validator
  {
    internal static void Validate(FirmwareUpdateFileV2 file)
    {
      if (file.RootFirmware == null)
        throw new InvalidDataException(
          "Update file must contain exactly one TARGET=ROOT section");

      if (file.Type == UpdateFileType.Single)
        ValidateSingle(file);
      else
        ValidateComplex(file);

      ValidateUniqueIds(file);
    }

    private static void ValidateSingle(FirmwareUpdateFileV2 file)
    {
      if (file.SlaveFirmwares.Count > 0)
        throw new InvalidDataException(
          "TYPE=SINGLE must not contain TARGET=SLAVE sections");
    }

    private static void ValidateComplex(FirmwareUpdateFileV2 file)
    {
      if (file.SlaveFirmwares.Count == 0)
        throw new InvalidDataException(
          "TYPE=COMPLEX must contain at least one TARGET=SLAVE section");

      if (file.RootFirmware.ProxyVersion == null)
        throw new InvalidDataException(
          "TYPE=COMPLEX requires PROXY= field in TARGET=ROOT section");
    }

    private static void ValidateUniqueIds(FirmwareUpdateFileV2 file)
    {
      var seen = new HashSet<string>(System.StringComparer.Ordinal);

      CheckUniqueId(seen, file.RootFirmware, "ROOT");

      for (int i = 0; i < file.SlaveFirmwares.Count; i++)
        CheckUniqueId(seen, file.SlaveFirmwares[i],
          string.Format("SLAVE[{0}]", i));
    }

    private static void CheckUniqueId(
      HashSet<string> seen, FirmwareData data, string context)
    {
      if (string.IsNullOrEmpty(data.DeviceId))
        throw new InvalidDataException(
          string.Format("{0}: DeviceId must not be empty", context));

      if (!seen.Add(data.DeviceId))
        throw new InvalidDataException(
          string.Format("Duplicate DeviceId '{0}' in {1}", data.DeviceId, context));
    }
  }
}