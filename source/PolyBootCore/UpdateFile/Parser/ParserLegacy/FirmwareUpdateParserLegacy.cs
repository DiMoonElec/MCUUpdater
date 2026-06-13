namespace PolyBootCore.UpdateFile.Parser.ParserLegacy
{
  internal static class FirmwareUpdateParserLegacy
  {
    // FORMAT=0: нет #POLYBOOT заголовка, все строки — data chunks
    internal static FirmwareUpdateFileV2 Parse(string[] lines)
    {
      var firmwareData = new FirmwareData(
        headerChunkBase64: null,
        dataChunksBase64: lines);

      return new FirmwareUpdateFileV2(
        formatVersion: 0,
        rootFirmware: firmwareData);
    }
  }
}