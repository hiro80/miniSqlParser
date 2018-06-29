
namespace MiniSqlParser
{
  public abstract class Expr : Node, IValue
  {
    //public abstract Expr Clone();
    //virtual public Expr Clone() { return this; }
    virtual public IValue Clone() { return this; }

    public bool IsDefault {
      get {
        return false;
      }
    }
  }
}
