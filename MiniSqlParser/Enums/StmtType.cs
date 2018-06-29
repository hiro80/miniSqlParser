
namespace MiniSqlParser
{
  public enum StmtType
  {
    Unkown = 0,
    Null,  // For NullStmt
    Select,
    Update,
    InsertValue,
    InsertSelect,
    Delete,
    Merge,
    Call,
    Truncate,
    If,
    SqlitePragma
  }
}
