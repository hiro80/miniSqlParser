
namespace MiniSqlParser
{
  public class AggregateFuncExpr : Expr
  {
    public AggregateFuncExpr(string name
                            , QuantifierType quantifier
                            , bool wildcard
                            , Expr argument1
                            , Expr argument2) {
      this.Name = name;
      this.Quantifier = quantifier;
      this.Wildcard = wildcard;
      this.Argument1 = argument1;
      this.Argument2 = argument2;

      var c = CountTrue(this.Quantifier != QuantifierType.None
                      , this.Wildcard
                      , this.Argument2 != null // Commaの有無
                        );
      this.Comments = new Comments(c + 3);
    }

    internal AggregateFuncExpr(string name
                              , QuantifierType quantifier
                              , bool wildcard
                              , Expr argument1
                              , Expr argument2
                              , Comments comments) {
      this.Comments = comments;
      this.Name = name;
      this.Quantifier = quantifier;
      this.Wildcard = wildcard;
      this.Argument1 = argument1;
      this.Argument2 = argument2;
    }

    public string Name { get; private set; }
    public QuantifierType Quantifier { get; private set; }
    public bool Wildcard { get; private set; }

    private Expr _argument1;
    public Expr Argument1 {
      get {
        return _argument1;
      }
      set {
        _argument1 = value;
        this.SetParent(value);
      }
    }

    private Expr _argument2;
    public Expr Argument2 {
      get {
        return _argument2;
      }
      set {
        _argument2 = value;
        this.SetParent(value);
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      int offset = this.Quantifier != QuantifierType.None ? 3 : 2;
      if(this.Wildcard) {
        visitor.VisitOnWildCard(this, offset);
        offset += 1;
      } else {
        if(this.Argument1 != null) {
          this.Argument1.Accept(visitor);
        }
        if(this.Argument2 != null) {
          visitor.VisitOnSeparator(this, offset, 0);
          offset += 1;
          this.Argument2.Accept(visitor);
        }
      }
      visitor.VisitAfter(this);
    }
  }
}
