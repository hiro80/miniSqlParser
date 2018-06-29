using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MiniSqlParser
{
  public class IfStmt : Stmt
  {
    internal IfStmt(List<Predicate> conditions
                  , List<Stmts> statementsList
                  , Stmts elseStatements
                  , Comments comments) {
      this.Comments = comments;
      _conditions = conditions;
      _statmentsList = statementsList;
      this.ElseStatements = elseStatements;

      foreach(var condition in conditions) {
        this.SetParent(condition);
      }
      foreach(var statements in statementsList) {
        this.SetParent(statements);
      }
    }

    private List<Predicate> _conditions;
    public ReadOnlyCollection<Predicate> Conditions {
      get {
        return _conditions.AsReadOnly();
      }
    }

    private List<Stmts> _statmentsList;
    public ReadOnlyCollection<Stmts> StatementsList {
      get {
        return _statmentsList.AsReadOnly();
      }
    }

    private Stmts _elseStatements;
    public Stmts ElseStatements {
      get {
        return _elseStatements;
      }
      private set {
        _elseStatements = value;
        this.SetParent(value);
      }
    }

    public int CountElsIfStatements {
      get { return this.Conditions.Count - 1; }
    }

    public bool HasElseStatements {
      get { return this.ElseStatements != null; }
    }

    //public Tuple<Predicate, Stmts> this[int i] {
    //  get {
    //    return Tuple.Create(_conditions[i], _statmentsList[i]);
    //  }
    //  set {
    //    _conditions[i] = value.Item1;
    //    this.SetParent(value.Item1);
    //    _statmentsList[i] = value.Item2;
    //    this.SetParent(value.Item2);
    //  }
    //}

    public void SetBranch(int index, Predicate condition, Stmts statments) {
      _conditions[index] = condition;
      this.SetParent(condition);
      _statmentsList[index] = statments;
      this.SetParent(statments);
    }

    public override StmtType Type {
      get {
        return StmtType.If;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);

      int offset = 1;

      _conditions[0].Accept(visitor);
      visitor.VisitOnThen(this, 0, offset);
      offset += 1;
      _statmentsList[0].Accept(visitor);

      for(var i = 1; i < this.Conditions.Count; ++i) {
        visitor.VisitOnElsIf(this, i, offset);
        offset += 1;
        _conditions[i].Accept(visitor);
        visitor.VisitOnThen(this, i, offset);
        offset += 1;
        _statmentsList[i].Accept(visitor);
      }

      if(this.HasElseStatements) {
        visitor.VisitOnElse(this, offset);
        offset += 1;
        this.ElseStatements.Accept(visitor);
      }

      visitor.VisitOnEndIf(this, offset);
      offset += 2;

      for(var i = 0; i < this.StmtSeparators; ++i) {
        visitor.VisitOnStmtSeparator(this, offset, i);
      }

      visitor.VisitAfter(this);
    }

  }
}
