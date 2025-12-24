using System;

namespace TestTemplate17.Application.Sorting;

public class InvalidPropertyMappingException : Exception
{
    public InvalidPropertyMappingException(string message)
        : base(message)
    {
    }
}
