using Custom_Text_Parser.Interfaces;
using System.Text.RegularExpressions;
using Custom_Text_Parser.Enums;

namespace Custom_Text_Parser.Common;

/// <summary>
/// Template implementation, here is MT940 parser.
/// </summary>
public class MT940Template : ITemplate
{
    public string TemplateText { get; set; }
    public IList<string> Placeholders { get; private set; }
    public IList<string> OuterPlaceholders { get; private set; }
    public IList<string> RecurringPlaceholders { get; private set; }
    public string RecurringTemplate { get; private set; }
    public MT940Template(string templateText)
    {
        TemplateText = templateText.Replace("\r\n", "\n");
        Placeholders = ExtractPlaceholders(templateText);
        RecurringTemplate = ExtractRecurringTemplate(templateText);
        RecurringPlaceholders = ExtractPlaceholders(RecurringTemplate);
        OuterPlaceholders = Placeholders.Except(RecurringPlaceholders).ToList();
    }


    /// <summary>
    /// Extraction of all placeholders within template provided earlier.
    /// </summary>
    /// <returns>List of extracted placeholders.</returns>
    public IList<string> ExtractPlaceholders()
    {
        return ExtractPlaceholders(TemplateText);
    }

    /// <summary>
    /// Extraction of all placeholders within template
    /// </summary>
    /// <param name="template">Template from which placeholders should be extracted.</param>
    /// <returns>List of extracted placeholders.</returns>
    public IList<string> ExtractPlaceholders(string template)
    {
        ArgumentNullException.ThrowIfNull(template);
        var matches = Regex.Matches(template, @"{{(.*?)}}", RegexOptions.Singleline);
        return matches.Select(m => m.Groups[1].Value).Distinct().ToList();
    }

    public string ExtractRecurringTemplate()
    {
        return ExtractRecurringTemplate(TemplateText);
    }
    /// <summary>
    /// Extract only recurring part of the template
    /// </summary>
    /// <param name="template">Template from which to extract</param>
    /// <returns>Recurring data within the template</returns>
    public string ExtractRecurringTemplate(string template)
    {
        string pattern = $"{Keyword.GetRegexPattern(KeywordType.RecurringStart)}(.*?){Keyword.GetRegexPattern(KeywordType.RecurringEnd)}";
        var match = Regex.Match(TemplateText, pattern, RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
