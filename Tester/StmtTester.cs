using MiniSqlParser;
using System.Collections.Generic;
using NUnit.Framework;

namespace Tester
{
  [TestFixture]
  public class StmtTester
  {
    [SetUp]
    public void initTest() {
    }

    [TearDown]
    public void finalTest() {
    }


    [Test]
    public void Literals() {
      Assert.That(parse(@"select
                         'abc'
                        ,123
                        ,123.4e+5f
                        ,.4e+5f
                        ,null
                        ,DATE'2015-02-01'
                        ,'2015-12-31'
                        ,TIME'08:01:59.0123'
                        ,'08:01:59.0123'
                        ,TIMESTAMP'2015-12-31 08:01:59.0123'
                        ,'2015-12-31 08:01:59.0123'
                        ,'2015-12-31T08:01:59.0123'
                        ,interval '300' MONTH
                        ,N'漢字'
                        ,Q'<abc>'
                        ,X'abcdef'")
          , Is.EqualTo(@"SELECT 'abc',123,123.4e+5f,.4e+5f,NULL,DATE'2015-02-01',"
                      + "'2015-12-31',TIME'08:01:59.0123','08:01:59.0123',"
                      + "TIMESTAMP'2015-12-31 08:01:59.0123','2015-12-31 08:01:59.0123',"
                      + "'2015-12-31T08:01:59.0123',interval '300' MONTH,N'漢字',Q'<abc>',"
                      + "X'abcdef'"));

      Assert.That(parse(@"select/*1*/
                         'abc'/*2*/
                        ,123/*3*/
                        ,123.4e+5f/*4*/
                        ,.4e+5f/*5*/
                        ,null/*6*/
                        ,DATE'2015-02-01'/*7*/
                        ,'2015-12-31'/*8*/
                        ,TIME'08:01:59.0123'/*9*/
                        ,'08:01:59.0123'/*10*/
                        ,TIMESTAMP'2015-12-31 08:01:59.0123'/*11*/
                        ,'2015-12-31 08:01:59.0123'/*12*/
                        ,'2015-12-31T08:01:59.0123'/*13*/
                        ,interval '300' MONTH/*14*/
                        ,N'漢字'/*15*/
                        ,Q'<abc>'/*16*/
                        ,X'abcdef'/*17*/")
          , Is.EqualTo(@"SELECT/*1*/ 'abc'/*2*/,123/*3*/,123.4e+5f/*4*/,.4e+5f/*5*/,NULL"
                      + "/*6*/,DATE'2015-02-01'/*7*/,'2015-12-31'/*8*/,"
                      + "TIME'08:01:59.0123'/*9*/,'08:01:59.0123'/*10*/,"
                      + "TIMESTAMP'2015-12-31 08:01:59.0123'/*11*/,"
                      + "'2015-12-31 08:01:59.0123'/*12*/,'2015-12-31T08:01:59.0123'"
                      + "/*13*/,interval '300' MONTH/*14*/,N'漢字'/*15*/,Q'<abc>'/*16*/,"
                      + "X'abcdef'/*17*/"));

      Assert.That(parse(@"select * from T
                          where d = CURRENT_DATE /* 1 */ 
                            and t = current_time /* 2 */ 
                            and s = Current_TimeStamp /* 3 */")
           , Is.EqualTo(@"SELECT * FROM T "
                       + "WHERE d=CURRENT_DATE/* 1 */ "
                       + "AND t=current_time/* 2 */ "
                       + "AND s=Current_TimeStamp/* 3 */"));
    }

    [Test]
    public void LiteralsModify() {
      var select = (SelectStmt)MiniSqlParserAST.CreateStmt("select T.*");
      var query = (SingleQuery)select.Query;

      query.Results.Clear();
      query.Results.Add(new ResultExpr(new StringLiteral("'abc'")));
      query.Results.Add(new ResultExpr(new UNumericLiteral("123")));
      query.Results.Add(new ResultExpr(new UNumericLiteral("123.4e+5f")));
      query.Results.Add(new ResultExpr(new UNumericLiteral(".4e+5f")));
      query.Results.Add(new ResultExpr(new NullLiteral()));
      query.Results.Add(new ResultExpr(new DateLiteral("DATE'2015-02-01'")));
      query.Results.Add(new ResultExpr(new DateLiteral("'2015-12-31'")));
      query.Results.Add(new ResultExpr(new TimeLiteral("TIME'08:01:59.0123'")));
      query.Results.Add(new ResultExpr(new TimeLiteral("'08:01:59.0123'")));
      query.Results.Add(new ResultExpr(new TimeStampLiteral("TIMESTAMP'2015-12-31 08:01:59.0123'")));
      query.Results.Add(new ResultExpr(new TimeStampLiteral("'2015-12-31 08:01:59.0123'")));
      query.Results.Add(new ResultExpr(new TimeStampLiteral("'2015-12-31T08:01:59.0123'")));
      query.Results.Add(new ResultExpr(new IntervalLiteral("interval '300' MONTH")));
      query.Results.Add(new ResultExpr(new StringLiteral("N'漢字'")));
      query.Results.Add(new ResultExpr(new StringLiteral("Q'<abc>'")));
      query.Results.Add(new ResultExpr(new BlobLiteral("X'abcdef'")));

      Assert.That(parse(new SelectStmt(null, query, null))
          , Is.EqualTo(@"SELECT 'abc',123,123.4e+5f,.4e+5f,NULL,DATE'2015-02-01',"
                      + "'2015-12-31',TIME'08:01:59.0123','08:01:59.0123',"
                      + "TIMESTAMP'2015-12-31 08:01:59.0123','2015-12-31 08:01:59.0123',"
                      + "'2015-12-31T08:01:59.0123',interval '300' MONTH,N'漢字',Q'<abc>',"
                      + "X'abcdef'"));
    }


    [Test]
    public void UpdateStmt() {
      Assert.That(parse("UPDATE T SET a=a"), Is.EqualTo("UPDATE T SET a=a"));
      // SQL文の末尾にTable.Columnがある場合、Schema.Tableと解析されることがあった
      Assert.That(parse("UPDATE T SET a= T.a"), Is.EqualTo("UPDATE T SET a=T.a"));
      Assert.That(parse("UPDATE T SET a= S.T.a"), Is.EqualTo("UPDATE T SET a=S.T.a"));
      Assert.That(parse("update S.T t1 set S.t1.x = 1 ,y = 'abc' where 1=1")
                , Is.EqualTo("UPDATE S.T t1 SET S.t1.x=1,y='abc' WHERE 1=1"));
      Assert.That(parse("update /*1*/ A/*2*/./*3*/B/*4*/ c/*5*/ set/*6*/ "
                        + "a/*7*/./*8*/b/*9*/./*10*/c/*11*/=/*12*/'1'/*13*/"
                        + ",/*14*/ a/*15*/./*16*/b/*17*/./*18*/d/*19*/=/*20*/'2'/*21*/ "
                        + "where/*22*/ a/*23*/=/*24*/a/*25*/")
                , Is.EqualTo("UPDATE/*1*/ A/*2*/./*3*/B/*4*/ c/*5*/ SET/*6*/ "
                            + "a/*7*/./*8*/b/*9*/./*10*/c/*11*/=/*12*/'1'/*13*/"
                            + ",/*14*/a/*15*/./*16*/b/*17*/./*18*/d/*19*/=/*20*/'2'/*21*/ "
                            + "WHERE/*22*/ a/*23*/=/*24*/a/*25*/"));
      Assert.That(parse("UPDATE T t1 SET x=1 WHERE/*a*/ 1/*b*/=/*c*/1/*d*/ or/*e*/ 'a'/*f*/=/*g*/'b'/*h*/ "
                            + "and/*i*/ 2/*j*/=/*k*/2/*l*/")
                , Is.EqualTo("UPDATE T t1 SET x=1 WHERE/*a*/ 1/*b*/=/*c*/1/*d*/ OR/*e*/ 'a'/*f*/=/*g*/'b'/*h*/ "
                            + "AND/*i*/ 2/*j*/=/*k*/2/*l*/"));
      Assert.That(parse("UPDATE T AS t1 SET y=CASE 0 WHEN 1 THEN 2 WHEN 3 THEN 4 ELSE 5 END")
                , Is.EqualTo("UPDATE T AS t1 SET y=CASE 0 WHEN 1 THEN 2 WHEN 3 THEN 4 ELSE 5 END"));
      Assert.That(parse("UPDATE T AS t1 SET y=CASE WHEN 1=1 THEN 1 ELSE 0 END")
                , Is.EqualTo("UPDATE T AS t1 SET y=CASE WHEN 1=1 THEN 1 ELSE 0 END"));
      Assert.That(parse("UPDATE T AS t1 SET y=CASE WHEN 1=1 THEN 1 WHEN 2=2 THEN 2 ELSE 3 END")
                , Is.EqualTo("UPDATE T AS t1 SET y=CASE WHEN 1=1 THEN 1 WHEN 2=2 THEN 2 ELSE 3 END"));

    }

    [Test]
    public void SelectStmt() {
      Assert.That(parse("SELECT 1, 'abc' from T join U on T.id=U.id")
                , Is.EqualTo("SELECT 1,'abc' FROM T JOIN U ON T.id=U.id"));
      Assert.That(parse("select 1, 'abc'"
                       + " from T join U"
                       + " on T.id = U.id and T.id2 = U.id2"
                       + " where T.attr = 'aaa' and 1=1"
                       + " group by T.attr, U.attr"
                       + " having 1=1 and 'a' <> 'b'"
                       + " order by 2 collate JP desc nulls first , 3 asc")
                , Is.EqualTo("SELECT 1,'abc' FROM T JOIN U ON T.id=U.id AND "
                           + "T.id2=U.id2 WHERE T.attr='aaa' AND 1=1 GROUP BY "
                           + "T.attr,U.attr HAVING 1=1 AND 'a'<>'b' ORDER BY "
                           + "2 COLLATE JP DESC NULLS FIRST,3 ASC"));
      Assert.That(parse("select 'a', 'b'"
                        + " from T t1 left join U u1"
                        + " using (id, id2)"
                        + " where t1.attr = 'a'"
                        + " group by 1"
                        + " having count(*) > 0"
                        + " order by t1.attr")
               , Is.EqualTo("SELECT 'a','b' FROM T t1 LEFT JOIN U u1 "
                          + "USING(id,id2) WHERE t1.attr='a' GROUP BY 1 "
                          + "HAVING count(*)>0 ORDER BY t1.attr"));
      Assert.That(parse("select top 10 'abc' as A, 'def' F from T as t1")
               , Is.EqualTo("SELECT TOP 10 'abc' AS A,'def' F FROM T AS t1"));
      Assert.That(parse("select T.* from T limit 10")
         , Is.EqualTo("SELECT T.* FROM T LIMIT 10"));
      Assert.That(parse("select * from T limit 10 offset 1")
         , Is.EqualTo("SELECT * FROM T LIMIT 10 OFFSET 1"));
      Assert.That(parse("select * from T order by 1 limit 1,10")
         , Is.EqualTo("SELECT * FROM T ORDER BY 1 LIMIT 1,10"));
      Assert.That(parse("select top 10 * from T")
         , Is.EqualTo("SELECT TOP 10 * FROM T"));
      Assert.That(parse("select top (100) * from T")
         , Is.EqualTo("SELECT TOP 100 * FROM T"));
      Assert.That(parse(@"select/*1*/ a/*2*/ as/*3*/ `a`/*4*/ from/*5*/ T/*6*/")
         , Is.EqualTo(@"SELECT/*1*/ a/*2*/ AS/*3*/ `a`/*4*/ FROM/*5*/ T/*6*/"));
    }

    [Test]
    public void SelectStmtComments(){
      Assert.That(parse("select /*1*/ * /*2*/ from /*3*/ T /*4*/")
         , Is.EqualTo("SELECT/*1*/ */*2*/ FROM/*3*/ T/*4*/"));
      Assert.That(parse("select /*1*/ * /*2*/ from /*3*/ T /*4*/"
                      + "order /*5*/ by /*6*/ 1 /*7*/")
         , Is.EqualTo("SELECT/*1*/ */*2*/ FROM/*3*/ T/*4*/"
                      + " ORDER/*5*/ BY/*6*/ 1/*7*/"));
      Assert.That(parse("select /*1*/ * /*2*/ from /*3*/ T /*4*/"
                      + "order /*5*/ by /*6*/ 1 /*7*/ limit /*8*/ 1/*9*/")
         , Is.EqualTo("SELECT/*1*/ */*2*/ FROM/*3*/ T/*4*/"
                      + " ORDER/*5*/ BY/*6*/ 1/*7*/ LIMIT/*8*/ 1/*9*/"));
      Assert.That(parse("select /*1*/ * /*2*/ from /*3*/ T /*4*/"
                      + "order /*5*/ by /*6*/ 1 /*7*/ limit /*8*/ 1/*9*/ ,/*10*/ 10 /*11*/")
         , Is.EqualTo("SELECT/*1*/ */*2*/ FROM/*3*/ T/*4*/"
                      + " ORDER/*5*/ BY/*6*/ 1/*7*/ LIMIT/*8*/ 1/*9*/,/*10*/10/*11*/"));
      Assert.That(parse("select/*1*/ ALL/*2*/ TOP/*3*/ 10/*4*/"
                      + "  a/*5*/ as/*6*/ b/*7*/,/*8*/ a/*9*/ b/*10*/"
                      + "from/*11*/"
                      + "  S/*12*/./*13*/T/*14*/ t1/*15*/ right/*16*/ outer/*17*/ join/*18*/ S/*19*/./*20*/U/*21*/ u1/*22*/"
                      + "on/*23*/"
                      + "  t1/*24*/./*25*/id/*26*/ =/*27*/ u1/*28*/./*29*/id/*30*/"
                      + "where/*31*/"
                      + "      t1/*32*/./*33*/attr/*34*/ =/*35*/ 'abc'/*36*/"
                      + "  and/*37*/ t1/*38*/./*39*/name/*40*/ =/*41*/ 'def'/*42*/"
                      + "group/*43*/ by/*44*/"
                      + "  t1/*45*/./*46*/id/*47*/,/*48*/ u1/*49*/./*50*/id/*51*/"
                      + "having/*52*/"
                      + "      count/*53*/(/*54*/*/*55*/)/*56*/ >/*57*/ 0/*58*/"
                      + "  and/*59*/ 1/*60*/=/*61*/1/*62*/"
                      + "order/*63*/ by/*64*/"
                      + "  t1/*65*/./*66*/id/*67*/ collate/*68*/ UTF8/*69*/ desc/*70*/ nulls/*71*/ first/*72*/"
                      + ",/*73*/ t1/*74*/./*75*/attr/*76*/ collate/*77*/ UTF16/*78*/ asc/*79*/  nulls/*80*/ last/*81*/"
                      + "limit/*82*/ a/*83*/ ,/*84*/ b/*85*/")
          , Is.EqualTo("SELECT/*1*/ ALL/*2*/ TOP/*3*/ 10/*4*/ a/*5*/ AS/*6*/ b/*7*/,"
                      + "/*8*/a/*9*/ b/*10*/ FROM/*11*/ S/*12*/./*13*/T/*14*/ t1/*15*/ "
                      + "RIGHT/*16*/ OUTER/*17*/ JOIN/*18*/ S/*19*/./*20*/U/*21*/ u1"
                      + "/*22*/ ON/*23*/ t1/*24*/./*25*/id/*26*/=/*27*/u1/*28*/./*29*/id"
                      + "/*30*/ WHERE/*31*/ t1/*32*/./*33*/attr/*34*/=/*35*/'abc'/*36*/ "
                      + "AND/*37*/ t1/*38*/./*39*/name/*40*/=/*41*/'def'/*42*/ GROUP"
                      + "/*43*/ BY/*44*/ t1/*45*/./*46*/id/*47*/,/*48*/u1/*49*/./*50*/id"
                      + "/*51*/ HAVING/*52*/ count/*53*/(/*54*/*/*55*/)/*56*/>/*57*/0"
                      + "/*58*/ AND/*59*/ 1/*60*/=/*61*/1/*62*/ ORDER/*63*/ BY/*64*/ t1"
                      + "/*65*/./*66*/id/*67*/ COLLATE/*68*/ UTF8/*69*/ DESC/*70*/ NULLS"
                      + "/*71*/ FIRST/*72*/,/*73*/t1/*74*/./*75*/attr/*76*/ COLLATE/*77*/ "
                      + "UTF16/*78*/ ASC/*79*/ NULLS/*80*/ LAST/*81*/ LIMIT/*82*/ a/*83*/"
                      + ",/*84*/b/*85*/"));
      Assert.That(parse("select/*1*/ 1/*2*/,/*3*/2/*4*/,/*5*/3/*6*/,/*7*/null/*8*/ "
                        + "from/*9*/ T/*10*/ natural/*11*/ join/*12*/ U/*13*/"
                        + "using/*14*/ (/*15*/a/*16*/,/*17*/b/*18*/)/*19*/"
                        + "group/*20*/ by/*21*/ 1/*22*/ having/*23*/ 1/*24*/=/*25*/1/*26*/"
                        + "union/*27*/ all/*28*/"
                        + "select/*29*/ a/*30*/,/*31*/b/*32*/,/*33*/c/*34*/,/*35*/null/*36*/"
                        + "from/*37*/ V/*38*/"
                        + "order/*39*/ by/*40*/ a/*41*/"
                        + "limit/*42*/ a/*43*/,/*44*/b/*45*/")
          , Is.EqualTo("SELECT/*1*/ 1/*2*/,/*3*/2/*4*/,/*5*/3/*6*/,/*7*/NULL/*8*/ "
                        + "FROM/*9*/ T/*10*/ NATURAL/*11*/ JOIN/*12*/ U/*13*/ "
                        + "USING/*14*/(/*15*/a/*16*/,/*17*/b/*18*/)/*19*/ "
                        + "GROUP/*20*/ BY/*21*/ 1/*22*/ HAVING/*23*/ 1/*24*/=/*25*/1/*26*/ "
                        + "UNION/*27*/ ALL/*28*/ "
                        + "SELECT/*29*/ a/*30*/,/*31*/b/*32*/,/*33*/c/*34*/,/*35*/NULL/*36*/ "
                        + "FROM/*37*/ V/*38*/ "
                        + "ORDER/*39*/ BY/*40*/ a/*41*/ "
                        + "LIMIT/*42*/ a/*43*/,/*44*/b/*45*/"));

      Assert.That(parse("select/*1*/ 'str'/*2*/"
                        + "order/*3*/ by/*4*/ a/*5*/"
                        + "limit/*6*/ a/*7*/,/*8*/b/*9*/")
          , Is.EqualTo("SELECT/*1*/ 'str'/*2*/ "
                        + "ORDER/*3*/ BY/*4*/ a/*5*/ "
                        + "LIMIT/*6*/ a/*7*/,/*8*/b/*9*/"));
      Assert.That(parse("select/*1*/ func/*2*/(/*3*/1/*4*/,/*5*/2/*6*/)/*7*/ "
                        + "order/*8*/ by/*9*/ a/*10*/"
                        + "limit/*11*/ a/*12*/")
          , Is.EqualTo("SELECT/*1*/ func/*2*/(/*3*/1/*4*/,/*5*/2/*6*/)/*7*/ "
                        + "ORDER/*8*/ BY/*9*/ a/*10*/ "
                        + "LIMIT/*11*/ a/*12*/"));
      Assert.That(parse("select/*1*/ count/*2*/(/*3*/*/*4*/)/*5*/"
                        + "order/*6*/ by/*7*/ a/*8*/"
                        + "limit/*9*/ b/*10*/ offset/*11*/ a/*12*/")
          , Is.EqualTo("SELECT/*1*/ count/*2*/(/*3*/*/*4*/)/*5*/ "
                        + "ORDER/*6*/ BY/*7*/ a/*8*/ "
                        + "LIMIT/*9*/ b/*10*/ OFFSET/*11*/ a/*12*/"));
      Assert.That(parse("select/*1*/ 'str'/*2*/"
                        + "union/*3*/"
                        + "select/*4*/ T/*5*/./*6*/*/*7*/ from/*8*/ T/*9*/"
                        + "order/*10*/ by/*11*/ a/*12*/"
                        + "limit/*13*/ a/*14*/,/*15*/b/*16*/")
          , Is.EqualTo("SELECT/*1*/ 'str'/*2*/ "
                        + "UNION/*3*/ "
                        + "SELECT/*4*/ T/*5*/./*6*/*/*7*/ FROM/*8*/ T/*9*/ "
                        + "ORDER/*10*/ BY/*11*/ a/*12*/ "
                        + "LIMIT/*13*/ a/*14*/,/*15*/b/*16*/"));
      Assert.That(parse("select/*1*/ func/*2*/(/*3*/1/*4*/,/*5*/2/*6*/)/*7*/"
                        + "intersect/*8*/"
                        + "select/*9*/ S/*10*/./*11*/T/*12*/./*13*/*/*14*/"
                        + "order/*15*/ by/*16*/ a/*17*/"
                        + "limit/*18*/ a/*19*/")
          , Is.EqualTo("SELECT/*1*/ func/*2*/(/*3*/1/*4*/,/*5*/2/*6*/)/*7*/ "
                        + "INTERSECT/*8*/ "
                        + "SELECT/*9*/ S/*10*/./*11*/T/*12*/./*13*/*/*14*/ "
                        + "ORDER/*15*/ BY/*16*/ a/*17*/ "
                        + "LIMIT/*18*/ a/*19*/"));
      Assert.That(parse("select/*1*/ distinct/*2*/ count/*3*/(/*4*/*/*5*/)/*6*/"
                        + "except/*7*/"
                        + "select/*8*/ all/*9*/ count/*10*/(/*11*/*/*12*/)/*13*/"
                        + "order/*14*/ by/*15*/ a/*16*/"
                        + "limit/*17*/ b/*18*/ offset/*19*/ a/*20*/")
          , Is.EqualTo("SELECT/*1*/ DISTINCT/*2*/ count/*3*/(/*4*/*/*5*/)/*6*/ "
                        + "EXCEPT/*7*/ "
                        + "SELECT/*8*/ ALL/*9*/ count/*10*/(/*11*/*/*12*/)/*13*/ "
                        + "ORDER/*14*/ BY/*15*/ a/*16*/ "
                        + "LIMIT/*17*/ b/*18*/ OFFSET/*19*/ a/*20*/"));
      Assert.That(parse("select * from T/*1*/ indexed/*2*/ by/*3*/ idx/*4*/")
          , Is.EqualTo("SELECT * FROM T/*1*/ INDEXED/*2*/ BY/*3*/ idx/*4*/"));
      Assert.That(parse("select T.* from T/*1*/ indexed/*2*/ by/*3*/ S/*4*/./*5*/idx/*6*/")
          , Is.EqualTo("SELECT T.* FROM T/*1*/ INDEXED/*2*/ BY/*3*/ S/*4*/./*5*/idx/*6*/"));
      Assert.That(parse("select S.T.* from S/*1*/./*2*/T/*3*/ not/*4*/ indexed/*5*/")
          , Is.EqualTo("SELECT S.T.* FROM S/*1*/./*2*/T/*3*/ NOT/*4*/ INDEXED/*5*/"));
      Assert.That(parse("select S.T.x from S/*1*/./*2*/T/*3*/ as/*4*/ t1/*5*/ indexed/*6*/ by/*7*/ idx/*8*/")
          , Is.EqualTo("SELECT S.T.x FROM S/*1*/./*2*/T/*3*/ AS/*4*/ t1/*5*/ INDEXED/*6*/ BY/*7*/ idx/*8*/"));
      Assert.That(parse("select S/*1*/./*2*/T./*3*/x/*4*/ as/*5*/ c1/*6*/ from T")
          , Is.EqualTo("SELECT S/*1*/./*2*/T./*3*/x/*4*/ AS/*5*/ c1/*6*/ FROM T"));
      Assert.That(parse("select count(*) from T/*1*/ cross/*2*/ join/*3*/ "
                        + "(/*4*/select/*5*/ */*6*/ from/*7*/ dual/*8*/)/*9*/ as/*10*/ a/*11*/")
          , Is.EqualTo("SELECT count(*) FROM T/*1*/ CROSS/*2*/ JOIN/*3*/ "
                        + "(/*4*/SELECT/*5*/ */*6*/ FROM/*7*/ dual/*8*/)/*9*/ AS/*10*/ a/*11*/"));
      Assert.That(parse("select '2011-11-11' from T/*1*/ cross/*2*/ join/*3*/ "
                        + "(/*4*/select/*5*/ */*6*/ from/*7*/ dual/*8*/)/*9*/a/*10*/")
          , Is.EqualTo("SELECT '2011-11-11' FROM T/*1*/ CROSS/*2*/ JOIN/*3*/ "
                        + "(/*4*/SELECT/*5*/ */*6*/ FROM/*7*/ dual/*8*/)/*9*/a/*10*/"));
      Assert.That(parse("select S.func(1,2) from T/*1*/ cross/*2*/ join/*3*/ "
                        + "(/*4*/select/*5*/ */*6*/ from/*7*/ dual/*8*/)/*9*/")
          , Is.EqualTo("SELECT S.func(1,2) FROM T/*1*/ CROSS/*2*/ JOIN/*3*/ "
                        + "(/*4*/SELECT/*5*/ */*6*/ FROM/*7*/ dual/*8*/)/*9*/"));
    }

    [Test]
    public void WithClauseTest(){
      Assert.That(parse("with/*1*/ myView/*2*/ (/*3*/a/*4*/,/*5*/b/*6*/,/*7*/c/*8*/)/*9*/ as/*10*/"
                        + "(/*11*/select/*12*/ 1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/ from/*18*/ T/*19*/)/*20*/"
                        + "select * from myView")
          , Is.EqualTo("WITH/*1*/ myView/*2*/(/*3*/a/*4*/,/*5*/b/*6*/,/*7*/c/*8*/)/*9*/ AS/*10*/"
                        + "(/*11*/SELECT/*12*/ 1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/ FROM/*18*/ T/*19*/)/*20*/"
                        + "SELECT * FROM myView"));
      Assert.That(parse("with/*1*/ myView/*2*/ as/*3*/ (/*4*/select/*5*/ 1/*6*/,/*7*/2/*8*/,/*9*/3/*10*/ "
                        + "from/*11*/ T/*12*/)/*13*/select * from myView")
          , Is.EqualTo("WITH/*1*/ myView/*2*/ AS/*3*/(/*4*/SELECT/*5*/ 1/*6*/,/*7*/2/*8*/,/*9*/3/*10*/ "
                       + "FROM/*11*/ T/*12*/)/*13*/SELECT * FROM myView"));
      Assert.That(parse("with S.V as (select * from T)"
                        + "    ,S.W as (select 1,2 from T)"
                        + "update T set a=1")
          , Is.EqualTo("WITH S.V AS(SELECT * FROM T)"
                        + ",S.W AS(SELECT 1,2 FROM T)"
                        + "UPDATE T SET a=1"));
      Assert.That(parse( "with/*1*/ S/*2*/./*3*/V/*4*/ as/*5*/ (/*6*/select/*7*/ */*8*/ from/*9*/ T/*10*/)/*11*/"
                        + ",/*12*/S/*13*/./*14*/W/*15*/ as/*16*/ (/*17*/select/*18*/ 1/*19*/,/*20*/2/*21*/ from/*22*/ T/*23*/)/*24*/"
                        + "update/*25*/ T/*26*/ set/*27*/ a/*28*/=/*29*/1/*30*/")
          , Is.EqualTo("WITH/*1*/ S/*2*/./*3*/V/*4*/ AS/*5*/(/*6*/SELECT/*7*/ */*8*/ FROM/*9*/ T/*10*/)/*11*/"
                        + ",/*12*/S/*13*/./*14*/W/*15*/ AS/*16*/(/*17*/SELECT/*18*/ 1/*19*/,/*20*/2/*21*/ FROM/*22*/ T/*23*/)/*24*/"
                        + "UPDATE/*25*/ T/*26*/ SET/*27*/ a/*28*/=/*29*/1/*30*/"));
    }

    [Test]
    public void InsertStmtTest() {
      Assert.That(parse("insert/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/)/*8*/ " 
                       + "values/*9*/(/*10*/1/*11*/,/*12*/2/*13*/)/*14*/")
          , Is.EqualTo("INSERT/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/)/*8*/ " 
                     + "VALUES/*9*/(/*10*/1/*11*/,/*12*/2/*13*/)/*14*/"));
      Assert.That(parse("insert/*1*/ into/*2*/ T/*3*/ values/*4*/(/*5*/1/*6*/,/*7*/2/*8*/)/*9*/")
          , Is.EqualTo("INSERT/*1*/ INTO/*2*/ T/*3*/ VALUES/*4*/(/*5*/1/*6*/,/*7*/2/*8*/)/*9*/"));
      Assert.That(parse("insert/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/)/*8*/ "
                       + "select/*9*/ 1/*10*/,/*11*/2/*12*/ from/*13*/ dual/*14*/")
          , Is.EqualTo("INSERT/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/)/*8*/ "
                       + "SELECT/*9*/ 1/*10*/,/*11*/2/*12*/ FROM/*13*/ dual/*14*/"));
      Assert.That(parse("insert/*1*/ into/*2*/ T/*3*/ select/*4*/ 1/*5*/,/*6*/2/*7*/ from/*8*/ dual/*9*/")
          , Is.EqualTo("INSERT/*1*/ INTO/*2*/ T/*3*/ SELECT/*4*/ 1/*5*/,/*6*/2/*7*/ FROM/*8*/ dual/*9*/"));
      Assert.That(parse("insert/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/)/*6*/ "
                       + "values/*7*/(/*8*/1/*9*/)/*10*/")
          , Is.EqualTo("INSERT/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/)/*6*/ "
                        + "VALUES/*7*/(/*8*/1/*9*/)/*10*/"));
      Assert.That(parse("insert/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/)/*8*/ "
                        + "values/*9*/(/*10*/1/*11*/)/*12*/,/*13*/(/*14*/10/*15*/)/*16*/")
          , Is.EqualTo("INSERT/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/)/*8*/ "
                        + "VALUES/*9*/(/*10*/1/*11*/)/*12*/,/*13*/(/*14*/10/*15*/)/*16*/"));
      Assert.That(parse("insert/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/)/*10*/ "
                        + "values/*11*/(/*12*/1/*13*/)/*14*/,/*15*/(/*16*/10/*17*/)/*18*/,/*19*/(/*20*/100/*21*/)/*22*/")
          , Is.EqualTo("INSERT/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/)/*10*/ "
                        + "VALUES/*11*/(/*12*/1/*13*/)/*14*/,/*15*/(/*16*/10/*17*/)/*18*/,/*19*/(/*20*/100/*21*/)/*22*/"));
      Assert.That(parse("insert/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/,/*8*/c/*9*/)/*10*/ "
                        + "values/*11*/(/*12*/1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/)/*18*/")
          , Is.EqualTo("INSERT/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/,/*8*/c/*9*/)/*10*/ "
                        + "VALUES/*11*/(/*12*/1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/)/*18*/"));
      Assert.That(parse("insert/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/,/*8*/c/*9*/)/*10*/ "
                        + "values/*11*/(/*12*/1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/)/*18*/"
                        + ",/*19*/(/*20*/x/*21*/,/*22*/y/*23*/,/*24*/z/*25*/)/*26*/")
          , Is.EqualTo("INSERT/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/,/*8*/c/*9*/)/*10*/ "
                        + "VALUES/*11*/(/*12*/1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/)/*18*/"
                        + ",/*19*/(/*20*/x/*21*/,/*22*/y/*23*/,/*24*/z/*25*/)/*26*/"));
      Assert.That(parse("insert/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/,/*8*/c/*9*/)/*10*/ "
                        + "values/*11*/(/*12*/1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/)/*18*/"
                        + ",/*19*/(/*20*/x/*21*/,/*22*/y/*23*/,/*24*/z/*25*/)/*26*/"
                        + ",/*27*/(/*28*/x/*29*/,/*30*/y/*31*/,/*32*/z/*33*/)/*34*/")
          , Is.EqualTo("INSERT/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/,/*8*/c/*9*/)/*10*/ "
                        + "VALUES/*11*/(/*12*/1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/)/*18*/"
                        + ",/*19*/(/*20*/x/*21*/,/*22*/y/*23*/,/*24*/z/*25*/)/*26*/"
                        + ",/*27*/(/*28*/x/*29*/,/*30*/y/*31*/,/*32*/z/*33*/)/*34*/"));
      Assert.That(parse(@"Insert/*1*/ S/*2*/./*3*/T/*4*/ /** t1 */ (/*5*/a/*6*/,/*7*/b/*8*/,/*9*/c/*10*/)/*11*/
                          values/*12*/ (/*13*/1/*14*/,/*15*/2/*16*/,/*17*/3/*18*/)/*19*/,/*20*/
                          (/*21*/4/*22*/,/*23*/5/*24*/,/*25*/6/*26*/)/*27*/")
      , Is.EqualTo(@"INSERT/*1*/ S/*2*/./*3*/T/*4*//** t1 */(/*5*/a/*6*/,/*7*/b/*8*/,"
                        + "/*9*/c/*10*/)/*11*/ VALUES/*12*/(/*13*/1/*14*/,/*15*/2/*16*/,"
                        + "/*17*/3/*18*/)/*19*/,/*20*/(/*21*/4/*22*/,/*23*/5/*24*/,/*25*/6"
                        + "/*26*/)/*27*/"));
    }

    [Test]
    public void ReplaceStmtTest() {
      Assert.That(parse("replace/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/)/*8*/ "
                       + "values/*9*/(/*10*/1/*11*/,/*12*/2/*13*/)/*14*/")
          , Is.EqualTo("REPLACE/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/)/*8*/ "
                     + "VALUES/*9*/(/*10*/1/*11*/,/*12*/2/*13*/)/*14*/"));
      Assert.That(parse("replace/*1*/ into/*2*/ T/*3*/ values/*4*/(/*5*/1/*6*/,/*7*/2/*8*/)/*9*/")
          , Is.EqualTo("REPLACE/*1*/ INTO/*2*/ T/*3*/ VALUES/*4*/(/*5*/1/*6*/,/*7*/2/*8*/)/*9*/"));
      Assert.That(parse("replace/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/)/*8*/ "
                       + "select/*9*/ 1/*10*/,/*11*/2/*12*/ from/*13*/ dual/*14*/")
          , Is.EqualTo("REPLACE/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/)/*8*/ "
                       + "SELECT/*9*/ 1/*10*/,/*11*/2/*12*/ FROM/*13*/ dual/*14*/"));
      Assert.That(parse("replace/*1*/ into/*2*/ T/*3*/ select/*4*/ 1/*5*/,/*6*/2/*7*/ from/*8*/ dual/*9*/")
          , Is.EqualTo("REPLACE/*1*/ INTO/*2*/ T/*3*/ SELECT/*4*/ 1/*5*/,/*6*/2/*7*/ FROM/*8*/ dual/*9*/"));
      Assert.That(parse("replace/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/)/*6*/ "
                       + "values/*7*/(/*8*/1/*9*/)/*10*/")
          , Is.EqualTo("REPLACE/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/)/*6*/ "
                        + "VALUES/*7*/(/*8*/1/*9*/)/*10*/"));
      Assert.That(parse("replace/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/)/*8*/ "
                        + "values/*9*/(/*10*/1/*11*/)/*12*/,/*13*/(/*14*/10/*15*/)/*16*/")
          , Is.EqualTo("REPLACE/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/)/*8*/ "
                        + "VALUES/*9*/(/*10*/1/*11*/)/*12*/,/*13*/(/*14*/10/*15*/)/*16*/"));
      Assert.That(parse("replace/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/)/*10*/ "
                        + "values/*11*/(/*12*/1/*13*/)/*14*/,/*15*/(/*16*/10/*17*/)/*18*/,/*19*/(/*20*/100/*21*/)/*22*/")
          , Is.EqualTo("REPLACE/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/)/*10*/ "
                        + "VALUES/*11*/(/*12*/1/*13*/)/*14*/,/*15*/(/*16*/10/*17*/)/*18*/,/*19*/(/*20*/100/*21*/)/*22*/"));
      Assert.That(parse("replace/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/,/*8*/c/*9*/)/*10*/ "
                        + "values/*11*/(/*12*/1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/)/*18*/")
          , Is.EqualTo("REPLACE/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/,/*8*/c/*9*/)/*10*/ "
                        + "VALUES/*11*/(/*12*/1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/)/*18*/"));
      Assert.That(parse("replace/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/,/*8*/c/*9*/)/*10*/ "
                        + "values/*11*/(/*12*/1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/)/*18*/"
                        + ",/*19*/(/*20*/x/*21*/,/*22*/y/*23*/,/*24*/z/*25*/)/*26*/")
          , Is.EqualTo("REPLACE/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/,/*8*/c/*9*/)/*10*/ "
                        + "VALUES/*11*/(/*12*/1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/)/*18*/"
                        + ",/*19*/(/*20*/x/*21*/,/*22*/y/*23*/,/*24*/z/*25*/)/*26*/"));
      Assert.That(parse("replace/*1*/ into/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/,/*8*/c/*9*/)/*10*/ "
                        + "values/*11*/(/*12*/1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/)/*18*/"
                        + ",/*19*/(/*20*/x/*21*/,/*22*/y/*23*/,/*24*/z/*25*/)/*26*/"
                        + ",/*27*/(/*28*/x/*29*/,/*30*/y/*31*/,/*32*/z/*33*/)/*34*/")
          , Is.EqualTo("REPLACE/*1*/ INTO/*2*/ T/*3*/(/*4*/a/*5*/,/*6*/b/*7*/,/*8*/c/*9*/)/*10*/ "
                        + "VALUES/*11*/(/*12*/1/*13*/,/*14*/2/*15*/,/*16*/3/*17*/)/*18*/"
                        + ",/*19*/(/*20*/x/*21*/,/*22*/y/*23*/,/*24*/z/*25*/)/*26*/"
                        + ",/*27*/(/*28*/x/*29*/,/*30*/y/*31*/,/*32*/z/*33*/)/*34*/"));
      Assert.That(parse(@"Replace/*1*/ S/*2*/./*3*/T/*4*/ /** t1 */ (/*5*/a/*6*/,/*7*/b/*8*/,/*9*/c/*10*/)/*11*/
                          values/*12*/ (/*13*/1/*14*/,/*15*/2/*16*/,/*17*/3/*18*/)/*19*/,/*20*/
                          (/*21*/4/*22*/,/*23*/5/*24*/,/*25*/6/*26*/)/*27*/")
      , Is.EqualTo(@"REPLACE/*1*/ S/*2*/./*3*/T/*4*//** t1 */(/*5*/a/*6*/,/*7*/b/*8*/,"
                        + "/*9*/c/*10*/)/*11*/ VALUES/*12*/(/*13*/1/*14*/,/*15*/2/*16*/,"
                        + "/*17*/3/*18*/)/*19*/,/*20*/(/*21*/4/*22*/,/*23*/5/*24*/,/*25*/6"
                        + "/*26*/)/*27*/"));
    }

    [Test]
    public void StmtSeparatorTest() {
      Assert.That(parse(";/*1*/select * from T/*2*/;/*3*/;/*4*/update U set a= 1/*5*/"
                      + ";/*6*/;/*7*/;/*8*/insert into V values(1)/*9*/;/*10*/;/*11*/;/*12*/;/*13*/")
            ,Is.EqualTo(";/*1*/SELECT * FROM T/*2*/;/*3*/;/*4*/UPDATE U SET a=1/*5*/;"
                      + "/*6*/;/*7*/;/*8*/INSERT INTO V VALUES(1)/*9*/;/*10*/;/*11*/;/*12*/;/*13*/"));

      /* RightTrimSeparators()のテスト */
      /* ";"と共にコメントも削除される*/
      var ast = MiniSqlParserAST.CreateStmts(";/*1*/select * from T/*2*/;/*3*/;/*4*/update U set a=1/*5*/"
                                        + ";/*6*/;/*7*/;/*8*/insert into V values(1)/*9*/;/*10*/;/*11*/;/*12*/;/*13*/"
                                        , DBMSType.Unknown, false);
      ast[0].TrimRightSeparators(); // NULL   stmt
      ast[1].TrimRightSeparators(); // SELECT stmt
      ast[2].TrimRightSeparators(); // UPDATE stmt
      ast[3].TrimRightSeparators(); // INSERT stmt
      ast[4].TrimRightSeparators(); // Null   stmt
      var stringifier = new CompactStringifier(4098, true);
      ast.Accept(stringifier);
      Assert.That(stringifier.ToString()
            , Is.EqualTo("/*1*/SELECT * FROM T/*2*//*4*/UPDATE U SET a=1/*5*/"
                       + "/*8*/INSERT INTO V VALUES(1)/*9*//*13*/"));

      /* NULL文のみのテスト */
      Assert.That(parse("/* 1 */;"), Is.EqualTo("/* 1 */;"));
    }

    [Test]
    public void IfStmtTest() {
      Assert.That(parse(@"if 1=1 then
                          select * from S
                        elsif 2=2 then
                          select * from T;
                          update T set a=1;
                        else
                          select * from U
                        end if")
            , Is.EqualTo(@"IF 1=1 THEN SELECT * FROM S ELSIF 2=2 THEN SELECT * FROM T;"
                         +"UPDATE T SET a=1; ELSE SELECT * FROM U END IF "));
      Assert.That(parse(@"if/*1*/ 1=1/*2*/ then/*3*/
                          select * from S/*4*/
                        elsif/*5*/ 2=2/*6*/ then/*7*/
                          select * from T;/*8*/
                          update T set a=1;/*9*/
                        else/*10*/
                          select * from U/*11*/
                        end/*12*/ if/*13*/")
            , Is.EqualTo(@"IF/*1*/ 1=1/*2*/ THEN /*3*/SELECT * FROM S/*4*/ ELSIF/*5*/ 2=2"
                         + "/*6*/ THEN /*7*/SELECT * FROM T;/*8*/UPDATE T SET a=1;/*9*/ ELSE"
                         + " /*10*/SELECT * FROM U/*11*/ END/*12*/ IF/*13*/ "));
      Assert.That(parse(@"if 1 < 2 then
                          elsif 2 = 2 and 3 = 3 then
                          elsif 1 = 1 then
                          else
                          end if")
            , Is.EqualTo(@"IF 1<2 THEN  ELSIF 2=2 AND 3=3 THEN  ELSIF 1=1 THEN  ELSE  END IF "));
      Assert.That(parse(@"if 1 < 2 then
                           /* 1 */
                          elsif 2 = 2 and 3 = 3 then
                           /* 2 */
                          elsif 1 = 1 then
                           /* 3 */
                          else
                           /* 4 */
                          end if
                           /* 5 */")
            , Is.EqualTo(@"IF 1<2 THEN /* 1 */ ELSIF 2=2 AND 3=3 THEN /* 2 */ ELSIF 1=1 "
                   + "THEN /* 3 */ ELSE /* 4 */ END IF/* 5 */ "));
      Assert.That(parse(@"if 1=1 then
                           select 1+2;;
                          end if ;/*1*/")
            , Is.EqualTo(@"IF 1=1 THEN SELECT 1+2;; END IF ;/*1*/"));

      Assert.That(parse(@"if/* 1 */ 1=1 then/* 2 */ else/* 3 */ select 1 end/* 4 */ if/* 5 */")
      , Is.EqualTo(@"IF/* 1 */ 1=1 THEN /* 2 */ ELSE /* 3 */SELECT 1 END/* 4 */ IF/* 5 */ "));

      Assert.That(parse(@"")
            , Is.EqualTo(@""));
    }

    [Test]
    public void QualifiedNameTest() {
      Assert.That(parse(@"select/*1*/
                        A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*/./*9*/a/*10*/
                        ,/*11*/ A/*12*/./*13*/B/*14*/./*15*/C/*16*/./*17*/func/*18*/(/*19*/)/*20*/
                        ,/*21*/ A/*22*/./*23*/B/*24*/./*25*/C/*26*/./*27*/f/*28*/(/*29*/)/*30*/ over/*31*/ (/*32*/order/*33*/ by/*34*/ 1/*35*/)/*36*/
                        from/*37*/ A/*38*/./*39*/B/*40*/./*41*/C/*42*/./*43*/T/*44*/ as/*45*/ t1/*46*/ 
                        indexed/*47*/ by/*48*/ A/*49*/./*50*/B/*51*/./*52*/C/*53*/./*54*/i/*55*/")
            , Is.EqualTo(@"SELECT/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*/./*9*/a"
                                + "/*10*/,/*11*/A/*12*/./*13*/B/*14*/./*15*/C/*16*/./*17*/func"
                                + "/*18*/(/*19*/)/*20*/,/*21*/A/*22*/./*23*/B/*24*/./*25*/C/*26*/."
                                + "/*27*/f/*28*/(/*29*/)/*30*/OVER/*31*/(/*32*/ ORDER/*33*/ BY"
                                + "/*34*/ 1/*35*/)/*36*/ FROM/*37*/ A/*38*/./*39*/B/*40*/./*41*/C"
                                + "/*42*/./*43*/T/*44*/ AS/*45*/ t1/*46*/ INDEXED/*47*/ BY/*48*/ A"
                                + "/*49*/./*50*/B/*51*/./*52*/C/*53*/./*54*/i/*55*/"));

      Assert.That(parse(@"select/*1*/ all/*2*/
                          B/*3*/./*4*/C/*5*/./*6*/T/*7*/./*8*/a/*9*/
                          ,/*10*/ B/*11*/./*12*/C/*13*/./*14*/func/*15*/(/*16*/)/*17*/
                          ,/*18*/ B/*19*/./*20*/C/*21*/./*22*/f/*23*/(/*24*/)/*25*/ over/*26*/ (/*27*/order/*28*/ by/*29*/ 1/*30*/)/*31*/
                          from/*32*/ B/*33*/./*34*/C/*35*/./*36*/T/*37*/ t1/*38*/
                          indexed/*39*/ by/*40*/ B/*41*/./*42*/C/*43*/./*44*/i/*45*/")
            , Is.EqualTo(@"SELECT/*1*/ ALL/*2*/ B/*3*/./*4*/C/*5*/./*6*/T/*7*/./*8*/a/*9*/,"
                                + "/*10*/B/*11*/./*12*/C/*13*/./*14*/func/*15*/(/*16*/)/*17*/,"
                                + "/*18*/B/*19*/./*20*/C/*21*/./*22*/f/*23*/(/*24*/)/*25*/OVER"
                                + "/*26*/(/*27*/ ORDER/*28*/ BY/*29*/ 1/*30*/)/*31*/ FROM/*32*/ B"
                                + "/*33*/./*34*/C/*35*/./*36*/T/*37*/ t1/*38*/ INDEXED/*39*/ BY"
                                + "/*40*/ B/*41*/./*42*/C/*43*/./*44*/i/*45*/"));

      Assert.That(parse(@"select/*1*/
                        C/*2*/./*3*/T/*4*/./*5*/a/*6*/
                        ,/*7*/ C/*8*/./*9*/func/*10*/(/*11*/)/*12*/
                        ,/*13*/ C/*14*/./*15*/f/*16*/(/*17*/)/*18*/ over/*19*/ (/*20*/order/*21*/ by/*22*/ 1/*23*/)/*24*/
                        from/*25*/ C/*26*/./*27*/T/*28*/ t1/*29*/ 
                        indexed/*30*/ by/*31*/ C/*32*/./*33*/i/*34*/")
          , Is.EqualTo(@"SELECT/*1*/ C/*2*/./*3*/T/*4*/./*5*/a/*6*/,/*7*/C/*8*/./*9*/func"
                             + "/*10*/(/*11*/)/*12*/,/*13*/C/*14*/./*15*/f/*16*/(/*17*/)/*18*/"
                             + "OVER/*19*/(/*20*/ ORDER/*21*/ BY/*22*/ 1/*23*/)/*24*/ FROM/*25*/"
                             + " C/*26*/./*27*/T/*28*/ t1/*29*/ INDEXED/*30*/ BY/*31*/ C/*32*/."
                             + "/*33*/i/*34*/"));

      Assert.That(parse(@"INSERT/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*/ /** t1 */
                        values/*9*/ (/*10*/A/*11*/./*12*/B/*13*/./*14*/C/*15*/./*16*/T/*17*/./*18*/a/*19*/
                                  ,/*20*/ A/*21*/./*22*/B/*23*/./*24*/C/*25*/./*26*/T/*27*/./*28*/b/*29*/)/*30*/")
          , Is.EqualTo(@"INSERT/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*//** t1 */"
                             + " VALUES/*9*/(/*10*/A/*11*/./*12*/B/*13*/./*14*/C/*15*/./*16*/T"
                             + "/*17*/./*18*/a/*19*/,/*20*/A/*21*/./*22*/B/*23*/./*24*/C/*25*/."
                             + "/*26*/T/*27*/./*28*/b/*29*/)/*30*/"));
    }


    [Test]
    public void LimitClauseModify() {
      var select = (SelectStmt)MiniSqlParserAST.CreateStmt("SELECT * FROM T");
      var query = (SingleQuery)select.Query;

      var limitClause = new LimitClause(new UNumericLiteral("3")
                                      , new UNumericLiteral("5"), true);
      query.Limit = limitClause;
      Assert.That(query.HasLimit, Is.True);
      Assert.That(parse(select), Is.EqualTo(@"SELECT * FROM T LIMIT 3,5"));

      var oracleLimitClause = new OffsetFetchClause(3, 5, true);
      query.Limit = oracleLimitClause;
      Assert.That(query.HasLimit, Is.True);
      Assert.That(parse(select), Is.EqualTo(@"SELECT * FROM T OFFSET 3 ROWS FETCH NEXT 5 ROWS WITH TIES"));

      oracleLimitClause.FetchWithTies = false;
      Assert.That(parse(select), Is.EqualTo(@"SELECT * FROM T OFFSET 3 ROWS FETCH NEXT 5 ROWS ONLY"));

      oracleLimitClause.FetchRowCountType = FetchRowCountType.Percentile;
      Assert.That(parse(select), Is.EqualTo(@"SELECT * FROM T OFFSET 3 ROWS FETCH NEXT 5 PERCENT ROWS ONLY"));
      oracleLimitClause.HasOffset = false;
      Assert.That(parse(select), Is.EqualTo(@"SELECT * FROM T FETCH NEXT 5 PERCENT ROWS ONLY"));
      oracleLimitClause.HasOffset = true;
      oracleLimitClause.HasFetch = false;
      Assert.That(parse(select), Is.EqualTo(@"SELECT * FROM T OFFSET 3 ROWS"));

      query.Limit = null;
      Assert.That(query.HasLimit, Is.False);
    }


    [Test]
    public void TableModify() {
      var select = (SelectStmt)MiniSqlParserAST.CreateStmt("SELECT Tbl.* ");
      var query = (SingleQuery)select.Query;
      var table = new Table("Tbl");
      query.From = table;

      Assert.That(query.Comments.Count, Is.EqualTo(2));
      Assert.That(parse(select)
          , Is.EqualTo(@"SELECT Tbl.* FROM Tbl"));

      table.ServerName = "server";
      table.DataBaseName = "db";
      table.SchemaName = "schema";
      table.HasAs = true;
      table.AliasName = "alias";

      Assert.That(query.Comments.Count, Is.EqualTo(2));
      Assert.That(parse(select)
          , Is.EqualTo(@"SELECT Tbl.* FROM server.db.schema.Tbl AS alias"));

      table.ServerName = "";
      table.DataBaseName = "";
      table.SchemaName = "";
      table.HasAs = false;
      table.AliasName = "";

      Assert.That(query.Comments.Count, Is.EqualTo(2));
      Assert.That(parse(select)
          , Is.EqualTo(@"SELECT Tbl.* FROM Tbl"));

      table = new Table("s", "d", "sc", "t", true, "t1", "idxS", "idxD", "idxSc", "idx", false);
      query.From = table;

      Assert.That(query.Comments.Count, Is.EqualTo(2));
      Assert.That(parse(select)
          , Is.EqualTo(@"SELECT Tbl.* FROM s.d.sc.t AS t1 INDEXED BY "
                       + "idxS.idxD.idxSc.idx"));

      table = new Table("s", "d", "sc", "t", true, "t1", "", "", "", "", true);
      query.From = table;

      Assert.That(query.Comments.Count, Is.EqualTo(2));
      Assert.That(parse(select)
          , Is.EqualTo(@"SELECT Tbl.* FROM s.d.sc.t AS t1 NOT INDEXED"));
    }

    [Test]
    public void ColumnModify() {
      var select = (SelectStmt)MiniSqlParserAST.CreateStmt("SELECT T.* ");
      var query = (SingleQuery)select.Query;
      query.Results.RemoveAt(0);
      query.Results.Add(new ResultExpr(new Column("column1"), true, "c1"));

      Assert.That(query.Comments.Count, Is.EqualTo(1));
      Assert.That(parse(select)
          , Is.EqualTo(@"SELECT column1 AS c1"));

      var result = (ResultExpr)query.Results[0];
      result.AliasName = "aaa";
      var col = (Column)result.Value;
      col.ServerName = "sss";
      col.DataBaseName = "ddd";
      col.SchemaName = "sch";
      col.TableAliasName = "ttt";
      col.Name = "カラム1";

      Assert.That(query.Comments.Count, Is.EqualTo(1));
      Assert.That(parse(select)
          , Is.EqualTo(@"SELECT sss.ddd.sch.ttt.カラム1 AS aaa"));

      result.HasAs = false;
      result.AliasName = "";
      col.ServerName = "";
      col.DataBaseName = "";
      col.SchemaName = "";
      col.TableAliasName = "";
      col.Name = "カラム1";

      Assert.That(query.Comments.Count, Is.EqualTo(1));
      Assert.That(parse(select)
          , Is.EqualTo(@"SELECT カラム1"));

    }

    [Test]
    public void InsertSelectStmtModify() {
      var insert = (InsertSelectStmt)MiniSqlParserAST.CreateStmt("INSERT INTO T SELECT 1");

      // Set Nodes
      insert.With = new WithClause(true
                      , new WithDefinition(
                            new Table("V")
                          , new UnqualifiedColumnNames(
                                new UnqualifiedColumnName("a")
                              , new UnqualifiedColumnName("b")
                            )
                          , MiniSqlParserAST.CreateQuery("SELECT * FROM T")
                        )
                    );
      insert.Table.Name = "T1";
      insert.Columns = new UnqualifiedColumnNames(
                            new UnqualifiedColumnName("a")
                          , new UnqualifiedColumnName("b")
                       );

      Assert.That(insert.Comments.Count, Is.EqualTo(2));
      Assert.That(parse(insert)
          , Is.EqualTo(@"WITH RECURSIVE V(a,b) AS(SELECT * FROM T)"
                      + "INSERT INTO T1(a,b) SELECT 1"));

      // Unset Nodes
      insert.With = null;
      insert.Columns = null;
      Assert.That(insert.Comments.Count, Is.EqualTo(2));
      Assert.That(parse(insert)
          , Is.EqualTo(@"INSERT INTO T1 SELECT 1"));
    }

    [Test]
    public void InsertValuesStmtModify() {
      var insert = (InsertValuesStmt)MiniSqlParserAST.CreateStmt("INSERT INTO T VALUES(1,2)");

      // Set Nodes
      insert.With = new WithClause(true
                      , new WithDefinition(
                            new Table("V")
                          , new UnqualifiedColumnNames(
                                new UnqualifiedColumnName("a")
                              , new UnqualifiedColumnName("b")
                            )
                          , MiniSqlParserAST.CreateQuery("SELECT * FROM T")
                        )
                    );
      insert.Table.Name = "T1";
      insert.Columns = new UnqualifiedColumnNames(
                            new UnqualifiedColumnName("a")
                          , new UnqualifiedColumnName("b")
                       );
      insert.ValuesList.Add(
          new Values(new NullLiteral(), new UNumericLiteral("1"))
      );
      insert.ValuesList.Add(
          new Values(new StringLiteral("a"), new Default())
      );

      Assert.That(insert.Comments.Count, Is.EqualTo(3));
      Assert.That(parse(insert)
          , Is.EqualTo(@"WITH RECURSIVE V(a,b) AS(SELECT * FROM T)"
                      + "INSERT INTO T1(a,b) VALUES(1,2),(NULL,1),(a,DEFAULT)"));

      // Unset Nodes
      insert.With = null;
      insert.Columns = null;
      insert.ValuesList.Clear();
      insert.ValuesList.Insert(0, new Values(new StringLiteral("a")));
      Assert.That(insert.Comments.Count, Is.EqualTo(3));
      Assert.That(parse(insert)
          , Is.EqualTo(@"INSERT INTO T1 VALUES(a)"));

      // Insert more value
      insert.ValuesList[0].Insert(1, new StringLiteral("b"));
      Assert.That(insert.Comments.Count, Is.EqualTo(3));
      Assert.That(insert.ValuesList[0].Comments.Count, Is.EqualTo(3));
      Assert.That(parse(insert)
          , Is.EqualTo(@"INSERT INTO T1 VALUES(a,b)"));
    }

    [Test]
    public void UpdateStmtModify() {
      var update = (UpdateStmt)MiniSqlParserAST.CreateStmt("UPDATE T SET a=1");
      var pred = MiniSqlParserAST.CreatePredicate("1=1");

      // Set Nodes
      update.With = new WithClause(true
                      , new WithDefinition(
                            new Table("V")
                          , new UnqualifiedColumnNames(
                                new UnqualifiedColumnName("a")
                              , new UnqualifiedColumnName("b")
                            )
                          , MiniSqlParserAST.CreateQuery("SELECT * FROM T")
                        )
                    );
      update.Assignments.Add(
          new Assignment(new Column("b"), new Default())
      );
      update.Assignments.Add(
          new Assignment(new Column("c"), new NullLiteral())
      );
      update.Table.Name = "T1";
      update.Where = new BinaryOpPredicate(
                        new StringLiteral("a")
                      , PredicateOperator.Equal
                      , new StringLiteral("b")
                     );

      Assert.That(update.Comments.Count, Is.EqualTo(3));
      Assert.That(parse(update)
          , Is.EqualTo(@"WITH RECURSIVE V(a,b) AS(SELECT * FROM T)"
                      + "UPDATE T1 SET a=1,b=DEFAULT,c=NULL "
                      + "WHERE a=b"));

      //Insert Comment
      Assert.That(update.Assignments.Comments.Count, Is.EqualTo(2));
      update.Assignments.Comments[0] = "/*0*/";
      update.Assignments.Comments[1] = "/*1*/";
      Assert.That(parse(update)
          , Is.EqualTo(@"WITH RECURSIVE V(a,b) AS(SELECT * FROM T)"
                      + "UPDATE T1 SET a=1,/*0*/b=DEFAULT,/*1*/c=NULL "
                      + "WHERE a=b"));

      //Unset Nodes
      update.With = null;
      update.Assignments.RemoveAt(1);
      update.Where = null;

      Assert.That(update.Comments.Count, Is.EqualTo(2));
      Assert.That(update.Assignments.Comments.Count, Is.EqualTo(1));
      Assert.That(parse(update)
          , Is.EqualTo(@"UPDATE T1 SET a=1,/*0*/c=NULL"));

      // Update having double tables
      var update2 = (UpdateStmt)MiniSqlParserAST.CreateStmt("UPDATE T SET b=2 FROM U");
      update2.Where = MiniSqlParserAST.CreatePredicate("1=1");
      Assert.That(parse(update2)
          , Is.EqualTo(@"UPDATE T SET b=2 FROM U WHERE 1=1"));
      update2.Where = null;
      Assert.That(parse(update2)
          , Is.EqualTo(@"UPDATE T SET b=2 FROM U"));
    }

    [Test]
    public void SelectStmtModify() {
      var select = (SelectStmt)MiniSqlParserAST.CreateStmt("SELECT T.* ");
      var query = (SingleQuery)select.Query;
      var pred = MiniSqlParserAST.CreatePredicate("1=1");
      var expr = MiniSqlParserAST.CreateExpr("1");

      // Set Nodes
      query.Quantifier = QuantifierType.Distinct;
      query.HasTop = true;
      query.HasWildcard = false;
      query.Results.RemoveAt(0);
      query.Results.AddRange(new ResultExpr(expr, true, "aa")
                                      , new ResultExpr(expr, false, ""));
      query.From = new Table("", "", "", "Tbl");
      query.Where = pred;
      query.GroupBy.Add(expr);
      query.Having = pred;
      query.OrderBy.Add(new OrderingTerm(expr, "col", OrderSpec.Desc, NullOrder.First));

      Assert.That(query.Comments.Count, Is.EqualTo(7));
      Assert.That(parse(select)
          , Is.EqualTo(@"SELECT DISTINCT TOP 0 1 AS aa,1 FROM Tbl "
                       + "WHERE 1=1 GROUP BY 1 HAVING 1=1 "
                       + "ORDER BY 1 COLLATE col DESC NULLS FIRST"));

      // Set Comments
      query.Comments[0] = "/*select*/";
      query.Comments[1] = "/*distinct*/";
      query.Comments[2] = "/*top*/";
      query.Comments[3] = "/*0*/";
      query.Results.Comments[0] = "/*,*/";
      query.Comments[4] = "/*from*/";
      query.Comments[5] = "/*where*/";
      query.GroupBy.Comments[0] = "/*group*/";
      query.GroupBy.Comments[1] = "/*by*/";
      query.Comments[6] = "/*having*/";
      query.OrderBy.Comments[0] = "/*order*/";
      query.OrderBy.Comments[1] = "/*by*/";
      query.OrderBy[0].Comments[0] = "/*collate*/";
      query.OrderBy[0].Comments[1] = "/*col*/";
      query.OrderBy[0].Comments[2] = "/*desc*/";
      query.OrderBy[0].Comments[3] = "/*nulls*/";
      query.OrderBy[0].Comments[4] = "/*first*/";

      Assert.That(query.Comments.Count, Is.EqualTo(7));
      Assert.That(query.OrderBy.Comments.Count, Is.EqualTo(2));
      Assert.That(query.OrderBy[0].Comments.Count, Is.EqualTo(5));
      Assert.That(parse(select)
            , Is.EqualTo(@"SELECT/*select*/ DISTINCT/*distinct*/ TOP/*top*/ 0/*0*/ 1 AS aa,/*,*/1 FROM/*from*/ Tbl "
                         + "WHERE/*where*/ 1=1 GROUP/*group*/ BY/*by*/ 1 HAVING/*having*/ 1=1 "
                         + "ORDER/*order*/ BY/*by*/ 1 COLLATE/*collate*/ col/*col*/ DESC/*desc*/ NULLS/*nulls*/ FIRST/*first*/"));

      // Unset Comments
      query.Comments.UnSetAll();
      query.Results.Comments[0] = null;
      query.GroupBy.Comments.UnSetAll();
      query.OrderBy.Comments.UnSetAll();
      query.OrderBy[0].Comments.UnSetAll();

      Assert.That(query.Comments.Count, Is.EqualTo(7));
      Assert.That(parse(select)
          , Is.EqualTo(@"SELECT DISTINCT TOP 0 1 AS aa,1 FROM Tbl "
                       + "WHERE 1=1 GROUP BY 1 HAVING 1=1 "
                       + "ORDER BY 1 COLLATE col DESC NULLS FIRST"));

      // Unset Nodes
      query.Quantifier = QuantifierType.None;
      query.HasTop = false;
      query.HasWildcard = true;
      query.Results.Clear();
      query.From = null;
      query.Where = null;
      query.GroupBy.Clear();
      query.Having = null;
      query.OrderBy.Clear();

      Assert.That(query.Comments.Count, Is.EqualTo(2));
      Assert.That(parse(select), Is.EqualTo(@"SELECT *"));
    }

    [Test]
    public void OnConflict() {
      Assert.That(parse(@"UPDATE/*1*/ OR/*2*/ ROLLBACK/*3*/ T/*4*/ 
                          SET/*5*/ a/*6*/=/*7*/1/*8*/")
      , Is.EqualTo(@"UPDATE/*1*/ OR/*2*/ ROLLBACK/*3*/ T/*4*/ "
                   +"SET/*5*/ a/*6*/=/*7*/1/*8*/"));

      Assert.That(parse(@"INSERT/*1*/ OR/*2*/ ABORT/*3*/ INTO/*4*/ T/*5*/ 
                          VALUES/*6*/(/*7*/1/*8*/,/*9*/2/*10*/)/*11*/")
      , Is.EqualTo(@"INSERT/*1*/ OR/*2*/ ABORT/*3*/ INTO/*4*/ T/*5*/ "
                   +"VALUES/*6*/(/*7*/1/*8*/,/*9*/2/*10*/)/*11*/"));

      Assert.That(parse(@"INSERT/*1*/ OR/*2*/ FAIL/*3*/ INTO/*4*/ T/*5*/ 
                          SELECT/*6*/ */*7*/ FROM/*8*/ T/*9*/")
      , Is.EqualTo(@"INSERT/*1*/ OR/*2*/ FAIL/*3*/ INTO/*4*/ T/*5*/ "
                  + "SELECT/*6*/ */*7*/ FROM/*8*/ T/*9*/"));
    }

    [Test]
    public void UnionTest() {
      Assert.That(parse(@"select * from (
	                        select * from T1 left join T2 on T1.id = T2.id
	                        union
	                        select * from U1 right join U2 on U1.id = U2.id
	                        union
	                        select * from V1 full join V2 on V1.id = V2.id
                        ) Z")
      , Is.EqualTo(@"SELECT * FROM (SELECT * FROM T1 LEFT JOIN T2 ON T1.id=T2.id "
                  + "UNION SELECT * FROM U1 RIGHT JOIN U2 ON U1.id=U2.id UNION SELECT "
                  + "* FROM V1 FULL JOIN V2 ON V1.id=V2.id)Z"));

      Assert.That(parse(@"select/*1*/ 1/*2*/ from/*3*/ S/*4*/
                          union/*5*/
                          select/*6*/ 2/*7*/ from/*8*/ T/*9*/
                          union/*10*/ all/*11*/
                          select/*12*/ 3/*13*/ from/*14*/ U/*15*/")
      , Is.EqualTo(@"SELECT/*1*/ 1/*2*/ FROM/*3*/ S/*4*/ UNION/*5*/ SELECT/*6*/ 2"
                        + "/*7*/ FROM/*8*/ T/*9*/ UNION/*10*/ ALL/*11*/ SELECT/*12*/ 3"
                        + "/*13*/ FROM/*14*/ U/*15*/"));

      Assert.That(parse(@"select/*1*/ 1/*2*/ from/*3*/ S/*4*/
                          union/*5*/
                          select/*6*/ 2/*7*/ from/*8*/ T/*9*/
                          union/*10*/ all/*11*/
                          select/*12*/ 3/*13*/ from/*14*/ U/*15*/
                          order/*16*/ by/*17*/ id1/*18*/,/*19*/ id2/*20*/
                          limit/*21*/ 1/*22*/ offset/*23*/ 2/*24*/")
      , Is.EqualTo(@"SELECT/*1*/ 1/*2*/ FROM/*3*/ S/*4*/ UNION/*5*/ SELECT/*6*/ 2"
                        + "/*7*/ FROM/*8*/ T/*9*/ UNION/*10*/ ALL/*11*/ SELECT/*12*/ 3"
                        + "/*13*/ FROM/*14*/ U/*15*/ ORDER/*16*/ BY/*17*/ id1/*18*/,/*19*/"
                        + "id2/*20*/ LIMIT/*21*/ 1/*22*/ OFFSET/*23*/ 2/*24*/"));

      Assert.That(parse(@"select/*1*/ 1/*2*/ from/*3*/ S/*4*/
                          union/*5*/
                          select/*6*/ 2/*7*/ from/*8*/ T/*9*/
                          union/*10*/ all/*11*/
                          select/*12*/ 3/*13*/ from/*14*/ U/*15*/
                          order/*16*/ by/*17*/ id1/*18*/,/*19*/ id2/*20*/
                          limit/*21*/ 1/*22*/ ,/*23*/ 2/*24*/")
      , Is.EqualTo(@"SELECT/*1*/ 1/*2*/ FROM/*3*/ S/*4*/ UNION/*5*/ SELECT/*6*/ 2"
                        + "/*7*/ FROM/*8*/ T/*9*/ UNION/*10*/ ALL/*11*/ SELECT/*12*/ 3"
                        + "/*13*/ FROM/*14*/ U/*15*/ ORDER/*16*/ BY/*17*/ id1/*18*/,/*19*/"
                        + "id2/*20*/ LIMIT/*21*/ 1/*22*/,/*23*/2/*24*/")); 
    }

    [Test]
    public void BracketedQueryTest() {
      Assert.That(parse(@"(select 1 from T)
                          ;
                          (select 1 from T)
                          order by x
                          ;
                          (select 1 from T)
                          limit 1, 2
                          ;
                          (select 1 from T)
                          order by x
                          limit 1, 2
                          ;
                          (select 1 from T)
                          order by x
                          limit 2 offset 1
                          ;
                          (select 1)
                          union
                          (select 2)
                          union
                          (select 3)
                          ;
                          (
                          (select 1
                           union
                           select 2)
                          union
                          (select 3)
                          )
                          ;
                          select 1
                          union
                          (select 2
                           union
                           select 3)
                          ")
      , Is.EqualTo(@"(SELECT 1 FROM T);(SELECT 1 FROM T) ORDER BY x;(SELECT 1 FROM T) "
                        + "LIMIT 1,2;(SELECT 1 FROM T) ORDER BY x LIMIT 1,2;(SELECT 1 FROM "
                        + "T) ORDER BY x LIMIT 2 OFFSET 1;(SELECT 1) UNION (SELECT 2) UNION "
                        + "(SELECT 3);((SELECT 1 UNION SELECT 2) UNION (SELECT 3));SELECT 1 "
                        + "UNION (SELECT 2 UNION SELECT 3)"));


      Assert.That(parse(@"(/*1*/select/*2*/ 1/*3*/ from/*4*/ T/*5*/)/*6*/")
      , Is.EqualTo(@"(/*1*/SELECT/*2*/ 1/*3*/ FROM/*4*/ T/*5*/)/*6*/"));

      Assert.That(parse(@"(/*1*/select/*2*/ 1/*3*/ from/*4*/ T/*5*/)/*6*/
                          order/*7*/ by/*8*/ x/*9*/")
      , Is.EqualTo(@"(/*1*/SELECT/*2*/ 1/*3*/ FROM/*4*/ T/*5*/)/*6*/ "
                  + "ORDER/*7*/ BY/*8*/ x/*9*/"));

      Assert.That(parse(@"(/*1*/select/*2*/ 1/*3*/ from/*4*/ T/*5*/)/*6*/
                          limit/*7*/ 1/*8*/,/*9*/ 2/*10*/")
      , Is.EqualTo(@"(/*1*/SELECT/*2*/ 1/*3*/ FROM/*4*/ T/*5*/)/*6*/ "
                  + "LIMIT/*7*/ 1/*8*/,/*9*/2/*10*/"));


      Assert.That(parse(@"(/*1*/select/*2*/ 1/*3*/ from/*4*/ T/*5*/)/*6*/
                          order/*7*/ by/*8*/ x/*9*/
                          limit/*10*/ 1/*11*/,/*12*/ 2/*13*/")
      , Is.EqualTo(@"(/*1*/SELECT/*2*/ 1/*3*/ FROM/*4*/ T/*5*/)/*6*/ "
                  + "ORDER/*7*/ BY/*8*/ x/*9*/ LIMIT/*10*/ 1/*11*/,/*12*/2/*13*/"));

      Assert.That(parse(@"(/*1*/select/*2*/ 1/*3*/ from/*4*/ T/*5*/)/*6*/
                          order/*7*/ by/*8*/ x/*9*/
                          limit/*10*/ 2/*11*/ offset/*12*/ 1/*13*/")
      , Is.EqualTo(@"(/*1*/SELECT/*2*/ 1/*3*/ FROM/*4*/ T/*5*/)/*6*/ "
                  + "ORDER/*7*/ BY/*8*/ x/*9*/ LIMIT/*10*/ 2/*11*/ OFFSET/*12*/ 1/*13*/"));

      Assert.That(parse(@"(/*1*/select/*2*/ 1/*3*/)/*4*/
                          union/*5*/
                          (/*6*/select/*7*/ 2/*8*/)/*9*/
                          union/*10*/
                          (/*11*/select/*12*/ 3/*13*/)/*14*/")
      , Is.EqualTo(@"(/*1*/SELECT/*2*/ 1/*3*/)/*4*/ UNION/*5*/ (/*6*/SELECT/*7*/ 2"
                  + "/*8*/)/*9*/ UNION/*10*/ (/*11*/SELECT/*12*/ 3/*13*/)/*14*/"));

      Assert.That(parse(@"(/*1*/
                          (/*2*/select/*3*/ 1/*4*/
                           union/*5*/
                           select/*6*/ 2/*7*/)/*8*/
                          union/*9*/
                          (/*10*/select/*11*/ 3/*12*/)/*13*/
                          )/*14*/")
      , Is.EqualTo(@"(/*1*/(/*2*/SELECT/*3*/ 1/*4*/ UNION/*5*/ SELECT/*6*/ 2/*7*/)"
                  + "/*8*/ UNION/*9*/ (/*10*/SELECT/*11*/ 3/*12*/)/*13*/)/*14*/"));

      Assert.That(parse(@"select/*1*/ 1/*2*/
                          union/*3*/
                          (/*4*/select/*5*/ 2/*6*/
                           union/*7*/
                           select/*8*/ 3/*9*/)/*10*/")
      , Is.EqualTo(@"SELECT/*1*/ 1/*2*/ UNION/*3*/ (/*4*/SELECT/*5*/ 2/*6*/ UNION"
                  + "/*7*/ SELECT/*8*/ 3/*9*/)/*10*/"));

    }

    [Test]
    public void ExprTest() {
      Assert.That(parse(@"select
                          +1
                        , +1.4
                        , -1
                        , -0.6
                        , @ph
                        , sv.db.sc.tb.col
                        , (select * from T)
                        , ~1
                        , 'aa' || 'bb'
                        , 'aa' || 'bb' || 'cc'
                        , 1*2
                        , 1/2
                        , 1%2
                        , 1+2
                        , 1-2
                        , 1<<2
                        , 1>>2
                        , 1&2
                        , 1|2
                        , substring('abc' from 2 for 1)
                        , substring('abc' from 2)
                        , extract(day from '2015-11-11')
                        , count(distinct *)
                        , count(col)
                        , max(col)
                        , winfunc(distinct *) over (partition by col order by col)
                        , func(a,b,c)
                        , func()
                        , ('abc')
                        , cast('123' as number)
                        , cast('123' as varchar2(10))
                        , cast('123' as integer(10,3))
                        , '123' :: varchar2(10)")
      , Is.EqualTo(@"SELECT +1,+1.4,-1,-0.6,@ph,sv.db.sc.tb.col,(SELECT * FROM T),~1,'aa'||'bb','aa'"
                  + "||'bb'||'cc',1*2,1/2,1%2,1+2,1-2,1<<2,1>>2,1&2,1|2,substring('abc' FROM 2 FOR 1)"
                  + ",substring('abc' FROM 2),extract(DAY FROM '2015-11-11'),count(DISTINCT *),count("
                  + "col),max(col),winfunc(DISTINCT *)OVER( PARTITION BY col ORDER BY col),func(a,b,c"
                  + "),func(),('abc'),CAST('123' AS number),CAST('123' AS varchar2(10)),CAST('123' AS "
                  + "integer(10,3)),'123'::varchar2(10)"));

      Assert.That(parse(@"select/*0*/
                            +/*1*/1/*2*/
                          ,/*3*/ +/*4*/1.4/*5*/
                          ,/*6*/ -/*7*/1/*8*/
                          ,/*9*/ -/*10*/0.6/*11*/
                          ,/*12*/ @ph/*13*/
                          ,/*14*/ sv/*15*/./*16*/db/*17*/./*18*/sc/*19*/./*20*/tb/*21*/.col/*22*/
                          ,/*23*/ (/*24*/select/*25*/ */*26*/ from/*27*/ T/*28*/)/*29*/
                          ,/*30*/ ~/*31*/1/*32*/
                          ,/*33*/ 'aa'/*34*/ ||/*35*/ 'bb'/*36*/
                          ,/*37*/ 'aa'/*38*/ ||/*39*/ 'bb'/*40*/ ||/*41*/ 'cc'/*42*/
                          ,/*43*/ 1/*44*/*/*45*/2/*46*/
                          ,/*47*/ 1/*48*///*49*/2/*50*/
                          ,/*51*/ 1/*52*/%/*53*/2/*54*/
                          ,/*55*/ 1/*56*/+/*57*/2/*58*/
                          ,/*59*/ 1/*60*/-/*61*/2/*62*/
                          ,/*63*/ 1/*64*/<</*65*/2/*66*/
                          ,/*67*/ 1/*68*/>>/*69*/2/*70*/
                          ,/*71*/ 1/*72*/&/*73*/2/*74*/
                          ,/*75*/ 1/*76*/|/*77*/2/*78*/
                          ,/*79*/ substring/*80*/(/*81*/'abc'/*82*/ from/*83*/ 2/*84*/ for/*85*/ 1/*86*/)/*87*/
                          ,/*88*/ substring/*89*/(/*90*/'abc'/*91*/ from/*92*/ 2/*93*/)/*94*/
                          ,/*95*/ extract/*96*/(/*97*/day/*98*/ from/*99*/ '2015-11-11'/*100*/)/*101*/
                          ,/*102*/ count/*103*/(/*104*/distinct/*105*/ */*106*/)/*107*/
                          ,/*108*/ count/*109*/(/*110*/col/*111*/)/*112*/
                          ,/*113*/ max/*114*/(/*115*/col/*116*/)/*117*/
                          ,/*118*/ winfunc/*119*/(/*120*/distinct/*121*/ */*122*/)/*123*/ over/*124*/ (/*125*/partition/*126*/ by/*127*/ col/*128*/ order/*129*/ by/*130*/ col/*131*/)/*132*/
                          ,/*133*/ func/*134*/(/*135*/a,/*136*/b,/*137*/c)/*138*/
                          ,/*139*/ func/*140*/(/*141*/)/*142*/
                          ,/*143*/ (/*144*/'abc'/*145*/)/*146*/
                          ,/*147*/ cast/*148*/(/*149*/'123'/*150*/ as/*151*/ number/*152*/)/*153*/
                          ,/*154*/ cast/*155*/(/*156*/'123'/*157*/ as/*158*/ varchar2(10)/*159*/)/*160*/
                          ,/*161*/ cast/*162*/(/*163*/'123'/*164*/ as/*165*/ integer(10,3)/*166*/)/*167*/
                          ,/*168*/ '123'/*169*/ ::/*170*/ varchar2(10)/*171*/")
      , Is.EqualTo(@"SELECT/*0*/ +/*1*/1/*2*/,/*3*/+/*4*/1.4/*5*/,/*6*/-/*7*/1/*8*/,/*9*/-/*10*/0.6"
                  + "/*11*/,/*12*/@ph/*13*/,/*14*/sv/*15*/./*16*/db/*17*/./*18*/sc/*19*/./*20*/tb"
                  + "/*21*/.col/*22*/,/*23*/(/*24*/SELECT/*25*/ */*26*/ FROM/*27*/ T/*28*/)/*29*/,"
                  + "/*30*/~/*31*/1/*32*/,/*33*/'aa'/*34*/||/*35*/'bb'/*36*/,/*37*/'aa'/*38*/||/*39*/"
                  + "'bb'/*40*/||/*41*/'cc'/*42*/,/*43*/1/*44*/*/*45*/2/*46*/,/*47*/1/*48*///*49*/2"
                  + "/*50*/,/*51*/1/*52*/%/*53*/2/*54*/,/*55*/1/*56*/+/*57*/2/*58*/,/*59*/1/*60*/-"
                  + "/*61*/2/*62*/,/*63*/1/*64*/<</*65*/2/*66*/,/*67*/1/*68*/>>/*69*/2/*70*/,/*71*/1"
                  + "/*72*/&/*73*/2/*74*/,/*75*/1/*76*/|/*77*/2/*78*/,/*79*/substring/*80*/(/*81*/"
                  + "'abc'/*82*/ FROM/*83*/ 2/*84*/ FOR/*85*/ 1/*86*/)/*87*/,/*88*/substring/*89*/("
                  + "/*90*/'abc'/*91*/ FROM/*92*/ 2/*93*/)/*94*/,/*95*/extract/*96*/(/*97*/DAY/*98*/ "
                  + "FROM/*99*/ '2015-11-11'/*100*/)/*101*/,/*102*/count/*103*/(/*104*/DISTINCT"
                  + "/*105*/ */*106*/)/*107*/,/*108*/count/*109*/(/*110*/col/*111*/)/*112*/,/*113*/"
                  + "max/*114*/(/*115*/col/*116*/)/*117*/,/*118*/winfunc/*119*/(/*120*/DISTINCT"
                  + "/*121*/ */*122*/)/*123*/OVER/*124*/(/*125*/ PARTITION/*126*/ BY/*127*/ col"
                  + "/*128*/ ORDER/*129*/ BY/*130*/ col/*131*/)/*132*/,/*133*/func/*134*/(/*135*/a,"
                  + "/*136*/b,/*137*/c)/*138*/,/*139*/func/*140*/(/*141*/)/*142*/,/*143*/(/*144*/"
                  + "'abc'/*145*/)/*146*/,/*147*/CAST/*148*/(/*149*/'123'/*150*/ AS/*151*/ number"
                  + "/*152*/)/*153*/,/*154*/CAST/*155*/(/*156*/'123'/*157*/ AS/*158*/ varchar2(10)"
                  + "/*159*/)/*160*/,/*161*/CAST/*162*/(/*163*/'123'/*164*/ AS/*165*/ integer(10,3)"
                  + "/*166*/)/*167*/,/*168*/'123'/*169*/::/*170*/varchar2(10)/*171*/"));

      Assert.That(parse(@"select
                            '[{""foo"":""bar""}]'::json ->  1
                          , '[{""foo"":""bar""}]'::json ->  'foo'
                          ,  '{""foo"":""bar""}' ::json ->> 2
                          ,  '{""foo"":""bar""}' ::json ->> 'foo'
                          ,  '{""foo"":""bar""}' ::json #>  'foo'
                          ,  '{""foo"":""bar""}' ::json #>> 'foo'
                          ,  '{""foo"":""bar""}' ::json #-  'foo'
                         from T
                         where '{""foo"":""bar""}' <@ '{""foo"":1}'
                           and '{""foo"":""bar""}' @> '{""foo"":1}'
                           and '{""foo"":""bar""}' ?| 'a'
                           and '{""foo"":""bar""}' ?& 'b'")
      , Is.EqualTo(@"SELECT '[{""foo"":""bar""}]'::json->1,'[{""foo"":""bar""}]'::json->'foo'"
                 + @",'{""foo"":""bar""}'::json->>2,'{""foo"":""bar""}'::json->>'foo',"
                 + @"'{""foo"":""bar""}'::json #>'foo','{""foo"":""bar""}'::json #>>'foo',"
                 + @"'{""foo"":""bar""}'::json #-'foo' FROM T WHERE '{""foo"":""bar""}'<@"
                 + @"'{""foo"":1}' AND '{""foo"":""bar""}'@>'{""foo"":1}' AND '{""foo"":""bar""}'"
                 + @"?|'a' AND '{""foo"":""bar""}'?&'b'"));

      Assert.That(parse(@"select
                                '[{""foo"":""bar""}]'/*0*/::/*1*/json/*2*/ ->/*3*/  1/*4*/
                          ,/*5*/ '[{""foo"":""bar""}]'/*6*/::/*7*/json/*8*/ ->/*9*/  'foo'/*10*/
                          ,/*11*/  '{""foo"":""bar""}' /*12*/::/*13*/json/*14*/ ->>/*15*/ 2/*16*/
                          ,/*17*/  '{""foo"":""bar""}' /*18*/::/*19*/json/*20*/ ->>/*21*/ 'foo'/*22*/
                          ,/*23*/  '{""foo"":""bar""}' /*24*/::/*25*/json/*26*/ #>/*27*/  'foo'/*28*/
                          ,/*29*/  '{""foo"":""bar""}' /*30*/::/*31*/json/*32*/ #>>/*33*/ 'foo'/*34*/
                          ,/*35*/  '{""foo"":""bar""}' /*36*/::/*37*/json/*38*/ #-/*39*/  'foo'/*40*/
                         from/*41*/ T/*42*/
                         where/*43*/ '{""foo"":""bar""}'/*44*/ <@/*45*/ '{""foo"":1}'/*46*/
                           and/*47*/ '{""foo"":""bar""}'/*48*/ @>/*49*/ '{""foo"":1}'/*50*/
                           and/*51*/ '{""foo"":""bar""}'/*52*/ ?|/*53*/ 'a'/*54*/
                           and/*55*/ '{""foo"":""bar""}'/*56*/ ?&/*57*/ 'b'/*58*/")
      , Is.EqualTo(@"SELECT '[{""foo"":""bar""}]'/*0*/::/*1*/json/*2*/->/*3*/1/*4*/,/*5*/'[{""foo"":""bar""}]'/*6*/::/*7*/"
                 + @"json/*8*/->/*9*/'foo'/*10*/,/*11*/'{""foo"":""bar""}'/*12*/::/*13*/json/*14*/->>/*15*/2/*16*/,/*17*/'{""foo"":""bar""}'/*18*/::/*19*/"
                 + @"json/*20*/->>/*21*/'foo'/*22*/,/*23*/'{""foo"":""bar""}'/*24*/::/*25*/json/*26*/ #>/*27*/'foo'/*28*/,/*29*/"
                 + @"'{""foo"":""bar""}'/*30*/::/*31*/json/*32*/ #>>/*33*/'foo'/*34*/,/*35*/'{""foo"":""bar""}'/*36*/::/*37*/json/*38*/ #-/*39*/"
                 + @"'foo'/*40*/ FROM/*41*/ T/*42*/ WHERE/*43*/ '{""foo"":""bar""}'/*44*/<@/*45*/'{""foo"":1}'/*46*/ AND/*47*/ "
                 + @"'{""foo"":""bar""}'/*48*/@>/*49*/'{""foo"":1}'/*50*/ AND/*51*/ '{""foo"":""bar""}'/*52*/?|/*53*/'a'/*54*/ "
                 + @"AND/*55*/ '{""foo"":""bar""}'/*56*/?&/*57*/'b'/*58*/"));
    }

    [Test]
    public void PredicateTest() {
      Assert.That(parse(@"select * from T
                          where
                              @ph
                          and 1< 2
                          and 1<=2
                          and 1> 2
                          and 1>=2
                          and 'abc' like 'a'
                          and 'abc' not like 'b' escape 'a'
                          and col is null
                          and col is not null
                          and col is col
                          and col is not col
                          and col between 'a' and 'b'
                          and col not between 1 and 2
                          and col in (1,2,3)
                          and col not in (1,2,3)
                          and col in ()
                          and col in (select * from T)
                          and col = any (select * from T)
                          and col <> all (select * from T)
                          and exists (select * from T)
                          and not exists (select * from T)
                          and 1=1 collate JP
                          and (1=1)")
      , Is.EqualTo(@"SELECT * FROM T WHERE @ph AND 1<2 AND 1<=2 AND 1>2 AND 1>=2 AND 'abc' LIKE "
                  + "'a' AND 'abc' NOT LIKE 'b' ESCAPE 'a' AND col IS NULL AND col IS NOT NULL AND "
                  + "col IS col AND col IS NOT col AND col BETWEEN 'a' AND 'b' AND col NOT BETWEEN 1 "
                  + "AND 2 AND col IN(1,2,3) AND col NOT IN(1,2,3) AND col IN() AND col IN(SELECT * FROM T) "
                  + "AND col= ANY(SELECT * FROM T) AND col<> ALL(SELECT * FROM T) AND EXISTS(SELECT * FROM T) "
                  + "AND NOT EXISTS(SELECT * FROM T) AND 1=1 COLLATE JP AND (1=1)"));

      Assert.That(parse(@"select/*0*/ */*1*/ from/*168*/ T/*169*/
                          where/*170*/
                              @ph/*171*/
                          and/*172*/ 1/*173*/</*174*/ 2/*175*/
                          and/*176*/ 1/*177*/<=/*178*/2/*179*/
                          and/*180*/ 1/*181*/>/*182*/ 2/*183*/
                          and/*184*/ 1/*185*/>=/*186*/2/*187*/
                          and/*188*/ 'abc'/*189*/ like/*190*/ 'a'/*191*/
                          and/*192*/ 'abc'/*193*/ not/*194*/ like/*195*/ 'b'/*196*/ escape/*197*/ 'a'/*198*/
                          and/*199*/ col/*200*/ is/*201*/null/*202*/
                          and/*203*/ col/*204*/ is/*205*/not/*206*/ null/*207*/
                          and/*208*/ col/*209*/ is/*210*/col/*211*/
                          and/*212*/ col/*213*/ is/*214*/not/*215*/ col/*216*/
                          and/*217*/ col/*218*/ between/*219*/ 'a'/*220*/ and/*221*/ 'b'/*222*/
                          and/*223*/ col/*224*/ not/*225*/ between/*226*/ 1/*227*/ and/*228*/ 2/*229*/
                          and/*230*/ col/*231*/ in/*232*/ (/*233*/1,/*234*/2,/*235*/3)/*236*/
                          and/*237*/ col/*238*/ not/*239*/ in/*240*/ (/*241*/1,/*242*/2,/*243*/3)/*244*/
                          and/*245*/ col/*246*/ in/*247*/ (/*248*/)/*249*/
                          and/*250*/ col/*251*/ in/*252*/ (/*253*/select/*254*/ */*255*/ from T)/*256*/
                          and/*257*/ col/*258*/ =/*259*/ any/*260*/ (/*261*/select/*262*/ */*263*/ from T)/*264*/
                          and/*265*/ col/*266*/ <>/*267*/ all/*268*/ (/*269*/select/*270*/ */*271*/ from T)/*272*/
                          and/*273*/ exists/*274*/ (/*275*/select/*276*/ */*277*/ from T)/*278*/
                          and/*279*/ not/*280*/ exists/*281*/ (/*282*/select/*283*/ */*284*/ from T)/*285*/
                          and/*286*/ 1/*287*/=/*288*/1/*289*/ collate/*290*/ JP/*291*/
                          and/*292*/ (/*293*/1/*294*/=/*295*/1/*296*/)/*297*/")
      , Is.EqualTo(@"SELECT/*0*/ */*1*/ FROM/*168*/ T/*169*/ WHERE/*170*/ @ph/*171*/ AND/*172*/ 1/*173*/"
                  + "</*174*/2/*175*/ AND/*176*/ 1/*177*/<=/*178*/2/*179*/ AND/*180*/ 1/*181*/>"
                  + "/*182*/2/*183*/ AND/*184*/ 1/*185*/>=/*186*/2/*187*/ AND/*188*/ 'abc'/*189*/ "
                  + "LIKE/*190*/ 'a'/*191*/ AND/*192*/ 'abc'/*193*/ NOT/*194*/ LIKE/*195*/ 'b'/*196*/ "
                  + "ESCAPE/*197*/ 'a'/*198*/ AND/*199*/ col/*200*/ IS/*201*/ NULL/*202*/ AND/*203*/ "
                  + "col/*204*/ IS/*205*/ NOT/*206*/ NULL/*207*/ AND/*208*/ col/*209*/ IS/*210*/ col"
                  + "/*211*/ AND/*212*/ col/*213*/ IS/*214*/ NOT/*215*/ col/*216*/ AND/*217*/ col"
                  + "/*218*/ BETWEEN/*219*/ 'a'/*220*/ AND/*221*/ 'b'/*222*/ AND/*223*/ col/*224*/ "
                  + "NOT/*225*/ BETWEEN/*226*/ 1/*227*/ AND/*228*/ 2/*229*/ AND/*230*/ col/*231*/ IN"
                  + "/*232*/(/*233*/1,/*234*/2,/*235*/3)/*236*/ AND/*237*/ col/*238*/ NOT/*239*/ IN"
                  + "/*240*/(/*241*/1,/*242*/2,/*243*/3)/*244*/ AND/*245*/ col/*246*/ IN/*247*/("
                  + "/*248*/)/*249*/ AND/*250*/ col/*251*/ IN/*252*/(/*253*/SELECT/*254*/ */*255*/ FROM T)"
                  + "/*256*/ AND/*257*/ col/*258*/=/*259*/ ANY/*260*/(/*261*/SELECT/*262*/ */*263*/ FROM T)"
                  + "/*264*/ AND/*265*/ col/*266*/<>/*267*/ ALL/*268*/(/*269*/SELECT/*270*/ */*271*/ FROM T)"
                  + "/*272*/ AND/*273*/ EXISTS/*274*/(/*275*/SELECT/*276*/ */*277*/ FROM T)/*278*/ AND"
                  + "/*279*/ NOT/*280*/ EXISTS/*281*/(/*282*/SELECT/*283*/ */*284*/ FROM T)/*285*/ AND"
                  + "/*286*/ 1/*287*/=/*288*/1/*289*/ COLLATE/*290*/ JP/*291*/ AND/*292*/ (/*293*/1"
                  + "/*294*/=/*295*/1/*296*/)/*297*/"));
    }

    [Test]
    public void PlaceHolderModify() {
      var select = (SelectStmt)MiniSqlParserAST.CreateStmt("select 1 FROM T");
      var query = (SingleQuery)select.Query;

      query.Results.Clear();
      query.Results.Add(new ResultExpr(new PlaceHolderExpr("@EEE")));
      query.Where = new PlaceHolderPredicate("@PPP");
      Assert.That(parse(select)
          , Is.EqualTo(@"SELECT @EEE FROM T WHERE @PPP"));

      query.Results.Clear();
      query.Results.Add(new ResultExpr(new PlaceHolderExpr(":EEE")));
      query.Where = new PlaceHolderPredicate(":PPP");
      Assert.That(parse(select)
          , Is.EqualTo(@"SELECT :EEE FROM T WHERE :PPP"));

      query.Results.Clear();
      query.Results.Add(new ResultExpr(new PlaceHolderExpr("?")));
      query.Where = new PlaceHolderPredicate("?");
      Assert.That(parse(select)
          , Is.EqualTo(@"SELECT ? FROM T WHERE ?"));

      Assert.Throws<System.ArgumentNullException>(() => { new PlaceHolderExpr(null); });
      Assert.Throws<System.ArgumentNullException>(() => { new PlaceHolderExpr(""); });
      Assert.Throws<CannotBuildASTException>(() => { new PlaceHolderExpr("ABC"); });
      Assert.Throws<System.ArgumentNullException>(() => { new PlaceHolderPredicate(null); });
      Assert.Throws<System.ArgumentNullException>(() => { new PlaceHolderPredicate(""); });
      Assert.Throws<CannotBuildASTException>(() => { new PlaceHolderPredicate("ABC"); });
    }

    [Test]
    public void DeleteStmt() {
      Assert.That(parse(@"delete T")
      , Is.EqualTo(@"DELETE T"));

      Assert.That(parse(@"delete from T;;")
      , Is.EqualTo(@"DELETE FROM T;;"));

      Assert.That(parse(@"DELETE FROM T WHERE 1=1 OR 2=2")
      , Is.EqualTo(@"DELETE FROM T WHERE 1=1 OR 2=2"));

      Assert.That(parse(@"with V as (select * from T)
                          delete from T")
      , Is.EqualTo(@"WITH V AS(SELECT * FROM T)"
                  + "DELETE FROM T"));

      Assert.That(parse(@"delete A.B.C.D indexed by D.id1")
      , Is.EqualTo(@"DELETE A.B.C.D INDEXED BY D.id1"));

      Assert.That(parse(@"delete/*1*/ T/*2*/")
      , Is.EqualTo(@"DELETE/*1*/ T/*2*/"
                  + ""));

      Assert.That(parse(@"delete/*1*/ from/*2*/ T/*3*/;/*4*/;/*5*/")
      , Is.EqualTo(@"DELETE/*1*/ FROM/*2*/ T/*3*/;/*4*/;/*5*/"
                  + ""));

      Assert.That(parse(@"delete/*1*/ from/*2*/ T/*3*/ 
                          where/*4*/ 1/*5*/=/*6*/1/*7*/ or/*8*/ 2/*9*/=/**/2/*10*/")
      , Is.EqualTo(@"DELETE/*1*/ FROM/*2*/ T/*3*/ "
                   +"WHERE/*4*/ 1/*5*/=/*6*/1/*7*/ OR/*8*/ 2/*9*/=/**/2/*10*/"));

      Assert.That(parse(@"with/*1*/ V/*2*/ as/*3*/ (/*4*/select/*5*/ */*6*/ from/*7*/ T/*8*/)/*9*/
                          delete/*10*/ from/*11*/ T/*12*/")
      , Is.EqualTo(@"WITH/*1*/ V/*2*/ AS/*3*/(/*4*/SELECT/*5*/ */*6*/ FROM/*7*/ T/*8*/)/*9*/"
                  + "DELETE/*10*/ FROM/*11*/ T/*12*/"));

      Assert.That(parse(@"delete/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/D/*8*/
                          indexed/*9*/ by/*10*/ D/*11*/./*12*/id1/*13*/")
      , Is.EqualTo(@"DELETE/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/D/*8*/ "
                  + "INDEXED/*9*/ BY/*10*/ D/*11*/./*12*/id1/*13*/"));
    }

    [Test]
    public void CallStmt() {
      Assert.That(parse(@"call func();",false, DBMSType.MySql)
      , Is.EqualTo(@"CALL func();"));

      Assert.That(parse(@"call func(1,2,3)", false, DBMSType.MySql)
      , Is.EqualTo(@"CALL func(1,2,3)"));

      Assert.That(parse(@"call S.D.C.F(1)")
      , Is.EqualTo(@"CALL S.D.C.F(1)"));

//      Assert.That(parse(@"with V as (select * from T)
//                          call func(1,2);", false, DBMSType.MySql)
//      , Is.EqualTo(@"WITH V AS(SELECT * FROM T)"
//                  + "CALL func(1,2);"));

      Assert.That(parse(@"call/*1*/ func/*2*/(/*3*/)/*4*/;/*5*/", false, DBMSType.MySql)
      , Is.EqualTo(@"CALL/*1*/ func/*2*/(/*3*/)/*4*/;/*5*/"
                  + ""));

      Assert.That(parse(@"call/*1*/ func/*2*/(/*3*/1/*4*/,/*5*/2/*6*/,/*7*/3/*8*/)/*9*/", false, DBMSType.MySql)
      , Is.EqualTo(@"CALL/*1*/ func/*2*/(/*3*/1/*4*/,/*5*/2/*6*/,/*7*/3/*8*/)/*9*/"
                  + ""));

      Assert.That(parse(@"call/*1*/ S/*2*/./*3*/D/*4*/./*5*/C/*6*/./*7*/F/*8*/(/*9*/1/*10*/)/*11*/", false, DBMSType.MySql)
      , Is.EqualTo(@"CALL/*1*/ S/*2*/./*3*/D/*4*/./*5*/C/*6*/./*7*/F/*8*/(/*9*/1/*10*/)/*11*/"
                  + ""));

//      Assert.That(parse(@"with/*1*/ V/*2*/ as/*3*/ (/*4*/select/*5*/ */*6*/ from/*7*/ T/*8*/)/*9*/
//                          call/*10*/ func/*11*/(/*12*/1/*13*/,/*14*/2/*15*/)/*16*/;/*17*/", false, DBMSType.MySql)
//      , Is.EqualTo(@"WITH/*1*/ V/*2*/ AS/*3*/(/*4*/SELECT/*5*/ */*6*/ FROM/*7*/ T/*8*/)/*9*/"
//                  + "CALL/*10*/ func/*11*/(/*12*/1/*13*/,/*14*/2/*15*/)/*16*/;/*17*/"));
    }

    [Test]
    public void TruncateStmt() {
      Assert.That(parse(@"truncate table T")
      , Is.EqualTo(@"TRUNCATE TABLE T"));

      Assert.That(parse(@"truncate/*1*/ table/*2*/ T/*3*/")
      , Is.EqualTo(@"TRUNCATE/*1*/ TABLE/*2*/ T/*3*/"));

      Assert.That(parse(@"truncate/*1*/ table/*2*/ U/** u1 */")
      , Is.EqualTo(@"TRUNCATE/*1*/ TABLE/*2*/ U/** u1 */"));

      Assert.That(parse(@"truncate/*0*/ TABLE/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/D/*8*/")
      , Is.EqualTo(@"TRUNCATE/*0*/ TABLE/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/D/*8*/"));
    }

    [Test]
    public void PragmaStmt() {
      Assert.That(parse(@"pragma table_info(T)")
      , Is.EqualTo(@"PRAGMA TABLE_INFO(T)"));

      Assert.That(parse(@"pragma table_info(@tableName);;")
      , Is.EqualTo(@"PRAGMA TABLE_INFO(@tableName);;"));

      Assert.That(parse(@"pragma/*1*/ table_info/*2*/(/*3*/T/*4*/)/*5*/")
      , Is.EqualTo(@"PRAGMA/*1*/ TABLE_INFO/*2*/(/*3*/T/*4*/)/*5*/"));

      Assert.That(parse(@"pragma/*1*/ table_info/*2*/(/*3*/@tableName/*4*/)/*5*/;/*6*/;/*7*/")
      , Is.EqualTo(@"PRAGMA/*1*/ TABLE_INFO/*2*/(/*3*/@tableName/*4*/)/*5*/;/*6*/;/*7*/"));

      Assert.That(parse(@"")
      , Is.EqualTo(@""));
    }

    [Test]
    public void CommaJoinSource() {
      Assert.That(parse(@"select (1) from T,S")
      , Is.EqualTo(@"SELECT (1) FROM T,S"));

      Assert.That(parse(@"select T.* from T,S,U")
      , Is.EqualTo(@"SELECT T.* FROM T,S,U"));

      Assert.That(parse(@"select/*1*/ (/*2*/1/*3*/)/*4*/ from/*5*/ T/*6*/,/*7*/S/*8*/")
      , Is.EqualTo(@"SELECT/*1*/ (/*2*/1/*3*/)/*4*/ FROM/*5*/ T/*6*/,/*7*/S/*8*/"));

      Assert.That(parse(@"select/*1*/ T/*2*/./*3*/*/*4*/ from/*5*/ T/*6*/,/*7*/S/*8*/,/*9*/U/*10*/")
      , Is.EqualTo(@"SELECT/*1*/ T/*2*/./*3*/*/*4*/ FROM/*5*/ T/*6*/,/*7*/S/*8*/,/*9*/U/*10*/"));
    }

    [Test]
    public void LimitClause() {
      Assert.That(parse(@"select * from T limit 1 offset 10")
           , Is.EqualTo(@"SELECT * FROM T LIMIT 1 OFFSET 10"));

      Assert.That(parse(@"select a,b,c from T limit 10*10 offset (select 1000)")
           , Is.EqualTo(@"SELECT a,b,c FROM T LIMIT 10*10 OFFSET (SELECT 1000)"));

      Assert.That(parse(@"select * from T offset 10 rows fetch next 1 rows only")
           , Is.EqualTo(@"SELECT * FROM T OFFSET 10 ROWS FETCH NEXT 1 ROWS ONLY"));

      Assert.That(parse(@"select * from T offset 10 rows")
           , Is.EqualTo(@"SELECT * FROM T OFFSET 10 ROWS"));

      Assert.That(parse(@"select a from T fetch first 8 percent row with ties")
           , Is.EqualTo(@"SELECT a FROM T FETCH FIRST 8 PERCENT ROWS WITH TIES"));

      Assert.That(parse(@"select/*0*/ */*1*/ from/*2*/ T/*3*/ limit/*4*/ 1/*5*/ offset/*6*/ 10/*7*/")
           , Is.EqualTo(@"SELECT/*0*/ */*1*/ FROM/*2*/ T/*3*/ LIMIT/*4*/ 1/*5*/ OFFSET/*6*/ 10/*7*/"));

      Assert.That(parse(@"select/*0*/ a/*1*/,/*2*/b/*3*/,/*4*/c/*5*/ from/*6*/ T/*7*/ "
                       + "limit/*8*/ 10/*9*/*/*10*/10/*11*/ offset/*12*/ (/*13*/select/*14*/ 1000/*15*/)/*16*/")
           , Is.EqualTo(@"SELECT/*0*/ a/*1*/,/*2*/b/*3*/,/*4*/c/*5*/ FROM/*6*/ T/*7*/ "
                       + "LIMIT/*8*/ 10/*9*/*/*10*/10/*11*/ OFFSET/*12*/ (/*13*/SELECT/*14*/ 1000/*15*/)/*16*/"));

      Assert.That(parse(@"select/*0*/ */*1*/ from/*2*/ T/*3*/ offset/*4*/ 10/*5*/ rows/*6*/ "
                       + "fetch/*7*/ next/*8*/ 1/*9*/ rows/*10*/ only/*11*/")
           , Is.EqualTo(@"SELECT/*0*/ */*1*/ FROM/*2*/ T/*3*/ OFFSET/*4*/ 10/*5*/ ROWS/*6*/ "
                       + "FETCH/*7*/ NEXT/*8*/ 1/*9*/ ROWS/*10*/ ONLY/*11*/"));

      Assert.That(parse(@"select/*0*/ */*1*/ from/*2*/ T/*3*/ offset/*4*/ 10/*5*/ rows/*6*/")
           , Is.EqualTo(@"SELECT/*0*/ */*1*/ FROM/*2*/ T/*3*/ OFFSET/*4*/ 10/*5*/ ROWS/*6*/"));

      Assert.That(parse(@"select/*0*/ a/*1*/ from/*2*/ T/*3*/ "
                       + "fetch/*4*/ first/*5*/ 8/*6*/ percent/*7*/ row /*8*/ with/*9*/ ties/*10*/")
           , Is.EqualTo(@"SELECT/*0*/ a/*1*/ FROM/*2*/ T/*3*/ "
                       + "FETCH/*4*/ FIRST/*5*/ 8/*6*/ PERCENT/*7*/ ROWS/*8*/ WITH/*9*/ TIES/*10*/"));
    }

    [Test]
    public void ForUpdate() {
      Assert.That(parse(@"select * from T
                          order by T.id
                          limit 1,2
                          for update of T.id, T.attr wait 64")
          , Is.EqualTo(@"SELECT * FROM T "
                      + "ORDER BY T.id "
                      + "LIMIT 1,2 "
                      + "FOR UPDATE OF T.id,T.attr WAIT 64"));
      Assert.That(parse(@"select '' for update nowait")
          , Is.EqualTo(@"SELECT '' FOR UPDATE NOWAIT"));
      Assert.That(parse(@"select 3.14 for update")
          , Is.EqualTo(@"SELECT 3.14 FOR UPDATE"));
      Assert.That(parse(@"select -1 for update skip locked")
          , Is.EqualTo(@"SELECT -1 FOR UPDATE SKIP LOCKED"));
      Assert.That(parse(@"select '2016-07-31' for update wait")
          , Is.EqualTo(@"SELECT '2016-07-31' FOR UPDATE WAIT"));

      Assert.That(parse(@"select/*1*/ */*2*/ from/*3*/ T/*4*/
                          order/*5*/ by/*6*/ T/*7*/./*8*/id/*9*/
                          limit/*10*/ 1/*11*/,/*12*/2/*13*/
                          for/*14*/ update/*15*/ of/*16*/ T/*17*/./*18*/id/*19*/,/*20*/ T/*21*/./*22*/attr/*23*/ wait/*24*/ 64/*25*/")
          , Is.EqualTo(@"SELECT/*1*/ */*2*/ FROM/*3*/ T/*4*/ "
                      + "ORDER/*5*/ BY/*6*/ T/*7*/./*8*/id/*9*/ "
                      + "LIMIT/*10*/ 1/*11*/,/*12*/2/*13*/ "
                      + "FOR/*14*/ UPDATE/*15*/ OF/*16*/ T/*17*/./*18*/id/*19*/,/*20*/T/*21*/./*22*/attr/*23*/ WAIT/*24*/ 64/*25*/"));
      Assert.That(parse(@"select/*1*/ ''/*2*/ for/*3*/ update/*4*/ nowait/*5*/")
          , Is.EqualTo(@"SELECT/*1*/ ''/*2*/ FOR/*3*/ UPDATE/*4*/ NOWAIT/*5*/"));
      Assert.That(parse(@"select/*1*/ 3.14/*2*/ for/*3*/ update/*4*/")
          , Is.EqualTo(@"SELECT/*1*/ 3.14/*2*/ FOR/*3*/ UPDATE/*4*/"));
      Assert.That(parse(@"select/*1*/ -1/*2*/ for/*3*/ update/*4*/ skip/*5*/ locked/*6*/")
          , Is.EqualTo(@"SELECT/*1*/ -1/*2*/ FOR/*3*/ UPDATE/*4*/ SKIP/*5*/ LOCKED/*6*/"));
      Assert.That(parse(@"select/*1*/ '2016-07-31'/*2*/ for/*3*/ update/*4*/ wait/*5*/")
          , Is.EqualTo(@"SELECT/*1*/ '2016-07-31'/*2*/ FOR/*3*/ UPDATE/*4*/ WAIT/*5*/"));
    }

    [Test]
    public void OuterJoinKeyword() {
      Assert.That(parse(@"select * from T,S
                          where T.id   (+) = S.id
                            and T.attr (+) = 1
                            and attr (+) is not null")
          , Is.EqualTo(@"SELECT * FROM T,S "
                      + "WHERE T.id(+)=S.id "
                      + "AND T.attr(+)=1 "
                      + "AND attr(+) IS NOT NULL"));
      Assert.That(parse(@"select 1
                          where T.id = S.id (+)")
          , Is.EqualTo(@"SELECT 1 "
                      + "WHERE T.id=S.id(+)"));

      Assert.That(parse(@"select/*0*/ */*1*/ from/*2*/ T/*3*/,/*4*/S/*5*/
                          where/*6*/ T/*7*/./*8*/id/*9*/   (+)/*10*/ =/*11*/ S/*12*/./*13*/id/*14*/
                            and/*15*/ T/*16*/./*17*/attr/*18*/ (+)/*19*/ =/*20*/ 1/*21*/
                            and/*22*/ attr/*23*/ (+)/*24*/ is/*25*/ not/*26*/ null/*27*/")
          , Is.EqualTo(@"SELECT/*0*/ */*1*/ FROM/*2*/ T/*3*/,/*4*/S/*5*/ "
                      + "WHERE/*6*/ T/*7*/./*8*/id/*9*/(+)/*10*/=/*11*/S/*12*/./*13*/id/*14*/ "
                      + "AND/*15*/ T/*16*/./*17*/attr/*18*/(+)/*19*/=/*20*/1/*21*/ "
                      + "AND/*22*/ attr/*23*/(+)/*24*/ IS/*25*/ NOT/*26*/ NULL/*27*/"));
      Assert.That(parse(@"select/*0*/ 'abc'/*1*/
                          where/*2*/ T/*3*/./*4*/id/*5*/ =/*6*/ S/*7*/./*8*/id/*9*/ (+)/*10*/")
          , Is.EqualTo(@"SELECT/*0*/ 'abc'/*1*/ "
                      + "WHERE/*2*/ T/*3*/./*4*/id/*5*/=/*6*/S/*7*/./*8*/id/*9*/(+)/*10*/"));
    }

    [Test]
    public void From2() {
      Assert.That(parse(@"update T set a=1, b=2
                          from U
                          where T.id = U.id")
          , Is.EqualTo(@"UPDATE T SET a=1,b=2 "
                      + "FROM U "
                      + "WHERE T.id=U.id"));
      Assert.That(parse(@"delete from T from U
                          where T.id = U.id")
          , Is.EqualTo(@"DELETE FROM T FROM U "
                      + "WHERE T.id=U.id"));
      Assert.That(parse(@"delete T from U
                          where T.id = U.id(+)")
          , Is.EqualTo(@"DELETE T FROM U "
                      + "WHERE T.id=U.id(+)"));

      Assert.That(parse(@"update/*0*/ T/*1*/ set/*2*/ a/*3*/=/*4*/1/*5*/,/*6*/ b/*7*/=/*8*/2/*9*/
                          from/*10*/ U/*11*/
                          where/*12*/ T/*13*/./*14*/id/*15*/ =/*16*/ U/*17*/./*18*/id/*19*/")
          , Is.EqualTo(@"UPDATE/*0*/ T/*1*/ SET/*2*/ a/*3*/=/*4*/1/*5*/,/*6*/b/*7*/=/*8*/2/*9*/ "
                      + "FROM/*10*/ U/*11*/ "
                      + "WHERE/*12*/ T/*13*/./*14*/id/*15*/=/*16*/U/*17*/./*18*/id/*19*/"));
      Assert.That(parse(@"delete/*0*/ from/*1*/ T/*2*/ from/*3*/ U/*4*/
                          where/*5*/ T/*6*/./*7*/id/*8*/ =/*9*/ U/*10*/./*11*/id/*12*/")
          , Is.EqualTo(@"DELETE/*0*/ FROM/*1*/ T/*2*/ FROM/*3*/ U/*4*/ "
                      + "WHERE/*5*/ T/*6*/./*7*/id/*8*/=/*9*/U/*10*/./*11*/id/*12*/"));
      Assert.That(parse(@"delete/*0*/ T/*1*/ from/*2*/ U/*3*/
                          where/*4*/ T/*5*/./*6*/id/*7*/ =/*8*/ U/*9*/./*10*/id/*11*/(+)/*12*/")
          , Is.EqualTo(@"DELETE/*0*/ T/*1*/ FROM/*2*/ U/*3*/ "
                      + "WHERE/*4*/ T/*5*/./*6*/id/*7*/=/*8*/U/*9*/./*10*/id/*11*/(+)/*12*/"));
    }

    [Test]
    public void MergeStmt() {
      Assert.That(parse(@"MERGE INTO test_table AS A
                          USING (SELECT 10 AS no,'太郎さん' AS name, 30 AS age ) AS B
                          ON
                          (
                             A.no = B.no
                          )
                          WHEN MATCHED THEN
                              UPDATE SET
                                   name = B.name
                                  ,age = B.age
                          WHEN NOT MATCHED THEN
                              INSERT (no,name,age)
                              VALUES
                              (
                                   B.no
                                  ,B.name
                                  ,B.age
                              )
                      ;")
          , Is.EqualTo(@"MERGE INTO test_table AS A USING (SELECT 10 AS no,'太郎さん' AS name,30 AS age) AS B "
                      + "ON (A.no=B.no) WHEN MATCHED THEN UPDATE SET name=B.name,age=B.age WHEN NOT "
                      + "MATCHED THEN INSERT(no,name,age) VALUES(B.no,B.name,B.age);"));

      Assert.That(parse(@"merge into A.B.C.T A
                          using U as u1
                          on (A.id = u1.id and A.attr = u1.attr)")
          , Is.EqualTo(@"MERGE INTO A.B.C.T A "
                      + "USING U AS u1 "
                      + "ON (A.id=u1.id AND A.attr=u1.attr)"));

      Assert.That(parse(@"merge into A.B.C.T A
                          using U as u1
                          on (A.id = u1.id and A.attr = u1.attr)
                          when matched then update set A.attr = 1
                          when not matched then insert (A.id, A.attr) values(1,2)")
          , Is.EqualTo(@"MERGE INTO A.B.C.T A "
                      + "USING U AS u1 "
                      + "ON (A.id=u1.id AND A.attr=u1.attr) "
                      + "WHEN MATCHED THEN UPDATE SET A.attr=1 "
                      + "WHEN NOT MATCHED THEN INSERT(A.id,A.attr) VALUES(1,2)"));

      Assert.That(parse(@"merge into A.B.C.T A
                          using U as u1
                          on (A.id = u1.id and A.attr = u1.attr)
                          when not matched then insert (A.id, A.attr) values(1,2)
                          when matched then update set A.attr = 1")
          , Is.EqualTo(@"MERGE INTO A.B.C.T A "
                      + "USING U AS u1 "
                      + "ON (A.id=u1.id AND A.attr=u1.attr) "
                      + "WHEN NOT MATCHED THEN INSERT(A.id,A.attr) VALUES(1,2) "
                      + "WHEN MATCHED THEN UPDATE SET A.attr=1"));

      Assert.That(parse(@"merge into A.B.C.T A
                          using U as u1
                          on (A.id = u1.id and A.attr = u1.attr)
                          when matched then update set A.id = 0, A.attr = 1")
          , Is.EqualTo(@"MERGE INTO A.B.C.T A "
                      + "USING U AS u1 "
                      + "ON (A.id=u1.id AND A.attr=u1.attr) "
                      + "WHEN MATCHED THEN UPDATE SET A.id=0,A.attr=1"));

      Assert.That(parse(@"merge into A.B.C.T A
                          using U as u1
                          on (A.id = u1.id and A.attr = u1.attr)
                          when not matched then insert (A.id, A.attr) values(1,2)")
          , Is.EqualTo(@"MERGE INTO A.B.C.T A "
                      + "USING U AS u1 "
                      + "ON (A.id=u1.id AND A.attr=u1.attr) "
                      + "WHEN NOT MATCHED THEN INSERT(A.id,A.attr) VALUES(1,2)"));

      Assert.That(parse(@"merge into T
                          using (select * from U)
                          on (T.id = U.id)")
          , Is.EqualTo(@"MERGE INTO T "
                      + "USING (SELECT * FROM U) "
                      + "ON (T.id=U.id)"));

      Assert.That(parse(@"merge/*0*/ into/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*/ A/*9*/
                          using/*10*/ U/*11*/ as/*12*/ u1/*13*/
                          on/*14*/ (/*15*/A/*16*/./*17*/id/*18*/ =/*19*/ u1/*20*/./*21*/id/*22*/ and/*23*/ A/*24*/./*25*/attr/*26*/ =/*27*/ u1/*28*/./*29*/attr/*30*/)/*31*/")
          , Is.EqualTo(@"MERGE/*0*/ INTO/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*/ A/*9*/ "
                      + "USING/*10*/ U/*11*/ AS/*12*/ u1/*13*/ "
                      + "ON/*14*/ (/*15*/A/*16*/./*17*/id/*18*/=/*19*/u1/*20*/./*21*/id/*22*/ AND/*23*/ A/*24*/./*25*/attr/*26*/=/*27*/u1/*28*/./*29*/attr/*30*/)/*31*/"));

      Assert.That(parse(@"merge/*0*/ into/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*/ A/*9*/
                          using/*10*/ U/*11*/ as/*12*/ u1/*13*/
                          on/*14*/ (/*15*/A/*16*/./*17*/id/*18*/ =/*19*/ u1/*20*/./*21*/id/*22*/ and/*23*/ A/*24*/./*25*/attr/*26*/ =/*27*/ u1/*28*/./*29*/attr/*30*/)/*31*/
                          when/*32*/ matched/*33*/ then/*34*/ update/*35*/ set/*36*/ A/*37*/./*38*/attr/*39*/ =/*40*/ 1/*41*/ 
                          when/*42*/ not/*43*/ matched/*44*/ then/*45*/ insert/*46*/ (/*47*/A/*48*/./*49*/id/*50*/,/*51*/ A/*52*/./*53*/attr/*54*/)/*55*/ values/*56*/(/*57*/1/*58*/,/*59*/2/*60*/)/*61*/")
          , Is.EqualTo(@"MERGE/*0*/ INTO/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*/ A/*9*/ "
                      + "USING/*10*/ U/*11*/ AS/*12*/ u1/*13*/ "
                      + "ON/*14*/ (/*15*/A/*16*/./*17*/id/*18*/=/*19*/u1/*20*/./*21*/id/*22*/ AND/*23*/ A/*24*/./*25*/attr/*26*/=/*27*/u1/*28*/./*29*/attr/*30*/)/*31*/ "
                      + "WHEN/*32*/ MATCHED/*33*/ THEN/*34*/ UPDATE/*35*/ SET/*36*/ A/*37*/./*38*/attr/*39*/=/*40*/1/*41*/ "
                      + "WHEN/*42*/ NOT/*43*/ MATCHED/*44*/ THEN/*45*/ INSERT/*46*/(/*47*/A/*48*/./*49*/id/*50*/,/*51*/A/*52*/./*53*/attr/*54*/)/*55*/ VALUES/*56*/(/*57*/1/*58*/,/*59*/2/*60*/)/*61*/"));

      Assert.That(parse(@"merge/*0*/ into/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*/ A/*9*/
                          using/*10*/ U/*11*/ as/*12*/ u1/*13*/
                          on/*14*/ (/*15*/A/*16*/./*17*/id/*18*/ =/*19*/ u1/*20*/./*21*/id/*22*/ and/*23*/ A/*24*/./*25*/attr/*26*/ =/*27*/ u1/*28*/./*29*/attr/*30*/)/*31*/
                          when/*32*/ not/*33*/ matched/*34*/ then/*35*/ insert/*36*/ (/*37*/A/*38*/./*39*/id/*40*/,/*41*/ A/*42*/./*43*/attr/*44*/)/*45*/ values/*46*/(/*47*/1/*48*/,/*49*/2/*50*/)/*51*/
                          when/*52*/ matched/*53*/ then/*54*/ update/*55*/ set/*56*/ A/*57*/./*58*/attr/*59*/ =/*60*/ 1/*61*/")
          , Is.EqualTo(@"MERGE/*0*/ INTO/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*/ A/*9*/ "
                      + "USING/*10*/ U/*11*/ AS/*12*/ u1/*13*/ "
                      + "ON/*14*/ (/*15*/A/*16*/./*17*/id/*18*/=/*19*/u1/*20*/./*21*/id/*22*/ AND/*23*/ A/*24*/./*25*/attr/*26*/=/*27*/u1/*28*/./*29*/attr/*30*/)/*31*/ "
                      + "WHEN/*32*/ NOT/*33*/ MATCHED/*34*/ THEN/*35*/ INSERT/*36*/(/*37*/A/*38*/./*39*/id/*40*/,/*41*/A/*42*/./*43*/attr/*44*/)/*45*/ VALUES/*46*/(/*47*/1/*48*/,/*49*/2/*50*/)/*51*/ "
                      + "WHEN/*52*/ MATCHED/*53*/ THEN/*54*/ UPDATE/*55*/ SET/*56*/ A/*57*/./*58*/attr/*59*/=/*60*/1/*61*/"));

      Assert.That(parse(@"merge/*0*/ into/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*/ A/*9*/
                          using/*10*/ U/*11*/ as/*12*/ u1/*13*/
                          on/*14*/ (/*15*/A/*16*/./*17*/id/*18*/ =/*19*/ u1/*20*/./*21*/id/*22*/ and/*23*/ A/*24*/./*25*/attr/*26*/ =/*27*/ u1/*28*/./*29*/attr/*30*/)/*31*/
                          when/*32*/ matched/*33*/ then/*34*/ update/*35*/ set/*36*/ A/*37*/./*38*/id/*39*/ =/*40*/ 0/*41*/,/*42*/ A/*43*/./*44*/attr/*45*/ =/*46*/ 1/*47*/")
          , Is.EqualTo(@"MERGE/*0*/ INTO/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*/ A/*9*/ "
                      + "USING/*10*/ U/*11*/ AS/*12*/ u1/*13*/ "
                      + "ON/*14*/ (/*15*/A/*16*/./*17*/id/*18*/=/*19*/u1/*20*/./*21*/id/*22*/ AND/*23*/ A/*24*/./*25*/attr/*26*/=/*27*/u1/*28*/./*29*/attr/*30*/)/*31*/ "
                      + "WHEN/*32*/ MATCHED/*33*/ THEN/*34*/ UPDATE/*35*/ SET/*36*/ A/*37*/./*38*/id/*39*/=/*40*/0/*41*/,/*42*/A/*43*/./*44*/attr/*45*/=/*46*/1/*47*/"));

      Assert.That(parse(@"merge/*0*/ into/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*/ A/*9*/
                          using/*10*/ U/*11*/ as/*12*/ u1/*13*/
                          on/*14*/ (/*15*/A/*16*/./*17*/id/*18*/ =/*19*/ u1/*20*/./*21*/id/*22*/ and/*23*/ A/*24*/./*25*/attr/*26*/ =/*27*/ u1/*28*/./*29*/attr/*30*/)/*31*/
                          when/*32*/ not/*33*/ matched/*34*/ then/*35*/ insert/*36*/ (/*37*/A/*38*/./*39*/id/*40*/,/*41*/ A/*42*/./*43*/attr/*44*/)/*45*/ values/*46*/(/*47*/1/*48*/,/*49*/2/*50*/)/*51*/")
          , Is.EqualTo(@"MERGE/*0*/ INTO/*1*/ A/*2*/./*3*/B/*4*/./*5*/C/*6*/./*7*/T/*8*/ A/*9*/ "
                      + "USING/*10*/ U/*11*/ AS/*12*/ u1/*13*/ "
                      + "ON/*14*/ (/*15*/A/*16*/./*17*/id/*18*/=/*19*/u1/*20*/./*21*/id/*22*/ AND/*23*/ A/*24*/./*25*/attr/*26*/=/*27*/u1/*28*/./*29*/attr/*30*/)/*31*/ "
                      + "WHEN/*32*/ NOT/*33*/ MATCHED/*34*/ THEN/*35*/ INSERT/*36*/(/*37*/A/*38*/./*39*/id/*40*/,/*41*/A/*42*/./*43*/attr/*44*/)/*45*/ VALUES/*46*/(/*47*/1/*48*/,/*49*/2/*50*/)/*51*/"));

      Assert.That(parse(@"merge/*1*/ into/*2*/ T/*3*/
                          using/*4*/ (/*5*/select/*6*/ */*7*/ from/*8*/ U/*9*/)/*10*/
                          on/*11*/ (/*12*/T/*13*/./*14*/id/*15*/ =/*16*/ U/*17*/./*18*/id/*19*/)/*20*/")
          , Is.EqualTo(@"MERGE/*1*/ INTO/*2*/ T/*3*/ "
                      + "USING/*4*/ (/*5*/SELECT/*6*/ */*7*/ FROM/*8*/ U/*9*/)/*10*/ "
                      + "ON/*11*/ (/*12*/T/*13*/./*14*/id/*15*/=/*16*/U/*17*/./*18*/id/*19*/)/*20*/"));
    }

    [Test]
    public void IQueryClauseAddResult() {
      var newResult1 = new ResultExpr(new NullLiteral(), true, "newResult1");
      var newResult2 = new TableWildcard("S","D","Sch","T");

      var select = (SelectStmt)MiniSqlParserAST.CreateStmt("SELECT T.* ");
      var query = (IQueryClause)select.Query;
      var visitor = new AddResultVisitor(newResult1);
      query.Accept(visitor);
      Assert.That(parse(select)
                , Is.EqualTo(@"SELECT T.*,NULL AS newResult1"));

      select = (SelectStmt)MiniSqlParserAST.CreateStmt("SELECT * FROM T");
      query = (SingleQuery)select.Query;
      //visitor = new AddResultVisitor(newResult1);
      var visitor1 = new InsertResultVisitor(1, newResult1);
      query.Accept(visitor1);
      Assert.That(parse(select)
                , Is.EqualTo(@"SELECT T.*,NULL AS newResult1 FROM T"));

      select = (SelectStmt)MiniSqlParserAST.CreateStmt("SELECT * FROM T left join S on T.id = S.id");
      query = (SingleQuery)select.Query;
      visitor = new AddResultVisitor(newResult1);
      query.Accept(visitor);
      Assert.That(parse(select)
                , Is.EqualTo(@"SELECT T.*,S.*,NULL AS newResult1 "
                            + "FROM T LEFT JOIN S ON T.id=S.id"));

      select = (SelectStmt)MiniSqlParserAST.CreateStmt("SELECT * FROM T t1, S s1 where t1.id = s1.id");
      query = (SingleQuery)select.Query;
      visitor = new AddResultVisitor(newResult1);
      query.Accept(visitor);
      Assert.That(parse(select)
                , Is.EqualTo(@"SELECT t1.*,s1.*,NULL AS newResult1 "
                            + "FROM T t1,S s1 WHERE t1.id=s1.id"));

      select = (SelectStmt)MiniSqlParserAST.CreateStmt(
        "SELECT * FROM T t1, S s1 where t1.id = s1.id " +
        "UNION SELECT t1.* FROM T t1 " +
        "UNION SELECT * FROM S.D.SCH.S");
      query = (IQueryClause)select.Query;
      visitor = new AddResultVisitor(newResult1);
      query.Accept(visitor);
      Assert.That(parse(select)
                , Is.EqualTo(@"SELECT t1.*,s1.*,NULL AS newResult1 "
                            + "FROM T t1,S s1 WHERE t1.id=s1.id "
                            + "UNION SELECT t1.*,NULL AS newResult1 FROM T t1 "
                            + "UNION SELECT S.D.SCH.S.*,NULL AS newResult1 FROM S.D.SCH.S"));

      select = (SelectStmt)MiniSqlParserAST.CreateStmt(
        "SELECT * FROM (" +
        "  (SELECT * FROM T UNION SELECT * FROM (" +
        "      (SELECT * FROM R UNION ALL SELECT * FROM Q) v1" +
        "       CROSS JOIN " +
        "      (SELECT R.* FROM R UNION ALL SELECT Q.* FROM Q) v2" +
        "     ) " +
        "  ) " +
        ") v0");
      query = (IQueryClause)select.Query;
      visitor = new AddResultVisitor(newResult1);
      query.Accept(visitor);
      Assert.That(parse(select)
                , Is.EqualTo(@"SELECT v0.*,NULL AS newResult1 FROM (" +
                              "(SELECT * FROM T UNION SELECT * FROM (" +
                              "(SELECT * FROM R UNION ALL SELECT * FROM Q)v1 " +
                              "CROSS JOIN " +
                              "(SELECT R.* FROM R UNION ALL SELECT Q.* FROM Q)v2" +
                              ")))v0"));

      select = (SelectStmt)MiniSqlParserAST.CreateStmt("((SELECT * FROM T left join S on T.id = S.id))");
      query = (BracketedQuery)select.Query;
      visitor = new AddResultVisitor(newResult1);
      query.Accept(visitor);
      Assert.That(parse(select)
                , Is.EqualTo(@"((SELECT T.*,S.*,NULL AS newResult1 "
                            + "FROM T LEFT JOIN S ON T.id=S.id))"));

      select = (SelectStmt)MiniSqlParserAST.CreateStmt(
        "SELECT * FROM T t1, S s1 where t1.id = s1.id " +
        "UNION SELECT t1.* FROM T t1 " +
        "UNION SELECT * FROM S.D.SCH.S");
      query = (IQueryClause)select.Query;
      visitor = new AddResultVisitor(newResult2);
      query.Accept(visitor);
      Assert.That(parse(select)
                , Is.EqualTo(@"SELECT t1.*,s1.*,S.D.Sch.T.* "
                            + "FROM T t1,S s1 WHERE t1.id=s1.id "
                            + "UNION SELECT t1.*,S.D.Sch.T.* FROM T t1 "
                            + "UNION SELECT S.D.SCH.S.*,S.D.Sch.T.* FROM S.D.SCH.S"));

      select = (SelectStmt)MiniSqlParserAST.CreateStmt(
        "SELECT * FROM (" +
        "  (SELECT * FROM T UNION SELECT * FROM (" +
        "      (SELECT * FROM R UNION ALL SELECT * FROM Q) v1" +
        "       CROSS JOIN " +
        "      (SELECT R.* FROM R UNION ALL SELECT Q.* FROM Q) v2" +
        "     ) " +
        "  ) " +
        ") v0");
      query = (IQueryClause)select.Query;
      visitor = new AddResultVisitor(newResult2);
      query.Accept(visitor);
      Assert.That(parse(select)
                , Is.EqualTo(@"SELECT v0.*,S.D.Sch.T.* FROM (" +
                              "(SELECT * FROM T UNION SELECT * FROM (" +
                              "(SELECT * FROM R UNION ALL SELECT * FROM Q)v1 " +
                              "CROSS JOIN " +
                              "(SELECT R.* FROM R UNION ALL SELECT Q.* FROM Q)v2" +
                              ")))v0"));

      select = (SelectStmt)MiniSqlParserAST.CreateStmt("((SELECT * FROM T left join S on T.id = S.id))");
      query = (BracketedQuery)select.Query;
      visitor = new AddResultVisitor(newResult2);
      query.Accept(visitor);
      Assert.That(parse(select)
                , Is.EqualTo(@"((SELECT T.*,S.*,S.D.Sch.T.* "
                            + "FROM T LEFT JOIN S ON T.id=S.id))"));
    }

    [Test]
    public void HeaderComment() {
      var insert = (Stmt)MiniSqlParserAST.CreateStmt("/* c1 */ INSERT INTO T VALUES(1,2,3)");
      Assert.That(parse(insert), Is.EqualTo("/* c1 */INSERT INTO T VALUES(1,2,3)"));

      var merge = (Stmt)MiniSqlParserAST.CreateStmt("/* c1 */ MERGE INTO T USING U ON (1=1)");
      Assert.That(parse(merge), Is.EqualTo("/* c1 */MERGE INTO T USING U ON (1=1)"));

      var call = (Stmt)MiniSqlParserAST.CreateStmt("/* c1 */ CALL func(1,2,3)");
      Assert.That(parse(call), Is.EqualTo("/* c1 */CALL func(1,2,3)"));

      var truncate = (Stmt)MiniSqlParserAST.CreateStmt("/* c1 */ TRUNCATE TABLE T");
      Assert.That(parse(truncate), Is.EqualTo("/* c1 */TRUNCATE TABLE T"));

      var ifstmt = (Stmt)MiniSqlParserAST.CreateStmt("/* c1 */ IF x=1 THEN SELECT 1 END IF");
      Assert.That(parse(ifstmt), Is.EqualTo("/* c1 */IF x=1 THEN SELECT 1 END IF "));

      var pragma = (Stmt)MiniSqlParserAST.CreateStmt("/* c1 */ PRAGMA TABLE_INFO(T)");
      Assert.That(parse(pragma), Is.EqualTo("/* c1 */PRAGMA TABLE_INFO(T)"));
    }

    [Test]
    public void DefaultValuePlaceHolders() {
      var insert = (Stmt)MiniSqlParserAST.CreateStmt("/** @PH1 = \"3\" */ INSERT INTO T VALUES(1,2,3)");
      Assert.That(insert.PlaceHolderAssignComments, Is.EqualTo(new Dictionary<string, string>() { { "PH1", "3" } }));

      var merge = (Stmt)MiniSqlParserAST.CreateStmt("/** @PH2 = \"'ABC'\" */ MERGE INTO T USING U ON (1=1)");
      Assert.That(merge.PlaceHolderAssignComments, Is.EqualTo(new Dictionary<string, string>() { { "PH2", "'ABC'" } }));

      var call = (Stmt)MiniSqlParserAST.CreateStmt("/** @PH3 = \"'abc'\" */ CALL func(1,2,3)");
      Assert.That(call.PlaceHolderAssignComments, Is.EqualTo(new Dictionary<string, string>() { { "PH3", "'abc'" } }));

      var truncate = (Stmt)MiniSqlParserAST.CreateStmt("/** @PH4 = \"'1900-01-01'\" */ TRUNCATE TABLE T");
      Assert.That(truncate.PlaceHolderAssignComments, Is.EqualTo(new Dictionary<string, string>() { { "PH4", "'1900-01-01'" } }));

      var ifstmt = (Stmt)MiniSqlParserAST.CreateStmt("/** @PH5 = \"NULL\" */ IF x=1 THEN SELECT 1 END IF");
      Assert.That(ifstmt.PlaceHolderAssignComments, Is.EqualTo(new Dictionary<string, string>() { { "PH5", "NULL" } }));

      var pragma = (Stmt)MiniSqlParserAST.CreateStmt("/** @PH6 = \"''\" */ PRAGMA TABLE_INFO(T)");
      Assert.That(pragma.PlaceHolderAssignComments, Is.EqualTo(new Dictionary<string, string>() { { "PH6", "''" } }));

      var update = (Stmt)MiniSqlParserAST.CreateStmt("/** @PH7 = \"3\" */ /** @PH7 = \"4\" */ UPDATE T SET x=1");
      Assert.That(update.PlaceHolderAssignComments, Is.EqualTo(new Dictionary<string, string>() { { "PH7", "4" } }));
    }

    [Test]
    public void AutoWhereComment() {
      // AutoWhere値の初期値はTrueである
      var select = (SelectStmt)MiniSqlParserAST.CreateStmt("/* aa2 */ /* bb2 */ SELECT * FROM U /* cc2 */");
      Assert.That(parse(select), Is.EqualTo("/* aa2 *//* bb2 */SELECT * FROM U/* cc2 */"));
      Assert.That(select.AutoWhere, Is.True);

      select = (SelectStmt)MiniSqlParserAST.CreateStmt("/* aa2 */ /** autoWhere = \"true\" */  /* bb2 */ SELECT * FROM U /* cc2 */");
      Assert.That(parse(select), Is.EqualTo("/* aa2 *//** autoWhere = \"true\" *//* bb2 */SELECT * FROM U/* cc2 */"));
      Assert.That(select.AutoWhere, Is.True);

      select = (SelectStmt)MiniSqlParserAST.CreateStmt("/* aa2 */ /** autoWhere = \"false\" */  /* bb2 */ SELECT * FROM U /* cc2 */");
      Assert.That(parse(select), Is.EqualTo("/* aa2 *//** autoWhere = \"false\" *//* bb2 */SELECT * FROM U/* cc2 */"));
      Assert.That(select.AutoWhere, Is.False);

      var stmts = (Stmts)MiniSqlParserAST.CreateStmts("/* aa1 */ /* bb1 */ SELECT * FROM T;" +
                                                   "/* aa2 */ /* bb2 */ SELECT * FROM U;" +
                                                   "/* aa3 */ /* bb3 */ SELECT * FROM V");
      Assert.That(parse(stmts[0]), Is.EqualTo("/* aa1 *//* bb1 */SELECT * FROM T;"));
      Assert.That(parse(stmts[1]), Is.EqualTo("/* aa2 *//* bb2 */SELECT * FROM U;"));
      Assert.That(parse(stmts[2]), Is.EqualTo("/* aa3 *//* bb3 */SELECT * FROM V"));
      Assert.That(stmts[0].AutoWhere, Is.True);
      Assert.That(stmts[1].AutoWhere, Is.True);
      Assert.That(stmts[2].AutoWhere, Is.True);

      stmts = (Stmts)MiniSqlParserAST.CreateStmts("/* aa1 */ /** autoWhere = \"true\" *//* bb1 */ SELECT * FROM T;" +
                                               "/* aa2 */ /** autoWhere = \"true\" *//* bb2 */ SELECT * FROM U;" +
                                               "/* aa3 */ /** autoWhere = \"true\" *//* bb3 */ SELECT * FROM V");
      Assert.That(parse(stmts[0]), Is.EqualTo("/* aa1 *//** autoWhere = \"true\" *//* bb1 */SELECT * FROM T;"));
      Assert.That(parse(stmts[1]), Is.EqualTo("/* aa2 *//** autoWhere = \"true\" *//* bb2 */SELECT * FROM U;"));
      Assert.That(parse(stmts[2]), Is.EqualTo("/* aa3 *//** autoWhere = \"true\" *//* bb3 */SELECT * FROM V"));
      Assert.That(stmts[0].AutoWhere, Is.True);
      Assert.That(stmts[1].AutoWhere, Is.True);
      Assert.That(stmts[2].AutoWhere, Is.True);

      stmts = (Stmts)MiniSqlParserAST.CreateStmts("/* aa1 */ /** autoWhere = \"false\" */ /* bb1 */ SELECT * FROM T;" +
                                               "/* aa2 */ /** autoWhere = \"false\" */ /* bb2 */ SELECT * FROM U;" +
                                               "/* aa3 */ /** autoWhere = \"false\" */ /* bb3 */ SELECT * FROM V");
      Assert.That(parse(stmts[0]), Is.EqualTo("/* aa1 *//** autoWhere = \"false\" *//* bb1 */SELECT * FROM T;"));
      Assert.That(parse(stmts[1]), Is.EqualTo("/* aa2 *//** autoWhere = \"false\" *//* bb2 */SELECT * FROM U;"));
      Assert.That(parse(stmts[2]), Is.EqualTo("/* aa3 *//** autoWhere = \"false\" *//* bb3 */SELECT * FROM V"));
      Assert.That(stmts[0].AutoWhere, Is.False);
      Assert.That(stmts[1].AutoWhere, Is.False);
      Assert.That(stmts[2].AutoWhere, Is.False);

      var insert = (Stmt)MiniSqlParserAST.CreateStmt("/* aa1 */ /** autoWhere = \"false\" */ /* bb1 */ INSERT INTO T SELECT * FROM T");
      Assert.That(parse(insert), Is.EqualTo("/* aa1 *//** autoWhere = \"false\" *//* bb1 */INSERT INTO T SELECT * FROM T"));
      Assert.That(insert.AutoWhere, Is.False);

      var update = (Stmt)MiniSqlParserAST.CreateStmt("/* aa2 */ /** autoWhere = \"false\" */ /* bb2 */ UPDATE T SET x=1");
      Assert.That(parse(update), Is.EqualTo("/* aa2 *//** autoWhere = \"false\" *//* bb2 */UPDATE T SET x=1"));
      Assert.That(update.AutoWhere, Is.False);

      var delete = (Stmt)MiniSqlParserAST.CreateStmt("/* aa3 */ /** autoWhere = \"false\" */ /* bb3 */ DELETE FROM T");
      Assert.That(parse(delete), Is.EqualTo("/* aa3 *//** autoWhere = \"false\" *//* bb3 */DELETE FROM T"));
      Assert.That(delete.AutoWhere, Is.False);

      stmts = (Stmts)MiniSqlParserAST.CreateStmts("/* aa1 */ /** autoWhere = \"false\" */ /* bb1 */ INSERT INTO T SELECT * FROM T;" +
                                               "/* aa2 */ /** autoWhere = \"false\" */ /* bb2 */ UPDATE T SET x=1;" +
                                               "/* aa3 */ /** autoWhere = \"false\" */ /* bb3 */ DELETE FROM T");
      Assert.That(parse(stmts[0]), Is.EqualTo("/* aa1 *//** autoWhere = \"false\" *//* bb1 */INSERT INTO T SELECT * FROM T;"));
      Assert.That(parse(stmts[1]), Is.EqualTo("/* aa2 *//** autoWhere = \"false\" *//* bb2 */UPDATE T SET x=1;"));
      Assert.That(parse(stmts[2]), Is.EqualTo("/* aa3 *//** autoWhere = \"false\" *//* bb3 */DELETE FROM T"));
      Assert.That(stmts[0].AutoWhere, Is.False);
      Assert.That(stmts[1].AutoWhere, Is.False);
      Assert.That(stmts[2].AutoWhere, Is.False);
    }

    [Test]
    public void ForSqlAccessor() {
      // inline viewの別名指定は必須
      Assert.Throws<SqlSyntaxErrorsException>(() => parse(@"select * from (select x from T)", true));
      
      // inline viewの別名指定にASキーワードは記述禁止
      Assert.Throws<SqlSyntaxErrorsException>(() => parse(@"select * from (select x from T) as V", true));

      // カンマ(,)による結合は禁止
      Assert.Throws<SqlSyntaxErrorsException>(() => parse(@"select * from T, U where id = id", true));

      // 外部結合演算子(+)は記述禁止
      Assert.Throws<SqlSyntaxErrorsException>(() => parse(@"select * from T join U on id = id (+)", true));

      // NATURALキーワードは記述禁止
      Assert.Throws<SqlSyntaxErrorsException>(() => parse(@"select * from T natural join U", true));

      // USING句は記述禁止
      Assert.Throws<SqlSyntaxErrorsException>(() => parse(@"select * from T join U using (id)", true));

      // プレースホルダに'?'は使えない
      Assert.Throws<SqlSyntaxErrorsException>(() => parse(@"select * from T join U on id = ?", true));

      // プレースホルダに':'は使えない
      Assert.Throws<SqlSyntaxErrorsException>(() => parse(@"select * from T join U on id = :ph", true));

      // INSERT文のColumnリストは必須
      Assert.Throws<SqlSyntaxErrorsException>(() => parse(@"insert into T values (1, 2, 3)", true));

      // INSERT文のINTOキーワードは必須
      Assert.Throws<SqlSyntaxErrorsException>(() => parse(@"insert T(x,y,z) values (1, 2, 3)", true));

      // DELETE文のFROMキーワードは必須
      Assert.Throws<SqlSyntaxErrorsException>(() => parse(@"delete T where id in (1,2,3)", true));
    }

    private string parse(Stmt ast) {
      var stringifier = new CompactStringifier(4098, true);
      var checkParent = new CheckParentExistsVisitor();
      ast.Accept(stringifier);

      // Parentプロパティの有無チェック
      // RootノードのParenetはnullだが、チェックの時だけダミーのオブジェクトをParentに設定する
      ast.Parent = new UNumericLiteral("1");
      ast.Accept(checkParent);

      return stringifier.ToString();
    }

    private string parse(string inputText, bool forSqlAccessor = false, DBMSType dbmsType = DBMSType.Unknown) {
      var ast = MiniSqlParserAST.CreateStmts(inputText, dbmsType, forSqlAccessor);
      var stringifier = new CompactStringifier(4098, true);
      var checkParent = new CheckParentExistsVisitor();
      ast.Accept(stringifier);

      // Parentプロパティの有無チェック
      // RootノードのParenetはnullだが、チェックの時だけダミーのオブジェクトをParentに設定する
      ast.Parent = new UNumericLiteral("1");
      ast.Accept(checkParent);

      return stringifier.ToString();
    }
  }
}
