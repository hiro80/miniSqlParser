using System;

namespace MiniSqlParser
{
  [System.Diagnostics.DebuggerDisplay("Value: {Value}")]
  abstract public class Literal : Expr
  {
    virtual public string Value { get; protected set; }

    public override string ToString() {
      return this.Value;
    }

    public static bool operator ==(Literal lLiteral, Literal rLiteral) {
      var typeOflLiteral = lLiteral.GetType();
      var typeOfrLiteral = rLiteral.GetType();

      if(typeOflLiteral != typeOfrLiteral){
        return false;
      } else if(typeOflLiteral == typeof(StringLiteral)) {
        return (StringLiteral)lLiteral == (StringLiteral)rLiteral;
      } else if(typeOflLiteral == typeof(UNumericLiteral)) {
        return (UNumericLiteral)lLiteral == (UNumericLiteral)rLiteral;
      } else if(typeOflLiteral == typeof(NullLiteral)) {
        return (NullLiteral)lLiteral == (NullLiteral)rLiteral;
      } else if(typeOflLiteral == typeof(DateLiteral)) {
        return (DateLiteral)lLiteral == (DateLiteral)rLiteral;
      } else if(typeOflLiteral == typeof(TimeLiteral)) {
        return (TimeLiteral)lLiteral == (TimeLiteral)rLiteral;
      } else if(typeOflLiteral == typeof(TimeStampLiteral)) {
        return (TimeStampLiteral)lLiteral == (TimeStampLiteral)rLiteral;
      } else if(typeOflLiteral == typeof(IntervalLiteral)) {
        return (IntervalLiteral)lLiteral == (IntervalLiteral)rLiteral;
      } else if(typeOflLiteral == typeof(BlobLiteral)) {
        return (BlobLiteral)lLiteral == (BlobLiteral)rLiteral;
      } else {
        throw new ApplicationException("Undefined Literal Type is used");
      }
    }

    public static bool operator !=(Literal lLiteral, Literal rLiteral) {
      return !(lLiteral == rLiteral);
    }
  }
}
