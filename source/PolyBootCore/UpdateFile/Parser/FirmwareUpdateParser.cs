using PolyBootCore.UpdateFile.Parser.ParserLegacy;
using PolyBootCore.UpdateFile.Parser.ParserV1;
using PolyBootCore.UpdateFile.Parser.ParserV2;
using System;
using System.IO;
using System.Text;

namespace PolyBootCore.UpdateFile.Parser
{
  public static class FirmwareUpdateParser
  {
    private const string PolyBootPrefix = "#POLYBOOT";

    /// <summary>
    /// Загружает файл обновления любого поддерживаемого формата (0, 1, 2).
    /// </summary>
    public static FirmwareUpdateFileV2 Parse(string filePath)
    {
      if (!File.Exists(filePath))
        throw new FileNotFoundException("Update file not found", filePath);

      try
      {
        string[] lines = File.ReadAllLines(filePath, Encoding.ASCII);

        if (lines.Length == 0)
          throw new InvalidDataException("Empty update file");

        if (!lines[0].TrimStart().StartsWith(
              PolyBootPrefix, StringComparison.OrdinalIgnoreCase))
          return FirmwareUpdateParserLegacy.Parse(lines);

        var kv = ParseHelpers.ParseKeyValue(lines[0]);
        string formatRaw;
        if (!kv.TryGetValue("FORMAT", out formatRaw))
          throw new InvalidDataException(
            "Missing FORMAT field in #POLYBOOT header");

        int format = ParseHelpers.ParseInt(formatRaw, "#POLYBOOT FORMAT");

        switch (format)
        {
          case 1: return FirmwareUpdateParserV1.Parse(lines);
          case 2: return FirmwareUpdateParserV2.Parse(lines);
          default:
            throw new NotSupportedException(
              string.Format("Unsupported file format version: {0}", format));
        }
      }
      catch (NotSupportedException) { throw; }
      catch (FileNotFoundException) { throw; }
      catch (Exception ex)
      {
        throw new InvalidOperationException(
          "The selected file is corrupted, has a newer unsupported format," +
          " or is not a valid firmware update file", ex);
      }
    }
  }
}