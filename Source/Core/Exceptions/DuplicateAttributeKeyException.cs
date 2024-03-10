namespace Sanguosha.Core.Exceptions;

public class DuplicateAttributeKeyException : SgsException
{
    public DuplicateAttributeKeyException() { }

    public DuplicateAttributeKeyException(string name)
    {
        AttributeName = name;
    }

    public string AttributeName { get; set; }
}
