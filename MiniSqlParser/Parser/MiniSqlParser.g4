/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2014 by Bart Kiers
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * Project      : sqlite-parser; an ANTLR4 grammar for SQLite
 *                https://github.com/bkiers/sqlite-parser
 * Developed by : Bart Kiers, bart@big-o.nl
 */

grammar MiniSqlParser;

options {
  language=CSharp;
}

@lexer::members{
  /* {!IsOracle}のようなDBMS指定の否定形を記述すると、*/
  /* DBMS指定なし(全てのDBMS指定がTrue)の場合に       */
  /* その文法が除外されるため記述しないようにすること */
  public bool IsOracle{ get; set; }
  public bool IsMySql { get; set; }
  public bool IsSQLite{ get; set; }
  public bool IsMsSql { get; set; }
  public bool IsPostgreSql{ get; set; }
  public bool IsPervasive { get; set; }
  /* With ANSI_QUOTES enabled, */
  /* you cannot use double quotation marks to quote literal strings */
  public bool MySqlAnsiQuotes{ get; set; }
  public readonly int COMMENTS = 2;
  private int MaxIntValueLength = int.MaxValue.ToString().Length;
  private int MaxHexIntValueLength = Convert.ToString(int.MaxValue, 16).Length;
  private bool IsValidInt(string i){
    // It does'nt use Parse(), TryParse() functions,
    // because of improbing process speed.
    if(i.Length < MaxIntValueLength){
      // if Number of digits is less than max int value, int
      return true;
    } else if(i.Length > MaxIntValueLength){
      // if Number of digits is more than max int value, numeric
      return false;
    } else {
      // if Number of digits is equal to max int value,
      // compared by string comparison.
      return string.CompareOrdinal(i, int.MaxValue.ToString()) <= 0;
    }
  }
  private bool IsValidHexicalInt(string h){
    h = h.Remove(0, 2);
    if(h.Length < MaxHexIntValueLength){
      return true;
    } else if(h.Length > MaxHexIntValueLength){
      return false;
    } else {
      return string.CompareOrdinal(h, Convert.ToString(int.MaxValue, 16)) <= 0;
    }
  }
  private bool IsValidDate(string d){
    d = d.Remove(0, d.IndexOf('\'')).Trim('\'');
    DateTime dt;
    return DateTime.TryParseExact(d
                                 ,new string[]{"yyyy-MM-dd","yyyy/MM/dd"}
                                 ,System.Globalization.CultureInfo.InvariantCulture
                                 ,System.Globalization.DateTimeStyles.None
                                 ,out dt);
  }
}

@parser::members{
  public bool IsOracle{ get; set; }
  public bool IsMySql { get; set; }
  public bool IsSQLite{ get; set; }
  public bool IsMsSql { get; set; }
  public bool IsPostgreSql{ get; set; }
  public bool IsPervasive { get; set; }
}


/*
   Parser Start Rools
*/

stmts_root
 : stmts EOF
 ; 

stmt_root
 : stmt EOF
 ;

query_root
 : query EOF
 ;

predicate_root
 : predicate EOF
 ;

expr_root
 : expr EOF
 ;

placeholder_root
 : ( value_column | predicate ) EOF
 ;

stmts
   /* If there is a semicolon at the beginning of the input character string, */
   /* the head is parsed into a NULL statement */
 : stmt c1=scols ( SCOL stmt_sub c2+=scols )*
 | error
 ;

scols
 : SCOL*
 ;

error
 : UNEXPECTED_CHAR
   { 
     throw new RecognitionException("UNEXPECTED_CHAR=" + $UNEXPECTED_CHAR.text
                                    ,this, this.InputStream, _localctx); 
   }
 ;

stmt
 : stmt_sub
 | null_stmt
 ;

stmt_sub
 : select_stmt
 | update_stmt
 | insert_stmt
 | {IsMySql || IsSQLite}?
   replace_stmt
 | delete_stmt
 | {IsOracle || IsMsSql}?
   merge_stmt
 | {IsOracle || IsMySql || IsMsSql || IsPervasive}?
   call_stmt
 | {IsOracle || IsMySql || IsMsSql || IsPostgreSql}?
   truncate_stmt
 | if_stmt
 | {IsSQLite}?
   sqlite_pragma_stmt
 ;


/*
   Statements
*/

select_stmt
 : with_clause? query for_update_clause?
 ;

update_stmt
 : with_clause? K_UPDATE
  (  {IsSQLite}? K_OR K_ROLLBACK
   | {IsSQLite}? K_OR K_ABORT
   | {IsSQLite}? K_OR K_REPLACE
   | {IsSQLite}? K_OR K_FAIL
   | {IsSQLite}? K_OR K_IGNORE )?
  (  {IsSQLite || IsMsSql}?                                indexed_table_name
   | {IsOracle || IsMySql || IsPostgreSql || IsPervasive}? aliased_table_name )
   K_SET assignments
   ( {IsMsSql || IsPostgreSql}? K_FROM aliased_table_name )?
   ( K_WHERE predicate )?
 ;

insert_stmt
 : with_clause? 
  (              K_INSERT                 K_INTO?
   | {IsSQLite}? K_INSERT K_OR K_REPLACE  K_INTO
   | {IsSQLite}? K_INSERT K_OR K_ROLLBACK K_INTO
   | {IsSQLite}? K_INSERT K_OR K_ABORT    K_INTO
   | {IsSQLite}? K_INSERT K_OR K_FAIL     K_INTO
   | {IsSQLite}? K_INSERT K_OR K_IGNORE   K_INTO )
   table_name unqualified_column_names?
   ( K_VALUES values_clauses | query /*| K_DEFAULT K_VALUES*/ )
 ;

replace_stmt
 : with_clause? K_REPLACE K_INTO?
   table_name unqualified_column_names?
   ( K_VALUES values_clauses | query /*| K_DEFAULT K_VALUES*/ )
 ;

delete_stmt
 : with_clause? K_DELETE
   f1=K_FROM? 
  (  {IsMySql  || IsSQLite     || IsMsSql}?     indexed_table_name
   | {IsOracle || IsPostgreSql || IsPervasive}? aliased_table_name )
   ( {IsMsSql}? f2=K_FROM aliased_table_name )?
   ( K_WHERE predicate )?
 ;

merge_stmt
 : with_clause? 
   K_MERGE K_INTO aliased_table_name
   K_USING ( aliased_table_name | aliased_query )
   K_ON ( {IsMsSql}? p=predicate | {IsOracle}? '(' predicate ')' )
   (  primary=  merge_update_clause merge_insert_clause?
    | secondary=merge_insert_clause merge_update_clause?
   )?
 ;

merge_update_clause
 : K_WHEN K_MATCHED K_THEN K_UPDATE K_SET assignments
 ;

merge_insert_clause
 : K_WHEN K_NOT K_MATCHED K_THEN K_INSERT column_names?
   K_VALUES value_columns
 ;

call_stmt
 : {IsOracle}?
   declare* ';'+ K_BEGIN K_CALL function_name '(' exprs? ')' ';'+ K_END ';'+
 | {IsMySql || IsPervasive}?
   K_CALL function_name '(' exprs? ')'
 | {IsMsSql}?
   declare* ( K_EXEC | K_EXECUTE ) function_name params?
 ;

truncate_stmt
 : K_TRUNCATE K_TABLE table_name
 ;

if_stmt
 : K_IF predicate K_THEN stmts
   ( K_ELSIF predicate K_THEN stmts )*
   ( K_ELSE stmts )?
   K_END K_IF
 ;

sqlite_pragma_stmt
 : K_PRAGMA K_TABLE_INFO 
  '(' ( PLACEHOLDER1 | PLACEHOLDER2 | table_name ) ')'
 ;

null_stmt
 :
 ;


/*
   Clauses
*/

with_clause
 : K_WITH K_RECURSIVE? with_definition ( ',' with_definition )*
 ;

with_definition
 : table_name unqualified_column_names? K_AS '(' query ')'
 ;

query
 : query_clause
   orderBy_clause?
   limit_clause?
 ;

query_clause
 : K_SELECT
   ( K_DISTINCT | K_ALL )?
   ( {IsMsSql || IsPervasive}? K_TOP (                 UINTEGER_LITERAL
                                      | {IsMsSql}? '(' UINTEGER_LITERAL ')' ) )?
   ( STAR | result_columns )
   ( K_FROM join_clause )?
   ( K_WHERE predicate )?
   ( groupBy_clause ( K_HAVING predicate )? )?   # SingleQueryClause
 | query_clause
   ( K_UNION K_ALL? | K_INTERSECT | K_EXCEPT | K_MINUS )
   query_clause                                  # CompoundQueryClause
 | '(' query_clause ')'                          # BracketedQueryClause
 | K_VALUES '(' exprs ')' ( ',' '(' exprs ')' )* # ValueQueryClause 
 ;

aliased_query
 : '(' query ')' ( K_AS? table_alias )?
 ; 

result_columns
 : result_column ( COMMA result_column )*
 ;

result_column
 : table_name '.' STAR
 | expr ( K_AS? column_alias )?
 ;

assignments
 : column_name ASSIGN value_column ( COMMA column_name ASSIGN value_column )*
 ;

column_names
 : '(' column_name ( COMMA column_name )* ')'
 ;

unqualified_column_names
 : '(' unqualified_column_name ( COMMA unqualified_column_name )* ')'
 ;

values_clauses
 : value_columns ( COMMA value_columns )*
 ;

value_columns
 : '(' value_column ( COMMA value_column )* ')'
 ;

value_column
 : K_DEFAULT
 | expr
 ;

exprs
 : expr ( ',' expr )*
 ;

join_clause
 : indexed_aliased_table_name                             # TableSource
 | aliased_query                                          # SubQuerySource
 | join_clause join_operator join_clause join_constraint? # JoinSource
 | '(' join_clause ')' ( K_AS? table_alias )?             # BracketedSource
 | join_clause COMMA join_clause                          # CommaJoinSource
 ;

join_constraint
 : K_ON predicate
 | {IsOracle || IsMySql || IsSQLite || IsPostgreSql || IsPervasive}?
   K_USING unqualified_column_names
 ;

join_operator
 : ({IsOracle || IsMySql || IsSQLite || IsPostgreSql}? K_NATURAL)? 
  (  K_LEFT  K_OUTER?
   | K_RIGHT K_OUTER?
   | K_FULL  K_OUTER?
   | K_INNER
   | K_CROSS
  )?
   K_JOIN
 ;

groupBy_clause
 : K_GROUP K_BY exprs
 ;

orderBy_clause
 : K_ORDER K_BY ordering_term ( ',' ordering_term )*
 ;

ordering_term
 : expr ( K_COLLATE collation_name )? ( K_ASC | K_DESC )? ( K_NULLS ( K_FIRST | K_LAST ) )?
 ;

partitionBy_clause
 : K_PARTITION K_BY partitioning_term ( ',' partitioning_term )*
 ;

partitioning_term
 : expr ( K_COLLATE collation_name )?
 ;

declare
 : K_DECLARE ( {IsMsSql}? PLACEHOLDER1 | unqualified_column_name ) type_name
 ;

params
 : ( param | out_param ) ( COMMA ( param | out_param ) )*
 ;

param
  : (PLACEHOLDER1 '=' )? literal_value
  ;

out_param
 : (PLACEHOLDER1 '=' )? PLACEHOLDER1 K_OUTPUT
 ;

limit_clause
 : {IsMySql || IsSQLite || IsPostgreSql || IsPervasive}?
   K_LIMIT expr ( ( K_OFFSET | COMMA ) expr )?
 | {IsOracle || IsMsSql}?
   K_OFFSET uint0=UINTEGER_LITERAL row0=( K_ROW | K_ROWS )
   ( K_FETCH ( K_FIRST | K_NEXT ) uint1=UINTEGER_LITERAL K_PERCENT?
     row1=( K_ROW | K_ROWS ) ( K_ONLY | {IsOracle}? K_WITH K_TIES ) 
   )?
 | {IsOracle}?
   ( K_FETCH ( K_FIRST | K_NEXT ) uint1=UINTEGER_LITERAL K_PERCENT?
     row1=( K_ROW | K_ROWS ) ( K_ONLY | K_WITH K_TIES ) 
   )
 ;

for_update_clause
 : {IsOracle || IsMySql || IsPostgreSql}?
   K_FOR K_UPDATE
   for_update_of_clause?
   ( K_NOWAIT | K_WAIT UINTEGER_LITERAL? |  K_SKIP K_LOCKED )?
 ;

for_update_of_clause
 : K_OF column_name ( ',' column_name )*
 ;

/*
   Predicates
*/

predicate
 : PLACEHOLDER1                                         # PhPredicate
 | PLACEHOLDER2                                         # PhPredicate
 | expr op=( '<' | '<=' | '>'  | '>=' ) expr            # BinaryOpPredicate
 | expr op=( '=' | '==' | '!=' | '<>' ) expr            # BinaryOpPredicate
 | expr K_NOT? op=( K_LIKE | K_ILIKE | K_GLOB | K_MATCH | K_REGEXP )
   expr ( K_ESCAPE expr )?                              # LikePredicate
 | expr K_IS K_NOT? K_NULL                              # IsNullPredicate
 | expr K_IS K_NOT? expr                                # IsPredicate
 | expr K_NOT? K_BETWEEN expr K_AND expr                # BetweenPredicate
 | expr K_NOT? K_IN '(' ( exprs | query )? ')'          # InPredicate
 | expr op1=( '<' | '<=' | '>' | '>=' | '=' | '==' | '!=' | '<>' ) 
        op2=( K_ANY | K_SOME | K_ALL) '(' query ')'     # SubQueryPredicate
 | K_EXISTS '(' query ')'                               # ExistsPredicate
 | predicate K_COLLATE collation_name                   # CollatePredicate
 | K_NOT predicate                                      # NotPredicate
 | predicate K_AND predicate                            # AndPredicate
 | predicate K_OR predicate                             # OrPredicate
 | '(' predicate ')'                                    # BracketedPredicate
 ;

/*
    SQLite understands the following binary operators, in order from highest to
    lowest precedence:

    ||
    *    /    %
    +    -
    <<   >>   &    |
    <    <=   >    >=
    =    ==   !=   <>   IS   IS NOT   IN   LIKE   GLOB   MATCH   REGEXP
    AND
    OR
*/

expr
 : op=( '+' | '-' ) ( UINTEGER_LITERAL 
                    | UNUMERIC_LITERAL ) # SignedNumberExpr
 | literal_value                      # LiteralExpr
 | PLACEHOLDER1                       # PhExpr
 | PLACEHOLDER2                       # PhExpr
 | column_name ( {IsOracle}? OUTER_JOIN )? # ColumnExpr
 | '(' query ')'                      # SubQueryExpr
 | '~' expr                           # BitwiseNotExpr
 | expr {IsOracle || IsSQLite || IsPostgreSql}?
        op='||' expr                  # BinaryOpExpr
 | expr op=( '*' | '/' | '%' ) expr   # BinaryOpExpr
 | expr op=( '+' | '-' ) expr         # BinaryOpExpr
 | expr op=( '<<' | '>>' | '&' | '|' ) expr  # BinaryOpExpr
 | substring_function                 # SubstrFuncExpr
 | extract_function                   # ExtractFuncExpr
 | aggregate_function1                # AggregateFuncExpr
 | aggregate_function2                # AggregateFuncExpr
 | window_function                    # WindowFuncExpr
 | generic_function                   # GenericFuncExpr
 | '(' expr ')'                       # BracketedExpr
 | K_CAST '(' expr K_AS type_name ')' # CastExpr
 | K_CASE expr ( K_WHEN expr K_THEN expr )+
          (K_ELSE expr)?
   K_END                              # Case1Expr
 | K_CASE ( K_WHEN predicate K_THEN expr )+
          (K_ELSE expr)?
   K_END                              # Case2Expr
 ;

substring_function
 : ( K_SUBSTRING | K_SUBSTR)
  '(' expr ( ',' | K_FROM ) expr ( ( ',' | K_FOR ) expr )? ')'
 ;

extract_function
 : K_EXTRACT
  '(' datetimeField ( ',' | K_FROM ) expr ')'
 ;

aggregate_function1
 : ( K_COUNT | K_SUM | K_AVG | {IsSQLite}? K_TOTAL | {IsMsSql || IsPervasive}? K_COUNT_BIG )
   '(' ( K_ALL | K_DISTINCT )? ( expr | STAR ) ')'
 ;

aggregate_function2
 : ( K_MAX | K_MIN ) '(' expr ')'
 | {IsOracle || IsPostgreSql}? K_CORR         '(' expr ',' expr ')'
 | {IsSQLite}?                 K_GROUP_CONCAT '(' expr ( ',' expr )? ')'
 | {IsOracle || IsPostgreSql || IsMySql}? ( K_STDDEV_POP | K_VAR_POP )  '(' expr ')'
 | {IsMsSql  || IsPervasive }?            ( K_STDEVP | K_VAR | K_VARP ) '(' expr ')'
 | {IsMsSql  || IsPervasive }?            K_STDEV    '(' expr ')'
 | {IsOracle || IsPostgreSql || IsMySql}? K_VARIANCE '(' expr ')'
 | {IsOracle || IsPostgreSql || IsMySql}? K_STDDEV   '(' expr ')'
 | {IsOracle}? K_MEDIAN '(' expr ')'
 ;

window_function
 : function_name '(' ( K_ALL | K_DISTINCT )? ( exprs | STAR )? ')'
   K_OVER '(' partitionBy_clause? orderBy_clause ')'
 ;

generic_function
 : function_name '(' exprs? ')'
 ;


/*
  Identifiers
*/

qualified_schema_name
 /* ServerName.DatabaseName.SchemaName */
 : ( ( s=identifier '.' )? d=identifier '.' )? n=identifier
 ;

function_name
 : ( qualified_schema_name '.' )? identifier
 ;

index_name 
 : ( qualified_schema_name '.' )? identifier
 ;

table_name
 : ( qualified_schema_name '.' )? identifier
 ;

column_name
 : ( table_name    '.' )? identifier
 ;

unqualified_column_name
 : identifier
 ;

aliased_table_name
 : table_name ( K_AS? table_alias )?
 ;

indexed_table_name
 : table_name
   ( {IsSQLite}? ( K_INDEXED K_BY index_name | K_NOT K_INDEXED ) )?
 ;

indexed_aliased_table_name
 : aliased_table_name
   ( {IsSQLite}? ( K_INDEXED K_BY index_name | K_NOT K_INDEXED ) )?
 ;

type_name
 : identifier (  '(' UINTEGER_LITERAL ')'
               | '(' UINTEGER_LITERAL ',' UINTEGER_LITERAL ')' )?
 ;

collation_name 
 : identifier
 ;

table_alias 
 : IDENTIFIER
 ;

column_alias
 : IDENTIFIER
 ;

identifier
 : IDENTIFIER
 | identifiable_keyword
 ;

identifiable_keyword
 : K_ABORT
/* | K_ALL */
/* | K_AND */
 | K_ANY
/* | K_AS  */
 | K_ASC
 | K_AVG
 | K_BEGIN
/* | K_BETWEEN */
/* | K_BY   */
/* | K_CALL */
/* | K_CASE */
 | K_CAST
 | K_COLLATE
 | K_CORR
 | K_COUNT
 | K_COUNT_BIG
 | K_CROSS
/* | K_CURRENT_DATE      */
/* | K_CURRENT_TIME      */
/* | K_CURRENT_TIMESTAMP */
 | K_DATE
 | K_DAY
 | K_DECLARE
 | K_DEFAULT
/* | K_DELETE   */
 | K_DESC
/* | K_DISTINCT */
/* | K_ELSE     */
/* | K_ELSIF    */
/* | K_END      */
 | K_ESCAPE
/* | K_EXCEPT   */
 | K_EXEC
 | K_EXECUTE
/* | K_EXISTS   */
 | K_EXTRACT
 | K_FAIL
 | K_FETCH
 | K_FIRST
 | K_FOR
/* | K_FROM     */
 | K_FULL
 | K_GLOB
/* | K_GROUP    */
 | K_GROUP_CONCAT
/* | K_HAVING   */
 | K_HOUR
/* | K_IF       */
 | K_IGNORE
 | K_ILIKE
/* | K_IN       */
 | K_INDEXED
 | K_INNER
/* | K_INSERT   */
/* | K_INTERSECT*/
 | K_INTERVAL
/* | K_INTO     */
/* | K_IS       */
/* | K_JOIN     */
 | K_LAST
 | K_LEFT
/* | K_LIKE     */
 | K_LIMIT
 | K_LOCKED
 | K_MATCH
 | K_MATCHED
 | K_MAX
 | K_MEDIAN
/* | K_MERGE    */
 | K_MIN
 | K_MINUS
 | K_MINUTE
 | K_MONTH
 | K_NATURAL
 | K_NEXT
/* | K_NOT      */
 | K_NOWAIT
/* | K_NULL     */
 | K_NULLS
 | K_OF
 | K_OFFSET
 | K_ON
 | K_ONLY
/* | K_OR       */
/* | K_ORDER    */
 | K_OUTER
 | K_OUTPUT
 | K_OVER
 | K_PRAGMA
 | K_PARTITION
 | K_RECURSIVE
 | K_REGEXP
 | K_REPLACE
 | K_PERCENT
 | K_RIGHT
 | K_ROLLBACK
 | K_ROW
 | K_ROWS
 | K_SECOND
/* | K_SELECT  */
/* | K_SET     */
 | K_SKIP
 | K_SOME
 | K_SUM
 | K_STDEVP
 | K_STDDEV
 | K_STDDEV_POP
 | K_STDEV
 | K_SUBSTR
 | K_SUBSTRING
 | K_TABLE
 | K_TABLE_INFO
/* | K_THEN    */
 | K_TIES
 | K_TIME
 | K_TIMESTAMP
 | K_TO
 | K_TOP
 | K_TOTAL
/* | K_TRUNCATE */
/* | K_UNION    */
/* | K_UPDATE   */
 | K_USING
/* | K_VALUES  */
 | K_VAR
 | K_VARIANCE
 | K_VARP
 | K_VAR_POP
 | K_WAIT
/* | K_WHEN    */
/* | K_WHERE   */
 | K_WITH
 | K_YEAR
 ;

/*
  Literals
*/

literal_value
 : STRING_LITERAL
 | UINTEGER_LITERAL
 | K_NULL
 | DATE_LITERAL
 | TIME_LITERAL
 | TIMESTAMP_LITERAL
 | INTERVAL_LITERAL
 | UNUMERIC_LITERAL
 | BLOB_LITERAL
 ;


/*
  Others
*/

datetimeField
 : K_YEAR
 | K_MONTH
 | K_DAY
 | K_HOUR
 | K_MINUTE
 | K_SECOND
 ;


SCOL  : ';';
DOT   : '.';
LPAR  : '(';
RPAR  : ')';
COMMA : ',';
ASSIGN  : '=';
STAR  : '*';
PLUS  : '+';
MINUS : '-';
TILDE : '~';
PIPE2 : '||';
DIV   : '/';
MOD   : '%';
LT2   : '<<';
GT2   : '>>';
AMP   : '&';
PIPE  : '|';
LT    : '<';
LT_EQ : '<=';
GT    : '>';
GT_EQ : '>=';
EQ    : '==';
NOT_EQ1 : '!=';
NOT_EQ2 : '<>';
OUTER_JOIN : '(+)';


UINTEGER_LITERAL
 : DIGIT+                     {IsValidInt(Text)}?
 | '0x' ( [a-fA-F] | DIGIT )+ {IsValidHexicalInt(Text)}?
 ;

UNUMERIC_LITERAL
 : DIGIT+ ( '.' DIGIT* )? ( E [-+]? DIGIT+ )? ( D | F )?
 | '.' DIGIT+ ( E [-+]? DIGIT+ )? ( D | F )?
 | '0x' ( [a-fA-F] | DIGIT )+
 ;

DATE_LITERAL
 : K_DATE? SPACES* '\'' YEAR_LITERAL 
   ( '-' MONTH_LITERAL '-' | '/' MONTH_LITERAL '/' )
   DAY_LITERAL '\''
   {IsValidDate(Text)}?
 | K_CURRENT_DATE
 ;

TIME_LITERAL
 : K_TIME? SPACES* '\'' HOUR_LITERAL ':' MINUTE_LITERAL ':' SECOND_LITERAL ( '.' DIGIT+ )? '\''
 | K_CURRENT_TIME
 ;

TIMESTAMP_LITERAL
 : K_TIMESTAMP? SPACES* '\'' YEAR_LITERAL
   ( '-' MONTH_LITERAL '-' | '/' MONTH_LITERAL '/')
   DAY_LITERAL {IsValidDate(Text)}?
   (' ' | 'T')
   HOUR_LITERAL ':' MINUTE_LITERAL ':' SECOND_LITERAL ( '.' DIGIT+ )? '\''
 | K_CURRENT_TIMESTAMP
 ;

INTERVAL_LITERAL
 : K_INTERVAL SPACES*
   '\'' ( [0-9] | ' ' | ':' | '-' )+ '\'' SPACES*
   ( K_YEAR | K_MONTH | K_DAY | K_HOUR | K_MINUTE | K_SECOND )
   ( SPACES* K_TO SPACES* ( K_YEAR | K_MONTH | K_DAY | K_HOUR | K_MINUTE | K_SECOND ))?
 ;

STRING_LITERAL
 : ( N {IsOracle || IsMySql}? | 'N' {IsMsSql}? )?   '\'' ( ~'\'' | '\'\'' )* '\''
 | ( N {IsOracle || IsMySql}? | 'N' {IsMsSql}? )? Q '\'' STRING_LITERAL_SUB  '\''
 |   '"'  ( ~'"'  | '""'   )* '"' {IsMySql && !MySqlAnsiQuotes}?
 | Q '"'  STRING_LITERAL_SUB  '"' {IsMySql && !MySqlAnsiQuotes}?
 ;

fragment
STRING_LITERAL_SUB
 : '<' ( ~('<'|'>') )* '>'
 | '{' ( ~('{'|'}') )* '}'
 | '[' ( ~('['|']') )* ']'
 | '(' ( ~('('|')') )* ')'
 ;

fragment
YEAR_LITERAL
 : DIGIT DIGIT DIGIT DIGIT
 ;

fragment
MONTH_LITERAL
 : '0' [1-9] | '10' | '11' | '12'
 ;

fragment
DAY_LITERAL
 : '0' [1-9] | [1-2] [0-9] | '30' | '31'
 ;

fragment
HOUR_LITERAL
 : [0-1] [0-9] | '2' [0-3]
 ;

fragment
MINUTE_LITERAL
 : [0-5] [0-9]
 ;

fragment
SECOND_LITERAL
 : [0-5] [0-9] | '60'
 ;

BLOB_LITERAL
 : X '\'' ( [a-fA-F] | DIGIT )+ '\''
 ;

K_ABORT    : A B O R T;
K_ALL      : A L L;
K_AND      : A N D;
K_ANY      : A N Y;
K_AS       : A S;
K_ASC      : A S C;
K_AVG      : A V G;
K_BEGIN    : B E G I N;
K_BETWEEN  : B E T W E E N;
K_BY       : B Y;
K_CALL     : C A L L;
K_CASE     : C A S E;
K_CAST     : C A S T;
K_COLLATE  : C O L L A T E;
K_CORR     : C O R R;
K_COUNT    : C O U N T;
K_COUNT_BIG: C O U N T '-' B I G;
K_CROSS    : C R O S S;
K_CURRENT_DATE      : C U R R E N T '_' D A T E;
K_CURRENT_TIME      : C U R R E N T '_' T I M E;
K_CURRENT_TIMESTAMP : C U R R E N T '_' T I M E S T A M P;
K_DATE     : D A T E;
K_DAY      : D A Y;
K_DECLARE  : D E C L A R E;
K_DEFAULT  : D E F A U L T;
K_DELETE   : D E L E T E;
K_DESC     : D E S C;
K_DISTINCT : D I S T I N C T;
K_ELSE     : E L S E;
K_ELSIF    : E L S I F;
K_END      : E N D;
K_ESCAPE   : E S C A P E;
K_EXCEPT   : E X C E P T;
K_EXEC     : E X E C;
K_EXECUTE  : E X E C U T E;
K_EXISTS   : E X I S T S;
K_EXTRACT  : E X T R A C T;
K_FAIL     : F A I L;
K_FETCH    : F E T C H;
K_FIRST    : F I R S T;
K_FOR      : F O R;
K_FROM     : F R O M;
K_FULL     : F U L L;
K_GLOB     : G L O B;
K_GROUP    : G R O U P;
K_GROUP_CONCAT : G R O U P '_' C O N C A T;
K_HAVING   : H A V I N G;
K_HOUR     : H O U R;
K_IF       : I F;
K_IGNORE   : I G N O R E;
K_ILIKE    : I L I K E;
K_IN       : I N;
K_INDEXED  : I N D E X E D;
K_INNER    : I N N E R;
K_INSERT   : I N S E R T;
K_INTERSECT  : I N T E R S E C T;
K_INTERVAL : I N T E R V A L;
K_INTO     : I N T O;
K_IS       : I S;
K_JOIN     : J O I N;
K_LAST     : L A S T;
K_LEFT     : L E F T;
K_LIKE     : L I K E;
K_LIMIT    : L I M I T;
K_LOCKED   : L O C K E D;
K_MATCH    : M A T C H;
K_MATCHED  : M A T C H E D;
K_MAX      : M A X;
K_MEDIAN   : M E D I A N;
K_MERGE    : M E R G E;
K_MIN      : M I N;
K_MINUS    : M I N U S;
K_MINUTE   : M I N U T E;
K_MONTH    : M O N T H;
K_NATURAL  : N A T U R A L;
K_NEXT     : N E X T;
K_NOT      : N O T;
K_NOWAIT   : N O W A I T;
K_NULL     : N U L L;
K_NULLS    : N U L L S;
K_OF       : O F;
K_OFFSET   : O F F S E T;
K_ON       : O N;
K_ONLY     : O N L Y;
K_OR       : O R;
K_ORDER    : O R D E R;
K_OUTER    : O U T E R;
K_OUTPUT   : O U T P U T;
K_OVER     : O V E R;
K_PRAGMA   : P R A G M A;
K_PARTITION  : P A R T I T I O N;
K_RECURSIVE  : R E C U R S I V E;
K_REGEXP   : R E G E X P;
K_REPLACE  : R E P L A C E;
K_PERCENT  : P E R C E N T;
K_RIGHT    : R I G H T;
K_ROLLBACK : R O L L B A C K;
K_ROW      : R O W;
K_ROWS     : R O W S;
K_SECOND   : S E C O N D;
K_SELECT   : S E L E C T;
K_SET      : S E T;
K_SKIP     : S K I P;
K_SOME     : S O M E;
K_SUM      : S U M;
K_STDEVP   : S T D E V P;
K_STDDEV   : S T D D E V;
K_STDDEV_POP : S T D D E V '_' P O P;
K_STDEV    : S T D E V;
K_SUBSTR   : S U B S T R;
K_SUBSTRING  : S U B S T R I N G;
K_TABLE    : T A B L E;
K_TABLE_INFO : T A B L E '_' I N F O;
K_THEN     : T H E N;
K_TIES     : T I E S;
K_TIME     : T I M E;
K_TIMESTAMP  : T I M E S T A M P;
K_TO       : T O;
K_TOP      : T O P;
K_TOTAL    : T O T A L;
K_TRUNCATE : T R U N C A T E;
K_UNION    : U N I O N;
K_UPDATE   : U P D A T E;
K_USING    : U S I N G;
K_VALUES   : V A L U E S;
K_VAR      : V A R;
K_VARIANCE : V A R I A N C E;
K_VARP     : V A R P;
K_VAR_POP  : V A R '_' P O P;
K_WAIT     : W A I T;
K_WHEN     : W H E N;
K_WHERE    : W H E R E;
K_WITH     : W I T H;
K_YEAR     : Y E A R;

PLACEHOLDER1
 : '@' ID_STARTABLE_CHAR ID_CHAR*
 ;

PLACEHOLDER2
 : ':' ID_STARTABLE_CHAR ID_CHAR*
 | '?'
 ;

IDENTIFIER
 : ID_STARTABLE_CHAR ID_CHAR*
 | '"'  ( ~'"'  | '""'  )* '"'  {  IsOracle
                                || IsSQLite
                                || IsMsSql
                                || IsPostgreSql
                                || IsPervasive
                                || (IsMySql && MySqlAnsiQuotes)}?
 | '['  ( ~']'          )* ']'  {IsMsSql || IsSQLite}?
/* Future versions of SQLite might not accept */
/* | '\'' ( ~'\'' | '\'\'')* '\'' {IsSQLite}? */
 | '`'  ( ~'`'  | '``'  )* '`'  {IsMySql}?
 ;

INVALID_IDENTIFIER
 : ID_NON_STARTABLE_CHAR ID_STARTABLE_CHAR2+
   { 
     throw new RecognitionException("INVALID_ID=" + Text
                                    ,this, this.InputStream, null);
   }
 ;

SINGLE_LINE_COMMENT
 : '--' ~[\r\n]*
   -> channel(HIDDEN)
 ;

PH_ASSIGN_COMMENT
 : '/**' SPACES* PLACEHOLDER1 SPACES* ASSIGN SPACES* '"' (~'"' | '""')* '"' SPACES* '*/'
   -> channel(3)
 ;

AUTO_WHERE_COMMENT
 : '/**' SPACES* A U T O W H E R E SPACES* ASSIGN SPACES*
   '"' F A L S E '"' SPACES* '*/'
   -> channel(4)
 ;

TABLE_ALIAS_COMMENT
 : '/**' SPACES* IDENTIFIER SPACES* '*/'
   -> channel(2)
 ;

MULTILINE_COMMENT
 : '/*' .*? ( '*/' | EOF )
   -> channel(HIDDEN)
 ;

SPACES
 : [\u0020\r\n\t\u00A0\u000B\u3000] -> skip
 ;

UNEXPECTED_CHAR
 : .
 ;

fragment
ID_CHAR
 : ID_STARTABLE_CHAR
 | ID_NON_STARTABLE_CHAR
 ;

fragment
ID_STARTABLE_CHAR
 : ID_STARTABLE_CHAR1
 | ID_STARTABLE_CHAR2
 ;

fragment
ID_NON_STARTABLE_CHAR
 : '0'..'9'
 | '\u00B7'
 | '\u0300'..'\u036F'
 | '\u203F'..'\u2040'
 ;

fragment
ID_STARTABLE_CHAR1
 : D | F
 ;

fragment
ID_STARTABLE_CHAR2
 : 'A'..'C' | 'E' | 'G'..'Z'
 | 'a'..'c' | 'e' | 'g'..'z'
 | '_'
 | '\u00C0'..'\u00D6'
 | '\u00D8'..'\u00F6'
 | '\u00F8'..'\u02FF'
 | '\u0370'..'\u037D'
 | '\u037F'..'\u1FFF'
 | '\u200C'..'\u200D'
 | '\u2070'..'\u218F'
 | '\u2C00'..'\u2FEF'
 | '\u3001'..'\uD7FF'
 | '\uF900'..'\uFDCF'
 | '\uFDF0'..'\uFFFD'
 | '#' {IsOracle}?
 ; // ignores | ['\u10000-'\uEFFFF] ;

fragment DIGIT : [0-9];

fragment A : [aA];
fragment B : [bB];
fragment C : [cC];
fragment D : [dD];
fragment E : [eE];
fragment F : [fF];
fragment G : [gG];
fragment H : [hH];
fragment I : [iI];
fragment J : [jJ];
fragment K : [kK];
fragment L : [lL];
fragment M : [mM];
fragment N : [nN];
fragment O : [oO];
fragment P : [pP];
fragment Q : [qQ];
fragment R : [rR];
fragment S : [sS];
fragment T : [tT];
fragment U : [uU];
fragment V : [vV];
fragment W : [wW];
fragment X : [xX];
fragment Y : [yY];
fragment Z : [zZ];
