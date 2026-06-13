using System.Collections.Generic;

namespace PolyBootCore.UpdateFile.Parser.ParserV2
{
  internal static class Tokenizer
  {
    internal static List<LineToken> Tokenize(string[] lines)
    {
      var tokens = new List<LineToken>(lines.Length);

      for (int i = 0; i < lines.Length; i++)
      {
        string line = lines[i].Trim();

        if (line.Length == 0)
          continue;

        tokens.Add(new LineToken
        {
          Type = ClassifyLine(line),
          Content = line,
          LineNumber = i + 1,
        });
      }

      return tokens;
    }

    private static LineTokenType ClassifyLine(string line)
    {
      if (line.StartsWith("#POLYBOOT", System.StringComparison.OrdinalIgnoreCase))
        return LineTokenType.GlobalHeader;

      if (line.StartsWith("#REM", System.StringComparison.OrdinalIgnoreCase))
        return LineTokenType.Rem;

      if (line.StartsWith("#FIRMWARE", System.StringComparison.OrdinalIgnoreCase))
        return LineTokenType.FirmwareHeader;

      if (IsHexSha256(line))
        return LineTokenType.Sha256;

      return LineTokenType.DataChunk;
    }

    private static bool IsHexSha256(string line)
    {
      if (line.Length != 64)
        return false;

      foreach (char c in line)
        if (!((c >= '0' && c <= '9') ||
              (c >= 'a' && c <= 'f') ||
              (c >= 'A' && c <= 'F')))
          return false;

      return true;
    }
  }
}