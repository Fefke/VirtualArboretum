using System.Collections.Immutable;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Application.Services.QueryParser.Expressions;

public class TerminalHyphaeExpression : IHyphaeQueryExpression
{
    private readonly HyphaeStrain _strain;

    public TerminalHyphaeExpression(HyphaeStrain strain)
    {
        _strain = strain;
    }

    public ImmutableList<Fingerprint> Interpret(MyceliumContext context)
    {
        return context.Mycelium.GetMycorrhization(_strain);
    }
}