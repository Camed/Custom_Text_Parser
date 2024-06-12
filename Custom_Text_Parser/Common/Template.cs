using Custom_Text_Parser.Interfaces;
using System.Text.RegularExpressions;
using Custom_Text_Parser.Enums;

namespace Custom_Text_Parser.Common;

/// <summary>
/// Template implementation
/// </summary>
public class Template : ITemplate
{
    public string TemplateText { get; set; }
    public IList<IPlaceholder> Placeholders { get; private set; }
    public IList<IPlaceholder> OuterPlaceholders { get; private set; }
    public IList<IPlaceholder> RecurringPlaceholders { get; private set; }
    public string RecurringTemplate { get; private set; }
    public bool IncludeDefaultPlaceholders { get; private set; }

    public Template(string templateText, bool includeDefaultPlaceholders = false)
    {
        IncludeDefaultPlaceholders = includeDefaultPlaceholders;
        TemplateText = templateText.Replace("\r\n", "\n");
        Placeholders = ExtractPlaceholders();
        RecurringTemplate = ExtractRecurringTemplate();
        RecurringPlaceholders = ExtractPlaceholders(RecurringTemplate);
        OuterPlaceholders = Placeholders.Except(RecurringPlaceholders).ToList();
    }


    /// <summary>
    /// Extraction of all placeholders within template provided earlier.
    /// </summary>
    /// <returns>List of extracted placeholders.</returns>
    private IList<IPlaceholder> ExtractPlaceholders()
    {
        var result = new List<IPlaceholder>();
        if (IncludeDefaultPlaceholders)
        {
            foreach(KeywordType enumerable in Enum.GetValues(typeof(KeywordType)).Cast<KeywordType>())
            {
                if(enumerable != KeywordType.None)
                    result.Add(new Placeholder(enumerable.ToString()));
            }
        }
        result.AddRange(ExtractPlaceholders(TemplateText));
        return result;
    }

    /// <summary>
    /// Extraction of all placeholders within template
    /// </summary>
    /// <param name="template">Template from which placeholders should be extracted.</param>
    /// <returns>List of extracted placeholders.</returns>
    private IList<IPlaceholder> ExtractPlaceholders(string template)
    {
        ArgumentNullException.ThrowIfNull(template);
        var matches = Regex.Matches(template, @"{{(.*?)}}", RegexOptions.Singleline);
        var strings =  matches.Select(m => m.Groups[1].Value).Distinct().ToList();

        var placeholders = new List<IPlaceholder>();
        strings.ForEach(x => placeholders.Add(new Placeholder(x, ":")));
        return placeholders;
    }

    private string ExtractRecurringTemplate()
    {
        return ExtractRecurringTemplate(TemplateText);
    }
    /// <summary>
    /// Extract only recurring part of the template
    /// </summary>
    /// <param name="template">Template from which to extract</param>
    /// <returns>Recurring data within the template</returns>
    private string ExtractRecurringTemplate(string template)
    {
        string pattern = $"{Keyword.GetRegexPattern(KeywordType.RecurringStart)}(.*?){Keyword.GetRegexPattern(KeywordType.RecurringEnd)}";
        var match = Regex.Match(TemplateText, pattern, RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
