using Custom_Text_Parser.Enums;
using Custom_Text_Parser.Interfaces;

namespace Custom_Text_Parser.Common;

/// <summary>
/// Keyword implementation.
/// </summary>
public class Keyword : IKeyword
{
    public KeywordType KeywordType { get; private set; }
    public string KeywordValue { get; private set; }
    private Keyword(KeywordType type)
    {
        KeywordType = type;
        KeywordValue = Enum.GetName(typeof(KeywordType), KeywordType) ?? throw new ArgumentException();
    }

    public static Keyword Create(KeywordType keywordType)
    {
        return new Keyword(keywordType);
    }
    public static string GetRegexPattern(KeywordType keywordType)
    {
        return $"{{{{{Enum.GetName(typeof(KeywordType), keywordType)}}}}}";
    }
}
