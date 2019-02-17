
namespace MiniSqlParser
{
  public enum ExpOperator
  {
     StringConcat
    ,Mult
    ,Div
    ,Mod
    ,Add
    ,Sub
    ,LeftBitShift
    ,RightBitShift
    ,BitAnd
    ,BitOr
    ,GetJsonObj        // ->
    ,GetJsonObjAsText  // ->>
    ,GetJsonPath       // #>
    ,GetJsonPathAsText // #>>
    ,DelJsonObj        // #-
  }
}
