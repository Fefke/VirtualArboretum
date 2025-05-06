using System.Collections.Immutable;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Application.Services.QueryParser.Expressions;

public class NotExpression : IHyphaeQueryExpression
{
    private readonly IHyphaeQueryExpression _expression;

    public NotExpression(IHyphaeQueryExpression expression)
    {
        _expression = expression;
    }

    public ImmutableList<Fingerprint> Interpret(MyceliumContext context)
    {
        var resultsToExclude = _expression
            .Interpret(context)
            .ToHashSet();

        // Get all unique plant fingerprints known to the mycelium
        var allPlantFingerprints = context.Mycelium.GetAllMycorrhizations()
            .SelectMany(kvp => kvp.Value)
            .Distinct();

        return allPlantFingerprints
            .Where(fingerprint => !resultsToExclude.Contains(fingerprint))
            .ToImmutableList();
    }
}