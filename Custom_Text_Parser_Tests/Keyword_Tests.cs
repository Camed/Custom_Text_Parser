using Custom_Text_Parser.Common;
using Custom_Text_Parser.Enums;

namespace Custom_Text_Parser;
public class Keyword_Tests
{
    [Theory]
    [InlineData(KeywordType.PostingKey, @"{{PostingKey}}")]
    [InlineData(KeywordType.RecurringStart, @"{{RecurringStart}}")]
    [InlineData(KeywordType.RecurringEnd, @"{{RecurringEnd}}")]
    public void Keyword_ShouldCreateCorrectRegexPattern_ForGivenKeywordTypes(KeywordType keywordType, string expectedPattern)
    {
        // Act
        var keyword = Keyword.Create(keywordType);
        var regexPattern = Keyword.GetRegexPattern(keywordType);

        // Assert
        keyword.KeywordType.Should().Be(
            keywordType,
            because: "Created keyword should match the specified type.");

        regexPattern.Should().Be(
            expectedPattern,
            because: "Regex pattern should match the expected format for given keyword type.");
    }
}
