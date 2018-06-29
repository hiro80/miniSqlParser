using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class ClonePlaceHoldersVisitor: Visitor
  {
    private Stack<INode> _stack = new Stack<INode>();


    // Identifiers
    public override void Visit(Column column) {
      _stack.Push(column);
    }

    // Literals
    public override void Visit(StringLiteral literal) {
      _stack.Push(literal);
    }
    public override void Visit(UNumericLiteral literal) {
      _stack.Push(literal);
    }
    public override void Visit(NullLiteral literal) {
      _stack.Push(literal);
    }
    public override void Visit(DateLiteral literal) {
      _stack.Push(literal);
    }
    public override void Visit(TimeLiteral literal) {
      _stack.Push(literal);
    }
    public override void Visit(TimeStampLiteral literal) {
      _stack.Push(literal);
    }
    public override void Visit(IntervalLiteral literal) {
      _stack.Push(literal);
    }
    public override void Visit(BlobLiteral literal) {
      _stack.Push(literal);
    }

    // Expressions
    public override void VisitAfter(SignedNumberExpr expr) { 
      var operand = (UNumericLiteral)_stack.Pop();
      _stack.Push(expr);
    }

     public override void Visit(PlaceHolderExpr expr) {
      var node = new PlaceHolderExpr(expr.Label, expr.Comments.Clone());
      _stack.Push(node);
    }

     public override void VisitAfter(BitwiseNotExpr expr) {
      var operand = (Expr)_stack.Pop();

      if(object.ReferenceEquals(expr.Operand, operand)) {
        _stack.Push(expr);
        return;
      }

      var node = new BitwiseNotExpr(operand, expr.Comments.Clone());
      _stack.Push(node);
    }

     public override void VisitAfter(BinaryOpExpr expr) {
      var right = (Expr)_stack.Pop();
      var left = (Expr)_stack.Pop();

      if(object.ReferenceEquals(expr.Right, right) &&
         object.ReferenceEquals(expr.Left, left)) {
        _stack.Push(expr);
        return;
      }

      var node = new BinaryOpExpr(left, expr.Operator, right, expr.Comments.Clone());
      _stack.Push(node);
    }

    public override void VisitAfter(SubstringFunc expr) {
       Expr arg3 = null;
       if(expr.Argument3 != null) {
         arg3 = (Expr)_stack.Pop();
       }
       Expr arg2 = (Expr)_stack.Pop();
       Expr arg1 = (Expr)_stack.Pop();

       if(object.ReferenceEquals(expr.Argument1, arg1) &&
          object.ReferenceEquals(expr.Argument2, arg2) &&
          object.ReferenceEquals(expr.Argument3, arg3)) {
         _stack.Push(expr);
         return;
       }

      var node = new SubstringFunc(expr.Name
                                  , arg1, arg2, arg3
                                  , expr.Separator1IsComma
                                  , expr.Separator2IsComma
                                  , expr.Comments.Clone());
      _stack.Push(node);
    }
    public override void VisitAfter(ExtractFuncExpr expr) {
      Expr arg = (Expr)_stack.Pop();

      if(object.ReferenceEquals(expr.Argument, arg)) {
        _stack.Push(expr);
        return;
      }

      var node = new ExtractFuncExpr(expr.Name
                                    , expr.DateTimeField
                                    , expr.SeparatorIsComma, arg);
      _stack.Push(node);
    }
    public override void VisitAfter(AggregateFuncExpr expr) {
      Expr arg2 = null;
      if(expr.Argument2 != null) {
        arg2 = (Expr)_stack.Pop();
      }
      Expr arg1 = null;
      if(expr.Argument1 != null) {
        arg1 = (Expr)_stack.Pop();
      }

      if(object.ReferenceEquals(expr.Argument1, arg1) &&
         object.ReferenceEquals(expr.Argument2, arg2)) {
        _stack.Push(expr);
        return;
      }

      var node = new AggregateFuncExpr(expr.Name
                                      , expr.Quantifier
                                      , expr.Wildcard
                                      , arg1, arg2
                                      , expr.Comments.Clone());
      _stack.Push(node);
    }
    public override void VisitAfter(WindowFuncExpr expr) {
      OrderBy orderBy = (OrderBy)_stack.Pop();
      PartitionBy partitionBy = null;
      if(expr.PartitionBy != null) {
        partitionBy = (PartitionBy)_stack.Pop();
      }
      Exprs arg = null;
      if(expr.Arguments != null) {
        arg = (Exprs)_stack.Pop();
      }

      if(object.ReferenceEquals(expr.Arguments, arg) &&
         object.ReferenceEquals(expr.PartitionBy, partitionBy) &&
         object.ReferenceEquals(expr.OrderBy, orderBy)) {
        _stack.Push(expr);
        return;
      }

      var node = new WindowFuncExpr(expr.ServerName
                                  , expr.DataBaseName
                                  , expr.SchemaName
                                  , expr.Name
                                  , expr.Quantifier
                                  , expr.HasWildcard
                                  , arg
                                  , partitionBy
                                  , orderBy
                                  , expr.Comments.Clone());
      _stack.Push(node);
    }
    public override void VisitAfter(FuncExpr expr) {
      Exprs args = null;
      if(expr.Arguments != null) {
        args = (Exprs)_stack.Pop();
      }

      if(object.ReferenceEquals(expr.Arguments, args)) {
        _stack.Push(expr);
        return;
      }

      var node = new FuncExpr(expr.ServerName
                            , expr.DataBaseName
                            , expr.SchemaName
                            , expr.Name
                            , args
                            , expr.Comments.Clone());
      _stack.Push(node);
    }
    public override void VisitAfter(BracketedExpr expr) {
      var operand = (Expr)_stack.Pop();

      if(object.ReferenceEquals(expr.Operand, operand)) {
        _stack.Push(expr);
        return;
      }

      var node = new BracketedExpr(operand, expr.Comments.Clone());
      _stack.Push(node);
    }
    public override void VisitAfter(CastExpr expr) {
      var operand = (Expr)_stack.Pop();

      if(object.ReferenceEquals(expr.Operand, operand)) {
        _stack.Push(expr);
        return;
      }

      var node = new CastExpr(operand, expr.TypeName, expr.Comments.Clone());
      _stack.Push(node); 
    }
    public override void VisitAfter(CaseExpr expr) {
      bool isReferenceEqual = true;

      Expr elseResult = null;
      if(expr.ElseResult != null) {
        elseResult = (Expr)_stack.Pop();
        isReferenceEqual = isReferenceEqual && object.ReferenceEquals(expr.ElseResult, elseResult);
      }

      var results = new List<Expr>();
      for(var i = expr.Results.Count - 1; i >= 0; --i) {
        var result = (Expr)_stack.Pop();
        results.Insert(0, result);
        isReferenceEqual = isReferenceEqual && object.ReferenceEquals(expr.Results[i], result);
      }

      if(expr.IsSimpleCase){
        var comparisons = new List<Expr>();
        for(var i = expr.Comparisons.Count - 1; i >= 0; --i) {
          var comparison = (Expr)_stack.Pop();
          comparisons.Insert(0, comparison);
          isReferenceEqual = isReferenceEqual && object.ReferenceEquals(expr.Comparisons[i], comparison);
        }

        Expr searchExpr = (Expr)_stack.Pop();

        if(isReferenceEqual && 
           object.ReferenceEquals(expr.SearchExpr, searchExpr)) {
          _stack.Push(expr);
          return;
        }

        var node = new CaseExpr(searchExpr, comparisons, results, elseResult, expr.Comments.Clone());
        _stack.Push(node);

      } else {
        var conditions = new List<Predicate>();
        for(var i = expr.Conditions.Count - 1; i >= 0; --i) {
          var condition = (Predicate)_stack.Pop();
          conditions.Insert(0, condition);
          isReferenceEqual = isReferenceEqual && object.ReferenceEquals(expr.Conditions[i], condition);
        }

        if(isReferenceEqual) {
          _stack.Push(expr);
          return;
        }

        var node = new CaseExpr(conditions, results, elseResult, expr.Comments.Clone());
        _stack.Push(node);
      }
    }
    public override void VisitAfter(SubQueryExp expr) {
      var operand = (IQuery)_stack.Pop();

      if(object.ReferenceEquals(expr.Query, operand)) {
        _stack.Push(expr);
        return;
      }

      var node = new SubQueryExp(operand, expr.Comments.Clone());
      _stack.Push(node);
    }

    // Predicates
    public override void VisitAfter(BinaryOpPredicate predicate) { }
    public override void VisitAfter(NotPredicate notPredicate) { }
    public override void VisitAfter(AndPredicate andPredicate) { }
    public override void VisitAfter(OrPredicate orPredicate) { }
    public override void Visit(PlaceHolderPredicate predicate) {
      var node = new PlaceHolderPredicate(predicate.Label, predicate.Comments.Clone());
      _stack.Push(node);
    }
    public override void VisitAfter(LikePredicate predicate) { }
    public override void VisitAfter(IsNullPredicate predicate) { }
    public override void VisitAfter(IsPredicate predicate) { }
    public override void VisitAfter(BetweenPredicate predicate) { }
    public override void VisitAfter(InPredicate predicate) { }
    public override void VisitAfter(SubQueryPredicate predicate) { }
    public override void VisitAfter(ExistsPredicate predicate) { }
    public override void VisitAfter(CollatePredicate predicate) { }
    public override void VisitAfter(BracketedPredicate predicate) { }

    // Clauses
    public override void VisitAfter(CompoundQueryClause compoundQuery) { }
    public override void VisitAfter(BracketedQueryClause bracketedQuery) { }
    public override void VisitAfter(SingleQueryClause query) {
      
      //if(query.GetType() == typeof(SingleQuery)){
      //  var singleQuery = (SingleQuery)query;
      //  var node = new SingleQuery(query.Quantifier
      //                            , singleQuery.HasTop
      //                            , singleQuery.Top
      //                            , singleQuery.HasWildcard
      //                            , results
      //                            , from
      //                            , where
      //                            , groupBy
      //                            , having
      //                            , orderBy
      //                            , limit
      //                            , singleQuery.Comments.Clone());
      //} else {
      //  var singleQueryClause = query;
      //  var node = new SingleQueryClause(query.Quantifier
      //                                  , singleQueryClause.HasTop
      //                                  , singleQueryClause.Top
      //                                  , singleQueryClause.HasWildcard
      //                                  , results
      //                                  , from
      //                                  , where
      //                                  , groupBy
      //                                  , having
      //                                  , singleQueryClause.Comments.Clone());
      //}


    }
    public override void VisitAfter(ResultColumns resultColumns) { }
    public override void VisitAfter(ResultExpr resultExpr) { }
    public override void VisitAfter(UnqualifiedColumnNames columns) { }
    public override void VisitAfter(Exprs exprs) {
      bool isReferenceEqual = true;

      var exprList = new List<Expr>();
      for(var i = exprs.Count - 1; i >= 0; --i) {
        var expr = (Expr)_stack.Pop();
        exprList.Insert(0, expr);
        isReferenceEqual = isReferenceEqual && object.ReferenceEquals(exprs[i], expr);
      }

      if(isReferenceEqual) {
        _stack.Push(exprs);
        return;
      }

      var node = new Exprs(exprList, exprs.Comments.Clone());
      _stack.Push(node);
    }
    public override void VisitAfter(JoinSource joinSource) { }
    public override void VisitAfter(CommaJoinSource commaJoinSource) { }
    public override void VisitAfter(AliasedQuery aliasedQuery) { }
    public override void VisitAfter(BracketedSource bracketedSource) { }
    public override void VisitAfter(GroupBy groupBy) { }
    public override void VisitAfter(OrderBy orderBy) { }
    public override void VisitAfter(OrderingTerm orderingTerm) { }
    public override void VisitAfter(PartitionBy partitionBy) { }
    public override void VisitAfter(PartitioningTerm partitioningTerm) { }
    virtual public void VisitAfter(ILimitClause iLimitClause) { }

    // Statements
    public override void VisitAfter(SqlitePragmaStmt pragmaStmt) { }
  }
}
