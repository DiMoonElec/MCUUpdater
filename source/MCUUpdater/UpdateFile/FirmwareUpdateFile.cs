using System.Collections.Generic;

namespace MCUUpdater
{
  public sealed class FirmwareUpdateFile
  {
    public int ProtocolVersion { get; private set; }
    public int FormatVersion { get; private set; }

    // Только для FORMAT >= 1
    public string HeaderChunkBase64 { get; private set; }

    // Всегда есть
    public IReadOnlyList<string> DataChunksBase64 { get; private set; }

    public FirmwareUpdateFile(int protocolVersion, int formatVersion, string headerChunkBase64, IReadOnlyList<string> dataChunksBase64)
    {
      ProtocolVersion = protocolVersion;
      FormatVersion = formatVersion;
      HeaderChunkBase64 = headerChunkBase64;
      DataChunksBase64 = dataChunksBase64;
    }
  }
}
