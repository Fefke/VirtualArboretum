using System.Collections.Immutable;
using System.Text.RegularExpressions;
using VirtualArboretum.Core.Application.DataTransferObjects;
using VirtualArboretum.Core.Application.Services.QueryParser.Expressions;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.ValueObjects;

using static VirtualArboretum.Core.Application.DataTransferObjects.ResultFactory;

namespace VirtualArboretum.Core.Application.Services.QueryParser;


public class HyphaeQueryParser
{
    private const char OrMarker = '|';
    private const char NotMarker = '!';
    private static readonly char HyphaeStartMarker = HyphaKey.StartMarker;

    /// <summary>
    /// Does resolve a hyphae query string into a valid tree of expressions or presents Failure-Results.
    /// </summary>
    public static Result<IHyphaeQueryExpression, MyceliumQueryParserError> Parse(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Fail<IHyphaeQueryExpression, MyceliumQueryParserError>(
                MyceliumQueryParserError.EmptyQuery, "Query cannot be empty.");
        }

        var orGroup = query.Split(OrMarker, StringSplitOptions.TrimEntries);

        if (orGroup.All(string.IsNullOrWhiteSpace))
        {
            return Fail<IHyphaeQueryExpression, MyceliumQueryParserError>(
                MyceliumQueryParserError.MalformedQuery,
                "Query contains no valid OR groups. All of them seem to be empty.");
        }

        IHyphaeQueryExpression? finalExpression = null;

        foreach (var orPart in orGroup.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            var andGroupResult = ParseAndGroup(orPart);
            if (!andGroupResult.IsSuccess)
            {
                return Fail<IHyphaeQueryExpression, MyceliumQueryParserError>(
                    andGroupResult.Error.Code, andGroupResult.Error.Message);
            }

            finalExpression = finalExpression == null
                ? andGroupResult.Value
                : new OrExpression(finalExpression, andGroupResult.Value);
        }

        return finalExpression == null
            ? Fail<IHyphaeQueryExpression, MyceliumQueryParserError>(
                MyceliumQueryParserError.MalformedQuery,
                "Failed to construct any expression from query."
                )
            : Ok<IHyphaeQueryExpression, MyceliumQueryParserError>(finalExpression);
    }


    private record TermToken(string Term, bool IsNegated);

    private static Result<IHyphaeQueryExpression, MyceliumQueryParserError> ParseAndGroup(string andGroupString)
    {
        if (string.IsNullOrWhiteSpace(andGroupString))
        {
            return Fail<IHyphaeQueryExpression, MyceliumQueryParserError>(
                MyceliumQueryParserError.EmptyAndGroup, "AND group cannot be empty.");
        }

        var termTokens = TokenizeAndGroup(andGroupString);
        if (!termTokens.Any())
        {
            return Fail<IHyphaeQueryExpression, MyceliumQueryParserError>(
                MyceliumQueryParserError.MalformedQuery,
                $"AND group '{andGroupString}' contains no valid terms.");
        }

        IHyphaeQueryExpression? currentAndExpression = null;

        foreach (var token in termTokens)
        {
            ImmutableList<HyphaeStrain> strains;
            try
            {
                strains = HyphaeSerializationService.Deserialize(token.Term);
                if (strains.Count != 1)
                {
                    return Fail<IHyphaeQueryExpression, MyceliumQueryParserError>(
                        MyceliumQueryParserError.InvalidTermFormat,
                        $"Term '{token.Term}' must resolve to a single HyphaeStrain.");
                }
            }
            catch (ArgumentException ex)
            {
                return Fail<IHyphaeQueryExpression, MyceliumQueryParserError>(
                    MyceliumQueryParserError.SerializationError,
                    $"Error serializing term '{token.Term}': {ex.Message}");
                //    => as all ArgumentExceptions thrown by HyphaeSerializationService are User-safe.
            }

            IHyphaeQueryExpression termExpression = new TerminalHyphaeExpression(strains.First());

            if (token.IsNegated)
            {
                termExpression = new NotExpression(termExpression);
            }

            currentAndExpression = currentAndExpression == null
                ? termExpression
                : new AndExpression(currentAndExpression, termExpression);
        }

        // currentAndExpression should not be null if termTokens had items and succeeded.
        return Ok<IHyphaeQueryExpression, MyceliumQueryParserError>(currentAndExpression!);
    }

    private static List<TermToken> TokenizeAndGroup(string groupString)
    {
        var tokens = new List<TermToken>();
        // Regex to find terms: optional '!' then '#' then non-'!'/'#'/'whitespace' chars,
        // then optionally more such segments separated by internal hyphens (part of HyphaeKey)
        // This regex assumes that hyphae strains themselves do not contain spaces!
        var regex = new Regex(
            $@"({NotMarker})?({HyphaeStartMarker}[^{NotMarker}{HyphaeStartMarker}\s{OrMarker}{NotMarker}]+(?:-[^{NotMarker}{HyphaeStartMarker}\s{OrMarker}{NotMarker}]+)*)");

        var matches = regex.Matches(groupString);

        foreach (Match match in matches)
        {
            // Was NotMarker group captured?
            var isNegated = match.Groups[1].Success;
            var term = match.Groups[2].Value;
            tokens.Add(new TermToken(term, isNegated));
        }
        return tokens;
    }

}