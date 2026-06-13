namespace PolyBootCore.UpdateFile.Parser.ParserV2
{
  internal static class FirmwareUpdateParserV2
  {
    internal static FirmwareUpdateFileV2 Parse(string[] lines)
    {
      var tokens = Tokenizer.Tokenize(lines);
      var file = SectionBuilder.Build(tokens);
      Validator.Validate(file);
      return file;
    }
  }
}