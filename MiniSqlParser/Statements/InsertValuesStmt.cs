using System;
using System.Collections.Generic;

namespace MiniSqlParser
{
  public class InsertValuesStmt: InsertStmt
  {
    internal InsertValuesStmt(WithClause with
                            , bool isReplaceStmt
                            , ConflictType onConflict
                            , bool hasIntoKeyword
                            , Table table
                            , UnqualifiedColumnNames columns
                            , ValuesList valuesList
                            , Comments comments) {
      this.Comments = comments;
      this.With = with;
      this.IsReplaceStmt = isReplaceStmt;
      this.OnConflict = onConflict;
      this.HasIntoKeyword = hasIntoKeyword;
      this.Table = table;
      this.Columns = columns;
      this.ValuesList = valuesList;
    }

    private ValuesList _valuesList;
    public  ValuesList ValuesList {
      get {
        return _valuesList;
      }
      private set {
        _valuesList = value;
        this.SetParent(value);
      }
    }

    public override StmtType Type {
      get {
        return StmtType.InsertValue;
      }
    }

    public override Assignments GetAssignments(int index) {
      //  テーブル列名の指定がない場合は、空のAssignmentsを返す
      if(this.Columns == null || this.Columns.Count == 0) {
        return new Assignments();
      }

      // テーブル列名とVALUES句の要素数が異なる場合はエラーとする
      if(this.Columns.Count != this.ValuesList[index].Count){
        throw new InvalidASTStructureError("テーブル列名とVALUES句の要素数が異なります");
      }

      var ret = new Assignments();
      for(var i = 0; i < this.Columns.Count; ++i) {
        var column = new Column(this.Columns[i].Name);
        var assignment = new Assignment(column, this.ValuesList[index][i]);
        ret.Add(assignment);
      }

      return ret;
    }

    /// <summary>
    /// 指定したテーブル列に格納する全ての値を取得する
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public IEnumerable<Assignment> GetAssignments(UnqualifiedColumnName columnName) {
      //  テーブル列名の指定がない場合は、空のAssignmentsを返す
      if(this.Columns == null || this.Columns.Count == 0) {
        return new Assignments();
      }

      int columnIndex = -1;
      for(var i = 0 ; i < this.Columns.Count; ++i){
        if(this.Columns[i].Name == columnName.Name){
          columnIndex = i;
          break;
        }
      }
      if(columnIndex < 0){
        throw new ArgumentOutOfRangeException("columnName",
                                              "指定したテーブル列名はありません");
      }
      
      var ret = new List<Assignment>();
      foreach(var values in this.ValuesList) {
        var column = new Column(columnName.Name);
        var assignment = new Assignment(column, values[columnIndex]);
        ret.Add(assignment);
      }
      return ret;
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);

      if(this.HasWithClause) {
        this.With.Accept(visitor);
      }
      visitor.VisitOnInsert(this);
      int offset = this.HasIntoKeyword ? 2 : 1;
      offset += this.OnConflict != ConflictType.None ? 2 : 0;

      this.Table.Accept(visitor);

      // Columnリストの走査
      if(this.HasTableColumns) {
        this.Columns.Accept(visitor);
      }

      visitor.VisitOnValues(this, offset);

      offset += 1;  // VALUES句

      // Valuesリストの走査
      this.ValuesList.Accept(visitor);

      for(var i = 0; i < this.StmtSeparators; ++i) {
        visitor.VisitOnStmtSeparator(this, offset, i);
      }

      visitor.VisitAfter(this);
    }
  }
}
