namespace VirtualArboretum.Core.Application.Services.QueryParser;

public enum MyceliumQueryParserError
{
    EmptyQuery,
    MalformedQuery,
    InvalidTermFormat,
    EmptyOrGroup,
    EmptyAndGroup,
    SerializationError
}