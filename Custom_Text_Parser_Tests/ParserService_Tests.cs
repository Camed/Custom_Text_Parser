using Custom_Text_Parser.Interfaces;

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