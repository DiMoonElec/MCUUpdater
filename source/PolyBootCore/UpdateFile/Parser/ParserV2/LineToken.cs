namespace PolyBootCore.UpdateFile.Parser.ParserV2
{
  internal enum LineTokenType
  {
    GlobalHeader,   // #POLYBOOT;...
    Rem,            // #REM ...
    FirmwareHeader, // #FIRMWARE;...
    DataChunk,      // Base64
    Sha256,         // финальная hex-строка 64 символа
  }

  internal struct LineToken
  {
    public LineTokenType Type;
    public string Content;    // строка после trim
    public int LineNumber; // для сообщений об ошибках
  }
}