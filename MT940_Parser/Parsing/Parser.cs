using Custom_Text_Parser.Exceptions;
using Custom_Text_Parser.Common;
using Custom_Text_Parser.Enums;
using Custom_Text_Parser.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace Custom_Text_Parser.Parsing;

/// <summary>
/// Parser implementation.
/// </summary>
public class Parser : IParser
{
    /// <summary>
    /// Parse provided content, based on template.
    /// </summary>
    /// <param name="content">Provided text that should be parsed.</param>
    /// <param name="template">Template to be parsed.</param>
    /// <returns></returns>
    /// <exception cref="ParsingException"></exception>
    public Dictionary<string, List<string>> Parse(string content, ITemplate template)
    {
        var result = new Dictionary<string, List<string>>();
        try
        {
            string regex = BuildRegexFromTemplate(template.TemplateText, template.Placeholders);

            // I had issues when \r\n and \n were in content, so I've decided to just replace these.
            // Might cause some performance issues, so to be changed in the future
            var contentClone = content.Replace("\r\n", "{{NEWLINE}}").Replace("\n", "{{NEWLINE}}");

            try
            {
                var matches = Regex.Matches(contentClone, regex, RegexOptions.Singleline);
                foreach (Match match in matches)
                {
                    var recurringMatches = match.Groups["RecurringSection"].Captures;
                    foreach (var placeholder in template.OuterPlaceholders)
                    {
                        if (match.Groups[placeholder].Success)
                        {
                            AddResult(ref result, placeholder, match.Groups[placeholder]
                                .Value
                                .Trim()
                                .Replace("{{NEWLINE}}", "\n")
                                );
                        }
                    }

                    foreach (Capture recurringMatch in recurringMatches)
                    {
                        GetRecurringData(ref result, recurringMatch.Value, template);
                    }
                }

                return result;
            }
            catch(Exception ex)
            {
                if(ex is ArgumentException || ex is ArgumentNullException || ex is ArgumentOutOfRangeException || ex is ParsingException)
                {
                    throw new ParsingException(content, regex);
                }
                else
                {
                    throw;
                }
            }
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Get's recurring data from a template.
    /// </summary>
    /// <param name="result">Where to store the data</param>
    /// <param name="content">From where data should be extracted</param>
    /// <param name="template">Based on which template</param>
    /// <exception cref="ParsingException"></exception>
    private void GetRecurringData(ref Dictionary<string, List<string>> result, string content, ITemplate template)
    {
        try
        {
            string recurringPattern = BuildRecurringPattern(template);
            var recurringMatches = Regex.Matches(content, recurringPattern, RegexOptions.Singleline);

            foreach (Match recurringMatch in recurringMatches)
            {
                foreach (var placeholder in template.Placeholders)
                {
                    if (recurringMatch.Groups[placeholder].Success)
                    {
                        AddResult(ref result, placeholder, recurringMatch.Groups[placeholder]
                            .Value
                            .Trim()
                            .Replace("{{NEWLINE}}", "\n"));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if(ex is ArgumentException || ex is ArgumentNullException || ex is ArgumentOutOfRangeException)
                throw new ParsingException(content, BuildRecurringPattern(template));
        }

    }

    /// <summary>
    /// Adds result to provided dictionary, cares about redundancy.
    /// </summary>
    /// <param name="result">Where to add</param>
    /// <param name="placeholder">Under what name</param>
    /// <param name="value">Value to store</param>
    private void AddResult(ref Dictionary<string, List<string>> result, string placeholder, string value)
    {
        if (!result.ContainsKey(placeholder))
            result[placeholder] = [];

        result[placeholder].Add(value);
    }

    /// <summary>
    /// Builds recurring pattern regex.
    /// </summary>
    /// <param name="template">Template to be used. Uses "RecurringTemplate" property of template.</param>
    /// <returns>Recurring pattern regex.</returns>
    private string BuildRecurringPattern(ITemplate template)
    {
        try
        {
            string recurringTemplate = template.RecurringTemplate;
            var placeholders = template.Placeholders;

            return BuildRegexFromTemplate(recurringTemplate, placeholders);
        }
        catch
        {
            throw;
        }

    }

    /// <summary>
    /// Builds regex pattern from the template
    /// </summary>
    /// <param name="templateText">Template text, raw form of the template</param>
    /// <param name="placeholders">List of keywords to be replaced</param>
    /// <returns>Template turned into regular expression</returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static string BuildRegexFromTemplate(string templateText, IList<string> placeholders)
    {
        try
        {
            string regex = Regex.Escape(templateText);

            regex = regex.Replace(Regex.Escape("{{RecurringStart}}"), "(?<RecurringSection>");
            regex = regex.Replace(Regex.Escape("{{RecurringEnd}}"), ")+");
            regex = regex.Replace("/", "\\/");

            foreach (var placeholder in placeholders)
            {
                string placeholderPattern = Regex.Escape("{{" + placeholder + "}}");
                regex = regex.Replace(placeholderPattern, $"(?<{placeholder}>.+?)");
            }

            regex = regex.Replace("\\ ", " ")
                 .Replace("\\:", ":")
                 .Replace("\\\\", "\\")
                 .Replace("\\r", "\r")
                 .Replace("\\n", "\n")
                 .Replace("\\{", "{")
                 .Replace("\r\n", "{{NEWLINE}}")
                 .Replace("\n", "{{NEWLINE}}");

            return string.Concat("^", regex, "$");
        }
        catch (ArgumentNullException ex)
        {
            throw new ArgumentNullException("Incorrect data provided.", ex);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Gets type of Keyword based on placeholder.
    /// </summary>
    /// <param name="placeholder">Placeholder on which Keyword is based</param>
    /// <returns>According keyword type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="OverflowException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private static KeywordType GetKeywordType(string placeholder)
    {
        try
        {
            var result = (KeywordType)Enum.Parse(typeof(KeywordType), placeholder);
            return result;
        }
        catch (ArgumentNullException ex)
        {
            throw new ArgumentNullException($"MT940Parser: Value of {nameof(placeholder)} is null.", ex);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"MT940Parser: Value of {nameof(placeholder)} is incorrect", ex);
        }
        catch (OverflowException ex)
        {
            throw new OverflowException($"MT940Parser: Couldn't parse {nameof(placeholder)}. Overflow.", ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"MT940Parser: Invalid operation on {nameof(placeholder)}.", ex);
        }
        catch
        {
            throw;
        }
    }


}