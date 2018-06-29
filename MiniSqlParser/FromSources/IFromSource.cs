
namespace MiniSqlParser
{
  public interface IFromSource : INode
  {
    FromSourceType Type { get; }
  }
}
