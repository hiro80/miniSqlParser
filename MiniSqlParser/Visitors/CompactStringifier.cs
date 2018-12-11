using System.Text;
using System.Collections.Generic;
using System.ComponentModel;

namespace MiniSqlParser
{
  public class CompactStringifier: SetPlaceHoldersVisitor
  {
    private static readonly string _newline = System.Environment.NewLine;
    private readonly int _maxLineLength;
    private readonly bool _printComments;

    private int _currentLineLength;
    private readonly StringBuilder _sql;

    public CompactStringifier(int maxLineLength
                            , bool printComments = false
                            , Dictionary<string, Node> placeHolders = null) : base(placeHolders)
    {
      _sql = new StringBuilder();
      _maxLineLength = maxLineLength;
      _printComments = printComments;
    }

    public StringBuilder ToStringBuilder() {
      return _sql;
    }

    public override string ToString() {
      return _sql.ToString();
    }

    private void AppendComment(string str) {
      if(!_printComments || str == null) {
        return;
      }
      if(str.StartsWith("--")) {
        _sql.Append(_newline);
        _sql.Append(str);
        _sql.Append(_newline);
      } else {
        _sql.Append(str);
      }
    }

    private void AppendKeyword(string str) {
      this.AppendString(str);
    }

    private void AppendSymbol(string str) {
      this.AppendString(str);
    }

    private void AppendString(Identifier id) {
      this.AppendString(id.ToRawString());
    }

    private void AppendString(string str) {
      if(string.IsNullOrEmpty(str)) {
        return;
      }
      if(_currentLineLength + str.Length > _maxLineLength) {
        _sql.Replace(" ", "", _sql.Length - 1, 1);
        _sql.Append(_newline);
        str = str.TrimStart(' ');
        _currentLineLength = 0;
      }
      _sql.Append(str);
      _currentLineLength += str.Length;
    }

    public override void VisitOnSeparator(Node node, int offset, int i) {
      this.AppendSymbol(",");
      this.AppendComment(node.Comments[offset + i]);
    }

    public override void VisitOnSeparator(Exprs exprs, int offset, int i) {
      this.AppendSymbol(",");
      this.AppendComment(exprs.Comments[offset + i]);
    }

    public override void VisitOnSeparator(CommaJoinSource commaJoinSource, int offset, int i) {
      this.AppendSymbol(",");
      this.AppendComment(commaJoinSource.Comments[offset + i]);
    }

    public override void VisitOnSeparator(ValuesList valuesList, int offset, int i) {
      this.AppendSymbol(",");
      this.AppendComment(valuesList.Comments[offset + i]);
    }

    public override void VisitOnSeparator(SubstringFunc expr, int offset, int i) {
      if(i == 0) {
        if(expr.Separator1IsComma) {
          this.AppendSymbol(",");
        } else {
          this.AppendKeyword(" FROM");
        }
      } else if(i == 1) {
        if(expr.Separator2IsComma) {
          this.AppendSymbol(",");
        } else {
          this.AppendKeyword(" FOR");
        }
      } else {
        throw new CannotStringifierException("SUBSTRING function has more than 2 parameters");
      }
      this.AppendComment(expr.Comments[offset + i]);
      if(i == 0) {
        if(!expr.Separator1IsComma) {
          this.AppendString(" ");
        }
      } else {
        if(!expr.Separator2IsComma) {
          this.AppendString(" ");
        }
      }
    }

    public override void VisitOnSeparator(ExtractFuncExpr expr, int offset) {
      if(expr.SeparatorIsComma) {
        this.AppendSymbol(",");
      } else {
        this.AppendKeyword(" FROM");
      }
      this.AppendComment(expr.Comments[offset]);
      if(!expr.SeparatorIsComma) {
        this.AppendString(" ");
      }
    }

    public override void VisitOnLParen(Node node, int offset) {
      this.AppendSymbol("(");
      this.AppendComment(node.Comments[offset]);
    }

    public override void VisitOnRParen(Node node, int offset) {
      this.AppendSymbol(")");
      this.AppendComment(node.Comments[offset]);
    }

    public override void VisitOnWildCard(Node node, int offset) {
      this.AppendSymbol("*");
      this.AppendComment(node.Comments[offset]);
    }

    public override void VisitOnStmtSeparator(Stmt stmt, int offset, int i) {
      this.AppendSymbol(";");
      this.AppendComment(stmt.Comments[offset + i]);
    }

    public override void Visit(Table table) {
      int i = -1;
      if(!string.IsNullOrEmpty(table.ServerName)) {
        this.AppendString(table.ServerName);
        this.AppendComment(table.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(table.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(table.DataBaseName)) {
        this.AppendString(table.DataBaseName);
        this.AppendComment(table.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(table.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(table.SchemaName)) {
        this.AppendString(table.SchemaName);
        this.AppendComment(table.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(table.Comments[++i]);
      }
      this.AppendString(table.Name);
      this.AppendComment(table.Comments[++i]);
      if(table.HasAs) {
        this.AppendKeyword(" AS");
        this.AppendComment(table.Comments[++i]);
      }

      if(!string.IsNullOrEmpty(table.AliasName)) {
        this.AppendString(" ");
        this.AppendString(table.AliasName);
        this.AppendComment(table.Comments[++i]);
      }

      if(table.HasNotIndexed) {
        this.AppendKeyword(" NOT");
        this.AppendComment(table.Comments[++i]);
        this.AppendKeyword(" INDEXED");
        this.AppendComment(table.Comments[++i]);
      } else if(!string.IsNullOrEmpty(table.IndexName)) {
        this.AppendKeyword(" INDEXED");
        this.AppendComment(table.Comments[++i]);
        this.AppendString(" ");
        this.AppendKeyword("BY");
        this.AppendComment(table.Comments[++i]);
        this.AppendString(" ");
        if(!string.IsNullOrEmpty(table.IndexServerName)) {
          this.AppendString(table.IndexServerName);
          this.AppendComment(table.Comments[++i]);
          this.AppendSymbol(".");
          this.AppendComment(table.Comments[++i]);
        }
        if(!string.IsNullOrEmpty(table.IndexDataBaseName)) {
          this.AppendString(table.IndexDataBaseName);
          this.AppendComment(table.Comments[++i]);
          this.AppendSymbol(".");
          this.AppendComment(table.Comments[++i]);
        }
        if(!string.IsNullOrEmpty(table.IndexSchemaName)) {
          this.AppendString(table.IndexSchemaName);
          this.AppendComment(table.Comments[++i]);
          this.AppendSymbol(".");
          this.AppendComment(table.Comments[++i]);
        }
        this.AppendString(table.IndexName);
        this.AppendComment(table.Comments[++i]);
      }
    }

    public override void Visit(Column column) {
      int i = -1;
      if(!string.IsNullOrEmpty(column.ServerName)) {
        this.AppendString(column.ServerName);
        this.AppendComment(column.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(column.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(column.DataBaseName)) {
        this.AppendString(column.DataBaseName);
        this.AppendComment(column.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(column.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(column.SchemaName)) {
        this.AppendString(column.SchemaName);
        this.AppendComment(column.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(column.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(column.TableAliasName)) {
        this.AppendString(column.TableAliasName);
        this.AppendComment(column.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(column.Comments[++i]);
      }
      this.AppendString(column.Name);
      this.AppendComment(column.Comments[++i]);
      if(column.HasOuterJoinKeyword) {
        this.AppendSymbol("(+)");
        this.AppendComment(column.Comments[++i]);
      }
    }

    public override void Visit(StringLiteral literal) {
      this.AppendString(literal.Value);
      this.AppendComment(literal.Comments[0]);
    }

    public override void Visit(UNumericLiteral literal) {
      this.AppendString(literal.Value);
      this.AppendComment(literal.Comments[0]);
    }

    public override void Visit(NullLiteral literal) {
      this.AppendKeyword("NULL");
      this.AppendComment(literal.Comments[0]);
    }

    public override void Visit(DateLiteral literal) {
      this.AppendString(literal.Value);
      this.AppendComment(literal.Comments[0]);
    }

    public override void Visit(TimeLiteral literal) {
      this.AppendString(literal.Value);
      this.AppendComment(literal.Comments[0]);
    }

    public override void Visit(TimeStampLiteral literal) {
      this.AppendString(literal.Value);
      this.AppendComment(literal.Comments[0]);
    }

    public override void Visit(IntervalLiteral literal) {
      this.AppendString(literal.Value);
      this.AppendComment(literal.Comments[0]);
    }

    public override void Visit(BlobLiteral literal) {
      this.AppendString(literal.Value);
      this.AppendComment(literal.Comments[0]);
    }

    public override void VisitBefore(SignedNumberExpr expr) {
      if(expr.Sign == Sign.Minus) {
        this.AppendSymbol("-");
      } else {
        this.AppendSymbol("+");
      }
      this.AppendComment(expr.Comments[0]);
    }

    protected override void VisitOnPlaceHolder(PlaceHolderExpr expr) {
      this.AppendString(expr.Label);
      this.AppendComment(expr.Comments[0]);
    }

    public override void VisitBefore(BitwiseNotExpr expr) {
      this.AppendSymbol("~");
      this.AppendComment(expr.Comments[0]);
    }

    public override void VisitOnOperator(BinaryOpExpr expr) {
      if(expr.Operator == ExpOperator.StringConcat) {
        this.AppendSymbol("||");
      } else if(expr.Operator == ExpOperator.Mult) {
        this.AppendSymbol("*");
      } else if(expr.Operator == ExpOperator.Div) {
        this.AppendSymbol("/");
      } else if(expr.Operator == ExpOperator.Mod) {
        this.AppendSymbol("%");
      } else if(expr.Operator == ExpOperator.Add) {
        this.AppendSymbol("+");
      } else if(expr.Operator == ExpOperator.Sub) {
        this.AppendSymbol("-");
      } else if(expr.Operator == ExpOperator.LeftBitShift) {
        this.AppendSymbol("<<");
      } else if(expr.Operator == ExpOperator.RightBitShift) {
        this.AppendSymbol(">>");
      } else if(expr.Operator == ExpOperator.BitAnd) {
        this.AppendSymbol("&");
      } else if(expr.Operator == ExpOperator.BitOr) {
        this.AppendSymbol("|");
      } else {
        throw new InvalidEnumArgumentException("Undefined ExpOperator is used"
                                              , (int)expr.Operator
                                              , typeof(ExpOperator));
      }
      this.AppendComment(expr.Comments[0]);
    }

    public override void VisitBefore(SubstringFunc expr) {
      this.AppendString(expr.Name);
      this.AppendComment(expr.Comments[0]);
      this.AppendSymbol("(");
      this.AppendComment(expr.Comments[1]);
    }

    public override void VisitAfter(SubstringFunc expr) {
      this.AppendSymbol(")");
      this.AppendComment(expr.Comments.Last);
    }

    public override void VisitBefore(ExtractFuncExpr expr) {
      this.AppendString(expr.Name);
      this.AppendComment(expr.Comments[0]);
      this.AppendSymbol("(");
      this.AppendComment(expr.Comments[1]);
      this.AppendKeyword(expr.DateTimeField.ToString().ToUpper());
      this.AppendComment(expr.Comments[2]);
    }

    public override void VisitAfter(ExtractFuncExpr expr) {
      this.AppendSymbol(")");
      this.AppendComment(expr.Comments.Last);
    }

    public override void VisitBefore(AggregateFuncExpr expr) {
      this.AppendString(expr.Name);
      this.AppendComment(expr.Comments[0]);
      this.AppendSymbol("(");
      this.AppendComment(expr.Comments[1]);
      if(expr.Quantifier != QuantifierType.None) {
        this.AppendKeyword(expr.Quantifier.ToString().ToUpper());
        this.AppendComment(expr.Comments[2]);
        this.AppendString(" ");
      }
    }

    public override void VisitAfter(AggregateFuncExpr expr) {
      this.AppendSymbol(")");
      this.AppendComment(expr.Comments.Last);
    }

    public override void VisitBefore(WindowFuncExpr expr) {
      var i = -1;
      if(!string.IsNullOrEmpty(expr.ServerName)) {
        this.AppendString(expr.ServerName);
        this.AppendComment(expr.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(expr.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(expr.DataBaseName)) {
        this.AppendString(expr.DataBaseName);
        this.AppendComment(expr.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(expr.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(expr.SchemaName)) {
        this.AppendString(expr.SchemaName);
        this.AppendComment(expr.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(expr.Comments[++i]);
      }
      this.AppendString(expr.Name);
      this.AppendComment(expr.Comments[++i]);
      this.AppendSymbol("(");
      this.AppendComment(expr.Comments[++i]);
      if(expr.Quantifier != QuantifierType.None) {
        this.AppendKeyword(expr.Quantifier.ToString().ToUpper());
        this.AppendComment(expr.Comments[++i]);
        this.AppendString(" ");
      }
    }

    public override void VisitOnOver(WindowFuncExpr expr, int offset) {
      this.AppendSymbol(")");
      this.AppendComment(expr.Comments[offset - 1]);
      this.AppendKeyword("OVER");
      this.AppendComment(expr.Comments[offset]);
      this.AppendSymbol("(");
      this.AppendComment(expr.Comments[offset + 1]);
    }

    public override void VisitAfter(WindowFuncExpr expr) {
      this.AppendSymbol(")");
      this.AppendComment(expr.Comments.Last);
    }

    public override void VisitBefore(FuncExpr expr) {
      var i = -1;
      if(!string.IsNullOrEmpty(expr.ServerName)) {
        this.AppendString(expr.ServerName);
        this.AppendComment(expr.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(expr.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(expr.DataBaseName)) {
        this.AppendString(expr.DataBaseName);
        this.AppendComment(expr.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(expr.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(expr.SchemaName)) {
        this.AppendString(expr.SchemaName);
        this.AppendComment(expr.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(expr.Comments[++i]);
      }
      this.AppendString(expr.Name);
      this.AppendComment(expr.Comments[++i]);
      this.AppendSymbol("(");
      this.AppendComment(expr.Comments[++i]);
    }

    public override void VisitAfter(FuncExpr expr) {
      this.AppendSymbol(")");
      this.AppendComment(expr.Comments.Last);
    }

    public override void VisitBefore(BracketedExpr expr) {
      this.AppendSymbol("(");
      this.AppendComment(expr.Comments[0]);
    }

    public override void VisitAfter(BracketedExpr expr) {
      this.AppendSymbol(")");
      this.AppendComment(expr.Comments[1]);
    }

    public override void VisitBefore(CastExpr expr) {
      this.AppendKeyword("CAST");
      this.AppendComment(expr.Comments[0]);
      this.AppendSymbol("(");
      this.AppendComment(expr.Comments[1]);
    }

    public override void VisitAfter(CastExpr expr) {
      this.AppendKeyword(" AS");
      this.AppendComment(expr.Comments[2]);
      this.AppendString(" ");
      this.AppendString(expr.TypeName);
      this.AppendComment(expr.Comments[3]);
      this.AppendSymbol(")");
      this.AppendComment(expr.Comments[4]);
    }

    public override void VisitBefore(CaseExpr expr) {
      this.AppendKeyword("CASE");
      this.AppendComment(expr.Comments[0]);
      if(expr.IsSimpleCase) {
        this.AppendString(" ");
      }
    }

    public override void VisitOnWhen(CaseExpr expr, int i) {
      this.AppendKeyword(" WHEN");
      this.AppendComment(expr.Comments[i + 1]);
      this.AppendString(" ");
    }

    public override void VisitOnThen(CaseExpr expr, int i) {
      this.AppendKeyword(" THEN");
      this.AppendComment(expr.Comments[i + 1]);
      this.AppendString(" ");
    }

    public override void VisitOnElse(CaseExpr expr) {
      this.AppendKeyword(" ELSE");
      this.AppendComment(expr.Comments[expr.Comments.Count - 2]);
      this.AppendString(" ");
    }

    public override void VisitAfter(CaseExpr expr) {
      this.AppendKeyword(" END");
      this.AppendComment(expr.Comments.Last);
    }

    public override void VisitBefore(SubQueryExp expr) {
      this.AppendSymbol("(");
      this.AppendComment(expr.Comments[0]);
    }

    public override void VisitAfter(SubQueryExp expr) {
      this.AppendSymbol(")");
      this.AppendComment(expr.Comments.Last);
    }

    public override void Visit(BinaryOpPredicate predicate) {
      if(predicate.Operator == PredicateOperator.Equal) {
        this.AppendSymbol("=");
      } else if(predicate.Operator == PredicateOperator.NotEqual) {
        this.AppendSymbol("<>");
      } else if(predicate.Operator == PredicateOperator.Less) {
        this.AppendSymbol("<");
      } else if(predicate.Operator == PredicateOperator.LessOrEqual) {
        this.AppendSymbol("<=");
      } else if(predicate.Operator == PredicateOperator.Greater) {
        this.AppendSymbol(">");
      } else if(predicate.Operator == PredicateOperator.GreaterOrEqual) {
        this.AppendSymbol(">=");
      } else if(predicate.Operator == PredicateOperator.Equal2) {
        this.AppendSymbol("==");
      } else if(predicate.Operator == PredicateOperator.NotEqual2) {
        this.AppendSymbol("!=");
      } else {
        throw new InvalidEnumArgumentException("Undefined PredicateOperator is used"
                                              , (int)predicate.Operator
                                              , typeof(PredicateOperator));
      }
      this.AppendComment(predicate.Comments[0]);
    }

    public override void VisitBefore(NotPredicate notPredicate) {
      this.AppendKeyword("NOT");
      this.AppendComment(notPredicate.Comments[0]);
      this.AppendString(" ");
    }

    public override void Visit(AndPredicate andPredicate) {
      this.AppendKeyword(" AND");
      this.AppendComment(andPredicate.Comments[0]);
      this.AppendString(" ");
    }

    public override void Visit(OrPredicate orPredicate) {
      this.AppendKeyword(" OR");
      this.AppendComment(orPredicate.Comments[0]);
      this.AppendString(" ");
    }

    protected override void VisitOnPlaceHolder(PlaceHolderPredicate predicate) {
      this.AppendString(predicate.Label);
      this.AppendComment(predicate.Comments[0]);
    }

    public override void Visit(LikePredicate predicate, int offset) {
      int i = offset;
      if(predicate.Not) {
        this.AppendKeyword(" NOT");
        this.AppendComment(predicate.Comments[i]);
        ++i;
      }

      if(predicate.Operator == LikeOperator.Like) {
        this.AppendKeyword(" LIKE");
      } else if(predicate.Operator == LikeOperator.Ilike) {
        this.AppendKeyword(" ILIKE");
      } else if(predicate.Operator == LikeOperator.Glog) {
        this.AppendKeyword(" GLOB");
      } else if(predicate.Operator == LikeOperator.Match) {
        this.AppendKeyword(" MATCH");
      } else if(predicate.Operator == LikeOperator.Regexp) {
        this.AppendKeyword(" REGEXP");
      } else {
        throw new InvalidEnumArgumentException("Undefined LikeOperator is used"
                                              , (int)predicate.Operator
                                              , typeof(LikeOperator));
      }
      this.AppendComment(predicate.Comments[i]);
      this.AppendString(" ");
    }

    public override void VisitOnEscape(LikePredicate predicate, int offset) {
      this.AppendKeyword(" ESCAPE");
      this.AppendComment(predicate.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitAfter(IsNullPredicate predicate) {
      int i = 0;
      this.AppendKeyword(" IS");
      this.AppendComment(predicate.Comments[i]);
      ++i;
      if(predicate.Not) {
        this.AppendKeyword(" NOT");
        this.AppendComment(predicate.Comments[i]);
        ++i;
      }
      this.AppendKeyword(" NULL");
      this.AppendComment(predicate.Comments[i]);
    }

    public override void Visit(IsPredicate predicate, int offset) {
      int i = 0;
      this.AppendKeyword(" IS");
      this.AppendComment(predicate.Comments[i]);
      ++i;
      if(predicate.Not) {
        this.AppendKeyword(" NOT");
        this.AppendComment(predicate.Comments[i]);
        ++i;
      }
      this.AppendString(" ");
    }

    public override void VisitOnBetween(BetweenPredicate predicate, int offset) {
      if(predicate.Not) {
        this.AppendKeyword(" NOT");
        this.AppendComment(predicate.Comments[offset - 1]);
      }
      this.AppendKeyword(" BETWEEN");
      this.AppendComment(predicate.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnAnd(BetweenPredicate predicate, int offset) {
      this.AppendKeyword(" AND");
      this.AppendComment(predicate.Comments[offset]);
      this.AppendString(" ");
    }

    public override void Visit(InPredicate predicate, int offset) {
      int i = 0;
      if(predicate.Not) {
        this.AppendKeyword(" NOT");
        this.AppendComment(predicate.Comments[i]);
        ++i;
      }
      this.AppendKeyword(" IN");
      this.AppendComment(predicate.Comments[i]);
      ++i;
      this.AppendSymbol("(");
      this.AppendComment(predicate.Comments[i]);
    }

    public override void VisitAfter(InPredicate predicate) {
      this.AppendSymbol(")");
      this.AppendComment(predicate.Comments.Last);
    }

    public override void Visit(SubQueryPredicate predicate, int offset) {
      if(predicate.Operator == PredicateOperator.Equal) {
        this.AppendSymbol("=");
      } else if(predicate.Operator == PredicateOperator.NotEqual) {
        this.AppendSymbol("<>");
      } else if(predicate.Operator == PredicateOperator.Less) {
        this.AppendSymbol("<");
      } else if(predicate.Operator == PredicateOperator.LessOrEqual) {
        this.AppendSymbol("<=");
      } else if(predicate.Operator == PredicateOperator.Greater) {
        this.AppendSymbol(">");
      } else if(predicate.Operator == PredicateOperator.GreaterOrEqual) {
        this.AppendSymbol(">=");
      } else if(predicate.Operator == PredicateOperator.Equal2) {
        this.AppendSymbol("==");
      } else if(predicate.Operator == PredicateOperator.NotEqual2) {
        this.AppendSymbol("!=");
      } else {
        throw new InvalidEnumArgumentException("Undefined PredicateOperator is used"
                                              , (int)predicate.Operator
                                              , typeof(PredicateOperator));
      }
      this.AppendComment(predicate.Comments[offset]);
      this.AppendString(" ");

      if(predicate.Quantifier == QueryQuantifier.Any) {
        this.AppendKeyword("ANY");
      } else if(predicate.Quantifier == QueryQuantifier.Some) {
        this.AppendKeyword("SOME");
      } else if(predicate.Quantifier == QueryQuantifier.All) {
        this.AppendKeyword("ALL");
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryQuantifier is used"
                                              , (int)predicate.Quantifier
                                              , typeof(QueryQuantifier));
      }
      this.AppendComment(predicate.Comments[offset + 1]);
      this.AppendSymbol("(");
      this.AppendComment(predicate.Comments[offset + 2]);
    }

    public override void VisitAfter(SubQueryPredicate predicate) {
      this.AppendSymbol(")");
      this.AppendComment(predicate.Comments.Last);
    }

    public override void VisitBefore(ExistsPredicate predicate) {
      this.AppendKeyword("EXISTS");
      this.AppendComment(predicate.Comments[0]);
      this.AppendSymbol("(");
      this.AppendComment(predicate.Comments[1]);
    }

    public override void VisitAfter(ExistsPredicate predicate) {
      this.AppendSymbol(")");
      this.AppendComment(predicate.Comments.Last);
    }

    public override void VisitAfter(CollatePredicate predicate) {
      this.AppendKeyword(" COLLATE");
      this.AppendComment(predicate.Comments[0]);
      this.AppendString(" ");
      this.AppendString(predicate.Collation);
      this.AppendComment(predicate.Comments[1]);
    }

    public override void VisitBefore(BracketedPredicate predicate) {
      this.AppendSymbol("(");
      this.AppendComment(predicate.Comments[0]);
    }

    public override void VisitAfter(BracketedPredicate predicate) {
      this.AppendSymbol(")");
      this.AppendComment(predicate.Comments.Last);
    }

    public override void Visit(Assignment assignment) {
      this.AppendSymbol("=");
      this.AppendComment(assignment.Comments[0]);
    }

    public override void Visit(Default defoult) {
      this.AppendKeyword("DEFAULT");
      this.AppendComment(defoult.Comments[0]);
    }

    public override void VisitBefore(WithClause withClause) {
      this.AppendKeyword("WITH");
      this.AppendComment(withClause.Comments[0]);
      this.AppendString(" ");
      if(withClause.HasRecursiveKeyword) {
        this.AppendKeyword("RECURSIVE");
        this.AppendComment(withClause.Comments[1]);
        this.AppendString(" ");
      }
    }

    public override void VisitOnAs(WithDefinition withDefinition, int offset) {
      this.AppendKeyword(" AS");
      this.AppendComment(withDefinition.Comments[offset]);
      this.AppendSymbol("(");
      this.AppendComment(withDefinition.Comments[offset + 1]);
    }

    public override void VisitAfter(WithDefinition withDefinition) {
      this.AppendSymbol(")");
      this.AppendComment(withDefinition.Comments.Last);
    }

    public override void VisitOnCompoundOp(CompoundQueryClause compoundQuery, int offset) {
      this.AppendString(" ");

      var op = compoundQuery.Operator;
      if(op == CompoundType.UnionAll) {
        this.AppendKeyword("UNION");
      } else if(op == CompoundType.Union) {
        this.AppendKeyword("UNION");
      } else if(op == CompoundType.Intersect) {
        this.AppendKeyword("INTERSECT");
      } else if(op == CompoundType.Except) {
        this.AppendKeyword("EXCEPT");
      } else if(op == CompoundType.Minus) {
        this.AppendKeyword("MINUS");
      }
      this.AppendComment(compoundQuery.Comments[offset]);
      this.AppendString(" ");

      if(op == CompoundType.UnionAll) {
        this.AppendKeyword("ALL");
        this.AppendComment(compoundQuery.Comments[offset + 1]);
        this.AppendString(" ");
      }
    }

    //public override void VisitBefore(SingleQuery query) {
    //  this.VisitBefore((SingleQueryClause)query);
    //}

    public override void VisitBefore(SingleQueryClause query) {
      var i = -1;
      this.AppendKeyword("SELECT");
      this.AppendComment(query.Comments[++i]);
      this.AppendString(" ");

      if(query.Quantifier == QuantifierType.Distinct) {
        this.AppendKeyword("DISTINCT");
        this.AppendComment(query.Comments[++i]);
        this.AppendString(" ");
      } else if(query.Quantifier == QuantifierType.All) {
        this.AppendKeyword("ALL");
        this.AppendComment(query.Comments[++i]);
        this.AppendString(" ");
      }

      if(query.HasTop) {
        this.AppendKeyword("TOP");
        this.AppendComment(query.Comments[++i]);
        this.AppendString(" ");
        this.AppendString(query.Top.ToString());
        this.AppendComment(query.Comments[++i]);
        this.AppendString(" ");
      }
    }

    public override void VisitOnFrom(SingleQueryClause query, int offset) {
      this.AppendKeyword(" FROM");
      this.AppendComment(query.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnWhere(SingleQueryClause query, int offset) {
      this.AppendKeyword(" WHERE");
      this.AppendComment(query.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnHaving(SingleQueryClause query, int offset) {
      this.AppendKeyword(" HAVING");
      this.AppendComment(query.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitAfter(ResultExpr resultExpr) {
      var i = -1;
      if(resultExpr.HasAs) {
        this.AppendKeyword(" AS");
        this.AppendComment(resultExpr.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(resultExpr.AliasName)) {
        this.AppendString(" ");
        this.AppendString(resultExpr.AliasName);
        this.AppendComment(resultExpr.Comments[++i]);
      }
    }

    public override void Visit(TableWildcard tableWildcard) {
      int i = -1;
      if(!string.IsNullOrEmpty(tableWildcard.ServerName)) {
        this.AppendString(tableWildcard.ServerName);
        this.AppendComment(tableWildcard.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(tableWildcard.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(tableWildcard.DataBaseName)) {
        this.AppendString(tableWildcard.DataBaseName);
        this.AppendComment(tableWildcard.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(tableWildcard.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(tableWildcard.SchemaName)) {
        this.AppendString(tableWildcard.SchemaName);
        this.AppendComment(tableWildcard.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(tableWildcard.Comments[++i]);
      }
      this.AppendString(tableWildcard.TableAliasName);
      this.AppendComment(tableWildcard.Comments[++i]);
      this.AppendSymbol(".");
      this.AppendComment(tableWildcard.Comments[++i]);
      this.AppendSymbol("*");
      this.AppendComment(tableWildcard.Comments[++i]);
    }

    public override void Visit(UnqualifiedColumnName columnName) {
      this.AppendString(columnName.Name);
      this.AppendComment(columnName.Comments[0]);
    }

    public override void VisitBefore(BracketedSource bracketedSource) {
      this.AppendSymbol("(");
      this.AppendComment(bracketedSource.Comments[0]);
    }

    public override void VisitAfter(BracketedSource bracketedSource) {
      var i = 0;
      this.AppendSymbol(")");
      this.AppendComment(bracketedSource.Comments[++i]);

      if(bracketedSource.HasAs) {
        this.AppendKeyword("AS");
        this.AppendComment(bracketedSource.Comments[++i]);
        this.AppendString(" ");
      }

      if(!string.IsNullOrEmpty(bracketedSource.AliasName)) {
        this.AppendString(bracketedSource.AliasName);
        this.AppendComment(bracketedSource.Comments[++i]);
        this.AppendString(" ");
      }
    }

    public override void Visit(JoinOperator joinOperator) {
      var i = -1;
      if(joinOperator.HasNaturalKeyword) {
        this.AppendKeyword(" NATURAL");
        this.AppendComment(joinOperator.Comments[++i]);
      }

      if(joinOperator.JoinType != JoinType.None) {
        this.AppendString(" ");
        if(joinOperator.JoinType == JoinType.Left) {
          this.AppendKeyword("LEFT");
        } else if(joinOperator.JoinType == JoinType.Right) {
          this.AppendKeyword("RIGHT");
        } else if(joinOperator.JoinType == JoinType.Full) {
          this.AppendKeyword("FULL");
        } else if(joinOperator.JoinType == JoinType.Inner) {
          this.AppendKeyword("INNER");
        } else if(joinOperator.JoinType == JoinType.Cross) {
          this.AppendKeyword("CROSS");
        } else {
          throw new InvalidEnumArgumentException("Undefined JoinType is used"
                                                , (int)joinOperator.JoinType
                                                , typeof(JoinType));
        }
        this.AppendComment(joinOperator.Comments[++i]);
        if(joinOperator.HasOuterKeyword) {
          this.AppendKeyword(" OUTER");
          this.AppendComment(joinOperator.Comments[++i]);
        }
      }

      this.AppendKeyword(" JOIN");
      this.AppendComment(joinOperator.Comments[++i]);
      this.AppendString(" ");
    }

    public override void Visit(JoinSource joinSource) {
      if(joinSource.HasConstraint) {
        this.AppendKeyword(" ON");
        this.AppendComment(joinSource.Comments[0]);
        this.AppendString(" ");
      } else if(joinSource.HasUsingConstraint) {
        this.AppendKeyword(" USING");
        this.AppendComment(joinSource.Comments[0]);
      }
    }

    public override void VisitBefore(GroupBy groupBy) {
      this.AppendKeyword(" GROUP");
      this.AppendComment(groupBy.Comments[0]);
      this.AppendKeyword(" BY");
      this.AppendComment(groupBy.Comments[1]);
      this.AppendString(" ");
    }

    public override void VisitBefore(OrderBy orderBy) {
      this.AppendKeyword(" ORDER");
      this.AppendComment(orderBy.Comments[0]);
      this.AppendKeyword(" BY");
      this.AppendComment(orderBy.Comments[1]);
      this.AppendString(" ");
    }

    public override void VisitAfter(OrderingTerm orderingTerm) {
      int i = -1;
      if(!string.IsNullOrEmpty(orderingTerm.Collation)) {
        this.AppendKeyword(" COLLATE");
        this.AppendComment(orderingTerm.Comments[++i]);
        this.AppendString(" ");
        this.AppendString(orderingTerm.Collation);
        this.AppendComment(orderingTerm.Comments[++i]);
      }
      if(orderingTerm.OrderSpec == OrderSpec.Desc) {
        this.AppendKeyword(" DESC");
        this.AppendComment(orderingTerm.Comments[++i]);
      } else if(orderingTerm.OrderSpec == OrderSpec.Asc) {
        this.AppendKeyword(" ASC");
        this.AppendComment(orderingTerm.Comments[++i]);
      }
      if(orderingTerm.NullOrder != NullOrder.None) {
        this.AppendKeyword(" NULLS");
        this.AppendComment(orderingTerm.Comments[++i]);
        if(orderingTerm.NullOrder == NullOrder.First) {
          this.AppendKeyword(" FIRST");
          this.AppendComment(orderingTerm.Comments[++i]);
        } else {
          this.AppendKeyword(" LAST");
          this.AppendComment(orderingTerm.Comments[++i]);
        }
      }
    }

    public override void VisitBefore(PartitionBy partitionBy) {
      this.AppendKeyword(" PARTITION");
      this.AppendComment(partitionBy.Comments[0]);
      this.AppendKeyword(" BY");
      this.AppendComment(partitionBy.Comments[1]);
      this.AppendString(" ");
    }

    public override void VisitAfter(PartitioningTerm partitioningTerm) {
      if(!string.IsNullOrEmpty(partitioningTerm.Collation)) {
        this.AppendKeyword(" COLLATE");
        this.AppendComment(partitioningTerm.Comments[0]);
        this.AppendString(" ");
        this.AppendString(partitioningTerm.Collation);
        this.AppendComment(partitioningTerm.Comments[0]);
      }
    }

    public override void VisitOnUpdate(UpdateStmt updateStmt) {
      this.AppendKeyword("UPDATE");
      this.AppendComment(updateStmt.Comments[0]);
      this.AppendString(" ");
      if(updateStmt.OnConflict != ConflictType.None) {
        this.AppendKeyword("OR");
        this.AppendComment(updateStmt.Comments[1]);
        this.AppendString(" ");
        this.AppendKeyword(updateStmt.OnConflict.ToString().ToUpper());
        this.AppendComment(updateStmt.Comments[2]);
        this.AppendString(" ");
      }
    }

    public override void VisitOnSet(UpdateStmt updateStmt, int offset) {
      this.AppendKeyword(" SET");
      this.AppendComment(updateStmt.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnFrom2(UpdateStmt updateStmt, int offset) {
      this.AppendKeyword(" FROM");
      this.AppendComment(updateStmt.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnWhere(UpdateStmt updateStmt, int offset) {
      this.AppendKeyword(" WHERE");
      this.AppendComment(updateStmt.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnInsert(InsertStmt insertStmt) {
      var i = -1;
      this.AppendKeyword("INSERT");
      this.AppendComment(insertStmt.Comments[++i]);
      this.AppendString(" ");

      if(insertStmt.OnConflict != ConflictType.None) {
        this.AppendKeyword("OR");
        this.AppendComment(insertStmt.Comments[++i]);
        this.AppendString(" ");
        this.AppendKeyword(insertStmt.OnConflict.ToString().ToUpper());
        this.AppendComment(insertStmt.Comments[++i]);
        this.AppendString(" ");
      }
      if(insertStmt.HasIntoKeyword) {
        this.AppendKeyword("INTO");
        this.AppendComment(insertStmt.Comments[++i]);
        this.AppendString(" ");
      }
    }

    public override void VisitOnValues(InsertValuesStmt insertValuesStmt, int offset) {
      this.AppendKeyword(" VALUES");
      this.AppendComment(insertValuesStmt.Comments[offset]);
    }

    public override void VisitBeforeQuery(InsertSelectStmt insertSelectStmt, int offset) {
      this.AppendString(" ");
    }

    public override void VisitBefore(IfStmt ifStmt) {
      this.AppendComment(ifStmt.HeaderComment);
      this.AppendKeyword("IF");
      this.AppendComment(ifStmt.Comments[0]);
      this.AppendString(" ");
    }

    public override void VisitOnThen(IfStmt ifStmt, int ifThenIndex, int offset) {
      this.AppendKeyword(" THEN");
      this.AppendComment(ifStmt.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnElsIf(IfStmt ifStmt, int ifThenIndex, int offset) {
      this.AppendKeyword(" ELSIF");
      this.AppendComment(ifStmt.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnElse(IfStmt ifStmt, int offset) {
      this.AppendKeyword(" ELSE");
      this.AppendComment(ifStmt.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnEndIf(IfStmt ifStmt, int offset) {
      this.AppendKeyword(" END");
      this.AppendComment(ifStmt.Comments[offset]);
      this.AppendString(" ");
      this.AppendKeyword("IF");
      this.AppendComment(ifStmt.Comments[offset + 1]);
      this.AppendString(" ");
    }

    public override void VisitOnDelete(DeleteStmt deleteStmt) {
      var i = -1;
      this.AppendKeyword("DELETE");
      this.AppendComment(deleteStmt.Comments[++i]);
      this.AppendString(" ");

      if(deleteStmt.HasFromKeyword) {
        this.AppendKeyword("FROM");
        this.AppendComment(deleteStmt.Comments[++i]);
        this.AppendString(" ");
      }
    }

    public override void VisitOnFrom2(DeleteStmt deleteStmt, int offset) {
      this.AppendKeyword(" FROM");
      this.AppendComment(deleteStmt.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnWhere(DeleteStmt deleteStmt, int offset) {
      this.AppendKeyword(" WHERE");
      this.AppendComment(deleteStmt.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnMerge(MergeStmt mergeStmt) {
      this.AppendKeyword("MERGE");
      this.AppendComment(mergeStmt.Comments[0]);
      this.AppendKeyword(" INTO");
      this.AppendComment(mergeStmt.Comments[1]);
      this.AppendString(" ");
    }

    public override void VisitOnUsing(MergeStmt mergeStmt, int offset) {
      this.AppendKeyword(" USING");
      this.AppendComment(mergeStmt.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnOn(MergeStmt mergeStmt, int offset) {
      this.AppendKeyword(" ON");
      this.AppendComment(mergeStmt.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitBefore(MergeUpdateClause updateClause) {
      this.AppendKeyword(" WHEN");
      this.AppendComment(updateClause.Comments[0]);
      this.AppendKeyword(" MATCHED");
      this.AppendComment(updateClause.Comments[1]);
      this.AppendKeyword(" THEN");
      this.AppendComment(updateClause.Comments[2]);
      this.AppendKeyword(" UPDATE");
      this.AppendComment(updateClause.Comments[3]);
      this.AppendKeyword(" SET");
      this.AppendComment(updateClause.Comments[4]);
      this.AppendString(" ");
    }

    public override void VisitBefore(MergeInsertClause insertClause) {
      this.AppendKeyword(" WHEN");
      this.AppendComment(insertClause.Comments[0]);
      this.AppendKeyword(" NOT");
      this.AppendComment(insertClause.Comments[1]);
      this.AppendKeyword(" MATCHED");
      this.AppendComment(insertClause.Comments[2]);
      this.AppendKeyword(" THEN");
      this.AppendComment(insertClause.Comments[3]);
      this.AppendKeyword(" INSERT");
      this.AppendComment(insertClause.Comments[4]);
    }

    public override void VisitOnValues(MergeInsertClause insertClause, int offset) {
      this.AppendKeyword(" VALUES");
      this.AppendComment(insertClause.Comments[offset]);
    }

    public override void VisitBefore(ILimitClause limitClause) {
      if(limitClause.Type == LimitClauseType.Limit) {
        this.AppendKeyword(" LIMIT");
        this.AppendComment(limitClause.Comments[0]);
        this.AppendString(" ");
      } else if(limitClause.Type == LimitClauseType.OffsetFetch) {
        int i = -1;
        var offsetFetchClause = (OffsetFetchClause)limitClause;
        if(offsetFetchClause.HasOffset) {
          this.AppendKeyword(" OFFSET");
          this.AppendComment(offsetFetchClause.Comments[++i]);
          this.AppendString(" ");

          this.AppendString(offsetFetchClause.Offset.ToString());
          this.AppendComment(offsetFetchClause.Comments[++i]);

          this.AppendKeyword(" ROWS");
          this.AppendComment(offsetFetchClause.Comments[++i]);
        }

        if(offsetFetchClause.HasFetch) {
          this.AppendKeyword(" FETCH");
          this.AppendComment(offsetFetchClause.Comments[++i]);

          if(offsetFetchClause.FetchFromFirst) {
            this.AppendKeyword(" FIRST");
          } else {
            this.AppendKeyword(" NEXT");
          }
          this.AppendComment(offsetFetchClause.Comments[++i]);
          this.AppendString(" ");

          this.AppendString(offsetFetchClause.Fetch.ToString());
          this.AppendComment(offsetFetchClause.Comments[++i]);

          if(offsetFetchClause.FetchRowCountType == FetchRowCountType.Percentile) {
            this.AppendKeyword(" PERCENT");
            this.AppendComment(offsetFetchClause.Comments[++i]);
          }

          this.AppendKeyword(" ROWS");
          this.AppendComment(offsetFetchClause.Comments[++i]);

          if(offsetFetchClause.FetchWithTies) {
            this.AppendKeyword(" WITH");
            this.AppendComment(offsetFetchClause.Comments[++i]);
            this.AppendKeyword(" TIES");
            this.AppendComment(offsetFetchClause.Comments[++i]);
          } else {
            this.AppendKeyword(" ONLY");
            this.AppendComment(offsetFetchClause.Comments[++i]);
          }
        }
      }
    }

    public override void VisitOnOffset(ILimitClause limitClause, int offset) {
      this.AppendKeyword(" OFFSET");
      this.AppendComment(limitClause.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitBefore(ForUpdateClause forUpdateClause) {
      this.AppendKeyword(" FOR");
      this.AppendComment(forUpdateClause.Comments[0]);
      this.AppendKeyword(" UPDATE");
      this.AppendComment(forUpdateClause.Comments[1]);
    }

    public override void VisitAfter(ForUpdateClause forUpdateClause) {
      if(forUpdateClause.WaitType == WaitType.Wait) {
        this.AppendKeyword(" WAIT");
        if(forUpdateClause.HasWaitTime) {
          this.AppendComment(forUpdateClause.Comments[forUpdateClause.Comments.Count - 2]);
          this.AppendString(" ");
          this.AppendString(forUpdateClause.WaitTime.ToString());
        }
        this.AppendComment(forUpdateClause.Comments.Last);
      } else if(forUpdateClause.WaitType == WaitType.NoWait) {
        this.AppendKeyword(" NOWAIT");
        this.AppendComment(forUpdateClause.Comments.Last);
      } else if(forUpdateClause.WaitType == WaitType.SkipLocked) {
        this.AppendKeyword(" SKIP");
        this.AppendComment(forUpdateClause.Comments[forUpdateClause.Comments.Count - 2]);
        this.AppendKeyword(" LOCKED");
        this.AppendComment(forUpdateClause.Comments.Last);
      }
    }

    public override void VisitBefore(ForUpdateOfClause forUpdateOfClause) {
      this.AppendKeyword(" OF");
      this.AppendComment(forUpdateOfClause.Comments[0]);
      this.AppendString(" ");
    }

    public override void VisitBefore(AliasedQuery aliasedQuery) {
      this.AppendSymbol("(");
      this.AppendComment(aliasedQuery.Comments[0]);
    }

    public override void VisitAfter(AliasedQuery aliasedQuery) {
      var i = 0;
      this.AppendSymbol(")");
      this.AppendComment(aliasedQuery.Comments[++i]);

      if(aliasedQuery.HasAs) {
        this.AppendKeyword(" AS");
        this.AppendComment(aliasedQuery.Comments[++i]);
        this.AppendString(" ");
      }

      if(!string.IsNullOrEmpty(aliasedQuery.AliasName)) {
        this.AppendString(aliasedQuery.AliasName);
        this.AppendComment(aliasedQuery.Comments[++i]);
      }
    }

    public override void VisitBefore(CallStmt callStmt) {
      this.AppendComment(callStmt.HeaderComment);

      var i = -1;
      this.AppendKeyword("CALL");
      this.AppendComment(callStmt.Comments[++i]);
      this.AppendString(" ");

      if(!string.IsNullOrEmpty(callStmt.ServerName)) {
        this.AppendString(callStmt.ServerName);
        this.AppendComment(callStmt.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(callStmt.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(callStmt.DataBaseName)) {
        this.AppendString(callStmt.DataBaseName);
        this.AppendComment(callStmt.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(callStmt.Comments[++i]);
      }
      if(!string.IsNullOrEmpty(callStmt.SchemaName)) {
        this.AppendString(callStmt.SchemaName);
        this.AppendComment(callStmt.Comments[++i]);
        this.AppendSymbol(".");
        this.AppendComment(callStmt.Comments[++i]);
      }
      this.AppendString(callStmt.Name);
      this.AppendComment(callStmt.Comments[++i]);
    }

    public override void VisitBefore(TruncateStmt truncateStmt) {
      this.AppendComment(truncateStmt.HeaderComment);
      this.AppendKeyword("TRUNCATE");
      this.AppendComment(truncateStmt.Comments[0]);
      this.AppendString(" ");
      this.AppendKeyword("TABLE");
      this.AppendComment(truncateStmt.Comments[1]);
      this.AppendString(" ");
    }

    public override void VisitBefore(SqlitePragmaStmt pragmaStmt) {
      this.AppendComment(pragmaStmt.HeaderComment);
      this.AppendKeyword("PRAGMA");
      this.AppendComment(pragmaStmt.Comments[0]);
      this.AppendString(" ");
      this.AppendKeyword("TABLE_INFO");
      this.AppendComment(pragmaStmt.Comments[1]);
    }

    public override void VisitBefore(NullStmt nullStmt) {
      this.AppendComment(nullStmt.HeaderComment);
    }

    public override bool VisitOnFromFirstInQuery {
      get {
        return false;
      }
    }

    #region Unused Methods
    public override void VisitAfter(SignedNumberExpr expr) { }
    public override void VisitAfter(BitwiseNotExpr expr) { }
    public override void VisitBefore(BinaryOpExpr expr) { }
    public override void VisitAfter(BinaryOpExpr expr) { }
    public override void VisitBefore(BinaryOpPredicate predicate) { }
    public override void VisitAfter(BinaryOpPredicate predicate) { }
    public override void VisitAfter(NotPredicate notPredicate) { }
    public override void VisitBefore(AndPredicate andPredicate) { }
    public override void VisitAfter(AndPredicate andPredicate) { }
    public override void VisitBefore(OrPredicate orPredicate) { }
    public override void VisitAfter(OrPredicate orPredicate) { }
    public override void VisitBefore(LikePredicate predicate) { }
    public override void VisitAfter(LikePredicate predicate) { }
    public override void VisitBefore(IsNullPredicate predicate) { }
    public override void VisitBefore(IsPredicate predicate) { }
    public override void VisitAfter(IsPredicate predicate) { }
    public override void VisitBefore(BetweenPredicate predicate) { }
    public override void VisitAfter(BetweenPredicate predicate) { }
    public override void VisitBefore(InPredicate predicate) { }
    public override void VisitBefore(SubQueryPredicate predicate) { }
    public override void VisitBefore(CollatePredicate predicate) { }
    public override void VisitBefore(Assignment assignment) { }
    public override void VisitAfter(Assignment assignment) { }
    public override void VisitBefore(Assignments assignments) { }
    public override void VisitAfter(Assignments assignments) { }
    public override void VisitAfter(WithClause withClause) { }
    public override void VisitBefore(WithDefinition withDefinition) { }
    public override void VisitBefore(CompoundQueryClause compoundQuery) { }
    public override void VisitAfter(CompoundQueryClause compoundQuery) { }
    public override void VisitBefore(BracketedQueryClause bracketedQuery) { }
    public override void VisitAfter(BracketedQueryClause bracketedQuery) { }
    public override void VisitAfter(SingleQueryClause query) { }
    //public override void VisitAfter(SingleQuery query) { }
    //public override void VisitBefore(CompoundQuery compoundQuery) { }
    //public override void VisitAfter(CompoundQuery compoundQuery) { }
    //public override void VisitBefore(BracketedQuery bracketedQuery){ }
    //public override void VisitAfter(BracketedQuery bracketedQuery){ }
    public override void VisitBefore(ResultColumns resultColumns) { }
    public override void VisitAfter(ResultColumns resultColumns) { }
    public override void VisitBefore(ResultExpr resultExpr) { }
    public override void VisitBefore(ColumnNames columns) { }
    public override void VisitAfter(ColumnNames columns) { }
    public override void VisitBefore(UnqualifiedColumnNames columns) { }
    public override void VisitAfter(UnqualifiedColumnNames columns) { }
    public override void VisitBefore(ValuesList valuesList) { }
    public override void VisitAfter(ValuesList valuesList) { }
    public override void VisitBefore(Values values) { }
    public override void VisitAfter(Values values) { }
    public override void VisitBefore(Exprs exprs) { }
    public override void VisitAfter(Exprs exprs) { }
    public override void VisitBefore(JoinSource joinSource) { }
    public override void VisitAfter(JoinSource joinSource) { }
    public override void VisitBefore(CommaJoinSource commaJoinSource) { }
    public override void VisitAfter(CommaJoinSource commaJoinSource) { }
    public override void VisitAfter(GroupBy groupBy) { }
    public override void VisitAfter(OrderBy orderBy) { }
    public override void VisitBefore(OrderingTerm orderingTerm) { }
    public override void VisitAfter(PartitionBy partitionBy) { }
    public override void VisitBefore(PartitioningTerm partitioningTerm) { }
    public override void VisitAfter(ILimitClause limitClause) { }
    public override void VisitAfter(ForUpdateOfClause forUpdateOfClause) { }
    public override void VisitBefore(SelectStmt selectStmt) {
      this.AppendComment(selectStmt.HeaderComment);
    }
    public override void VisitAfter(SelectStmt selectStmt) { }
    public override void VisitBefore(UpdateStmt updateStmt) {
      this.AppendComment(updateStmt.HeaderComment);
    }
    public override void VisitAfter(UpdateStmt updateStmt) { }
    public override void VisitBefore(InsertStmt insertStmt) {
      this.AppendComment(insertStmt.HeaderComment);
    }
    public override void VisitAfter(InsertStmt insertStmt) { }
    public override void VisitOnDefaultValues(InsertStmt insertStmt, int offset) { }
    public override void VisitAfter(IfStmt ifStmt) { }
    public override void VisitBefore(DeleteStmt deleteStmt) {
      this.AppendComment(deleteStmt.HeaderComment);
    }
    public override void VisitAfter(DeleteStmt deleteStmt) { }
    public override void VisitBefore(MergeStmt mergeStmt) {
      this.AppendComment(mergeStmt.HeaderComment);
    }
    public override void VisitAfter(MergeStmt mergeStmt) { }
    public override void VisitAfter(MergeUpdateClause updateClause) { }
    public override void VisitAfter(MergeInsertClause insertClause) { }
    public override void VisitAfter(CallStmt callStmt) { }
    public override void VisitAfter(TruncateStmt truncateStmt) { }
    public override void VisitAfter(SqlitePragmaStmt pragmaStmt) { }
    public override void VisitAfter(NullStmt nullStmt) { }
    #endregion
  }
}
