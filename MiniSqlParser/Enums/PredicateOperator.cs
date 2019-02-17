
namespace MiniSqlParser
{
  public enum PredicateOperator
  {
      Less
    , LessOrEqual
    , Greater
    , GreaterOrEqual
    , Equal
    , Equal2
    , NotEqual
    , NotEqual2
    , ContainsJsonValueL  // @>
    , ContainsJsonValueR  // <@
    , ExistsJsonValue1    // ?
    , ExistsJsonValue2    // ?|
    , ExistsJsonValue3    // ?&
  }
}
