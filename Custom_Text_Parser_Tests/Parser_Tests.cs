using Custom_Text_Parser.Parsing;
using Custom_Text_Parser.Common;
using System.Collections.Generic;
using Custom_Text_Parser.Interfaces;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Custom_Text_Parser;

public class Parser_Tests
{
    [Fact]
    public void Parse_ShouldExtractData_WhenGivenValidInput()
    {
        // Arrange
        string input = "Account: 123456789\nDate: 2022-01-01";
        var template = Substitute.For<ITemplate>();
        template.TemplateText.Returns("Account: {{AccountRecipient}}\nDate: {{PostingDate}}");
        template.Placeholders.Returns(new List<string> { "AccountRecipient", "PostingDate" });
        template.OuterPlaceholders.Returns(new List<string> { "AccountRecipient", "PostingDate" });
        template.RecurringPlaceholders.Returns(new List<string>());

        var parser = new Parser();

        // Act
        var result = parser.Parse(input, template);

        // Assert
        result["AccountRecipient"].Should().ContainSingle().Which.Should().Be("123456789", because: "it should correctly extract the account number.");
        result["PostingDate"].Should().ContainSingle().Which.Should().Be("2022-01-01", because: "it should correctly extract the posting date.");
    }

    [Fact]
    public void Parse_ShouldIdentifyAndProcess_RecurringSections()
    {
        // Arrange
        var template = Substitute.For<ITemplate>();
        template.TemplateText.Returns("Transactions Begin{{RecurringStart}}Transaction: {{PostingKey}}{{RecurringEnd}}Transactions End");
        template.Placeholders.Returns(new List<string>() { "PostingKey" });
        template.OuterPlaceholders.Returns(new List<string>() { });
        template.RecurringPlaceholders.Returns(new List<string>() { "PostingKey" });
        template.RecurringTemplate.Returns("Transaction: {{PostingKey}}");

        var parser = new Parser();

        string input =
            """
            Transactions BeginTransaction: 123Transaction: 456Transaction: 789Transactions End
            """;

        var expected = new Dictionary<string, List<string>>
        {
            { "PostingKey", new List<string>() {"123", "456", "789"} }
        };

        // Act
        var result = parser.Parse(input, template);

        // Assert
        result.Should().BeEquivalentTo(
            expected,
            because: "Parser should correctly extract and organize data according to the template and handle recurring sections.");
    }

    [Fact]
    public void BuildRegexFromTemplate_ShouldCorrectlyConstructRegexPattern()
    {
        // Arrange
        string templateText = "Account: {{AccountRecipient}}\nDate: {{PostingDate}}";
        IList<string> placeholders = new List<string> { "AccountRecipient", "PostingDate" };
        string expectedRegex = "^Account: (?<AccountRecipient>.+?)\nDate: (?<PostingDate>.+?)$";
        Type type = typeof(Parser);
        MethodInfo method = type.GetMethod("BuildRegexFromTemplate", BindingFlags.NonPublic | BindingFlags.Static);

        method.Should().NotBeNull("because the method needs to be tested to ensure it constructs regex patterns correctly");

        // Act
        var result = method!.Invoke(null, new object[] { templateText, placeholders });

        // Assert
        result.Should().Be(expectedRegex, because: "the regex should correctly replace placeholders with capture groups and handle special characters like newline properly.");
    }
    [Fact]
    public void Parser_Should_ProperlyParse_mBank_MT940()
    {
        // Arrange
        string mbank_MT940_Template =
        """
        :20:{{Date}}
        :25:{{IBAN}}
        :28C:{{StatementNumber}}
        :60F:{{OpeningBalance}}
        {{RecurringStart}}:61:{{Transaction}}//{{BankData}}
        :86:{{TransactionDetails}}{{RecurringEnd}}
        :62F:{{EndingBalance}}
        :64:{{AvailableBalance}}
        """;
        string mbank_MT940 =
        """
        :20:ST050112CYC/1
        :25:PL06114010100000111111001001
        :28C:8/1
        :60F:C050112PLN60213,04
        :61:0710091009DN2,50NCHGNONREF//BR07282102000059
        824-OPŁ. ZA PRZEL. ELIXIR MT
        :86:824 OPŁATA ZA PRZELEW ELIXIR; TNR: 145271016138274.040001
        :61:0501120112DN449,77NTRFSP300//BR05012139000001
        944-PRZEL.KRAJ.WYCH.MT.ELX
        :86:944 CompanyNet Przelew krajowy; na rach.: 35109010560000000006093440; dla: PHU Test ul.Dolna
        1 00-950 Warszawa; tyt.: fv 100/2007; TNR: 145271016138277.020002
        :61:0501120112DN2,50NCHGNONREF//BR05012139000003
        824-OPŁ. ZA PRZEL. ELIXIR MT
        :86: 824 OPŁATA ZA PRZELEW ELIXIR; TNR: 145271016138247.000001
        :61:0501120112DN76,03NTRFUS1234//BR05012139000003
        945-PRZEL.KRAJ.WYCH.MT.ELX.ZUS
        :86: 945 CopmanyNet Przelew ZUS; na rach.: 83101010230000261395100000; rodzaj składki:
        Ubezpieczenie społeczne; deklaracja S 200708 01; NIP: 12345678901; ID uzup.: R 123456789; ref.
        Klienta: teścik; TNR: 145211001552633.000001
        :61:0501120112DN2,50NCHGNONREF//BR05012139000004
        824-OPŁ. ZA PRZEL. ELIXIR MT
        :86:824 OPŁ. ZA PRZEL. ELIXIR CompanyNet; TNR: 145271016138274.050001
        :61:0710091009DN4,03NTRFREFER //BR07282102000060
        945-PRZEL.KRAJ.WYCH.MT.ELX.ZUS
        :86:945 CompanyNet PRZELEW ZUS; NA RACH.: 73101010230000261395300000;
        RODZAJ SKŁADKI: FPIFGSP; DEKLARACJA: D 200708 01; NIP:
        5555555555; ID UZUP.: R 011834870; NR DEC/UM/TW.: ST 4.3 NUMER
        DC; REF. KLIENTA: REFER; TNR: 145271016138274.050002
        :62F:C050112PLN18667,79
        :64:C050112PLN18667,79
        """;
       // mbank_MT940 = mbank_MT940.Replace("\n", "");
       // mbank_MT940_Template = mbank_MT940_Template.Replace("\n", "");

        var MT940Template = new Template(mbank_MT940_Template);
        var parser = new Parser();

        // Act
        parser.Parse(mbank_MT940, MT940Template);

        // Assert
        // To be created
        "x".Should().Be("x");
    }

}
