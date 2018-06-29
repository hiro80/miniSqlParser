
namespace MiniSqlParser
{
  public class ExtractFuncExpr : Expr
  {
    public ExtractFuncExpr(string name
                          , DateTimeField dateTimeField
                          , bool separatorIsComma
                          , Expr argument) {
      this.Comments = new Comments(5);
      this.Name = name;
      this.DateTimeField = dateTimeField;
      this.SeparatorIsComma = separatorIsComma;
      this.Argument = argument;
    }

    internal ExtractFuncExpr(string name
                            , DateTimeField dateTimeField
                            , bool separatorIsComma
                            , Expr argument
                            , Comments comments) {
      this.Comments = comments;
      this.Name = name;
      this.DateTimeField = dateTimeField;
      this.SeparatorIsComma = separatorIsComma;
      this.Argument = argument;
    }

    public string Name { get; set; }
    public DateTimeField DateTimeField { get; set; }
    public bool SeparatorIsComma { get; set; }

    private Expr _argument;
    public Expr Argument {
      get {
        return _argument;
      }
      set {
        _argument = value;
        this.SetParent(value);
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      visitor.VisitOnSeparator(this, 3);
      this.Argument.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
