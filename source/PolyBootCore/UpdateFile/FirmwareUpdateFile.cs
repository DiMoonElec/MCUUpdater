using System;
using System.Collections.Generic;

namespace PolyBootCore.UpdateFile
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

      ValidateHeaderChunk(formatVersion, headerChunkBase64);
      ValidateDataChunks(dataChunksBase64);

      HeaderChunkBase64 = headerChunkBase64;
      DataChunksBase64 = dataChunksBase64;
    }

    private static void ValidateHeaderChunk(int formatVersion, string headerChunkBase64)
    {
      if (formatVersion >= 1)
      {
        if (string.IsNullOrWhiteSpace(headerChunkBase64))
          throw new ArgumentException("Header chunk must be present for format version >= 1", nameof(headerChunkBase64));

        ValidateBase64(headerChunkBase64, "Header chunk");
      }
    }

    private static void ValidateDataChunks(IReadOnlyList<string> dataChunksBase64)
    {
      if (dataChunksBase64 == null)
        throw new ArgumentNullException(nameof(dataChunksBase64));

      if (dataChunksBase64.Count == 0)
        throw new ArgumentException("Data chunks list must not be empty", nameof(dataChunksBase64));

      for (int i = 0; i < dataChunksBase64.Count; i++)
      {
        string chunk = dataChunksBase64[i];

        if (string.IsNullOrWhiteSpace(chunk))
          throw new ArgumentException($"Data chunk at index {i} is null or empty", nameof(dataChunksBase64));

        ValidateBase64(chunk, $"Data chunk at index {i}");
      }
    }

    private static void ValidateBase64(string base64, string context)
    {
      try
      {
        Convert.FromBase64String(base64);
      }
      catch (FormatException ex)
      {
        throw new ArgumentException($"{context} contains invalid Base64 data", ex);
      }
    }
  }
}
