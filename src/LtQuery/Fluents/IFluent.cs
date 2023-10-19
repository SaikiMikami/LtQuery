using LtQuery.Elements;

namespace LtQuery.Fluents;

public interface IFluent
{
    IValue ToImmutable();
}
