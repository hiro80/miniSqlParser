
namespace MiniSqlParser
{
  public class JoinSource: Node, IFromSource
  {
    internal JoinSource(IFromSource left
                      , JoinOperator operater
                      , IFromSource right
                      , Predicate constraint
                      , UnqualifiedColumnNames usingConstraint
                      , Comments comments) {
      this.Comments = comments;
      this.Left = left;
      this.Operator = operater;
      this.Right = right;
      this.Constraint = constraint;
      this.UsingConstraint = usingConstraint;
    }

    private IFromSource _left;
    public IFromSource Left {
      get {
        return _left;
      }
      set {
        _left = value;
        this.SetParent(value);
      }
    }

    private JoinOperator _operator;
    public JoinOperator Operator {
      get {
        return _operator;
      }
      set {
        _operator = value;
        this.SetParent(value);
      }
    }

    private IFromSource _right;
    public IFromSource Right {
      get {
        return _right;
      }
      set {
        _right = value;
        this.SetParent(value);
      }
    }

    private Predicate _constraint;
    public Predicate Constraint {
      get {
        return _constraint;
      }
      set {
        _constraint = value;
        this.SetParent(value);
      }
    }

    private UnqualifiedColumnNames _usingConstraint;
    public UnqualifiedColumnNames UsingConstraint {
      get {
        return _usingConstraint;
      }
      set {
        _usingConstraint = value;
        this.SetParent(value);
      }
    }

    public bool HasConstraint {
      get {
        return _constraint != null;
      }
    }

    public bool HasUsingConstraint {
      get {
        return _usingConstraint != null && _usingConstraint.Count > 0;
      }
    }

    public FromSourceType Type {
      get {
        return FromSourceType.Join;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Left.Accept(visitor);
      this.Operator.Accept(visitor);
      this.Right.Accept(visitor);
      visitor.Visit(this);
      if(this.Constraint != null) {
        this.Constraint.Accept(visitor);
      } else if(this.HasUsingConstraint) {
        this.UsingConstraint.Accept(visitor);
      }
      visitor.VisitAfter(this);
    }
  }

}
