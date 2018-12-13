using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace MiniSqlParser
{
  public class BeautifulStringifier: SetPlaceHoldersVisitor
  {
    // SELECT句や比較式の縦位置を揃えるためにノードに付加する情報
    private class AlignVertical
    {
      // 対象ノードの文字列行の長さ
      public int LineLength { get; set; }
      // 出力文字列における対象ノードの位置
      public int IndexOfWhole { get; set; }
      public AlignVertical(int lineLength, int indexOfNewline) {
        this.LineLength = lineLength;
        this.IndexOfWhole = indexOfNewline;
      }
    }

    public enum JoinIndentType
    {
      A,
      B
    }

    public enum KeywordCase
    {
      Upper,
      Lower,
      Capitalization
    }

    private class StmtIndentInfo
    {
      public int StmtStartPos;
      public int AndorFirstOperand;
      public int AndorNestLevel;
      public int JoinNestLevel;
      public StmtIndentInfo(int stmtStartPos
                          , int andorFirstOperand = 0
                          , int andorNestLevel = 0
                          , int joinNestLevel = 0) {
        this.StmtStartPos = stmtStartPos;
        this.AndorFirstOperand = andorFirstOperand;
        this.AndorNestLevel = andorNestLevel;
        this.JoinNestLevel = joinNestLevel;
      }
    }

    private static readonly string _newline = System.Environment.NewLine;
    private readonly int _maxLineLength;
    private readonly bool _printComments;

    private readonly StringBuilder _sql;

    // インデントサイズ分の空白文字列
    private readonly string m_indentSpaces;

    // インデントサイズ分の空白文字列
    // (カンマ直前のインデントサイズはカンマ1文字分差し引く)
    private readonly string m_indentSpaces2;

    // AND/OR階層のネストレベルによるインデント
    private readonly string m_andorNestLevelIndent;

    // AND/OR演算子の最初の被演算子のインデント
    private readonly string m_andorFirstOperandIndent;

    // AND/ORの左被演算子内における改行後のインデント
    private readonly string m_andorLeftOperandIndent;

    // JOIN演算子の最初の被演算子のインデント
    private readonly string m_joinFirstOperandIndent;

    // QueryExpressionのネストレベル
    private int m_queryNestLevel;

    // AND/OR演算子のネストレベル
    //private int m_andorNestLevel;

    // AND/OR演算子の左被演算子の場合1
    //private int m_andorFirstOperand;

    // 括弧のネストレベル
    private int m_bracketNestLevel;

    // CASE式のネストレベル
    private int m_caseNestLevel;

    // インデントサイズ
    private readonly int m_indentSize = 4;

    // JOINキーワードのインデント方式
    private readonly JoinIndentType _joinIndent;

    // 予約語の大文字小文字
    private readonly KeywordCase _keywordCase;

    // 現在の列位置
    private int m_currentColumnPos;

    // 最後に出力したトークン(キーワード/記号)
    private string _lastAppendToken;

    // 文の開始列位置
    private readonly Stack<StmtIndentInfo> m_stmtStartPos;

    // 括弧の開始位置
    private readonly Stack<int> _bracketedStartPos;

    public BeautifulStringifier(int maxLineLength,
                                Dictionary<string, Node> placeHolders)
      : this(maxLineLength, 4, KeywordCase.Upper, JoinIndentType.A, false, placeHolders) {
    }

    public BeautifulStringifier(int maxLineLength,
                                int indentSize = 4,
                                KeywordCase keywordCase = KeywordCase.Upper,
                                JoinIndentType joinIndent = JoinIndentType.A,
                                bool printComments = false,
                                Dictionary<string, Node> placeHolders = null) : base(placeHolders)
    {
      _sql = new StringBuilder();
      _maxLineLength = maxLineLength;

      m_indentSize = indentSize;
      _keywordCase = keywordCase;
      _joinIndent = joinIndent;
      _printComments = printComments;

      m_indentSpaces = new string(' ', m_indentSize);
      m_indentSpaces2 = new string(' ', m_indentSize-1);
      m_andorNestLevelIndent = new string(' ', 4);
      m_andorFirstOperandIndent = new string(' ', 4);
      m_andorLeftOperandIndent = new string(' ', 4);
      m_joinFirstOperandIndent = new string(' ', 5);

      m_stmtStartPos = new Stack<StmtIndentInfo>();
      _bracketedStartPos = new Stack<int>();
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
        this.AppendString(str);
      }
    }

    // SQL文の先頭コメントがC言語コメントの場合は、末尾で改行する
    private void AppendHeaderComment(string str) {
      if(!_printComments || str == null) {
        return;
      }
      this.AppendComment(str);
      if(str.EndsWith("*/")) {
        // this.AppendString(_newline);
        this.AppendNewLine();
      }
    }

    private void AppendKeyword(string str) {
      if(_keywordCase == KeywordCase.Upper) {
        _lastAppendToken = str;
      } else if(_keywordCase == KeywordCase.Lower) {
        _lastAppendToken = str.ToLower();
      } else if(_keywordCase == KeywordCase.Capitalization) {
        _lastAppendToken = str[0].ToString().ToUpper() 
                         + str.Substring(1,str.Length-1).ToLower();
      }
      this.AppendString(_lastAppendToken);
    }

    private void AppendSymbol(string str) {
      _lastAppendToken = str;
      this.AppendString(str);
    }

    private void AppendString(Identifier id) {
      this.AppendString(id.ToRawString());
    }

    private void AppendString(string str) {
      m_currentColumnPos += str.Length;
      _sql.Append(str);
    }

    private void AppendNewLine() {
      m_currentColumnPos = 0;
      _sql.Append(_newline);

      if(m_stmtStartPos.Count > 0) {
        // AND/ORの左被演算子におけるインデントの挿入
        // ただしSELECT文のネストレベルは差し引く
        // ただしCASE式のネストレベルは差し引く
        for(int i = 0; i < m_stmtStartPos.Peek().AndorFirstOperand; ++i) {
          this.AppendString(m_andorLeftOperandIndent);
        }

        // AND/OR階層のネストレベルによるインデントの挿入
        // ただしSELECT文のネストレベルは差し引く
        for(int i = 0; i < m_stmtStartPos.Peek().AndorNestLevel; ++i) {
          this.AppendString(m_andorNestLevelIndent);
        }
      }

      // QueryとCaseExprのネストレベルによるインデントの挿入
      if(m_stmtStartPos.Count > 0) {
        this.AppendString(new string(' ', m_stmtStartPos.Peek().StmtStartPos));
      }
    }

    // 削除対象のSQL文字列を末尾から走査し指定文字cがあれば
    // 末尾からcまでの空白/改行文字も含め削除する
    private void RemoveTailCharIf(char c) {
      if(_sql.Length == 0) {
        return;
      }

      int i = _sql.Length -1;
      int j = 0;
      while(char.IsWhiteSpace(_sql[i])) {
        --i;
        ++j;
      }
      
      if(_sql[i] == c) {
        _sql.Remove(i, j + 1);
      }
    }

    public override void VisitOnSeparator(Node node, int offset, int i) {
      this.AppendNewLine();
      this.AppendString(m_indentSpaces2);
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
      // 括弧の開始位置を保持する
      _bracketedStartPos.Push(m_currentColumnPos);

      this.AppendSymbol("(");
      this.AppendComment(node.Comments[offset]);
      this.AppendNewLine();
      this.AppendString(m_indentSpaces);
    }

    public override void VisitOnRParen(Node node, int offset) {
      // 括弧の開始位置を戻す
      _bracketedStartPos.Pop();

      this.AppendSymbol(")");
      this.AppendComment(node.Comments[offset]);
      this.AppendNewLine();
    }

    public override void VisitOnWildCard(Node node, int offset) {
      this.AppendSymbol("*");
      this.AppendComment(node.Comments[offset]);
    }

    public override void VisitOnStmtSeparator(Stmt stmt, int offset, int i) {
      this.AppendSymbol(";");
      this.AppendComment(stmt.Comments[offset + i]);
      // 文末の";"が複数存在する場合は、最後の";"の直後に改行を挿入する
      if(i == stmt.StmtSeparators - 1) {
        this.AppendNewLine();
        // IF文内の場合は、";"の後の文もインデントする
        if(stmt.Parent.GetType() == typeof(Stmts) &&
           ((Stmts)stmt.Parent).GetType() == typeof(IfStmt)) {
             this.AppendString(m_indentSpaces);
        }
      }
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
      this.AppendString(" ");
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
      this.AppendString(" ");
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
      this.AppendKeyword(" OVER");
      this.AppendComment(expr.Comments[offset]);
      this.AppendSymbol(" (");
      // OVER句の開始列位置を保持する
      m_stmtStartPos.Push(new StmtIndentInfo(m_currentColumnPos));
      this.AppendComment(expr.Comments[offset + 1]);
    }

    public override void VisitAfter(WindowFuncExpr expr) {
      this.AppendSymbol(")");
      this.AppendComment(expr.Comments.Last);
      // OVER句の開始列位置を戻す
      m_stmtStartPos.Pop();
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
      // CASEの開始列位置を保持する
      m_stmtStartPos.Push(new StmtIndentInfo(m_currentColumnPos));
      // CASE式のネストレベルをカウントアップする
      ++m_caseNestLevel;
      this.AppendKeyword("CASE");
      this.AppendComment(expr.Comments[0]);
      if(expr.IsSimpleCase) {
        this.AppendString(" ");
      }
    }

    public override void VisitOnWhen(CaseExpr expr, int i) {
      this.AppendNewLine();
      this.AppendString(m_indentSpaces);
      this.AppendKeyword("WHEN");
      this.AppendComment(expr.Comments[i + 1]);
      this.AppendString(" ");
    }

    public override void VisitOnThen(CaseExpr expr, int i) {
      this.AppendKeyword(" THEN");
      this.AppendComment(expr.Comments[i + 1]);
      this.AppendString(" ");
    }

    public override void VisitOnElse(CaseExpr expr) {
      this.AppendNewLine();
      this.AppendString(m_indentSpaces);
      this.AppendKeyword("ELSE");
      this.AppendComment(expr.Comments[expr.Comments.Count - 2]);
      this.AppendString(" ");
    }

    public override void VisitAfter(CaseExpr expr) {
      this.AppendNewLine();
      this.AppendKeyword("END");
      this.AppendComment(expr.Comments.Last);
      // CASE式のネストレベルをカウントダウンする
      --m_caseNestLevel;
      // CASEの開始列位置を戻す
      m_stmtStartPos.Pop();
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
      predicate.Attachment = new AlignVertical(m_currentColumnPos, _sql.Length);

      this.AppendString(" ");
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
      this.AppendString(" ");
    }

    public override void VisitBefore(NotPredicate notPredicate) {
      this.AppendKeyword("NOT");
      this.AppendComment(notPredicate.Comments[0]);
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
      this.AppendSymbol(" (");
      this.AppendComment(predicate.Comments[i]);
    }

    public override void VisitAfter(InPredicate predicate) {
      this.AppendSymbol(")");
      this.AppendComment(predicate.Comments.Last);
    }

    public override void Visit(SubQueryPredicate predicate, int offset) {
      this.AppendString(" ");
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
      this.AppendSymbol(" (");
      this.AppendComment(predicate.Comments[offset + 2]);
    }

    public override void VisitAfter(SubQueryPredicate predicate) {
      this.AppendSymbol(")");
      this.AppendComment(predicate.Comments.Last);
    }

    public override void VisitBefore(ExistsPredicate predicate) {
      this.AppendKeyword("EXISTS");
      this.AppendComment(predicate.Comments[0]);
      this.AppendSymbol(" (");
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
      // 直前に開き括弧を出力した場合は、改行してインデントする
      if(_lastAppendToken == "(") {
        this.AppendNewLine();
        var spaces = _bracketedStartPos.Peek() - m_currentColumnPos;
        if(spaces > 0) {
          this.AppendString(new string(' ', spaces));
        }
        this.AppendString(m_indentSpaces);
      }

      // 括弧の開始位置を保持する
      _bracketedStartPos.Push(m_currentColumnPos);

      this.AppendSymbol("(");
      this.AppendComment(predicate.Comments[0]);

      ++m_bracketNestLevel;
    }

    public override void VisitAfter(BracketedPredicate predicate) {
      // 括弧の開始位置を戻す
      var rParen = _bracketedStartPos.Pop();

      // 直前に閉じ括弧を出力した場合は、改行してインデントする
      if(_lastAppendToken == ")") {
        this.AppendNewLine();
        this.AppendString(new string(' ',  rParen - m_currentColumnPos));
      }

      this.AppendSymbol(")");
      this.AppendComment(predicate.Comments.Last);

      --m_bracketNestLevel;
    }

    public override void Visit(Assignment assignment) {
      assignment.Attachment = new AlignVertical(m_currentColumnPos, _sql.Length);

      this.AppendSymbol(" =");
      this.AppendComment(assignment.Comments[0]);
      this.AppendString(" ");
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
      this.AppendNewLine();
      this.AppendString(m_indentSpaces);
    }

    public override void VisitAfter(WithDefinition withDefinition) {
      this.AppendSymbol(")");
      this.AppendComment(withDefinition.Comments.Last);
    }

    public override void VisitOnCompoundOp(CompoundQueryClause compoundQuery, int offset) {
      this.AppendNewLine();

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

      if(op == CompoundType.UnionAll) {
        this.AppendKeyword(" ALL");
        this.AppendComment(compoundQuery.Comments[offset + 1]);
        this.AppendString(" ");
      }
      this.AppendNewLine();
    }

    public override void VisitBefore(SingleQueryClause query) {
      if(query.GetType() == typeof(SingleQuery)){
        // SELECTの開始列位置を保持する
        m_stmtStartPos.Push(new StmtIndentInfo(m_currentColumnPos));
        // SELECT文内のネストレベルは1とする
        ++m_queryNestLevel;
      }

      var i = -1;
      this.AppendKeyword("SELECT");
      this.AppendComment(query.Comments[++i]);

      if(query.Quantifier == QuantifierType.Distinct) {
        this.AppendKeyword(" DISTINCT");
        this.AppendComment(query.Comments[++i]);
      } else if(query.Quantifier == QuantifierType.All) {
        this.AppendKeyword(" ALL");
        this.AppendComment(query.Comments[++i]);
      }

      if(query.HasTop) {
        this.AppendKeyword(" TOP");
        this.AppendComment(query.Comments[++i]);
        this.AppendString(" ");
        this.AppendString(query.Top.ToString());
        this.AppendComment(query.Comments[++i]);
      }
      
      // SELECT句の文字列長が短い場合はSELECT句は1行にまとめる
      if(query.HasWildcard ||
          (query.Results.Count == 1 &&
            (query.Results[0].IsTableWildcard ||
              (string.IsNullOrEmpty(((ResultExpr)query.Results[0]).AliasName) &&
                (((ResultExpr)query.Results[0]).Value is Literal ||
                 ((ResultExpr)query.Results[0]).Value is Column
                )
              )
            )
          )
        ) {
        // SELECTキーワードの直後に改行しない
        this.AppendString(" ");
      } else {
        this.AppendNewLine();
        this.AppendString(m_indentSpaces);
      }
    }

    public override void VisitOnFrom(SingleQueryClause query, int offset) {
      // FROM句が1つのテーブルのみの場合は1行にまとめる
      if(query.From.GetType() == typeof(Table) &&
         string.IsNullOrEmpty(((Table)query.From).IndexName) &&
         !((Table)query.From).HasNotIndexed) {

        // SELECT句とFROM句の文字列長が短い場合はFROMキーワードの直前に改行しない
        if(query.HasWildcard ||
            (query.Results.Count == 1 &&
              (query.Results[0].IsTableWildcard ||
                (string.IsNullOrEmpty(((ResultExpr)query.Results[0]).AliasName) &&
                  (((ResultExpr)query.Results[0]).Value is Literal ||
                  ((ResultExpr)query.Results[0]).Value is Column
                  )
                )
              )
            )
          ) {
          this.AppendString(" ");
        } else {
          this.AppendNewLine();
        }
        this.AppendKeyword("FROM");
        this.AppendComment(query.Comments[offset]);
        // FROMキーワードの直後に改行しない
        this.AppendString(" ");
      } else {
        this.AppendNewLine();
        this.AppendKeyword("FROM");
        this.AppendComment(query.Comments[offset]);
        this.AppendNewLine();
        this.AppendString(m_indentSpaces);
      }
    }

    public override void VisitOnWhere(SingleQueryClause query, int offset) {
      this.AppendNewLine();
      this.AppendKeyword("WHERE");
      this.AppendComment(query.Comments[offset]);
      if(m_indentSize > 1) {
        var spaces = m_indentSize - 1;
        this.AppendString(new string(' ', spaces));
      } else {
        this.AppendNewLine();
        this.AppendString(m_indentSpaces);
      }
    }

    public override void VisitOnHaving(SingleQueryClause query, int offset) {
      this.AppendNewLine();
      this.AppendKeyword("HAVING");
      this.AppendComment(query.Comments[offset]);
      this.AppendNewLine();
      this.AppendString(m_indentSpaces);
    }

    public override void VisitAfter(SingleQueryClause query) {
      if(query.GetType() == typeof(SingleQuery)) {
        // SELECT文内のネストレベルは1とする
        --m_queryNestLevel;
        // SELECTの開始列位置を戻す
        m_stmtStartPos.Pop();
      }
    }
    public override void VisitBefore(CompoundQueryClause compoundQuery) {
      if(compoundQuery.GetType() == typeof(CompoundQuery)) {
        // SELECTの開始列位置を保持する
        m_stmtStartPos.Push(new StmtIndentInfo(m_currentColumnPos));
        // SELECT文内のネストレベルは1とする
        ++m_queryNestLevel;
      }
    }
    public override void VisitAfter(CompoundQueryClause compoundQuery) {
      if(compoundQuery.GetType() == typeof(CompoundQuery)) {
        // SELECT文内のネストレベルは1とする
        --m_queryNestLevel;
        // SELECTの開始列位置を戻す
        m_stmtStartPos.Pop();
      }
    }
    public override void VisitBefore(BracketedQueryClause bracketedQuery) {
      if(bracketedQuery.GetType() == typeof(BracketedQuery)) {
        // SELECTの開始列位置を保持する
        m_stmtStartPos.Push(new StmtIndentInfo(m_currentColumnPos));
        // SELECT文内のネストレベルは1とする
        ++m_queryNestLevel;
      }
    }
    public override void VisitAfter(BracketedQueryClause bracketedQuery) {
      if(bracketedQuery.GetType() == typeof(BracketedQuery)) {
        // SELECT文内のネストレベルは1とする
        --m_queryNestLevel;
        // SELECTの開始列位置を戻す
        m_stmtStartPos.Pop();
      }
    }

    public override void VisitAfter(ResultExpr resultExpr) {
      if(resultExpr.HasAs || !string.IsNullOrEmpty(resultExpr.AliasName)) {
        resultExpr.Attachment = new AlignVertical(m_currentColumnPos, _sql.Length);
      }

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

      tableWildcard.Attachment = new AlignVertical(m_currentColumnPos, _sql.Length);
    }

    public override void Visit(UnqualifiedColumnName columnName) {
      this.AppendString(columnName.Name);
      this.AppendComment(columnName.Comments[0]);
    }

    public override void VisitBefore(BracketedSource bracketedSource) {
      // 括弧の開始位置を保持する
      _bracketedStartPos.Push(m_currentColumnPos);

      this.AppendSymbol("(");
      this.AppendComment(bracketedSource.Comments[0]);

      ++m_bracketNestLevel;
    }

    public override void VisitAfter(BracketedSource bracketedSource) {
      // 括弧の開始位置を戻す
      _bracketedStartPos.Pop();

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

      --m_bracketNestLevel;
    }

    public override void Visit(JoinOperator joinOperator) {
      // Rignt Nodeの走査前にNestレベルを上げる
      ++m_stmtStartPos.Peek().JoinNestLevel;

      if(joinOperator.HasNaturalKeyword ||
         joinOperator.JoinType != JoinType.None) {
        this.AppendNewLine();
        this.AppendString(m_indentSpaces);
        this.AppendBracketedSourceNestLevelSpaces();
      }
      var i = -1;
      if(joinOperator.HasNaturalKeyword) {
        this.AppendKeyword("NATURAL");
        this.AppendComment(joinOperator.Comments[++i]);
        this.AppendString(" ");
      }

      if(joinOperator.JoinType != JoinType.None) {
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

      if(_joinIndent == JoinIndentType.A) {
        if(joinOperator.JoinType != JoinType.None) {
          this.AppendString(" ");
        } else {
          this.AppendNewLine();
          this.AppendString(m_indentSpaces);
          this.AppendBracketedSourceNestLevelSpaces();
        }
      } else if(_joinIndent == JoinIndentType.B) {
        this.AppendNewLine();
        this.AppendString(m_indentSpaces);
        this.AppendBracketedSourceNestLevelSpaces();
      }

      this.AppendKeyword("JOIN");
      this.AppendComment(joinOperator.Comments[++i]);
      this.AppendString(" ");
    }

    public override void Visit(JoinSource joinSource) {
      if(joinSource.HasConstraint) {
        this.AppendNewLine();
        this.AppendString(m_indentSpaces);
        this.AppendBracketedSourceNestLevelSpaces();
        this.AppendKeyword("ON");
        this.AppendComment(joinSource.Comments[0]);
        this.AppendString("  ");
      } else if(joinSource.HasUsingConstraint) {
        this.AppendKeyword(" USING");
        this.AppendComment(joinSource.Comments[0]);
      }
    }
    private void AppendBracketedSourceNestLevelSpaces() {
      // 上段に"JOIN ("があればその文字列幅だけインデントする
      if(m_stmtStartPos.Peek().JoinNestLevel >= 1 && _bracketedStartPos.Count > 0) {
        var spaces = _bracketedStartPos.Peek() - m_currentColumnPos + 1;
        if(spaces > 0) {
          this.AppendString(new string(' ', spaces));
        }
      } else if(m_bracketNestLevel > 0) {
        // 括弧内であればその括弧分の空白文字を挿入する
        this.AppendString(" ");
      }
    }

    public override void VisitBefore(GroupBy groupBy) {
      this.AppendNewLine();
      this.AppendKeyword("GROUP");
      this.AppendComment(groupBy.Comments[0]);
      this.AppendKeyword(" BY");
      this.AppendComment(groupBy.Comments[1]);
      this.AppendNewLine();
      this.AppendString(m_indentSpaces);
    }

    public override void VisitBefore(OrderBy orderBy) {
      // OVER句の直前では改行しない
      if(  orderBy.Parent.GetType() != typeof(WindowFuncExpr) ||
         ((WindowFuncExpr)orderBy.Parent).PartitionBy != null) {
        this.AppendNewLine();
      }
      this.AppendKeyword("ORDER");
      this.AppendComment(orderBy.Comments[0]);
      this.AppendKeyword(" BY");
      this.AppendComment(orderBy.Comments[1]);
      this.AppendNewLine();
      this.AppendString(m_indentSpaces);
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
      this.AppendKeyword("PARTITION");
      this.AppendComment(partitionBy.Comments[0]);
      this.AppendKeyword(" BY");
      this.AppendComment(partitionBy.Comments[1]);
      this.AppendNewLine();
      this.AppendString(m_indentSpaces);
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

    public override void VisitBefore(UpdateStmt updateStmt) {
      this.AppendHeaderComment(updateStmt.HeaderComment);
      // UPDATEの開始列位置を保持する
      m_stmtStartPos.Push(new StmtIndentInfo(m_currentColumnPos));
    }

    public override void VisitOnUpdate(UpdateStmt updateStmt) {
      // UPDATE文内のネストレベルは1とする
      ++m_queryNestLevel;

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
      this.AppendNewLine();
      this.AppendString(m_indentSpaces);
    }

    public override void VisitOnFrom2(UpdateStmt updateStmt, int offset) {
      this.AppendNewLine();
      this.AppendKeyword("FROM");
      this.AppendComment(updateStmt.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnWhere(UpdateStmt updateStmt, int offset) {
      this.AppendNewLine();
      this.AppendKeyword("WHERE");
      this.AppendComment(updateStmt.Comments[offset]);
      if(m_indentSize > 1) {
        var spaces = m_indentSize - 1;
        this.AppendString(new string(' ', spaces));
      } else {
        this.AppendNewLine();
        this.AppendString(m_indentSpaces);
      }
    }

    public override void VisitAfter(UpdateStmt updateStmt) {
      // UPDATE文内のネストレベルは1とする
      --m_queryNestLevel;
      // UPDATEの開始列位置を戻す
      m_stmtStartPos.Pop();
    }

    public override void VisitBefore(InsertStmt insertStmt) {
      this.AppendHeaderComment(insertStmt.HeaderComment);
      // INSERTの開始列位置を保持する
      m_stmtStartPos.Push(new StmtIndentInfo(m_currentColumnPos));
    }

    public override void VisitOnInsert(InsertStmt insertStmt) {
      // INSERT文内のネストレベルは1とする
      ++m_queryNestLevel;

      var i = -1;
      if(insertStmt.IsReplaceStmt) {
        this.AppendKeyword("REPLACE");
      } else {
        this.AppendKeyword("INSERT");
      }
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
      if(insertValuesStmt.HasTableColumns) {
        // INSERT-VALUES文のVALUESの両端に括弧を配置する
        this.RemoveTailCharIf(')');
        this.AppendNewLine();
        this.AppendSymbol(")");
      } else {
        this.AppendNewLine();
      }
      this.AppendKeyword("VALUES");
      this.AppendComment(insertValuesStmt.Comments[offset]);
    }

    public override void VisitBeforeQuery(InsertSelectStmt insertSelectStmt, int offset) {
      if(insertSelectStmt.HasTableColumns) {
        // INSERT-SELECT文のテーブル列指定の閉括弧の前に改行する
        this.RemoveTailCharIf(')');
        this.AppendNewLine();
        this.AppendSymbol(")");
      }
      this.AppendNewLine();
    }

    public override void VisitAfter(InsertStmt insertStmt) {
      // INSERT文内のネストレベルは1とする
      --m_queryNestLevel;
      // INSERTの開始列位置を戻す
      m_stmtStartPos.Pop();
    }

    public override void VisitBefore(IfStmt ifStmt) {
      this.AppendHeaderComment(ifStmt.HeaderComment);
      // IFの開始列位置を保持する
      m_stmtStartPos.Push(new StmtIndentInfo(m_currentColumnPos));
      // IF文内のネストレベルは1とする
      ++m_queryNestLevel;

      this.AppendKeyword("IF");
      this.AppendComment(ifStmt.Comments[0]);
      this.AppendString(" ");
    }

    public override void VisitOnThen(IfStmt ifStmt, int ifThenIndex, int offset) {
      this.AppendKeyword(" THEN");
      this.AppendComment(ifStmt.Comments[offset]);
      this.AppendNewLine();
      // IF文岐内が";"を持たない1つのNullStmtだけの場合は、IF文内をインデントしない
      if(ifStmt.StatementsList[ifThenIndex].Count != 1 ||
         ifStmt.StatementsList[ifThenIndex][0].Type != StmtType.Null ||
         ifStmt.StatementsList[ifThenIndex][0].StmtSeparators > 0) {
        this.AppendString(m_indentSpaces);
      }
    }

    public override void VisitOnElsIf(IfStmt ifStmt, int ifThenIndex, int offset) {
      this.AppendNewLine();
      this.AppendKeyword("ELSIF");
      this.AppendComment(ifStmt.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnElse(IfStmt ifStmt, int offset) {
      this.AppendNewLine();
      this.AppendKeyword("ELSE");
      this.AppendComment(ifStmt.Comments[offset]);
      this.AppendNewLine();
      // IF文岐内が";"を持たない1つのNullStmtだけの場合は、IF文内をインデントしない
      if(ifStmt.ElseStatements.Count != 1 ||
         ifStmt.ElseStatements[0].Type != StmtType.Null ||
         ifStmt.ElseStatements[0].StmtSeparators > 0) {
        this.AppendString(m_indentSpaces);
      }
    }

    public override void VisitOnEndIf(IfStmt ifStmt, int offset) {
      this.AppendNewLine();
      this.AppendKeyword("END");
      this.AppendComment(ifStmt.Comments[offset]);
      this.AppendString(" ");
      this.AppendKeyword("IF");
      this.AppendComment(ifStmt.Comments[offset + 1]);
      this.AppendString(" ");

      // INSERT文内のネストレベルは1とする
      --m_queryNestLevel;
      // INSERTの開始列位置を戻す
      m_stmtStartPos.Pop();
    }

    public override void VisitBefore(DeleteStmt deleteStmt) {
      this.AppendHeaderComment(deleteStmt.HeaderComment);
      // DELETEの開始列位置を保持する
      m_stmtStartPos.Push(new StmtIndentInfo(m_currentColumnPos));
    }

    public override void VisitOnDelete(DeleteStmt deleteStmt) {
      // DELETE文内のネストレベルは1とする
      ++m_queryNestLevel;

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
      this.AppendNewLine();
      this.AppendKeyword("WHERE");
      this.AppendComment(deleteStmt.Comments[offset]);
      if(m_indentSize > 1) {
        var spaces = m_indentSize - 1;
        this.AppendString(new string(' ', spaces));
      } else {
        this.AppendNewLine();
        this.AppendString(m_indentSpaces);
      }
    }

    public override void VisitAfter(DeleteStmt deleteStmt) {
      // INSERT文内のネストレベルは1とする
      --m_queryNestLevel;
      // INSERTの開始列位置を戻す
      m_stmtStartPos.Pop();
    }

    public override void VisitBefore(MergeStmt mergeStmt) {
      this.AppendHeaderComment(mergeStmt.HeaderComment);
      // MERGEの開始列位置を保持する
      m_stmtStartPos.Push(new StmtIndentInfo(m_currentColumnPos));
    }

    public override void VisitOnMerge(MergeStmt mergeStmt) {
      // MERGE文内のネストレベルは1とする
      ++m_queryNestLevel;

      this.AppendKeyword("MERGE");
      this.AppendComment(mergeStmt.Comments[0]);
      this.AppendKeyword(" INTO");
      this.AppendComment(mergeStmt.Comments[1]);
      this.AppendString(" ");
    }

    public override void VisitOnUsing(MergeStmt mergeStmt, int offset) {
      this.AppendNewLine();
      this.AppendKeyword("USING");
      this.AppendComment(mergeStmt.Comments[offset]);
      this.AppendString(" ");
    }

    public override void VisitOnOn(MergeStmt mergeStmt, int offset) {
      this.AppendNewLine();
      this.AppendKeyword("ON");
      this.AppendComment(mergeStmt.Comments[offset]);
      this.AppendString("  ");
    }

    public override void VisitAfter(MergeStmt mergeStmt) { 
      // MERGE文内のネストレベルは1とする
      --m_queryNestLevel;
      // MERGEの開始列位置を戻す
      m_stmtStartPos.Pop();
    }

    public override void VisitBefore(MergeUpdateClause updateClause) {
      this.AppendNewLine();
      this.AppendKeyword("WHEN");
      this.AppendComment(updateClause.Comments[0]);
      this.AppendKeyword(" MATCHED");
      this.AppendComment(updateClause.Comments[1]);
      this.AppendKeyword(" THEN");
      this.AppendComment(updateClause.Comments[2]);
      this.AppendNewLine();
      this.AppendString(m_indentSpaces);
      // UPDATEの開始列位置を保持する
      m_stmtStartPos.Push(new StmtIndentInfo(m_currentColumnPos));
      this.AppendKeyword("UPDATE");
      this.AppendComment(updateClause.Comments[3]);
      this.AppendKeyword(" SET");
      this.AppendComment(updateClause.Comments[4]);
      this.AppendNewLine();
      this.AppendString(m_indentSpaces);
    }

    public override void VisitBefore(MergeInsertClause insertClause) {
      this.AppendNewLine();
      this.AppendKeyword("WHEN");
      this.AppendComment(insertClause.Comments[0]);
      this.AppendKeyword(" NOT");
      this.AppendComment(insertClause.Comments[1]);
      this.AppendKeyword(" MATCHED");
      this.AppendComment(insertClause.Comments[2]);
      this.AppendKeyword(" THEN");
      this.AppendComment(insertClause.Comments[3]);
      this.AppendNewLine();
      this.AppendString(m_indentSpaces);
      // INSERTの開始列位置を保持する
      m_stmtStartPos.Push(new StmtIndentInfo(m_currentColumnPos));
      this.AppendKeyword("INSERT");
      this.AppendComment(insertClause.Comments[4]);
    }

    public override void VisitOnValues(MergeInsertClause insertClause, int offset) {
      if(insertClause.HasTableColumns) {
        // INSERT-VALUES文のVALUESの両端に括弧を配置する
        this.RemoveTailCharIf(')');
        this.AppendNewLine();
        this.AppendSymbol(")");
      } else {
        this.AppendNewLine();
      }
      this.AppendKeyword("VALUES");
      this.AppendComment(insertClause.Comments[offset]);
    }

    public override void VisitBefore(ILimitClause limitClause) {
      this.AppendNewLine();
      if(limitClause.Type == LimitClauseType.Limit) {
        this.AppendKeyword("LIMIT");
        this.AppendComment(limitClause.Comments[0]);
        this.AppendString(" ");
      } else if(limitClause.Type == LimitClauseType.OffsetFetch) {
        int i = -1;
        var offsetFetchClause = (OffsetFetchClause)limitClause;
        if(offsetFetchClause.HasOffset) {
          this.AppendKeyword("OFFSET");
          this.AppendComment(offsetFetchClause.Comments[++i]);
          this.AppendString(" ");

          this.AppendString(offsetFetchClause.Offset.ToString());
          this.AppendComment(offsetFetchClause.Comments[++i]);

          this.AppendKeyword(" ROWS");
          this.AppendComment(offsetFetchClause.Comments[++i]);
        }

        if(offsetFetchClause.HasFetch) {
          if(offsetFetchClause.HasOffset) {
            this.AppendString(" ");
          }
          this.AppendKeyword("FETCH");
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
      // VisitAfter(IQuery)においてネストレベルを下げているため
      // 同じSELECT文内にもかかわらずFOR UPDATE句内のネストレベルが
      // 低くなる、そのためFOR UPDATE句内でネストレベルを1つ加算する
      ++m_queryNestLevel;
      this.AppendNewLine();
      this.AppendKeyword("FOR");
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
      // FOR UPDATE句内で加算したネストレベルを1つ減ずる
      --m_queryNestLevel;
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
      }

      if(!string.IsNullOrEmpty(aliasedQuery.AliasName)) {
        this.AppendString(" ");
        this.AppendString(aliasedQuery.AliasName);
        this.AppendComment(aliasedQuery.Comments[++i]);
      }
    }

    public override void VisitBefore(CallStmt callStmt) {
      this.AppendHeaderComment(callStmt.HeaderComment);

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
      this.AppendHeaderComment(pragmaStmt.HeaderComment);
      this.AppendKeyword("PRAGMA");
      this.AppendComment(pragmaStmt.Comments[0]);
      this.AppendString(" ");
      this.AppendKeyword("TABLE_INFO");
      this.AppendComment(pragmaStmt.Comments[1]);
    }

    public override void VisitBefore(NullStmt nullStmt) {
      this.AppendHeaderComment(nullStmt.HeaderComment);
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
    public override void VisitBefore(AndPredicate andPredicate) {
      //WHERE又は左括弧の直後の式(Expr)にAND又はORが続く場合は、インデントする
      if(andPredicate.Left.GetType() != typeof(AndPredicate) &&
         andPredicate.Left.GetType() != typeof(OrPredicate) ) {
        if(this.GetAncestorOfPredicate(andPredicate).GetType() != typeof(JoinSource)){
          // 直前に開き括弧を出力した場合は、インデントしない
          if(_lastAppendToken != "(") {
            // WHERE句の直後に改行を入れる場合は、インデントする
            if(m_indentSize < 2) {
              this.AppendString(m_andorFirstOperandIndent);
            }            
          }else if(andPredicate.Left.GetType() != typeof(BracketedPredicate)) {
            this.AppendString(m_andorFirstOperandIndent);
          }
        }
        ++m_stmtStartPos.Peek().AndorFirstOperand;
      }
    }

    public override void Visit(AndPredicate andPredicate) {
      //WHERE又は左括弧の直後の式(Expression)にAND又はORが続く場合は、インデントする
      if(andPredicate.Left.GetType() != typeof(AndPredicate) &&
         andPredicate.Left.GetType() != typeof(OrPredicate)) {
        --m_stmtStartPos.Peek().AndorFirstOperand;
      }

      this.AppendNewLine();
      this.AppendString(m_indentSpaces);

      // WHERE句の後に改行を入れるか否か
      int linebreakAfterWhere = m_indentSize < 2 ? 1 : 0;

      // 上段に"AND ("があればその文字列幅だけインデントする
      //if(m_stmtStartPos.Peek().AndorNestLevel - (m_queryNestLevel - 1) >= linebreakAfterWhere &&
      //   _bracketedStartPos.Count > 0) {
      if(m_stmtStartPos.Peek().AndorNestLevel >= linebreakAfterWhere &&
         _bracketedStartPos.Count > 0) {
        var spaces = _bracketedStartPos.Peek() - m_currentColumnPos + 1;
        if(spaces > 0) {
          this.AppendString(new string(' ', spaces));
        }
      } else if(m_bracketNestLevel > 0) {
        // 括弧内であればその括弧分の空白文字を挿入する
        this.AppendString(" ");
      }

      this.AppendKeyword("AND");
      this.AppendComment(andPredicate.Comments[0]);
      this.AppendString(" ");

      // Rignt Nodeの操作前にNestレベルを上げる
      ++m_stmtStartPos.Peek().AndorNestLevel;
    }

    public override void VisitAfter(AndPredicate andPredicate) {
      // AND連言の末尾ノードの場合は比較式の演算子を縦に揃える
      if(andPredicate.Parent.GetType() != typeof(AndPredicate) &&
         andPredicate.Parent.GetType() != typeof(OrPredicate) &&
         andPredicate.Right.GetType() != typeof(AndPredicate) &&
         andPredicate.Right.GetType() != typeof(OrPredicate)) {
        var maxLength = this.GetMaxLengthOfAndredicates(andPredicate);
        this.AlignAndPredicates(andPredicate, maxLength);
      }

      // Rignt Nodeの走査後にNestレベルを下げる
      --m_stmtStartPos.Peek().AndorNestLevel;
    }
    public override void VisitBefore(OrPredicate orPredicate) {
      //WHERE又は左括弧の直後の式(Expr)にAND又はORが続く場合は、インデントする
      if(orPredicate.Left.GetType() != typeof(AndPredicate) &&
         orPredicate.Left.GetType() != typeof(OrPredicate)  ) {
        if(this.GetAncestorOfPredicate(orPredicate).GetType() != typeof(JoinSource)) {
          // 直前に開き括弧を出力した場合は、インデントしない
          if(_lastAppendToken != "(") {
            // WHERE句の直後に改行を入れる場合は、インデントする
            if(m_indentSize < 2) {
              this.AppendString(m_andorFirstOperandIndent);
            }
          } else if(orPredicate.Left.GetType() != typeof(BracketedPredicate)) {
            this.AppendString(m_andorFirstOperandIndent);
          }
        }
        ++m_stmtStartPos.Peek().AndorFirstOperand;
      }
    }

    public override void Visit(OrPredicate orPredicate) {
      //WHERE又は左括弧の直後の式(Expression)にAND又はORが続く場合は、インデントする
      if(orPredicate.Left.GetType() != typeof(AndPredicate) &&
         orPredicate.Left.GetType() != typeof(OrPredicate)) {
        --m_stmtStartPos.Peek().AndorFirstOperand;
      }

      this.AppendNewLine();
      this.AppendString(m_indentSpaces);

      // WHERE句の後に改行を入れるか否か
      int linebreakAfterWhere = m_indentSize < 2 ? 1 : 0;

      // 上段に"AND ("があればその文字列幅だけインデントする
      //if(m_stmtStartPos.Peek().AndorNestLevel - (m_queryNestLevel - 1) >= linebreakAfterWhere &&
      //   _bracketedStartPos.Count > 0) {
      if(m_stmtStartPos.Peek().AndorNestLevel >= linebreakAfterWhere &&
         _bracketedStartPos.Count > 0) {
        var spaces = _bracketedStartPos.Peek() - m_currentColumnPos + 1;
        if(spaces > 0) {
          this.AppendString(new string(' ', spaces));
        }
      } else if(m_bracketNestLevel > 0) {
        // 括弧内であればその括弧分の空白文字を挿入する
        this.AppendString(" ");
      }

      this.AppendKeyword("OR ");
      this.AppendComment(orPredicate.Comments[0]);
      this.AppendString(" ");

      // Rignt Nodeの操作前にNestレベルを上げる
      ++m_stmtStartPos.Peek().AndorNestLevel;
    }

    public override void VisitAfter(OrPredicate orPredicate) {
      // OR連言の末尾ノードの場合は比較式の演算子を縦に揃える
      if(orPredicate.Parent.GetType() != typeof(AndPredicate) &&
         orPredicate.Parent.GetType() != typeof(OrPredicate) &&
         orPredicate.Right.GetType() != typeof(AndPredicate) &&
         orPredicate.Right.GetType() != typeof(OrPredicate)) {
        var maxLength = this.GetMaxLengthOfOrPredicates(orPredicate);
        this.AlignOrPredicates(orPredicate, maxLength);
      }

      // Rignt Nodeの走査後にNestレベルを下げる
      --m_stmtStartPos.Peek().AndorNestLevel;
    }

    // AND連言内の比較式において、その左辺式の最大の長さを取得する
    private int GetMaxLengthOfAndredicates(AndPredicate andPredicate) {
      int maxLength = 0;
      Predicate left = andPredicate;
      // AND連言は左ノードだけをたどった時の軌跡である
      while(left.GetType() == typeof(AndPredicate)) {
        var andNode = (AndPredicate)left;
        if(andNode.Right.GetType() == typeof(BinaryOpPredicate)) {
          var LengthBeforeOperator = ((AlignVertical)andNode.Right.Attachment).LineLength;
          maxLength = System.Math.Max(maxLength, LengthBeforeOperator);
        }
        left = andNode.Left;
      }
      // AND連言の末端ノード
      if(left.GetType() == typeof(BinaryOpPredicate)){
        var LengthBeforeOperator = ((AlignVertical)left.Attachment).LineLength;
        maxLength = System.Math.Max(maxLength, LengthBeforeOperator);
      }
      return maxLength;
    }

    // AND連言内の比較式において、縦位置を揃えるためにその左辺式に空白を挿入する
    private void AlignAndPredicates(AndPredicate andPredicate, int maxLength) {
      Predicate left = andPredicate;
      // AND連言は左ノードだけをたどった時の軌跡である
      while(left.GetType() == typeof(AndPredicate)) {
        var andNode = (AndPredicate)left;
        if(andNode.Right.GetType() == typeof(BinaryOpPredicate)) {
          var alignVertical = (AlignVertical)andNode.Right.Attachment;
          var pad = new string(' ', maxLength - alignVertical.LineLength);
          _sql.Insert(alignVertical.IndexOfWhole, pad);
        }
        left = andNode.Left;
      }
      // AND連言の末端ノード
      if(left.GetType() == typeof(BinaryOpPredicate)) {
        var alignVertical = (AlignVertical)left.Attachment;
        var pad = new string(' ', maxLength - alignVertical.LineLength);
        _sql.Insert(alignVertical.IndexOfWhole, pad);
        // 使用済みのAttachmentを削除する
        left.Attachment = null;
      }
    }

    // OR連言内の比較式において、その左辺式の最大の長さを取得する
    // (AND連言版と同じ実装)
    private int GetMaxLengthOfOrPredicates(OrPredicate orPredicate) {
      int maxLength = 0;
      Predicate left = orPredicate;
      // OR連言は左ノードだけをたどった時の軌跡である
      while(left.GetType() == typeof(OrPredicate)) {
        var andNode = (OrPredicate)left;
        if(andNode.Right.GetType() == typeof(BinaryOpPredicate)) {
          var LengthBeforeOperator = ((AlignVertical)andNode.Right.Attachment).LineLength;
          maxLength = System.Math.Max(maxLength, LengthBeforeOperator);
        }
        left = andNode.Left;
      }
      // OR連言の末端ノード
      if(left.GetType() == typeof(BinaryOpPredicate)) {
        var LengthBeforeOperator = ((AlignVertical)left.Attachment).LineLength;
        maxLength = System.Math.Max(maxLength, LengthBeforeOperator);
      }
      return maxLength;
    }

    // OR連言内の比較式において、縦位置を揃えるためにその左辺式に空白を挿入する
    // (AND連言版と同じ実装)
    private void AlignOrPredicates(OrPredicate orPredicate, int maxLength) {
      Predicate left = orPredicate;
      // AND連言は左ノードだけをたどった時の軌跡である
      while(left.GetType() == typeof(OrPredicate)) {
        var andNode = (OrPredicate)left;
        if(andNode.Right.GetType() == typeof(BinaryOpPredicate)) {
          var alignVertical = (AlignVertical)andNode.Right.Attachment;
          var pad = new string(' ', maxLength - alignVertical.LineLength);
          _sql.Insert(alignVertical.IndexOfWhole, pad);
        }
        left = andNode.Left;
      }
      // AND連言の末端ノード
      if(left.GetType() == typeof(BinaryOpPredicate)) {
        var alignVertical = (AlignVertical)left.Attachment;
        var pad = new string(' ', maxLength - alignVertical.LineLength);
        _sql.Insert(alignVertical.IndexOfWhole, pad);
        // 使用済みのAttachmentを削除する
        left.Attachment = null;
      }
    }

    private void AlignNodeCollections<TNode>(NodeCollections<TNode> nodes) 
      where TNode : Node {
      // Nodeの最大の長さを取得する
      int maxLength = 0;
      foreach(var result in nodes) {
        if(result.Attachment == null) {
          continue;
        }
        var length = ((AlignVertical)result.Attachment).LineLength;
        maxLength = System.Math.Max(maxLength, length);
      }
      // Nodeの縦位置を揃えるために空白を挿入する
      for(var i = nodes.Count - 1; i >= 0; --i) {
        if(nodes[i].Attachment == null) {
          continue;
        }
        var alignVertical = (AlignVertical)nodes[i].Attachment;
        var pad = new string(' ', maxLength - alignVertical.LineLength);
        _sql.Insert(alignVertical.IndexOfWhole, pad);
        // 使用済みのAttachmentを削除する
        nodes.Attachment = null;
      }
    }
    
    // Predicate式のAND/ORノード以外の直近の親ノードを取得する
    private INode GetAncestorOfPredicate(Predicate predicate) {
      INode node = predicate.Parent;
      while(node.GetType() == typeof(AndPredicate) ||
            node.GetType() == typeof(OrPredicate)) {
        node = node.Parent;
      }
      return node;
    }

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
    public override void VisitAfter(Assignments assignments) {
      // SET句の縦位置を揃える
      this.AlignNodeCollections(assignments);
    }
    public override void VisitAfter(WithClause withClause) {
      this.AppendNewLine();
    }
    public override void VisitBefore(WithDefinition withDefinition) { }
    //public override void VisitBefore(SingleQuery query) { }
    //public override void VisitAfter(SingleQuery query) { }
    //public override void VisitBefore(CompoundQuery compoundQuery) { }
    //public override void VisitAfter(CompoundQuery compoundQuery) { }
    //public override void VisitBefore(BracketedQuery bracketedQuery) { }
    //public override void VisitAfter(BracketedQuery bracketedQuery) { }
    public override void VisitBefore(ResultColumns resultColumns) { }
    public override void VisitAfter(ResultColumns resultColumns) {
      // SELECT句の縦位置を揃える
      this.AlignNodeCollections(resultColumns);
    }
    public override void VisitBefore(ResultExpr resultExpr) { }
    public override void VisitBefore(ColumnNames columns) { }
    public override void VisitAfter(ColumnNames columns) { }
    public override void VisitBefore(UnqualifiedColumnNames columns) { }
    public override void VisitAfter(UnqualifiedColumnNames columns) { }
    public override void VisitBefore(ValuesList valuesList) { }
    public override void VisitAfter(ValuesList valuesList) { }
    public override void VisitBefore(Values values) { }
    public override void VisitAfter(Values values) {
      // INSERT-VALUES文のVALUES句の両端の括弧は一行に配置する
      this.RemoveTailCharIf(')');
      this.AppendNewLine();
      this.AppendSymbol(")");
    }
    public override void VisitBefore(Exprs exprs) { }
    public override void VisitAfter(Exprs exprs) { }
    public override void VisitBefore(JoinSource joinSource) {
      // FROM又は左括弧の直後にJOIN式が続く場合は、インデントする
      if(joinSource.Left.GetType() != typeof(JoinSource)) {
        // 直前に開き括弧を出力した場合は、インデントしない
        if(_lastAppendToken != "(") {
          this.AppendString(m_joinFirstOperandIndent);
        } else if(joinSource.Left.GetType() != typeof(BracketedPredicate)) {
          this.AppendString(m_joinFirstOperandIndent);
        }
      }
    }
    public override void VisitAfter(JoinSource joinSource) {
      // Rignt Nodeの走査後にNestレベルを下げる
      --m_stmtStartPos.Peek().JoinNestLevel;
    }
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
      this.AppendHeaderComment(selectStmt.HeaderComment);
    }
    public override void VisitAfter(SelectStmt selectStmt) { }
    public override void VisitOnDefaultValues(InsertStmt insertStmt, int offset) { }
    public override void VisitAfter(IfStmt ifStmt) { }
    public override void VisitAfter(MergeUpdateClause updateClause) {
      // UPDATEの開始列位置を戻す
      m_stmtStartPos.Pop();
    }
    public override void VisitAfter(MergeInsertClause insertClause) {
      // INSERTの開始列位置を戻す
      m_stmtStartPos.Pop();
    }
    public override void VisitAfter(CallStmt callStmt) { }
    public override void VisitAfter(TruncateStmt truncateStmt) { }
    public override void VisitAfter(SqlitePragmaStmt pragmaStmt) { }
    public override void VisitAfter(NullStmt nullStmt) { }
    #endregion
  }
}
