
namespace MiniSqlParser
{
  public abstract class ResultColumn : Node
  {
    public abstract bool IsTableWildcard { get; }
    public abstract ResultColumn Clone();
  }
}
