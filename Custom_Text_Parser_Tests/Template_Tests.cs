using Custom_Text_Parser.Common;
using System.Collections.Generic;
using Custom_Text_Parser.Parsing;
using System.Reflection;

namespace Custom_Text_Parser;

public class Template_Tests
{
    [Fact]
    public void ExtractPlaceholders_ShouldReturnCorrectPlaceholders_WhenTemplateIsValid()
    {
        // Arrange
        string templateText = "Amount: {{OriginalPostingAmount}} Currency: {{OriginalCurrency}}";
        var template = new Template(templateText);

        var expectedResult = new List<string> { "OriginalPostingAmount", "OriginalCurrency" };

        // Act
        var placeholders = template.Placeholders;

        // Assert
        placeholders.Should().BeEquivalentTo(
            expectedResult,
            options => options.WithStrictOrdering(),
            because: "Template should correctly identify and extract all placeholders.");
    }

    [Fact]
    public void ExtractRecurringTemplate_ShouldReturnCorrectTemplate_WhenTemplateIsValid()
    {
        // Arrange & Act
        string templateText = "Start{{RecurringStart}}RecurringContent{{RecurringEnd}}End";
        var template = new Template(templateText);
        var recurringTemplate = template.RecurringTemplate;

        var expectedResult = "RecurringContent";

        // Assert
        recurringTemplate.Should().Be(expectedResult, because: "The template should correctly extract recurring section");
    }

    [Fact]
    public void ExtractPlaceholders_ShouldReturnEmptyList_WhenNoPlaceholdersInTemplate()
    {
        // Arrange & Act
        string templateText = "Template without placeholders";
        var template = new Template(templateText);

        var placeholders = template.Placeholders;

        // Assert
        placeholders.Should().BeEmpty(because: "The template does not contain any placeholders");
    }

    [Fact]
    public void ExtractRecurringTemplate_ShouldReturnEmptyString_WhenNoRecurringSection()
    {
        // Arrange & Act
        string templateText = "No recurring section in this {{Template}}";
        var template = new Template(templateText);

        var recurringTemplate = template.RecurringTemplate;

        // Assert
        recurringTemplate.Should().BeEmpty(because: "The template does not containt a recurring section");
    }

    [Fact]
    public void ExtractPlaceholders_ShouldReturnDefaultKeywordValues_WhenIncludeDefaultPlaceholdersIsTrue()
    {
        // Arrange & Act
        string templateText = "Template without placeholders";
        var template = new Template(templateText, true);
        var placeholders = template.Placeholders;

        // Assert
        placeholders.Should().BeEquivalentTo(
                new List<string>()
                {
                    "PostingKey",
                    "PostingDate",
                    "OrginalPostingAmount",
                    "OrginalCurrency",
                    "NameRecipient",
                    "Use",
                    "SortCodeRecipient",
                    "AccountRecipient",
                    "PostingText",
                    "BusinessTransactionCode",
                    "RecurringStart",
                    "RecurringEnd"
                }
            );
    }
}
