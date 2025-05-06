using System.Collections.Immutable;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Application.Services.QueryParser.Expressions;

public class AndExpression : IHyphaeQueryExpression
{
    private readonly IHyphaeQueryExpression _left;
    private readonly IHyphaeQueryExpression _right;

    public AndExpression(IHyphaeQueryExpression left, IHyphaeQueryExpression right)
    {
        _left = left;
        _right = right;
    }

    public ImmutableList<Fingerprint> Interpret(MyceliumContext context)
    {
        var leftResults = _left
            .Interpret(context)
            .ToHashSet(); // for efficient intersection.
        
        if (!leftResults.Any()) {
            // fast return
            return ImmutableList<Fingerprint>.Empty; 
        }

        var rightResults = _right.Interpret(context);
        return rightResults
            .Where(leftResults.Contains)
            .ToImmutableList();
    }
}