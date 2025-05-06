using System.Collections.Immutable;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Application.Services.QueryParser.Expressions;

public class OrExpression : IHyphaeQueryExpression
{
    private readonly IHyphaeQueryExpression _left;
    private readonly IHyphaeQueryExpression _right;

    public OrExpression(IHyphaeQueryExpression left, IHyphaeQueryExpression right)
    {
        _left = left;
        _right = right;
    }

    public ImmutableList<Fingerprint> Interpret(MyceliumContext context)
    {
        var leftResults = _left.Interpret(context);
        var rightResults = _right.Interpret(context);

        return leftResults
            .Union(rightResults)
            .ToImmutableList();
    }
}