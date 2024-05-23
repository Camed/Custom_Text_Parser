namespace Custom_Text_Parser.Exceptions;

[Serializable]
public class ParsingException : Exception
{
    public ParsingException() { }
    public ParsingException(string content, string regex) : base($"Could not parse string: \"{content}\" with regular exception \"{regex}\"") { }
}
