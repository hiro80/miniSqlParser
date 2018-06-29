using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MiniSqlParser
{
  public class CaseExpr : Expr
  {
    public Expr SearchExpr { get; set; }

    private List<Expr> _comparisons;
    public ReadOnlyCollection<Expr> Comparisons {
      get {
        return _comparisons.AsReadOnly();
      }
    }

    private List<Predicate> _conditions;
    public ReadOnlyCollection<Predicate> Conditions {
      get {
        return _conditions.AsReadOnly();
      }
    }

    private List<Expr> _results;
    public ReadOnlyCollection<Expr> Results {
      get {
        return _results.AsReadOnly();
      }
    }

    public Expr ElseResult { get; set; }
    public bool IsSimpleCase {
      get {
        return this.SearchExpr != null;
      }
    }

    //public Tuple<Node, Expr> this[int i] {
    //  get {
    //    if(this.IsSimpleCase) {
    //      return new Tuple<Node, Expr>(_comparisons[i], _results[i]);
    //    } else {
    //      return new Tuple<Node, Expr>(_conditions[i], _results[i]);
    //    }
    //  }
    //  set {
    //    if(this.IsSimpleCase) {
    //      if(value.Item1 is Expr) {
    //        _comparisons[i] = (Expr)value.Item1;
    //      } else {
    //        throw new InvalidASTStructureError("Type of placeholder value is mismatched.");
    //      }
    //    } else {
    //      if(value.Item1 is Predicate) {
    //        _conditions[i] = (Predicate)value.Item1;
    //      } else {
    //        throw new InvalidASTStructureError("Type of placeholder value is mismatched.");
    //      }
    //    }
    //    this.SetParent(value.Item1);
    //    _results[i] = value.Item2;
    //    this.SetParent(value.Item2);
    //  }
    //}

    public void SetBranch(int index, Expr comparison, Expr result) {
      if(!this.IsSimpleCase) {
        throw new InvalidASTStructureError("Can't set comparison to complex case statement.");
      }
      _comparisons[index] = comparison;
      this.SetParent(comparison);
      _results[index] = result;
      this.SetParent(result);
    }

    public void SetBranch(int index, Predicate condition, Expr result) {
      if(this.IsSimpleCase) {
        throw new InvalidASTStructureError("Can't set condition to simple case statement.");
      }
      _conditions[index] = condition;
      this.SetParent(condition);
      _results[index] = result;
      this.SetParent(result);
    }

    public CaseExpr(Expr searchExpr
                  , List<Expr> comparisons
                  , List<Expr> results
                  , Expr elseResult) {
      this.Comments = new Comments(results.Count * 2 
                                    + (elseResult == null ? 0 : 1) + 2);
      this.SearchExpr = searchExpr;
      _comparisons = comparisons;
      _results = results;
      this.ElseResult = elseResult;

      this.SetParent(searchExpr);
      foreach(var e in comparisons) {
        this.SetParent(e);
      }
      foreach(var e in results) {
        this.SetParent(e);
      }
      this.SetParent(elseResult);
    }

    public CaseExpr(List<Predicate> conditions
                  , List<Expr> results
                  , Expr elseResult) {
      this.Comments = new Comments(results.Count * 2 
                                     + (elseResult == null ? 0 : 1) + 2);
      _conditions = conditions;
      _results = results;
      this.ElseResult = elseResult;

      foreach(var e in conditions) {
        this.SetParent(e);
      }
      foreach(var e in results) {
        this.SetParent(e);
      }
      this.SetParent(elseResult);
    }

    internal CaseExpr(Expr searchExpr
                    , List<Expr> comparisons
                    , List<Expr> results
                    , Expr elseResult
                    , Comments comments) {
      this.Comments = comments;
      this.SearchExpr = searchExpr;
      _comparisons = comparisons;
      _results = results;
      this.ElseResult = elseResult;

      this.SetParent(searchExpr);
      foreach(var e in comparisons) {
        this.SetParent(e);
      }
      foreach(var e in results) {
        this.SetParent(e);
      }
      this.SetParent(elseResult);
    }

    internal CaseExpr(List<Predicate> conditions
                    , List<Expr> results
                    , Expr elseResult
                    , Comments comments) {
      this.Comments = comments;
      _conditions = conditions;
      _results = results;
      this.ElseResult = elseResult;

      foreach(var e in conditions) {
        this.SetParent(e);
      }
      foreach(var e in results) {
        this.SetParent(e);
      }
      this.SetParent(elseResult);
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      if(this.IsSimpleCase) {
        this.SearchExpr.Accept(visitor);
        for(var i = 0; i < _comparisons.Count; ++i) {
          visitor.VisitOnWhen(this, i);
          _comparisons[i].Accept(visitor);
          visitor.VisitOnThen(this, i);
          _results[i].Accept(visitor);
        }
        if(this.ElseResult != null) {
          visitor.VisitOnElse(this);
          this.ElseResult.Accept(visitor);
        }
      } else {
        for(var i = 0; i < _conditions.Count; ++i) {
          visitor.VisitOnWhen(this, i);
          _conditions[i].Accept(visitor);
          visitor.VisitOnThen(this, i);
          _results[i].Accept(visitor);
        }
        if(this.ElseResult != null) {
          visitor.VisitOnElse(this);
          this.ElseResult.Accept(visitor);
        }
      }
      visitor.VisitAfter(this);
    }

  }
}
