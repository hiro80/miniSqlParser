using System;

namespace MiniSqlParser
{
  public interface IVisitor
  {
    // Others
    void VisitOnSeparator(Node node, int offset, int i);
    void VisitOnSeparator(Exprs exprs, int offset, int i);
    void VisitOnSeparator(CommaJoinSource commaJoinSource, int offset, int i);
    void VisitOnSeparator(ValuesList valuesList, int offset, int i);
    void VisitOnSeparator(SubstringFunc expr, int offset, int i);
    void VisitOnSeparator(ExtractFuncExpr expr, int offset);
    void VisitOnLParen(Node node, int offset);
    void VisitOnRParen(Node node, int offset);
    void VisitOnWildCard(Node node, int offset);
    void VisitOnStmtSeparator(Stmt stmt, int offset, int i);

    // Identifiers
    void Visit(Table table);
    void Visit(Column column);
    void Visit(TableWildcard tableWildcard);
    void Visit(UnqualifiedColumnName columnName);

    // Literals
    void Visit(StringLiteral literal);
    void Visit(UNumericLiteral literal);
    void Visit(NullLiteral literal);
    void Visit(DateLiteral literal);
    void Visit(TimeLiteral literal);
    void Visit(TimeStampLiteral literal);
    void Visit(IntervalLiteral literal);
    void Visit(BlobLiteral literal);

    // Expressions
    void VisitBefore(SignedNumberExpr expr);
    void VisitAfter(SignedNumberExpr expr);
    void Visit(PlaceHolderExpr expr);
    void VisitBefore(BitwiseNotExpr expr);
    void VisitAfter(BitwiseNotExpr expr);
    void VisitBefore(BinaryOpExpr expr);
    void VisitAfter(BinaryOpExpr expr);
    void VisitOnOperator(BinaryOpExpr expr);
    void VisitBefore(SubstringFunc expr);
    void VisitAfter(SubstringFunc expr);
    void VisitBefore(ExtractFuncExpr expr);
    void VisitAfter(ExtractFuncExpr expr);
    void VisitBefore(AggregateFuncExpr expr);
    void VisitAfter(AggregateFuncExpr expr);
    void VisitBefore(WindowFuncExpr expr);
    void VisitAfter(WindowFuncExpr expr);
    void VisitOnOver(WindowFuncExpr expr, int offset);
    void VisitBefore(FuncExpr expr);
    void VisitAfter(FuncExpr expr);
    void VisitBefore(BracketedExpr expr);
    void VisitAfter(BracketedExpr expr);
    void VisitBefore(CastExpr expr);
    void VisitAfter(CastExpr expr);
    void VisitBefore(CaseExpr expr);
    void VisitAfter(CaseExpr expr);
    void VisitOnWhen(CaseExpr expr, int i);
    void VisitOnThen(CaseExpr expr, int i);
    void VisitOnElse(CaseExpr expr);
    void VisitBefore(SubQueryExp expr);
    void VisitAfter(SubQueryExp expr);

    // Predicates
    void VisitBefore(BinaryOpPredicate predicate);
    void VisitAfter(BinaryOpPredicate predicate);
    void Visit(BinaryOpPredicate predicate);
    void VisitBefore(NotPredicate notPredicate);
    void VisitAfter(NotPredicate notPredicate);
    void VisitBefore(AndPredicate andPredicate);
    void VisitAfter(AndPredicate andPredicate);
    void Visit(AndPredicate andPredicate);
    void VisitBefore(OrPredicate orPredicate);
    void VisitAfter(OrPredicate orPredicate);
    void Visit(OrPredicate orPredicate);
    void Visit(PlaceHolderPredicate predicate);
    void VisitBefore(LikePredicate predicate);
    void VisitAfter(LikePredicate predicate);
    void Visit(LikePredicate predicate, int offset);
    void VisitOnEscape(LikePredicate predicate, int offset);
    void VisitBefore(IsNullPredicate predicate);
    void VisitAfter(IsNullPredicate predicate);
    void VisitBefore(IsPredicate predicate);
    void VisitAfter(IsPredicate predicate);
    void Visit(IsPredicate predicate, int offset);
    void VisitBefore(BetweenPredicate predicate);
    void VisitAfter(BetweenPredicate predicate);
    void VisitOnBetween(BetweenPredicate predicate, int offset);
    void VisitOnAnd(BetweenPredicate predicate, int offset);
    void VisitBefore(InPredicate predicate);
    void VisitAfter(InPredicate predicate);
    void Visit(InPredicate predicate, int offset);
    void VisitBefore(SubQueryPredicate predicate);
    void VisitAfter(SubQueryPredicate predicate);
    void Visit(SubQueryPredicate predicate, int offset);
    void VisitBefore(ExistsPredicate predicate);
    void VisitAfter(ExistsPredicate predicate);
    void VisitBefore(CollatePredicate predicate);
    void VisitAfter(CollatePredicate predicate);
    void VisitBefore(BracketedPredicate predicate);
    void VisitAfter(BracketedPredicate predicate);

    // Clauses
    void VisitBefore(Assignment assignment);
    void VisitAfter(Assignment assignment);
    void VisitBefore(Assignments assignments);
    void VisitAfter(Assignments assignments);
    void Visit(Assignment assignment);
    void Visit(Default defoult);
    void VisitBefore(WithClause withClause);
    void VisitAfter(WithClause withClause);
    void VisitBefore(WithDefinition withDefinition);
    void VisitAfter(WithDefinition withDefinition);
    void VisitOnAs(WithDefinition withDefinition, int offset);
    void VisitBefore(CompoundQueryClause compoundQuery);
    void VisitAfter(CompoundQueryClause compoundQuery);
    void VisitOnCompoundOp(CompoundQueryClause compoundQuery, int offset);
    void VisitBefore(BracketedQueryClause bracketedQuery);
    void VisitAfter(BracketedQueryClause bracketedQuery);
    void VisitBefore(SingleQueryClause query);
    void VisitAfter(SingleQueryClause query);
    void VisitOnFrom(SingleQueryClause query, int offset);
    void VisitOnWhere(SingleQueryClause query, int offset);
    void VisitOnHaving(SingleQueryClause query, int offset);
    //void VisitBefore(SingleQuery query);
    //void VisitAfter(SingleQuery query);
    //void VisitBefore(CompoundQuery compoundQuery);
    //void VisitAfter(CompoundQuery compoundQuery);
    //void VisitBefore(BracketedQuery bracketedQuery);
    //void VisitAfter(BracketedQuery bracketedQuery);
    void VisitBefore(ResultColumns resultColumns);
    void VisitAfter(ResultColumns resultColumns);
    void VisitBefore(ResultExpr resultExpr);
    void VisitAfter(ResultExpr resultExpr);
    void VisitBefore(ColumnNames columns);
    void VisitAfter(ColumnNames columns);
    void VisitBefore(UnqualifiedColumnNames columns);
    void VisitAfter(UnqualifiedColumnNames columns);
    void VisitBefore(ValuesList valuesList);
    void VisitAfter(ValuesList valuesList);
    void VisitBefore(Values values);
    void VisitAfter(Values values);
    void VisitBefore(Exprs exprs);
    void VisitAfter(Exprs exprs);
    void VisitBefore(JoinSource joinSource);
    void VisitAfter(JoinSource joinSource);
    void Visit(JoinSource joinSource);
    void Visit(JoinOperator joinOperator);
    void VisitBefore(CommaJoinSource commaJoinSource);
    void VisitAfter(CommaJoinSource commaJoinSource);
    void VisitBefore(AliasedQuery aliasedQuery);
    void VisitAfter(AliasedQuery aliasedQuery);
    void VisitBefore(BracketedSource bracketedSource);
    void VisitAfter(BracketedSource bracketedSource);
    void VisitBefore(GroupBy groupBy);
    void VisitAfter(GroupBy groupBy);
    void VisitBefore(OrderBy orderBy);
    void VisitAfter(OrderBy orderBy);
    void VisitBefore(OrderingTerm orderingTerm);
    void VisitAfter(OrderingTerm orderingTerm);
    void VisitBefore(PartitionBy partitionBy);
    void VisitAfter(PartitionBy partitionBy);
    void VisitBefore(PartitioningTerm partitioningTerm);
    void VisitAfter(PartitioningTerm partitioningTerm);
    void VisitBefore(ILimitClause iLimitClause);
    void VisitAfter(ILimitClause iLimitClause);
    void VisitOnOffset(ILimitClause iLimitClause, int offset);
    void VisitBefore(ForUpdateClause forUpdateClause);
    void VisitAfter(ForUpdateClause forUpdateClause);
    void VisitBefore(ForUpdateOfClause forUpdateOfClause);
    void VisitAfter(ForUpdateOfClause forUpdateOfClause);

    // Statements
    void VisitBefore(SelectStmt selectStmt);
    void VisitAfter(SelectStmt selectStmt);
    void VisitBefore(UpdateStmt updateStmt);
    void VisitAfter(UpdateStmt updateStmt);
    void VisitOnUpdate(UpdateStmt updateStmt);
    void VisitOnSet(UpdateStmt updateStmt, int offset);
    void VisitOnFrom2(UpdateStmt updateStmt, int offset);
    void VisitOnWhere(UpdateStmt updateStmt, int offset);

    void VisitBefore(InsertStmt insertStmt);
    void VisitAfter(InsertStmt insertStmt);
    void VisitOnInsert(InsertStmt insertStmt);
    void VisitOnValues(InsertValuesStmt insertValuesStmt, int offset);
    void VisitBeforeQuery(InsertSelectStmt insertSelectStmt, int offset);
    void VisitOnDefaultValues(InsertStmt insertStmt, int offset);
    void VisitOnOn(InsertStmt insertStmt, int offset);
    void VisitOnDo(InsertStmt insertStmt, int offset);
    void VisitOnWhere(InsertStmt insertStmt, int offset);

    void VisitBefore(IfStmt ifStmt);
    void VisitAfter(IfStmt ifStmt);
    void VisitOnThen(IfStmt ifStmt, int ifThenIndex, int offset);
    void VisitOnElsIf(IfStmt ifStmt, int ifThenIndex, int offset);
    void VisitOnElse(IfStmt ifStmt, int offset);
    void VisitOnEndIf(IfStmt ifStmt, int offset);

    void VisitBefore(DeleteStmt deleteStmt);
    void VisitAfter(DeleteStmt deleteStmt);
    void VisitOnDelete(DeleteStmt deleteStmt);
    void VisitOnFrom2(DeleteStmt deleteStmt, int offset);
    void VisitOnWhere(DeleteStmt deleteStmt, int offset);

    void VisitBefore(MergeStmt mergeStmt);
    void VisitAfter(MergeStmt mergeStmt);
    void VisitOnMerge(MergeStmt mergeStmt);
    void VisitOnUsing(MergeStmt mergeStmt, int offset);
    void VisitOnOn(MergeStmt mergeStmt, int offset);
    void VisitBefore(MergeUpdateClause updateClause);
    void VisitAfter(MergeUpdateClause updateClause);
    void VisitBefore(MergeInsertClause insertClause);
    void VisitAfter(MergeInsertClause insertClause);
    void VisitOnValues(MergeInsertClause insertClause, int offset);

    void VisitBefore(CallStmt callStmt);
    void VisitAfter(CallStmt callStmt);

    void VisitBefore(TruncateStmt truncateStmt);
    void VisitAfter(TruncateStmt truncateStmt);

    void VisitBefore(SqlitePragmaStmt pragmaStmt);
    void VisitAfter(SqlitePragmaStmt pragmaStmt);

    void VisitBefore(NullStmt nullStmt);
    void VisitAfter(NullStmt nullStmt);

    bool VisitOnFromFirstInQuery{ get; }
  }
}
