using Custom_Text_Parser.Interfaces;

namespace Custom_Text_Parser.Services;

public class ParserService : IParserService
{
    private readonly IParser _parser;
    private readonly IDictionary<string, ITemplate> _templates;

    /// <param name="parser">Parser to be used by the service</param>
    /// <param name="templates">Dictionary of templates that parser will use</param>
    /// <exception cref="ArgumentNullException"></exception>
    public ParserService(IParser parser, IDictionary<string, ITemplate> templates)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _templates = templates ?? throw new ArgumentNullException(nameof(templates));
    }
    /// <summary>
    /// Parses provided content based on template.
    /// </summary>
    /// <param name="content">Content to be parsed</param>
    /// <param name="templateName">Name of the template (provided in constructor).</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public Dictionary<string, List<string>> ParseContent(string content, string templateName)
    {
        if(!_templates.TryGetValue(templateName, out ITemplate template))
        {
            throw new ArgumentException($"Template \"{content}\" not found.");
        }

        try
        {
            return _parser.Parse(content, template);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse content", ex);
        }
    }
}
