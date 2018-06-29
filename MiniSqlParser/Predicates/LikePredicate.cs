
namespace MiniSqlParser
{
  public class LikePredicate : Predicate
  {
    public LikePredicate(Expr operand
                        , bool not
                        , LikeOperator op
                        , Expr pattern
                        , Expr escape) {
      this.Comments = new Comments((not ? 1 : 0) + (escape != null ? 1 : 0) + 1);
      this.Operand = operand;
      this.Not = not;
      this.Operator = op;
      this.Pattern = pattern;
      this.Escape = escape;
    }

    internal LikePredicate(Expr operand
                          , bool not
                          , LikeOperator op
                          , Expr pattern
                          , Expr escape
                          , Comments comments) {
      this.Comments = comments;
      this.Operand = operand;
      this.Not = not;
      this.Operator = op;
      this.Pattern = pattern;
      this.Escape = escape;
    }

    private Expr _operand;
    public Expr Operand {
      get {
        return _operand;
      }
      set {
        _operand = value;
        this.SetParent(value);
      }
    }

    public bool Not { get; set; }
    public LikeOperator Operator { get; set; }

    private Expr _pattern;
    public Expr Pattern {
      get {
        return _pattern;
      }
      set {
        _pattern = value;
        this.SetParent(value);
      }
    }

    private Expr _escape;
    public Expr Escape {
      get {
        return _escape;
      }
      set {
        _escape = value;
        this.SetParent(value);
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Operand.Accept(visitor);
      visitor.Visit(this, 0);
      this.Pattern.Accept(visitor);
      if(this.Escape != null) {
        visitor.VisitOnEscape(this, this.Not ? 2 : 1);
        this.Escape.Accept(visitor);
      }
      visitor.VisitAfter(this);
    }
  }
}
