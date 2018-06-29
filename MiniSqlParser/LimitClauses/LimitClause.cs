
namespace MiniSqlParser
{
  public class LimitClause : Node, ILimitClause
  {
    public LimitClause(Expr offset
                      , Expr limit
                      , bool offsetSeparatorIsComma = true) {
      this.Comments = new Comments(2);
      this.Offset = offset;
      this.OffsetSeparatorIsComma = offsetSeparatorIsComma;
      this.Limit = limit;
    }

    internal LimitClause(Expr offset
                       , Expr limit
                       , bool offsetSeparatorIsComma
                       , Comments comments) {
      this.Comments = comments;
      _offset = offset;
      this.OffsetSeparatorIsComma = offsetSeparatorIsComma;
      _limit = limit;

      this.SetParent(_offset);
      this.SetParent(_limit);
    }

    public LimitClauseType Type {
      get {
        return LimitClauseType.Limit;
      }
    }

    private Expr _offset;
    public Expr Offset {
      get {
        return _offset;
      }
      set {
        if(this.OffsetSeparatorIsComma) {
          this.CorrectComments(_offset, value, 1);
        } else {
          this.CorrectComments(_offset, value, 2);
        }
        _offset = value;
        this.SetParent(value);
      }
    }

    private Expr _limit = null;
    public Expr Limit {
      get {
        return _limit;
      }
      set {
        if(this.OffsetSeparatorIsComma) {
          this.CorrectComments(_limit, value, 1, this.HasOffset);
        } else {
          this.CorrectComments(_limit, value, 1);
        }
        _limit = value;
        this.SetParent(value);
      }
    }

    public bool OffsetSeparatorIsComma { get; private set; }

    public bool HasOffset {
      get {
        return _offset != null;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);

      if(this.OffsetSeparatorIsComma) {
        this.Offset.Accept(visitor);
        visitor.VisitOnSeparator(this, 1, 0);
        this.Limit.Accept(visitor);
      } else {
        this.Limit.Accept(visitor);
        if(this.HasOffset) {
          visitor.VisitOnOffset(this, 1);
          this.Offset.Accept(visitor);
        }
      }
      
      visitor.VisitAfter(this);
    }

  }
}
