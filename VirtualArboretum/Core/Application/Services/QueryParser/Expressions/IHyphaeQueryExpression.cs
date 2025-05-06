using System.Collections.Immutable;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Application.Services.QueryParser.Expressions;

public interface IHyphaeQueryExpression
{
    ImmutableList<Fingerprint> Interpret(MyceliumContext context);
}