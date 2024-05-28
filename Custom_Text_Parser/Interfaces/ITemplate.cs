namespace Custom_Text_Parser.Interfaces;
/// <summary>
/// Interface, each template contains its text, recurring fragments and placeholders.
/// We should be able to extract data from text based on these templates.
/// </summary>
public interface ITemplate
{
    /// <summary>
    /// Determines whether default placeholders (based on KeywordType) are included.
    /// </summary>
    public bool IncludeDefaultPlaceholders { get; }
    /// <summary>
    /// Text of the template
    /// </summary>
    public string TemplateText { get; set; }

    /// <summary>
    /// Inner text from the template, contains recurring data. 
    /// </summary>
    public string RecurringTemplate { get; }

    /// <summary>
    /// All placeholders within template.
    /// </summary>
    public IList<string> Placeholders { get; }

    /// <summary>
    /// Placeholders outside recurring section
    /// </summary>
    public IList<string> OuterPlaceholders { get; }

    /// <summary>
    /// Placeholders within recurring section
    /// </summary>
    public IList<string> RecurringPlaceholders { get; }

    /// <summary>
    /// Extract placeholders from template
    /// </summary>
    /// <param name="template">Template from which extraction is done</param>
    /// <returns>List of placeholders</returns>
    protected IList<string> ExtractPlaceholders(string template);

    /// <summary>
    /// Extract placeholders from template (TemplateText property)
    /// </summary>
    /// <returns>List of placeholders</returns>
    protected IList<string> ExtractPlaceholders();

    /// <summary>
    /// Extract recurring section from template
    /// </summary>
    /// <param name="template">Template from which extraction is done</param>
    /// <returns>Recurring template string</returns>
    protected string ExtractRecurringTemplate(string template);

    /// <summary>
    /// Extract recurring section from template (TemplateText property)
    /// </summary>
    /// <returns>Recurring template string</returns>
    protected string ExtractRecurringTemplate();
}
