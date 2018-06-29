
namespace MiniSqlParser
{
  public interface IValue : INode
  {
    IValue Clone();
    bool IsDefault { get; }
  }
}
