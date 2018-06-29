using System;
using System.Collections.Generic;

namespace MiniSqlParser
{
  /// <summary>
  /// UPDATE文とINSERT文での代入関係を表す
  /// </summary>
  public class Assignment : Node
  {
    public Assignment(Column column, IValue value) {
      this.Comments = new Comments(1);
      this.Column = column;
      this.Value = value;
    }

    internal Assignment(Column column, IValue value, Comments comments) {
      this.Comments = comments;
      this.Column = column;
      this.Value = value;
    }

    private Column _column;
    public Column Column {
      get {
        return _column;
      }
      set {
        _column = value;
        this.SetParent(value);
      }
    }

    private IValue _value;
    public IValue Value {
      get {
        return _value;
      }
      set {
        _value = value;
        this.SetParent(value);
      }
    }

    override protected void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Column.Accept(visitor);
      visitor.Visit(this);
      this.Value.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
