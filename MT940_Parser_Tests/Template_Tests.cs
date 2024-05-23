using Xunit;
using FluentAssertions;
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
        var template = new MT940Template(templateText);

        var expectedResult = new List<string> { "OriginalPostingAmount", "OriginalCurrency" };

        // Act
        var placeholders = template.ExtractPlaceholders();

        // Assert
        placeholders.Should().BeEquivalentTo(
            expectedResult,
            options => options.WithStrictOrdering(),
            because: "Template should correctly identify and extract all placeholders.");
    }
}
