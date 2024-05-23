namespace Custom_Text_Parser.Interfaces;

/// <summary>
/// Provides API for parsing content.
/// </summary>
public interface IParserService
{
    Dictionary<string, List<string>> ParseContent(string content, string templateName);
}
