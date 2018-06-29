
namespace MiniSqlParser
{
  public enum ConflictType
  {
    None = 0,
    Rollback,
    Abort,
    Replace,
    Fail,
    Ignore
  }
}
