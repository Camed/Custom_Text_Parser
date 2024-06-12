namespace Custom_Text_Parser.Interfaces;

/// <summary>
/// Parser interface. Every parser should be able to parse.
/// </summary>
public interface IParser
{
    /// <summary>
    /// Parse provided content, based on template.
    /// </summary>
    /// <param name="content">Provided text that should be parsed.</param>
    /// <param name="template">Template to be parsed.</param>
    /// <returns></returns>
    /// <exception cref="ParsingException"></exception>
    public Dictionary<IPlaceholder, List<string>> Parse(string content, ITemplate template);
}
