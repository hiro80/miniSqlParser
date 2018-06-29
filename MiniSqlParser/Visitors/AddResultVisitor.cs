
namespace MiniSqlParser
{
  /// <summary>
  /// SELECT句を追加する、
  /// UNION文であればUNIONの被演算子のSELECT文にSELECT句を追加する
  /// </summary>
  public class AddResultVisitor: InsertResultVisitor
  {
    public AddResultVisitor(ResultColumn result): base(-1, result){
    }

    public AddResultVisitor(string exprStr
                          , Identifier aliasName
                          , DBMSType dbmsType = DBMSType.Unknown
                          , bool forSqlAccessor = false)
      : base(-1, exprStr, aliasName, dbmsType, forSqlAccessor){
    }
  }
}
