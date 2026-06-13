using System;
using System.IO;

namespace PolyBootCore.UpdateFile.Parser.ParserV1
{
  internal static class FirmwareUpdateParserV1
  {
    internal static FirmwareUpdateFileV2 Parse(string[] lines)
    {
      // Минимум: #POLYBOOT + header chunk + хотя бы один data chunk + SHA256
      if (lines.Length < 4)
        throw new InvalidDataException("Invalid FORMAT=1 file structure");

      VerifySha256(lines);

      string headerChunk = lines[1];

      int dataCount = lines.Length - 3; // минус заголовок, header chunk и SHA256
      var dataChunks = new string[dataCount];
      Array.Copy(lines, 2, dataChunks, 0, dataCount);

      var firmwareData = new FirmwareData(
        headerChunkBase64: headerChunk,
        dataChunksBase64: dataChunks);

      return new FirmwareUpdateFileV2(
        formatVersion: 1,
        rootFirmware: firmwareData);
    }

    private static void VerifySha256(string[] lines)
    {
      byte[] expected = ParseHelpers.ParseHex(lines[lines.Length - 1]);
      byte[] actual = ParseHelpers.ComputeSha256(lines, 0, lines.Length - 1);

      if (!ParseHelpers.HashesEqual(expected, actual))
        throw new InvalidDataException(
          "Firmware file is corrupted (SHA256 mismatch)");
    }
  }
}