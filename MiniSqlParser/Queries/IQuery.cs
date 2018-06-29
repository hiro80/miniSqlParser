
namespace MiniSqlParser
{
  public interface IQuery : INode
  {
    QueryType Type { get; }
    OrderBy OrderBy { get; }
    ILimitClause Limit { get; set; }
    bool HasOrderBy { get; }
    bool HasLimit { get; }
    /// <summary>
    /// SelectStmt.Queryに格納されている場合はfalse、それ以外はtrue
    /// </summary>
    bool IsSubQuery { get; }
    /// <summary>
    /// Queryのみを走査する
    /// </summary>
    /// <param name="visitor"></param>
    void AcceptOnMainQuery(IVisitor visitor);
  }
}
