using Custom_Text_Parser.Parsing;
using Custom_Text_Parser.Common;
using System.Collections.Generic;
using Custom_Text_Parser.Interfaces;
using System.Reflection;
using System.Text.RegularExpressions;
using Custom_Text_Parser.Exceptions;

namespace Custom_Text_Parser;

public class Parser_Tests
{
    [Fact]
    public void Parser_ShouldExtractData_WhenGivenValidInput()
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
    public void Parser_ShouldIdentifyAndProcess_RecurringSections()
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
        string expectedRegex = "^Account: (?<AccountRecipient>.+?){{NEWLINE}}Date: (?<PostingDate>.+?)$";
        Type type = typeof(Parser);

        // BuildRegexFromTemplate is a private static method within 'Parser' class. We cannot access it directly, hence we use reflection here.

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        MethodInfo method = type.GetMethod("BuildRegexFromTemplate", BindingFlags.NonPublic | BindingFlags.Static);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

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
        {{AdditionalInformation}}
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

        var expected = new Dictionary<string, List<string>>
        {
            { "Date", new List<string> { "ST050112CYC/1" } },
            { "IBAN", new List<string> { "PL06114010100000111111001001" } },
            { "StatementNumber", new List<string> { "8/1" } },
            { "OpeningBalance", new List<string> { "C050112PLN60213,04" } },
            { "Transaction", new List<string> { "0710091009DN2,50NCHGNONREF", "0501120112DN449,77NTRFSP300", "0501120112DN2,50NCHGNONREF", "0501120112DN76,03NTRFUS1234", "0501120112DN2,50NCHGNONREF", "0710091009DN4,03NTRFREFER" } },
            { "BankData", new List<string> { "BR07282102000059", "BR05012139000001", "BR05012139000003", "BR05012139000003", "BR05012139000004", "BR07282102000060" } },
            { "AdditionalInformation", new List<string> { "824-OPŁ. ZA PRZEL. ELIXIR MT", "944-PRZEL.KRAJ.WYCH.MT.ELX", "824-OPŁ. ZA PRZEL. ELIXIR MT", "945-PRZEL.KRAJ.WYCH.MT.ELX.ZUS", "824-OPŁ. ZA PRZEL. ELIXIR MT", "945-PRZEL.KRAJ.WYCH.MT.ELX.ZUS" } },
            { "TransactionDetails", new List<string> 
                { 
                    "824 OPŁATA ZA PRZELEW ELIXIR; TNR: 145271016138274.040001", 
                    "944 CompanyNet Przelew krajowy; na rach.: 35109010560000000006093440; dla: PHU Test ul.Dolna\n1 00-950 Warszawa; tyt.: fv 100/2007; TNR: 145271016138277.020002",
                    "824 OPŁATA ZA PRZELEW ELIXIR; TNR: 145271016138247.000001",
                    "945 CopmanyNet Przelew ZUS; na rach.: 83101010230000261395100000; rodzaj składki:\nUbezpieczenie społeczne; deklaracja S 200708 01; NIP: 12345678901; ID uzup.: R 123456789; ref.\nKlienta: teścik; TNR: 145211001552633.000001",
                    "824 OPŁ. ZA PRZEL. ELIXIR CompanyNet; TNR: 145271016138274.050001",
                    "945 CompanyNet PRZELEW ZUS; NA RACH.: 73101010230000261395300000;\nRODZAJ SKŁADKI: FPIFGSP; DEKLARACJA: D 200708 01; NIP:\n5555555555; ID UZUP.: R 011834870; NR DEC/UM/TW.: ST 4.3 NUMER\nDC; REF. KLIENTA: REFER; TNR: 145271016138274.050002" 
                } 
            },
            { "EndingBalance", new List<string> { "C050112PLN18667,79" } },
            { "AvailableBalance", new List<string> { "C050112PLN18667,79" } }
        };

        var MT940Template = new Template(mbank_MT940_Template);
        var parser = new Parser();

        // Act
        var result = parser.Parse(mbank_MT940, MT940Template);

        // Assert
        result.Should().BeEquivalentTo(expected, because: "the parser should correctly parse the MT940 file according to template");
    }

    [Fact]
    public void Parser_Should_ProperlyParse_PKOPB_MT940()
    {
        //Arrange
        var templateText =
            """
            :20:{{TYPE}}
            :25:{{IBAN}}
            :28C:{{ORDER_NUMBER}}
            :60F:{{OPENING_BALANCE}}
            {{RecurringStart}}:61:{{OPERATION_DESCRIPTION}}//{{OPERATION_NUMBER}}
            {{OZSI_CODE}} {{OPERATION_TYPE}}
            :86:{{CONST_02000}}
            ~20{{TITLE_1}}
            ~21{{TITLE_2}}
            ~22{{TITLE_3}}
            ~23{{TITLE_4}}
            ~24{{TITLE_5}}
            ~25{{TITLE_6}}
            ~30{{CONTRACTOR_BANK_NUMBER}}
            ~31{{CONTRACTOR_ACCOUNT_NUMBER}}
            ~32{{CONTRACTOR_NAME_ADDRESS_1}}
            ~33{{CONTRACTOR_NAME_ADDRESS_2}}
            ~38{{CONTRACTOR_IBAN}}
            ~60{{DOCUMENT_DATE}}
            ~63{{SWRK}}{{RecurringEnd}}
            :62F:{{ENDING_BALANCE}}
            :64:{{CURRENT_BALANCE}}
            """;

        var mt940_text =
            """
            :20:MT940
            :25:/PL44102055610000380209739045
            :28C:1
            :60F:D210526PLN2671,79
            :61:2105260526D25,00N152NONREF//6460500500000513
            152 0
            :86:020~00152
            ~20PRZELEW SRODKÓW
            ~21˙
            ~22˙
            ~23˙
            ~24˙
            ~25˙
            ~3010205561
            ~319000361245650140
            ~32FSDFSFDSF
            ~33˙
            ~38PL50102055619000361245650240
            ~60˙
            ~63˙
            :61:2105260526D434,00N210NONREF//6460502100001611
            210 0
            :86:020~00210
            ~20P 85100158550 0 PI
            ~21T-23
            ~22˙
            ~23˙
            ~24˙
            ~25˙
            ~3010100071
            ~312223147244000000
            ~32DRUGI MAZOWIECKI URZˇD SKAR
            ~33BOWY WARSZAWA
            ~38PL32101000712223147254000000
            ~60˙
            ~63˙
            :61:2105260526D205,18N107NONREF//6463600500000059
            107 0
            :86:020~00107
            ~20PRZELEW SRODKÓW
            ~21˙
            ~22˙
            ~23˙
            ~24˙
            ~25˙
            ~30˙
            ~31˙
            ~32IRENA KOWALSKA
            ~33˙
            ~38FR7630004013280001089882824
            ~60˙
            ~63˙
            :61:2105260526D0,75N108NONREF//6463600500000060
            108 0
            :86:020~00108
            ~20KOSZTY SR21IP00012613DS
            ~21˙
            ~22˙
            ~23˙
            ~24˙
            ~25˙
            ~30˙
            ~31˙
            ~32IRENA KOWALSKA
            ~33˙
            ~38FR7630004013280001089882824
            ~60˙
            ~63˙
            :62F:D210531PLN3336,72
            :64:C210531PLN91662,28
            """;

        var MT940Template = new Template(templateText);
        var parser = new Parser();

        var expected = new Dictionary<string, List<string>>
        {
            { "TYPE", new List<string> { "MT940" } },
            { "IBAN", new List<string> { "/PL44102055610000380209739045" } },
            { "ORDER_NUMBER", new List<string> { "1" } },
            { "OPENING_BALANCE", new List<string> { "D210526PLN2671,79" } },
            { "OPERATION_DESCRIPTION", new List<string> { "2105260526D25,00N152NONREF", "2105260526D434,00N210NONREF", "2105260526D205,18N107NONREF", "2105260526D0,75N108NONREF" } },
            { "OPERATION_NUMBER", new List<string> { "6460500500000513", "6460502100001611", "6463600500000059", "6463600500000060" } },
            { "OZSI_CODE", new List<string> { "152", "210", "107", "108" } },
            { "OPERATION_TYPE", new List<string> { "0", "0", "0", "0" } },
            { "CONST_02000", new List<string> { "020~00152", "020~00210", "020~00107", "020~00108" } },
            { "TITLE_1", new List<string> { "PRZELEW SRODKÓW", "P 85100158550 0 PI", "PRZELEW SRODKÓW", "KOSZTY SR21IP00012613DS" } },
            { "TITLE_2", new List<string> { "˙", "T-23", "˙", "˙" } },
            { "TITLE_3", new List<string> { "˙", "˙", "˙", "˙" } },
            { "TITLE_4", new List<string> { "˙", "˙", "˙", "˙" } },
            { "TITLE_5", new List<string> { "˙", "˙", "˙", "˙" } },
            { "TITLE_6", new List<string> { "˙", "˙", "˙", "˙" } },
            { "CONTRACTOR_BANK_NUMBER", new List<string> { "10205561", "10100071", "˙", "˙" } },
            { "CONTRACTOR_ACCOUNT_NUMBER", new List<string> { "9000361245650140", "2223147244000000", "˙", "˙" } },
            { "CONTRACTOR_NAME_ADDRESS_1", new List<string> { "FSDFSFDSF", "DRUGI MAZOWIECKI URZˇD SKAR", "IRENA KOWALSKA", "IRENA KOWALSKA" } },
            { "CONTRACTOR_NAME_ADDRESS_2", new List<string> { "˙", "BOWY WARSZAWA", "˙", "˙" } },
            { "CONTRACTOR_IBAN", new List<string> { "PL50102055619000361245650240", "PL32101000712223147254000000", "FR7630004013280001089882824", "FR7630004013280001089882824" } },
            { "DOCUMENT_DATE", new List<string> { "˙", "˙", "˙", "˙" } },
            { "SWRK", new List<string> { "˙", "˙", "˙", "˙" } },
            { "ENDING_BALANCE", new List<string> { "D210531PLN3336,72" } },
            { "CURRENT_BALANCE", new List<string> { "C210531PLN91662,28" } }
        };

        // Act
        var result = parser.Parse(mt940_text, MT940Template);

        // Assert
        result.Should().BeEquivalentTo(expected, because: "the paraser should properly parse PKOBP MT940 file.");

    }
    [Fact]
    public void Parser_ShouldHandleEmptyContent()
    {
        // Arrange
        var template = Substitute.For<ITemplate>();
        template.TemplateText.Returns("Account: {{AccountRecipient}}\nDate: {{PostingDate}}");
        template.Placeholders.Returns(new List<string>() { "AccountRecipient", "PostingDate" });
        var parser = new Parser();

        // Act
        var result = parser.Parse("", template);

        // Assert
        result.Should().BeEmpty(because: "Parsing empty content should return an empty result");
    }

    [Fact]
    public void Parser_ShoundThrowParsingException_WhenPlaceholdersDoNotMatch()
    {
        // Arrange
        string input = "Mismatched input data";
        var template = Substitute.For<ITemplate>();

        template.TemplateText.Returns(":Test:{{MismatchedPlaceholder}}");
        template.Placeholders.Returns(new List<string>() { "MismatchedPlaceholder" });

        var parser = new Parser();

        // Act
        Action action = () => parser.Parse(input, template);

        // Assert
        action.Should().Throw<ParsingException>(because: "The input data does not match the template");
    }
}
