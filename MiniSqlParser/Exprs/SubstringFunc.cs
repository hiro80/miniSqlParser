
namespace MiniSqlParser
{
  public class SubstringFunc : Expr
  {
    public string Name { get; set; }

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

    private Expr _argument3;
    public Expr Argument3 {
      get {
        return _argument3;
      }
      set {
        _argument3 = value;
        this.SetParent(value);
      }
    }

    public bool Separator1IsComma { get; set; }
    public bool Separator2IsComma { get; set; }

    public SubstringFunc(string name
                        , Expr argument1
                        , Expr argument2
                        , Expr argument3
                        , bool separator1IsComma
                        , bool separator2IsComma) {
      this.Comments = new Comments(argument3 == null ? 4 : 5);
      this.Name = name;
      this.Argument1 = argument1;
      this.Argument2 = argument2;
      this.Argument3 = argument3;
      this.Separator1IsComma = separator1IsComma;
      this.Separator2IsComma = separator2IsComma;
    }

    internal SubstringFunc(string name
                         , Expr argument1
                         , Expr argument2
                         , Expr argument3
                         , bool separator1IsComma
                         , bool separator2IsComma
                         , Comments comments) {
      this.Comments = comments;
      this.Name = name;
      this.Argument1 = argument1;
      this.Argument2 = argument2;
      this.Argument3 = argument3;
      this.Separator1IsComma = separator1IsComma;
      this.Separator2IsComma = separator2IsComma;
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);

      int offset = 2;
      if(this.Argument1 != null) {
        this.Argument1.Accept(visitor);
      }
      if(this.Argument2 != null) {
        visitor.VisitOnSeparator(this, offset, 0);
        this.Argument2.Accept(visitor);
      }
      if(this.Argument3 != null) {
        visitor.VisitOnSeparator(this, offset, 1);
        this.Argument3.Accept(visitor);
      }

      visitor.VisitAfter(this);
    }
  }
}
