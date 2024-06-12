using Custom_Text_Parser.Interfaces;

namespace Custom_Text_Parser.Common;

public class Placeholder : IPlaceholder
{
    public string Name { get; private set; }

    public int Length { get; private set; }

    public Placeholder(string name, int length = -1)
    {
        Name = name;
        Length = length;
    }

    public Placeholder(string value, string separator)
    {
        var values = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        if (values.Length > 2 && values.Length < 1)
            throw new ArgumentException("Unable to divide provided string");
        else if (values.Length == 1)
        {
            Name = values[0];
            Length = -1;
        }
        else 
        {
            Name = values[0];
            if (int.TryParse(values[1], out int parseResult))
            {
                Length = parseResult;
            }
            else
            {
                throw new InvalidCastException("Unable to cast provided data to integer");
            }
        }
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Placeholder);
    }
    
    private bool Equals(Placeholder other)
    {
        return other is not null && Name == other.Name && Length == other.Length;
    }

    public override int GetHashCode()
    {
        return (Name + Length.ToString()).GetHashCode();
    }
}
