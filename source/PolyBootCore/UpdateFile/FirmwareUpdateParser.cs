using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PolyBootCore.UpdateFile
{
  public static class FirmwareUpdateParser
  {
    private const string HeaderPrefix = "#POLYBOOT";

    public static FirmwareUpdateFile Parse(string filePath)
    {
      if (!File.Exists(filePath))
        throw new FileNotFoundException("Update file not found", filePath);

      string[] lines = File.ReadAllLines(filePath, Encoding.ASCII);

      if (lines.Length == 0)
        throw new InvalidDataException("Empty update file");

      // ---------- Legacy формат ----------
      if (!lines[0].StartsWith(HeaderPrefix, StringComparison.Ordinal))
      {
        return new FirmwareUpdateFile(0, 0, null, lines);
      }

      // ---------- Новый формат ----------
      int protocolVersion;
      int formatVersion;
      ParseHeaderLine(lines[0], out protocolVersion, out formatVersion);

      if (formatVersion != 1)
        throw new NotSupportedException("Unsupported format version: " + formatVersion);

      if (lines.Length < 3)
        throw new InvalidDataException("Invalid file structure");

      // Последняя строка — SHA256 в HEX
      string shaHexLine = lines[lines.Length - 1];
      byte[] expectedHash = ParseHex(shaHexLine);

      // Проверяем SHA256
      byte[] actualHash = ComputeSha256(lines, 0, lines.Length - 1);

      if (!HashesEqual(expectedHash, actualHash))
        throw new InvalidDataException("Firmware file is corrupted (SHA256 mismatch)");

      // Header chunk
      string headerChunk = lines[1];

      // Data chunks
      string[] dataChunks = lines
          .Skip(2)
          .Take(lines.Length - 3)
          .ToArray();

      return new FirmwareUpdateFile(protocolVersion, formatVersion, headerChunk, dataChunks);
    }

    // ================= helpers =================

    private static void ParseHeaderLine(string line, out int proto, out int format)
    {
      proto = 0;
      format = 0;

      // #POLYBOOT;PROTO=1;FORMAT=1
      string[] parts = line.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

      foreach (string part in parts)
      {
        if (part.StartsWith("PROTO=", StringComparison.OrdinalIgnoreCase))
          proto = int.Parse(part.Substring(6), CultureInfo.InvariantCulture);

        else if (part.StartsWith("FORMAT=", StringComparison.OrdinalIgnoreCase))
          format = int.Parse(part.Substring(7), CultureInfo.InvariantCulture);
      }
    }

    private static byte[] ComputeSha256(string[] lines, int start, int count)
    {
      using (var sha256 = SHA256.Create())
      {
        for (int i = start; i < start + count; i++)
        {
          byte[] bytes = Encoding.ASCII.GetBytes(lines[i]);
          sha256.TransformBlock(bytes, 0, bytes.Length, null, 0);
        }

        sha256.TransformFinalBlock(new byte[0], 0, 0);
        return sha256.Hash;
      }
    }

    private static byte[] ParseHex(string hex)
    {
      if (hex.Length % 2 != 0)
        throw new InvalidDataException("Invalid HEX SHA256 length");

      byte[] result = new byte[hex.Length / 2];

      for (int i = 0; i < result.Length; i++)
      {
        result[i] = byte.Parse(
            hex.Substring(i * 2, 2),
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture);
      }

      return result;
    }

    private static bool HashesEqual(byte[] a, byte[] b)
    {
      if (a == null || b == null || a.Length != b.Length)
        return false;

      // Простое постоянное по времени сравнение
      int diff = 0;
      for (int i = 0; i < a.Length; i++)
        diff |= a[i] ^ b[i];

      return diff == 0;
    }
  }
}
