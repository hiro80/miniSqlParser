
namespace MiniSqlParser
{
  internal partial class MakeASTListener: MiniSqlParserBaseListener
  {

    public override void ExitBinaryOpPredicate(MiniSqlParserParser.BinaryOpPredicateContext context) {
      var right = (Expr)_stack.Pop();
      var comments = this.GetComments(context.op);
      var left = (Expr)_stack.Pop();

      var opType = context.op.Type;
      PredicateOperator op = PredicateOperator.Equal;
      if(opType == MiniSqlParserLexer.ASSIGN) {
        op = PredicateOperator.Equal;
      } else if(opType == MiniSqlParserLexer.NOT_EQ2) {
        op = PredicateOperator.NotEqual;
      } else if(opType == MiniSqlParserLexer.LT) {
        op = PredicateOperator.Less;
      } else if(opType == MiniSqlParserLexer.LT_EQ) {
        op = PredicateOperator.LessOrEqual;
      } else if(opType == MiniSqlParserLexer.GT) {
        op = PredicateOperator.Greater;
      } else if(opType == MiniSqlParserLexer.GT_EQ) {
        op = PredicateOperator.GreaterOrEqual;
      } else if(opType == MiniSqlParserLexer.EQ) {
        op = PredicateOperator.Equal2;
      } else if(opType == MiniSqlParserLexer.NOT_EQ1) {
        op = PredicateOperator.NotEqual2;
      } else {
        throw new CannotBuildASTException("Undifined PredicateOperator is used");
      }

      var node = new BinaryOpPredicate(left, op, right, comments);
      _stack.Push(node);
    }

    public override void ExitNotPredicate(MiniSqlParserParser.NotPredicateContext context) {
      var operand = (Predicate)_stack.Pop();
      var comments = this.GetComments(context.K_NOT());
      var node = new NotPredicate(operand, comments);
      _stack.Push(node);
    }

    public override void ExitAndPredicate(MiniSqlParserParser.AndPredicateContext context) {
      var right = (Predicate)_stack.Pop();
      var comments = this.GetComments(context.K_AND());
      var left = (Predicate)_stack.Pop();
      var node = new AndPredicate(left, right, comments);
      _stack.Push(node);
    }

    public override void ExitOrPredicate(MiniSqlParserParser.OrPredicateContext context) {
      var right = (Predicate)_stack.Pop();
      var comments = this.GetComments(context.K_OR());
      var left = (Predicate)_stack.Pop();
      var node = new OrPredicate(left, right, comments);
      _stack.Push(node);
    }

    public override void ExitPhPredicate(MiniSqlParserParser.PhPredicateContext context) {
      var placeHolderNode =
          context.PLACEHOLDER1() != null ? context.PLACEHOLDER1() : context.PLACEHOLDER2();
      var comments = this.GetComments(placeHolderNode);
      var name = placeHolderNode.GetText();
      if(this.ForSqlAccessor) {
        if(name == "?") {
          this.AddSqlAccessorSyntaxError("SqlPodではプレースホルダに'?'を使えません", context);
        } else if(name.StartsWith(":")) {
          this.AddSqlAccessorSyntaxError("SqlPodではプレースホルダに':'を使えません", context);
        }
      }
      var node = new PlaceHolderPredicate(name, comments);
      _stack.Push(node);
    }

    public override void ExitLikePredicate(MiniSqlParserParser.LikePredicateContext context) {
      var comments = this.GetComments(context);

      Expr escape = null;
      if(context.K_ESCAPE() != null) {
        escape = (Expr)_stack.Pop();
      }
      Expr pattern = (Expr)_stack.Pop();

      var opType = context.op.Type;
      LikeOperator op = LikeOperator.Like;
      if(opType == MiniSqlParserLexer.K_LIKE) {
        op = LikeOperator.Like;
      } else if(opType == MiniSqlParserLexer.K_ILIKE) {
        op = LikeOperator.Ilike;
      } else if(opType == MiniSqlParserLexer.K_GLOB) {
        op = LikeOperator.Glog;
      } else if(opType == MiniSqlParserLexer.K_MATCH) {
        op = LikeOperator.Match;
      } else if(opType == MiniSqlParserLexer.K_REGEXP) {
        op = LikeOperator.Regexp;
      } else {
        throw new CannotBuildASTException("Undifined LikeOperator is used");
      }

      var not = context.K_NOT() != null;
      var operand = (Expr)_stack.Pop();

      var node = new LikePredicate(operand, not, op, pattern, escape, comments);
      _stack.Push(node);
    }

    public override void ExitIsNullPredicate(MiniSqlParserParser.IsNullPredicateContext context) {
      var comments = this.GetComments(context);
      var not = context.K_NOT() != null;
      var operand = (Expr)_stack.Pop();
      _stack.Push(new IsNullPredicate(operand, not, comments));
    }

    public override void ExitIsPredicate(MiniSqlParserParser.IsPredicateContext context) {
      var comments = this.GetComments(context);
      var right = (Expr)_stack.Pop();
      var not = context.K_NOT() != null;
      var left = (Expr)_stack.Pop();
      _stack.Push(new IsPredicate(left, not, right, comments));
    }

    public override void ExitBetweenPredicate(MiniSqlParserParser.BetweenPredicateContext context) {
      var comments = this.GetComments(context);
      var to = (Expr)_stack.Pop();
      var from = (Expr)_stack.Pop();
      var not = context.K_NOT() != null;
      var operand = (Expr)_stack.Pop();
      _stack.Push(new BetweenPredicate(operand, not, from, to, comments));
    }

    public override void ExitInPredicate(MiniSqlParserParser.InPredicateContext context) {
      var comments = this.GetComments(context);

      Exprs exprs = null;
      IQuery query = null;
      if(context.exprs() != null) {
        exprs = (Exprs)_stack.Pop();
      } else if(context.query() != null) {
        query = (IQuery)_stack.Pop();
      } else {
        exprs = new Exprs();
      }
      var operand = (Expr)_stack.Pop();
      var not = context.K_NOT() != null;

      var node = new InPredicate(operand, not, exprs, query, comments);
      _stack.Push(node);
    }

    public override void ExitSubQueryPredicate(MiniSqlParserParser.SubQueryPredicateContext context) {
      var comments = this.GetComments(context);

      var query = (IQuery)_stack.Pop();

      var opType2 = context.op2.Type;
      QueryQuantifier quantifier = QueryQuantifier.Any;
      if(opType2 == MiniSqlParserLexer.K_ANY) {
        quantifier = QueryQuantifier.Any;
      } else if(opType2 == MiniSqlParserLexer.K_SOME) {
        quantifier = QueryQuantifier.Some;
      } else if(opType2 == MiniSqlParserLexer.K_ALL) {
        quantifier = QueryQuantifier.All;
      } else {
        throw new CannotBuildASTException("Undifined QueryQuantifier is used");
      }

      var opType1 = context.op1.Type;
      PredicateOperator operater = PredicateOperator.Equal;
      if(opType1 == MiniSqlParserLexer.ASSIGN) {
        operater = PredicateOperator.Equal;
      } else if(opType1 == MiniSqlParserLexer.NOT_EQ2) {
        operater = PredicateOperator.NotEqual;
      } else if(opType1 == MiniSqlParserLexer.LT) {
        operater = PredicateOperator.Less;
      } else if(opType1 == MiniSqlParserLexer.LT_EQ) {
        operater = PredicateOperator.LessOrEqual;
      } else if(opType1 == MiniSqlParserLexer.GT) {
        operater = PredicateOperator.Greater;
      } else if(opType1 == MiniSqlParserLexer.GT_EQ) {
        operater = PredicateOperator.GreaterOrEqual;
      } else if(opType1 == MiniSqlParserLexer.EQ) {
        operater = PredicateOperator.Equal2;
      } else if(opType1 == MiniSqlParserLexer.NOT_EQ1) {
        operater = PredicateOperator.NotEqual2;
      } else {
        throw new CannotBuildASTException("Undifined PredicateOperator is used");
      }

      var operand = (Expr)_stack.Pop();

      var node = new SubQueryPredicate(operand, operater, quantifier, query, comments);
      _stack.Push(node);
    }

    public override void ExitExistsPredicate(MiniSqlParserParser.ExistsPredicateContext context) {
      _stack.Push(
          new ExistsPredicate(
              (IQuery)_stack.Pop()
            , this.GetComments(context)));
    }

    public override void ExitCollatePredicate(MiniSqlParserParser.CollatePredicateContext context) {
      var comments = this.GetComments(context);
      var collationName = this.GetIdentifier(context.collation_name().identifier());
      var operand = (Predicate)_stack.Pop();
      var node = new CollatePredicate(operand, collationName, comments);
      _stack.Push(node);
    }

    public override void ExitBracketedPredicate(MiniSqlParserParser.BracketedPredicateContext context) {
      var comments = this.GetComments(context);
      var operand = (Predicate)_stack.Pop();
      this._stack.Push(new BracketedPredicate(operand, comments));
    }
  }
}