using Custom_Text_Parser.Exceptions;
using Custom_Text_Parser.Interfaces;
using Custom_Text_Parser.Services;

namespace Custom_Text_Parser;

public class ParserService_Tests
{
    [Fact]
    public void ParseContent_ShouldReturnExpectedResult_WhenCalledWithValidInput()
    {
        // Arrange
        var mockParserService = Substitute.For<IParserService>();
        var expectedResults = new Dictionary<string, List<string>>
        {
            { "AccountRecipient", new List<string> { "1234567890" } }
        };

        mockParserService.ParseContent(Arg.Any<string>(), "DefaultTemplate").Returns(expectedResults);
        var customer = new CustomerClass(mockParserService);

        // Act
        var results = customer.Parse("test content 21/05/2024", "DefaultTemplate");

        // Assert
        results.Should().BeEquivalentTo(expectedResults,
            options => options.WithStrictOrdering(),
            because: "The parser service should return correct data as per given template.");

        mockParserService.Received(1).ParseContent("test content 21/05/2024", "DefaultTemplate");
    }

    [Fact]
    public void ParseContent_ShouldThrowArgumentException_WhenTemplateNotFound()
    {
        // Arrange
        var parser = Substitute.For<IParser>();
        var templates = new Dictionary<string, ITemplate>();
        var parserService = new ParserService(parser, templates);

        // Act
        Action action = () => parserService.ParseContent("content", "example invalid name");

        // Assert
        action.Should().Throw<ArgumentException>(because: "The parser service should throw ArgumentException when requested template does not exist.");
    }

    [Fact]
    public void ParseContent_ShouldThrowInvalidOperationException_WhenParsingFails()
    {
        // Arrange
        var parser = Substitute.For<IParser>();
        var templates = new Dictionary<string, ITemplate>
        {
            { "ValidTemplate", Substitute.For<ITemplate>() },
        };
        var parserService = new ParserService(parser, templates);

        parser.When(x => x.Parse(Arg.Any<string>(), Arg.Any<ITemplate>()))
              .Do(x => { throw new ParsingException(); });

        // Act
        Action action = () => parserService.ParseContent("InvalidContent", "ValidTemplate");

        // Assert
        action.Should().Throw<InvalidOperationException>(because: "Parsing invlaid content should throw InvalidOperationException");
    }
}
internal class CustomerClass
{
    private readonly IParserService _parserService;
    public CustomerClass(IParserService parserService)
    {
        _parserService = parserService;
    }

    public Dictionary<string, List<string>> Parse(string content, string templateName)
    {
        return _parserService.ParseContent(content, templateName);
    }
}