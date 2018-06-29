using System;
using System.Globalization;

namespace MiniSqlParser
{
  public class UNumericLiteral : Literal
  {
    public UNumericLiteral(string value) {
      this.Comments = new Comments(1);
      this.Value = value;
    }

    internal UNumericLiteral(string value, Comments comments) {
      this.Comments = comments;
      this.Value = value;
    }

    private readonly NumberStyles _numberStyles 
      = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;

    private readonly NumberStyles _hexNumberStyles
      = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;

    private Tuple<string, NumberStyles> RegulateNumberString() {
      string numberStr = this.Value;
      NumberStyles numberStyles;

      if(numberStr.StartsWith("0x")) {
        numberStr = numberStr.Substring(2, numberStr.Length - 2);
        numberStyles = _hexNumberStyles;
      } else {
        if(numberStr[0] == '.') {
          numberStr = "0" + numberStr;
        }
        if(numberStr.EndsWith("D", true, null) ||
           numberStr.EndsWith("F", true, null)) {
          numberStr = numberStr.TrimEnd('D', 'F', 'd', 'f');
        }
        numberStyles = _numberStyles;
      }

      return Tuple.Create(numberStr, numberStyles);
    }

    /// <summary>
    /// Int型数値に変換する、変換できない場合は-1を返す
    /// </summary>
    /// <returns></returns>
    public long TryParseToInt() {
      var n = this.RegulateNumberString();
      int i;
      if(int.TryParse(n.Item1, n.Item2, null, out i)) {
        return i;
      } else {
        return -1;
      }
    }

    /// <summary>
    /// Long型数値に変換する、変換できない場合は-1を返す
    /// </summary>
    /// <returns></returns>
    public long TryParseToLong() {
      var n = this.RegulateNumberString();
      long l;
      if(long.TryParse(n.Item1, n.Item2, null, out l)) {
        return l;
      } else {
        return -1L;
      }
    }

    /// <summary>
    /// Decimal型数値に変換する、変換できない場合は-1を返す
    /// </summary>
    /// <returns></returns>
    public decimal TryParseToDecimal() {
      var n = this.RegulateNumberString();
      decimal d;
      if(decimal.TryParse(n.Item1, n.Item2, null, out d)) {
        return d;
      } else {
        return -1M;
      }
    }

    public static bool operator ==(UNumericLiteral lLiteral, UNumericLiteral rLiteral) {
      return lLiteral.TryParseToDecimal() == rLiteral.TryParseToDecimal();
    }

    public static bool operator !=(UNumericLiteral lLiteral, UNumericLiteral rLiteral) {
      return !(lLiteral == rLiteral);
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }
  }
}
