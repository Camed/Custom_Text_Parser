using Custom_Text_Parser.Enums;

namespace Custom_Text_Parser.Interfaces;

/// <summary>
/// Keywords are objects behind placeholders.
/// Types of keywords are described in 'KeywordType' enum.
/// </summary>
public interface IKeyword
{
    public string? KeywordValue { get; }
    public KeywordType KeywordType { get; }
}
