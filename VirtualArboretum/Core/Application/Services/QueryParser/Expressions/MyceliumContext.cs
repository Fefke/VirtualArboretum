using VirtualArboretum.Core.Domain.AggregateRoots;

namespace VirtualArboretum.Core.Application.Services.QueryParser.Expressions;

/// <summary>
/// Does loosely represent the context of a Mycelium query, <br/>
/// which is just the Mycelium itself as of now.
/// </summary>
public record MyceliumContext(
    Mycelium Mycelium
    );