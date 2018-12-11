using System;

namespace MiniSqlParser
{
  abstract public class Visitor : IVisitor
  {
    // Others
    virtual public void VisitOnSeparator(Node node, int offset, int i) { }
    virtual public void VisitOnSeparator(Exprs exprs, int offset, int i) { }
    virtual public void VisitOnSeparator(CommaJoinSource commaJoinSource, int offset, int i) { }
    virtual public void VisitOnSeparator(ValuesList valuesList, int offset, int i) { }
    virtual public void VisitOnSeparator(SubstringFunc expr, int offset, int i) { }
    virtual public void VisitOnSeparator(ExtractFuncExpr expr, int offset) { }
    virtual public void VisitOnLParen(Node node, int offset) { }
    virtual public void VisitOnRParen(Node node, int offset) { }
    virtual public void VisitOnWildCard(Node node, int offset) { }
    virtual public void VisitOnStmtSeparator(Stmt stmt, int offset, int i) { }

    // Identifiers
    virtual public void Visit(Table table) { }
    virtual public void Visit(Column column) { }
    virtual public void Visit(TableWildcard tableWildcard) { }
    virtual public void Visit(UnqualifiedColumnName columnName) { }

    // Literals
    virtual public void Visit(StringLiteral literal) { }
    virtual public void Visit(UNumericLiteral literal) { }
    virtual public void Visit(NullLiteral literal) { }
    virtual public void Visit(DateLiteral literal) { }
    virtual public void Visit(TimeLiteral literal) { }
    virtual public void Visit(TimeStampLiteral literal) { }
    virtual public void Visit(IntervalLiteral literal) { }
    virtual public void Visit(BlobLiteral literal) { }

    // Expressions
    virtual public void VisitBefore(SignedNumberExpr expr) { }
    virtual public void VisitAfter(SignedNumberExpr expr) { }
    virtual public void Visit(PlaceHolderExpr expr) { }
    virtual public void VisitBefore(BitwiseNotExpr expr) { }
    virtual public void VisitAfter(BitwiseNotExpr expr) { }
    virtual public void VisitBefore(BinaryOpExpr expr) { }
    virtual public void VisitAfter(BinaryOpExpr expr) { }
    virtual public void VisitOnOperator(BinaryOpExpr expr) { }
    virtual public void VisitBefore(SubstringFunc expr) { }
    virtual public void VisitAfter(SubstringFunc expr) { }
    virtual public void VisitBefore(ExtractFuncExpr expr) { }
    virtual public void VisitAfter(ExtractFuncExpr expr) { }
    virtual public void VisitBefore(AggregateFuncExpr expr) { }
    virtual public void VisitAfter(AggregateFuncExpr expr) { }
    virtual public void VisitBefore(WindowFuncExpr expr) { }
    virtual public void VisitAfter(WindowFuncExpr expr) { }
    virtual public void VisitOnOver(WindowFuncExpr expr, int offset) { }
    virtual public void VisitBefore(FuncExpr expr) { }
    virtual public void VisitAfter(FuncExpr expr) { }
    virtual public void VisitBefore(BracketedExpr expr) { }
    virtual public void VisitAfter(BracketedExpr expr) { }
    virtual public void VisitBefore(CastExpr expr) { }
    virtual public void VisitAfter(CastExpr expr) { }
    virtual public void VisitBefore(CaseExpr expr) { }
    virtual public void VisitAfter(CaseExpr expr) { }
    virtual public void VisitOnWhen(CaseExpr expr, int i) { }
    virtual public void VisitOnThen(CaseExpr expr, int i) { }
    virtual public void VisitOnElse(CaseExpr expr) { }
    virtual public void VisitBefore(SubQueryExp expr) { }
    virtual public void VisitAfter(SubQueryExp expr) { }

    // Predicates
    virtual public void VisitBefore(BinaryOpPredicate predicate) { }
    virtual public void VisitAfter(BinaryOpPredicate predicate) { }
    virtual public void Visit(BinaryOpPredicate predicate) { }
    virtual public void VisitBefore(NotPredicate notPredicate) { }
    virtual public void VisitAfter(NotPredicate notPredicate) { }
    virtual public void VisitBefore(AndPredicate andPredicate) { }
    virtual public void VisitAfter(AndPredicate andPredicate) { }
    virtual public void Visit(AndPredicate andPredicate) { }
    virtual public void VisitBefore(OrPredicate orPredicate) { }
    virtual public void VisitAfter(OrPredicate orPredicate) { }
    virtual public void Visit(OrPredicate orPredicate) { }
    virtual public void Visit(PlaceHolderPredicate predicate) { }
    virtual public void VisitBefore(LikePredicate predicate) { }
    virtual public void VisitAfter(LikePredicate predicate) { }
    virtual public void Visit(LikePredicate predicate, int offset) { }
    virtual public void VisitOnEscape(LikePredicate predicate, int offset) { }
    virtual public void VisitBefore(IsNullPredicate predicate) { }
    virtual public void VisitAfter(IsNullPredicate predicate) { }
    virtual public void VisitBefore(IsPredicate predicate) { }
    virtual public void VisitAfter(IsPredicate predicate) { }
    virtual public void Visit(IsPredicate predicate, int offset) { }
    virtual public void VisitBefore(BetweenPredicate predicate) { }
    virtual public void VisitAfter(BetweenPredicate predicate) { }
    virtual public void VisitOnBetween(BetweenPredicate predicate, int offset) { }
    virtual public void VisitOnAnd(BetweenPredicate predicate, int offset) { }
    virtual public void VisitBefore(InPredicate predicate) { }
    virtual public void VisitAfter(InPredicate predicate) { }
    virtual public void Visit(InPredicate predicate, int offset) { }
    virtual public void VisitBefore(SubQueryPredicate predicate) { }
    virtual public void VisitAfter(SubQueryPredicate predicate) { }
    virtual public void Visit(SubQueryPredicate predicate, int offset) { }
    virtual public void VisitBefore(ExistsPredicate predicate) { }
    virtual public void VisitAfter(ExistsPredicate predicate) { }
    virtual public void VisitBefore(CollatePredicate predicate) { }
    virtual public void VisitAfter(CollatePredicate predicate) { }
    virtual public void VisitBefore(BracketedPredicate predicate) { }
    virtual public void VisitAfter(BracketedPredicate predicate) { }

    // Clauses
    virtual public void VisitBefore(Assignment assignment) { }
    virtual public void VisitAfter(Assignment assignment) { }
    virtual public void VisitBefore(Assignments assignments) { }
    virtual public void VisitAfter(Assignments assignments) { }
    virtual public void Visit(Assignment assignment) { }
    virtual public void Visit(Default defoult) { }
    virtual public void VisitBefore(WithClause withClause) { }
    virtual public void VisitAfter(WithClause withClause) { }
    virtual public void VisitBefore(WithDefinition withDefinition) { }
    virtual public void VisitAfter(WithDefinition withDefinition) { }
    virtual public void VisitOnAs(WithDefinition withDefinition, int offset) { }
    virtual public void VisitBefore(CompoundQueryClause compoundQuery) { }
    virtual public void VisitAfter(CompoundQueryClause compoundQuery) { }
    virtual public void VisitOnCompoundOp(CompoundQueryClause compoundQuery, int offset) { }
    virtual public void VisitBefore(BracketedQueryClause bracketedQuery) { }
    virtual public void VisitAfter(BracketedQueryClause bracketedQuery) { }
    virtual public void VisitBefore(SingleQueryClause query) { }
    virtual public void VisitAfter(SingleQueryClause query) { }
    virtual public void VisitOnFrom(SingleQueryClause query, int offset) { }
    virtual public void VisitOnWhere(SingleQueryClause query, int offset) { }
    virtual public void VisitOnHaving(SingleQueryClause query, int offset) { }
    //virtual public void VisitBefore(SingleQuery query) { }
    //virtual public void VisitAfter(SingleQuery query) { }
    //virtual public void VisitBefore(CompoundQuery compoundQuery) { }
    //virtual public void VisitAfter(CompoundQuery compoundQuery) { }
    //virtual public void VisitBefore(BracketedQuery bracketedQuery) { }
    //virtual public void VisitAfter(BracketedQuery bracketedQuery) { }
    virtual public void VisitBefore(ResultColumns resultColumns) { }
    virtual public void VisitAfter(ResultColumns resultColumns) { }
    virtual public void VisitBefore(ResultExpr resultExpr) { }
    virtual public void VisitAfter(ResultExpr resultExpr) { }
    virtual public void VisitBefore(ColumnNames columns) { }
    virtual public void VisitAfter(ColumnNames columns) { }
    virtual public void VisitBefore(UnqualifiedColumnNames columns) { }
    virtual public void VisitAfter(UnqualifiedColumnNames columns) { }
    virtual public void VisitBefore(ValuesList valuesList) { }
    virtual public void VisitAfter(ValuesList valuesList) { }
    virtual public void VisitBefore(Values values) { }
    virtual public void VisitAfter(Values values) { }
    virtual public void VisitBefore(Exprs exprs) { }
    virtual public void VisitAfter(Exprs exprs) { }
    virtual public void VisitBefore(JoinSource joinSource) { }
    virtual public void VisitAfter(JoinSource joinSource) { }
    virtual public void Visit(JoinSource joinSource) { }
    virtual public void Visit(JoinOperator joinOperator) { }
    virtual public void VisitBefore(CommaJoinSource commaJoinSource) { }
    virtual public void VisitAfter(CommaJoinSource commaJoinSource) { }
    virtual public void VisitBefore(AliasedQuery aliasedQuery) { }
    virtual public void VisitAfter(AliasedQuery aliasedQuery) { }
    virtual public void VisitBefore(BracketedSource bracketedSource) { }
    virtual public void VisitAfter(BracketedSource bracketedSource) { }
    virtual public void VisitBefore(GroupBy groupBy) { }
    virtual public void VisitAfter(GroupBy groupBy) { }
    virtual public void VisitBefore(OrderBy orderBy) { }
    virtual public void VisitAfter(OrderBy orderBy) { }
    virtual public void VisitBefore(OrderingTerm orderingTerm) { }
    virtual public void VisitAfter(OrderingTerm orderingTerm) { }
    virtual public void VisitBefore(PartitionBy partitionBy) { }
    virtual public void VisitAfter(PartitionBy partitionBy) { }
    virtual public void VisitBefore(PartitioningTerm partitioningTerm) { }
    virtual public void VisitAfter(PartitioningTerm partitioningTerm) { }
    virtual public void VisitBefore(ILimitClause iLimitClause) { }
    virtual public void VisitAfter(ILimitClause iLimitClause) { }
    virtual public void VisitOnOffset(ILimitClause iLimitClause, int offset) { }
    virtual public void VisitBefore(ForUpdateClause forUpdateClause) { }
    virtual public void VisitAfter(ForUpdateClause forUpdateClause) { }
    virtual public void VisitBefore(ForUpdateOfClause forUpdateOfClause) { }
    virtual public void VisitAfter(ForUpdateOfClause forUpdateOfClause) { }

    // Statements
    virtual public void VisitBefore(SelectStmt selectStmt) { }
    virtual public void VisitAfter(SelectStmt selectStmt) { }
    virtual public void VisitBefore(UpdateStmt updateStmt) { }
    virtual public void VisitAfter(UpdateStmt updateStmt) { }
    virtual public void VisitOnUpdate(UpdateStmt updateStmt) { }
    virtual public void VisitOnSet(UpdateStmt updateStmt, int offset) { }
    virtual public void VisitOnFrom2(UpdateStmt updateStmt, int offset) { }
    virtual public void VisitOnWhere(UpdateStmt updateStmt, int offset) { }

    virtual public void VisitBefore(InsertStmt insertStmt) { }
    virtual public void VisitAfter(InsertStmt insertStmt) { }
    virtual public void VisitOnInsert(InsertStmt insertStmt) { }
    virtual public void VisitOnValues(InsertValuesStmt insertValuesStmt, int offset) { }
    virtual public void VisitBeforeQuery(InsertSelectStmt insertSelectStmt, int offset) { }

    virtual public void VisitOnDefaultValues(InsertStmt insertStmt, int offset) { }
    virtual public void VisitBefore(IfStmt ifStmt) { }
    virtual public void VisitAfter(IfStmt ifStmt) { }
    virtual public void VisitOnThen(IfStmt ifStmt, int ifThenIndex, int offset) { }
    virtual public void VisitOnElsIf(IfStmt ifStmt, int ifThenIndex, int offset) { }
    virtual public void VisitOnElse(IfStmt ifStmt, int offset) { }
    virtual public void VisitOnEndIf(IfStmt ifStmt, int offset) { }

    virtual public void VisitBefore(DeleteStmt deleteStmt) { }
    virtual public void VisitAfter(DeleteStmt deleteStmt) { }
    virtual public void VisitOnDelete(DeleteStmt deleteStmt) { }
    virtual public void VisitOnFrom2(DeleteStmt deleteStmt, int offset) { }
    virtual public void VisitOnWhere(DeleteStmt deleteStmt, int offset) { }

    virtual public void VisitBefore(MergeStmt mergeStmt) { }
    virtual public void VisitAfter(MergeStmt mergeStmt) { }
    virtual public void VisitOnMerge(MergeStmt mergeStmt) { }
    virtual public void VisitOnUsing(MergeStmt mergeStmt, int offset) { }
    virtual public void VisitOnOn(MergeStmt mergeStmt, int offset) { }
    virtual public void VisitBefore(MergeUpdateClause updateClause) { }
    virtual public void VisitAfter(MergeUpdateClause updateClause) { }
    virtual public void VisitBefore(MergeInsertClause insertClause) { }
    virtual public void VisitAfter(MergeInsertClause insertClause) { }
    virtual public void VisitOnValues(MergeInsertClause insertClause, int offset) { }

    virtual public void VisitBefore(CallStmt callStmt) { }
    virtual public void VisitAfter(CallStmt callStmt) { }

    virtual public void VisitBefore(TruncateStmt truncateStmt) { }
    virtual public void VisitAfter(TruncateStmt truncateStmt) { }

    virtual public void VisitBefore(SqlitePragmaStmt pragmaStmt) { }
    virtual public void VisitAfter(SqlitePragmaStmt pragmaStmt) { }

    virtual public void VisitBefore(NullStmt nullStmt) { }
    virtual public void VisitAfter(NullStmt nullStmt) { }

    virtual public bool VisitOnFromFirstInQuery { get { return false; } }
  }
}
