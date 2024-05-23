namespace Custom_Text_Parser.Enums;

/// <summary>
/// Keywords we can find within our parsed files/templates.
/// Last two: 'RecurringStart' and 'RecurringEnd' are logic markings for start/end of recurring content
/// within parsed file.
/// </summary>
public enum KeywordType
{
    None = 0,
    PostingKey = 1,
    PostingDate = 2,
    OrginalPostingAmount = 4,
    OrginalCurrency = 8,
    NameRecipient = 16,
    Use = 32,
    SortCodeRecipient = 64,
    AccountRecipient = 128,
    PostingText = 256,
    BusinessTransactionCode = 512,
    RecurringStart = 1024,
    RecurringEnd = 2048,
}
