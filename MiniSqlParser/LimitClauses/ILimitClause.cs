
namespace MiniSqlParser
{
  public interface ILimitClause : INode
  {
    LimitClauseType Type { get; }
  }
}
