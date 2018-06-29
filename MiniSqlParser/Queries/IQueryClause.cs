
namespace MiniSqlParser
{
  public interface IQueryClause: INode
  {
    QueryType Type { get; }
    bool IsSubQuery { get; }
    /// <summary>
    /// Queryのみを走査する
    /// </summary>
    /// <param name="visitor"></param>
    void AcceptOnMainQuery(IVisitor visitor);
  }
}
