using System;
using System.Collections.Generic;
using MiniSqlParser;

namespace Tester
{
  class CheckParentExistsVisitor : IVisitor
  {
    #region IVisitor メンバー

    private void ParentExists(INode node) {
      if(node.Parent == null) {
        throw new Exception("The Parent of Node doesn't exists");
      }
    }

    void IVisitor.Visit(JoinOperator joinOperator) {
      this.ParentExists(joinOperator); 
    }

    void IVisitor.Visit(Default defoult) {
      this.ParentExists(defoult);
    }

    void IVisitor.Visit(Assignment assignment) {
      this.ParentExists(assignment);
    }

    void IVisitor.Visit(SubQueryPredicate predicate, int offset) {
      this.ParentExists(predicate);
    }

    void IVisitor.Visit(InPredicate predicate, int offset) {
      this.ParentExists(predicate);
    }

    void IVisitor.Visit(IsPredicate predicate, int offset) {
      this.ParentExists(predicate);
    }

    void IVisitor.Visit(LikePredicate predicate, int offset) {
      this.ParentExists(predicate);
    }

    void IVisitor.Visit(PlaceHolderPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.Visit(OrPredicate orPredicate) {
      this.ParentExists(orPredicate);
    }

    void IVisitor.Visit(AndPredicate andPredicate) {
      this.ParentExists(andPredicate);
    }

    void IVisitor.Visit(BinaryOpPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.Visit(PlaceHolderExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.Visit(BlobLiteral literal) {
      this.ParentExists(literal);
    }

    void IVisitor.Visit(UNumericLiteral literal) {
      this.ParentExists(literal);
    }

    void IVisitor.Visit(IntervalLiteral literal) {
      this.ParentExists(literal);
    }

    void IVisitor.Visit(TimeStampLiteral literal) {
      this.ParentExists(literal);
    }

    void IVisitor.Visit(TimeLiteral literal) {
      this.ParentExists(literal);
    }

    void IVisitor.Visit(DateLiteral literal) {
      this.ParentExists(literal);
    }

    void IVisitor.Visit(NullLiteral literal) {
      this.ParentExists(literal);
    }

    void IVisitor.Visit(StringLiteral literal) {
      this.ParentExists(literal);
    }

    void IVisitor.Visit(JoinSource joinSource) {
      this.ParentExists(joinSource);
    }

    void IVisitor.Visit(UnqualifiedColumnName columnName) {
      this.ParentExists(columnName);
    }

    void IVisitor.Visit(Column column) {
      this.ParentExists(column);
    }

    void IVisitor.Visit(TableWildcard tableWildcard) {
      this.ParentExists(tableWildcard);
    }

    void IVisitor.Visit(Table table) {
      this.ParentExists(table);
    }

    void IVisitor.VisitAfter(SqlitePragmaStmt pragmaStmt) {
      this.ParentExists(pragmaStmt);
    }

    void IVisitor.VisitAfter(TruncateStmt truncateStmt) {
      this.ParentExists(truncateStmt);
    }

    void IVisitor.VisitAfter(CallStmt callStmt) {
      this.ParentExists(callStmt);
    }

    void IVisitor.VisitAfter(MergeInsertClause insertClause) {
      this.ParentExists(insertClause);
    }

    void IVisitor.VisitAfter(MergeUpdateClause updateClause) {
      this.ParentExists(updateClause);
    }

    void IVisitor.VisitAfter(MergeStmt mergeStmt) {
      this.ParentExists(mergeStmt);
    }

    void IVisitor.VisitAfter(DeleteStmt deleteStmt) {
      this.ParentExists(deleteStmt);
    }

    void IVisitor.VisitAfter(IfStmt ifStmt) {
      this.ParentExists(ifStmt);
    }

    void IVisitor.VisitAfter(InsertStmt insertStmt) {
      this.ParentExists(insertStmt);
    }

    void IVisitor.VisitAfter(UpdateStmt updateStmt) {
      this.ParentExists(updateStmt);
    }

    void IVisitor.VisitAfter(SelectStmt selectStmt) {
      this.ParentExists(selectStmt);
    }

    void IVisitor.VisitAfter(ForUpdateOfClause forUpdateOfClause) {
      this.ParentExists(forUpdateOfClause);
    }

    void IVisitor.VisitAfter(ForUpdateClause forUpdateClause) {
      this.ParentExists(forUpdateClause);
    }

    void IVisitor.VisitAfter(ILimitClause limitClause) {
      this.ParentExists(limitClause);
    }

    void IVisitor.VisitAfter(PartitioningTerm partitioningTerm) {
      this.ParentExists(partitioningTerm);
    }

    void IVisitor.VisitAfter(PartitionBy partitionBy) {
      this.ParentExists(partitionBy);
    }

    void IVisitor.VisitAfter(OrderingTerm orderingTerm) {
      this.ParentExists(orderingTerm);
    }

    void IVisitor.VisitAfter(OrderBy orderBy) {
      this.ParentExists(orderBy);
    }

    void IVisitor.VisitAfter(GroupBy groupBy) {
      this.ParentExists(groupBy);
    }

    void IVisitor.VisitAfter(BracketedSource bracketedSource) {
      this.ParentExists(bracketedSource);
    }

    void IVisitor.VisitAfter(AliasedQuery aliasedQuery) {
      this.ParentExists(aliasedQuery);
    }

    void IVisitor.VisitAfter(CommaJoinSource commaJoinSource) {
      this.ParentExists(commaJoinSource);
    }

    void IVisitor.VisitAfter(JoinSource joinSource) {
      this.ParentExists(joinSource);
    }

    void IVisitor.VisitAfter(Exprs exprs) {
      this.ParentExists(exprs);
    }

    void IVisitor.VisitAfter(Values values) {
      this.ParentExists(values);
    }

    void IVisitor.VisitAfter(ValuesList valuesList) {
      this.ParentExists(valuesList);
    }

    void IVisitor.VisitAfter(UnqualifiedColumnNames columns) {
      this.ParentExists(columns);
    }

    void IVisitor.VisitAfter(ColumnNames columns) {
      this.ParentExists(columns);
    }

    void IVisitor.VisitAfter(ResultExpr resultExpr) {
      this.ParentExists(resultExpr);
    }

    void IVisitor.VisitAfter(ResultColumns resultColumns) {
      this.ParentExists(resultColumns);
    }

    //void IVisitor.VisitAfter(BracketedQuery bracketedQuery) {
    //  this.ParentExists(bracketedQuery);
    //}

    //void IVisitor.VisitAfter(CompoundQuery compoundQuery) {
    //  this.ParentExists(compoundQuery);
    //}

    //void IVisitor.VisitAfter(SingleQuery query) {
    //  this.ParentExists(query);
    //}

    void IVisitor.VisitAfter(SingleQueryClause queryClause) {
      this.ParentExists(queryClause);
    }

    void IVisitor.VisitAfter(BracketedQueryClause bracketedQuery) {
      this.ParentExists(bracketedQuery);
    }

    void IVisitor.VisitAfter(CompoundQueryClause compoundQuery) {
      this.ParentExists(compoundQuery);
    }

    void IVisitor.VisitAfter(WithDefinition withDefinition) {
      this.ParentExists(withDefinition);
    }

    void IVisitor.VisitAfter(WithClause withClause) {
      this.ParentExists(withClause);
    }

    void IVisitor.VisitAfter(Assignments assignments) {
      this.ParentExists(assignments);
    }

    void IVisitor.VisitAfter(Assignment assignment) {
      this.ParentExists(assignment);
    }

    void IVisitor.VisitAfter(BracketedPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitAfter(CollatePredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitAfter(ExistsPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitAfter(SubQueryPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitAfter(InPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitAfter(BetweenPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitAfter(IsPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitAfter(IsNullPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitAfter(LikePredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitAfter(OrPredicate orPredicate) {
      this.ParentExists(orPredicate);
    }

    void IVisitor.VisitAfter(AndPredicate andPredicate) {
      this.ParentExists(andPredicate);
    }

    void IVisitor.VisitAfter(NotPredicate notPredicate) {
      this.ParentExists(notPredicate);
    }

    void IVisitor.VisitAfter(BinaryOpPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitAfter(SubQueryExp expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitAfter(CaseExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitAfter(CastExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitAfter(BracketedExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitAfter(FuncExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitAfter(WindowFuncExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitAfter(AggregateFuncExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitAfter(ExtractFuncExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitAfter(SubstringFunc expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitAfter(BinaryOpExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitAfter(BitwiseNotExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitAfter(SignedNumberExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitBefore(SqlitePragmaStmt pragmaStmt) {
      this.ParentExists(pragmaStmt);
    }

    void IVisitor.VisitBefore(TruncateStmt truncateStmt) {
      this.ParentExists(truncateStmt);
    }

    void IVisitor.VisitBefore(CallStmt callStmt) {
      this.ParentExists(callStmt);
    }

    void IVisitor.VisitBefore(MergeInsertClause insertClause) {
      this.ParentExists(insertClause);
    }

    void IVisitor.VisitBefore(MergeUpdateClause updateClause) {
      this.ParentExists(updateClause);
    }

    void IVisitor.VisitBefore(MergeStmt mergeStmt) {
      this.ParentExists(mergeStmt);
    }

    void IVisitor.VisitBefore(DeleteStmt deleteStmt) {
      this.ParentExists(deleteStmt);
    }

    void IVisitor.VisitBefore(IfStmt ifStmt) {
      this.ParentExists(ifStmt);
    }

    void IVisitor.VisitBefore(InsertStmt insertStmt) {
      this.ParentExists(insertStmt);
    }

    void IVisitor.VisitBefore(UpdateStmt updateStmt) {
      this.ParentExists(updateStmt);
    }

    void IVisitor.VisitBefore(SelectStmt selectStmt) {
      this.ParentExists(selectStmt);
    }

    void IVisitor.VisitBefore(ForUpdateOfClause forUpdateOfClause) {
      this.ParentExists(forUpdateOfClause);
    }

    void IVisitor.VisitBefore(ForUpdateClause forUpdateClause) {
      this.ParentExists(forUpdateClause);
    }

    void IVisitor.VisitBefore(ILimitClause limitClause) {
      this.ParentExists(limitClause);
    }

    void IVisitor.VisitOnOffset(ILimitClause limitClause, int offset) {
      this.ParentExists(limitClause);
    }

    void IVisitor.VisitBefore(PartitioningTerm partitioningTerm) {
      this.ParentExists(partitioningTerm);
    }

    void IVisitor.VisitBefore(PartitionBy partitionBy) {
      this.ParentExists(partitionBy);
    }

    void IVisitor.VisitBefore(OrderingTerm orderingTerm) {
      this.ParentExists(orderingTerm);
    }

    void IVisitor.VisitBefore(OrderBy orderBy) {
      this.ParentExists(orderBy);
    }

    void IVisitor.VisitBefore(GroupBy groupBy) {
      this.ParentExists(groupBy);
    }

    void IVisitor.VisitBefore(BracketedSource bracketedSource) {
      this.ParentExists(bracketedSource);
    }

    void IVisitor.VisitBefore(AliasedQuery aliasedQuery) {
      this.ParentExists(aliasedQuery);
    }

    void IVisitor.VisitBefore(CommaJoinSource commaJoinSource) {
      this.ParentExists(commaJoinSource);
    }

    void IVisitor.VisitBefore(JoinSource joinSource) {
      this.ParentExists(joinSource);
    }

    void IVisitor.VisitBefore(Exprs exprs) {
      this.ParentExists(exprs);
    }

    void IVisitor.VisitBefore(Values values) {
      this.ParentExists(values);
    }

    void IVisitor.VisitBefore(ValuesList valuesList) {
      this.ParentExists(valuesList);
    }

    void IVisitor.VisitBefore(UnqualifiedColumnNames columns) {
      this.ParentExists(columns);
    }

    void IVisitor.VisitBefore(ColumnNames columns) {
      this.ParentExists(columns);
    }

    void IVisitor.VisitBefore(ResultExpr resultExpr) {
      this.ParentExists(resultExpr);
    }

    void IVisitor.VisitBefore(ResultColumns resultColumns) {
      this.ParentExists(resultColumns);
    }

    //void IVisitor.VisitBefore(BracketedQuery bracketedQuery) {
    //  this.ParentExists(bracketedQuery);
    //}

    //void IVisitor.VisitBefore(CompoundQuery compoundQuery) {
    //  this.ParentExists(compoundQuery);
    //}

    //void IVisitor.VisitBefore(SingleQuery query) {
    //  this.ParentExists(query);
    //}

    void IVisitor.VisitBefore(SingleQueryClause queryClause) {
      this.ParentExists(queryClause);
    }

    void IVisitor.VisitBefore(BracketedQueryClause bracketedQuery) {
      this.ParentExists(bracketedQuery);
    }

    void IVisitor.VisitBefore(CompoundQueryClause compoundQuery) {
      this.ParentExists(compoundQuery);
    }

    void IVisitor.VisitBefore(WithDefinition withDefinition) {
      this.ParentExists(withDefinition);
    }

    void IVisitor.VisitBefore(WithClause withClause) {
      this.ParentExists(withClause);
    }

    void IVisitor.VisitBefore(Assignments assignments) {
      this.ParentExists(assignments);
    }

    void IVisitor.VisitBefore(Assignment assignment) {
      this.ParentExists(assignment);
    }

    void IVisitor.VisitBefore(BracketedPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitBefore(CollatePredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitBefore(ExistsPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitBefore(SubQueryPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitBefore(InPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitBefore(BetweenPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitBefore(IsPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitBefore(IsNullPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitBefore(LikePredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitBefore(OrPredicate orPredicate) {
      this.ParentExists(orPredicate);
    }

    void IVisitor.VisitBefore(AndPredicate andPredicate) {
      this.ParentExists(andPredicate);
    }

    void IVisitor.VisitBefore(NotPredicate notPredicate) {
      this.ParentExists(notPredicate);
    }

    void IVisitor.VisitBefore(BinaryOpPredicate predicate) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitBefore(SubQueryExp expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitBefore(CaseExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitBefore(CastExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitBefore(BracketedExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitBefore(FuncExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitBefore(WindowFuncExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitBefore(AggregateFuncExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitBefore(ExtractFuncExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitBefore(SubstringFunc expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitBefore(BinaryOpExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitBefore(BitwiseNotExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitBefore(SignedNumberExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitBeforeQuery(InsertSelectStmt insertSelectStmt, int offset) {
      this.ParentExists(insertSelectStmt);
    }

    void IVisitor.VisitOnAnd(BetweenPredicate predicate, int offset) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitOnAs(WithDefinition withDefinition, int offset) {
      this.ParentExists(withDefinition);
    }

    void IVisitor.VisitOnBetween(BetweenPredicate predicate, int offset) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitOnCompoundOp(CompoundQueryClause compoundQuery, int offset) {
      this.ParentExists(compoundQuery);
    }

    void IVisitor.VisitOnDefaultValues(InsertStmt insertStmt, int offset) {
      this.ParentExists(insertStmt);
    }

    void IVisitor.VisitOnDelete(DeleteStmt deleteStmt) {
      this.ParentExists(deleteStmt);
    }

    void IVisitor.VisitOnDo(InsertStmt insertStmt, int offset) {
      this.ParentExists(insertStmt);
    }

    void IVisitor.VisitOnElsIf(IfStmt ifStmt, int ifThenIndex, int offset) {
      this.ParentExists(ifStmt);
    }

    void IVisitor.VisitOnElse(IfStmt ifStmt, int offset) {
      this.ParentExists(ifStmt);
    }

    void IVisitor.VisitOnElse(CaseExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitOnEndIf(IfStmt ifStmt, int offset) {
      this.ParentExists(ifStmt);
    }

    void IVisitor.VisitOnEscape(LikePredicate predicate, int offset) {
      this.ParentExists(predicate);
    }

    void IVisitor.VisitOnFrom(SingleQueryClause queryClause, int offset) {
      this.ParentExists(queryClause);
    }

    void IVisitor.VisitOnFrom2(DeleteStmt deleteStmt, int offset) {
      this.ParentExists(deleteStmt);
    }

    void IVisitor.VisitOnFrom2(UpdateStmt updateStmt, int offset) {
      this.ParentExists(updateStmt);
    }

    void IVisitor.VisitOnHaving(SingleQueryClause queryClause, int offset) {
      this.ParentExists(queryClause);
    }

    void IVisitor.VisitOnInsert(InsertStmt insertStmt) {
      this.ParentExists(insertStmt);
    }

    void IVisitor.VisitOnLParen(Node node, int offset) {
      this.ParentExists(node);
    }

    void IVisitor.VisitOnMerge(MergeStmt mergeStmt) {
      this.ParentExists(mergeStmt);
    }

    void IVisitor.VisitOnOn(MergeStmt mergeStmt, int offset) {
      this.ParentExists(mergeStmt);
    }

    void IVisitor.VisitOnOn(InsertStmt insertStmt, int offset) {
      this.ParentExists(insertStmt);
    }

    void IVisitor.VisitOnOperator(BinaryOpExpr expr) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitOnOver(WindowFuncExpr expr, int offset) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitOnRParen(Node node, int offset) {
      this.ParentExists(node);
    }

    void IVisitor.VisitOnSeparator(ExtractFuncExpr expr, int offset) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitOnSeparator(SubstringFunc expr, int offset, int i) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitOnSeparator(Exprs exprs, int offset, int i) {
      this.ParentExists(exprs);
    }

    void IVisitor.VisitOnSeparator(CommaJoinSource commaJoinSource, int offset, int i) {
      this.ParentExists(commaJoinSource);
    }

    void IVisitor.VisitOnSeparator(ValuesList valuesList, int offset, int i) {
      this.ParentExists(valuesList);
    }

    void IVisitor.VisitOnSeparator(Node node, int offset, int i) {
      this.ParentExists(node);
    }

    void IVisitor.VisitOnSet(UpdateStmt updateStmt, int offset) {
      this.ParentExists(updateStmt);
    }

    void IVisitor.VisitOnStmtSeparator(Stmt stmt, int offset, int i) {
      this.ParentExists(stmt);
    }

    void IVisitor.VisitOnThen(IfStmt ifStmt, int ifThenIndex, int offset) {
      this.ParentExists(ifStmt);
    }

    void IVisitor.VisitOnThen(CaseExpr expr, int i) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitOnUpdate(UpdateStmt updateStmt) {
      this.ParentExists(updateStmt);
    }

    void IVisitor.VisitOnUsing(MergeStmt mergeStmt, int offset) {
      this.ParentExists(mergeStmt);
    }

    void IVisitor.VisitOnValues(MergeInsertClause insertClause, int offset) {
      this.ParentExists(insertClause);
    }

    void IVisitor.VisitOnValues(InsertValuesStmt insertValuesStmt, int offset) {
      this.ParentExists(insertValuesStmt);
    }

    void IVisitor.VisitOnWhen(CaseExpr expr, int i) {
      this.ParentExists(expr);
    }

    void IVisitor.VisitOnWhere(DeleteStmt deleteStmt, int offset) {
      this.ParentExists(deleteStmt);
    }

    void IVisitor.VisitOnWhere(UpdateStmt updateStmt, int offset) {
      this.ParentExists(updateStmt);
    }

    void IVisitor.VisitOnWhere(InsertStmt insertStmt, int offset) {
      this.ParentExists(insertStmt);
    }

    void IVisitor.VisitOnWhere(SingleQueryClause queryClause, int offset) {
      this.ParentExists(queryClause);
    }

    void IVisitor.VisitOnWildCard(Node node, int offset) {
      this.ParentExists(node);
    }

    #endregion

    #region IVisitor メンバー


    void IVisitor.VisitAfter(NullStmt nullStmt) {
      this.ParentExists(nullStmt);
    }

    void IVisitor.VisitBefore(NullStmt nullStmt) {
      this.ParentExists(nullStmt);
    }

    #endregion

    bool IVisitor.VisitOnFromFirstInQuery {
      get {
        return false;
      }
    }
  }
}
