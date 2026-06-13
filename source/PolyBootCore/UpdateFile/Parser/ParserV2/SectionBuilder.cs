using PolyBootCore.PolyBootProtocol.Bootloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PolyBootCore.UpdateFile.Parser.ParserV2
{
  internal static class SectionBuilder
  {
    internal static FirmwareUpdateFileV2 Build(List<LineToken> tokens)
    {
      if (tokens.Count == 0 || tokens[0].Type != LineTokenType.GlobalHeader)
        throw new InvalidDataException("Missing #POLYBOOT header");

      // SHA256 — последний токен
      int lastIdx = tokens.Count - 1;
      if (tokens[lastIdx].Type != LineTokenType.Sha256)
        throw new InvalidDataException(
          string.Format("Line {0}: expected SHA256 checksum on last line",
                        tokens[lastIdx].LineNumber));

      VerifySha256(tokens);

      // Глобальный заголовок
      var globalKv = ParseHelpers.ParseKeyValue(tokens[0].Content);
      var minUpdaterVersion = ParseMinUpdaterVersion(globalKv);
      var fileType = ParseFileType(globalKv);

      // REM-строки — между GlobalHeader и первым #FIRMWARE
      var comments = new List<string>();
      int pos = 1;
      while (pos < lastIdx && tokens[pos].Type == LineTokenType.Rem)
      {
        string text = tokens[pos].Content;
        int remEnd = text.IndexOf(' ');
        comments.Add(remEnd >= 0 ? text.Substring(remEnd + 1) : string.Empty);
        pos++;
      }

      // Секции #FIRMWARE
      var sections = ParseFirmwareSections(tokens, pos, lastIdx);

      return BuildContainer(minUpdaterVersion, fileType, comments, sections);
    }

    // ---------------------------------------------------------------
    // Глобальный заголовок
    // ---------------------------------------------------------------

    private static FirmwareVersion ParseMinUpdaterVersion(
      Dictionary<string, string> kv)
    {
      string value;
      if (!kv.TryGetValue("MIN_UPDATER", out value) || string.IsNullOrEmpty(value))
        throw new InvalidDataException(
          "Missing required field 'MIN_UPDATER' in #POLYBOOT");

      return ParseHelpers.ParseVersion(value, "#POLYBOOT MIN_UPDATER");
    }

    private static UpdateFileType ParseFileType(Dictionary<string, string> kv)
    {
      string value;
      if (!kv.TryGetValue("TYPE", out value) || string.IsNullOrEmpty(value))
        throw new InvalidDataException(
          "Missing required field 'TYPE' in #POLYBOOT");

      if (value.Equals("SINGLE", StringComparison.OrdinalIgnoreCase))
        return UpdateFileType.Single;
      if (value.Equals("COMPLEX", StringComparison.OrdinalIgnoreCase))
        return UpdateFileType.Complex;

      throw new InvalidDataException(
        string.Format("Unknown TYPE value '{0}' in #POLYBOOT", value));
    }

    // ---------------------------------------------------------------
    // Секции #FIRMWARE
    // ---------------------------------------------------------------

    private static List<Tuple<bool, FirmwareData>> ParseFirmwareSections(
      List<LineToken> tokens, int start, int endExclusive)
    {
      var sections = new List<Tuple<bool, FirmwareData>>();
      int pos = start;

      while (pos < endExclusive)
      {
        if (tokens[pos].Type != LineTokenType.FirmwareHeader)
          throw new InvalidDataException(
            string.Format("Line {0}: expected #FIRMWARE header, got '{1}'",
                          tokens[pos].LineNumber, tokens[pos].Content));

        var firmwareKv = ParseHelpers.ParseKeyValue(tokens[pos].Content);
        int headerLine = tokens[pos].LineNumber;
        pos++;

        bool isSlave = ParseTarget(firmwareKv, headerLine);
        int bootloaderVer = ParseBootloaderVersion(firmwareKv, headerLine);
        int? proxyVer = ParseProxyVersion(firmwareKv, headerLine, isSlave);
        string deviceId = ParseHelpers.RequireKey(firmwareKv, "ID", "#FIRMWARE line " + headerLine);
        FirmwareVersion firmwareVer = ParseFirmwareVersion(firmwareKv, headerLine);
        string headerChunk = ReadHeaderChunk(tokens, ref pos, endExclusive, headerLine);
        var dataChunks = ReadDataChunks(tokens, ref pos, endExclusive, headerLine);

        var data = new FirmwareData(
          deviceId, firmwareVer, bootloaderVer, proxyVer,
          headerChunk, dataChunks);

        sections.Add(new Tuple<bool, FirmwareData>(isSlave, data));
      }

      return sections;
    }

    private static bool ParseTarget(
      Dictionary<string, string> kv, int headerLine)
    {
      string raw = ParseHelpers.RequireKey(kv, "TARGET", "#FIRMWARE line " + headerLine);

      if (raw.Equals("ROOT", StringComparison.OrdinalIgnoreCase)) return false;
      if (raw.Equals("SLAVE", StringComparison.OrdinalIgnoreCase)) return true;

      throw new InvalidDataException(
        string.Format("Line {0}: unknown TARGET value '{1}'", headerLine, raw));
    }

    private static int ParseBootloaderVersion(
      Dictionary<string, string> kv, int headerLine)
    {
      string raw = ParseHelpers.RequireKey(
        kv, "BOOTLOADER", "#FIRMWARE line " + headerLine);
      return ParseHelpers.ParseInt(raw, "#FIRMWARE BOOTLOADER line " + headerLine);
    }

    private static int? ParseProxyVersion(
      Dictionary<string, string> kv, int headerLine, bool isSlave)
    {
      string raw;
      if (!kv.TryGetValue("PROXY", out raw) || string.IsNullOrEmpty(raw))
        return null;

      // У слейвов PROXY игнорируется
      if (isSlave)
        return null;

      return ParseHelpers.ParseInt(raw, "#FIRMWARE PROXY line " + headerLine);
    }

    private static FirmwareVersion ParseFirmwareVersion(
      Dictionary<string, string> kv, int headerLine)
    {
      string raw = ParseHelpers.RequireKey(kv, "VER", "#FIRMWARE line " + headerLine);
      return ParseHelpers.ParseVersion(raw, "#FIRMWARE VER line " + headerLine);
    }

    private static string ReadHeaderChunk(
      List<LineToken> tokens, ref int pos, int endExclusive, int headerLine)
    {
      if (pos >= endExclusive || tokens[pos].Type != LineTokenType.DataChunk)
        throw new InvalidDataException(
          string.Format("Line {0}: #FIRMWARE section has no header chunk", headerLine));

      string chunk = tokens[pos].Content;
      pos++;
      return chunk;
    }

    private static List<string> ReadDataChunks(
      List<LineToken> tokens, ref int pos, int endExclusive, int headerLine)
    {
      var chunks = new List<string>();

      while (pos < endExclusive && tokens[pos].Type == LineTokenType.DataChunk)
      {
        chunks.Add(tokens[pos].Content);
        pos++;
      }

      if (chunks.Count == 0)
        throw new InvalidDataException(
          string.Format("Line {0}: #FIRMWARE section has no data chunks", headerLine));

      return chunks;
    }

    // ---------------------------------------------------------------
    // Сборка контейнера
    // ---------------------------------------------------------------

    private static FirmwareUpdateFileV2 BuildContainer(
      FirmwareVersion minUpdaterVersion,
      UpdateFileType fileType,
      List<string> comments,
      List<Tuple<bool, FirmwareData>> sections)
    {
      FirmwareData rootFirmware = null;
      var slaveFirmwares = new List<FirmwareData>();

      foreach (var section in sections)
      {
        if (!section.Item1)
          rootFirmware = section.Item2;
        else
          slaveFirmwares.Add(section.Item2);
      }

      if (fileType == UpdateFileType.Single)
        return new FirmwareUpdateFileV2(minUpdaterVersion, comments, rootFirmware);

      return new FirmwareUpdateFileV2(
        minUpdaterVersion, comments, rootFirmware, slaveFirmwares);
    }

    // ---------------------------------------------------------------
    // SHA256
    // ---------------------------------------------------------------

    private static void VerifySha256(List<LineToken> tokens)
    {
      using (var sha256 = SHA256.Create())
      {
        for (int i = 0; i < tokens.Count - 1; i++)
        {
          byte[] bytes = Encoding.ASCII.GetBytes(tokens[i].Content);
          sha256.TransformBlock(bytes, 0, bytes.Length, null, 0);
        }
        sha256.TransformFinalBlock(new byte[0], 0, 0);

        byte[] actual = sha256.Hash;
        byte[] expected = ParseHelpers.ParseHex(tokens[tokens.Count - 1].Content);

        if (!ParseHelpers.HashesEqual(expected, actual))
          throw new InvalidDataException(
            "Firmware file is corrupted (SHA256 mismatch)");
      }
    }
  }
}