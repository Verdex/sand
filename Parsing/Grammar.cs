
using System;
using System.Linq;
using System.Collections.Generic;

using sand.Util;
using static sand.Parsing.ParserEx;
using static sand.Util.ResultEx;
using static sand.Util.OptionEx;
using static sand.Util.UnitEx;

namespace sand.Parsing {

    public static class Ext {
        private static Parser<Unit> WS() 
            => from ws in Any() 
                where char.IsWhiteSpace(ws)
                select Unit();

        public static Parser<Unit> Sym(this string s) 
            => from sym in Expect(s).Trim()
               from next in Peek() 
               where char.IsLetterOrDigit(next) || next == '_'
               select Unit();

        public static Parser<Unit> Punct(this char c) 
            => (from v in Any()
               where v == c
               select Unit()).Trim();

        public static Parser<T> Trim<T>(this Parser<T> target) 
            => from ws1 in WS().ZeroOrMore()
               from t in target
               from ws2 in WS().ZeroOrMore()
               select t;

        public static Parser<IEnumerable<T>> List<T>(this Parser<T> parser, string sep = ",") {
            Parser<Unit> Sep() => Expect(sep).Select(x => Unit()).Trim();

            Parser<T> Rest() 
                => from c in Sep()
                   from tr in parser
                   select tr;

            return from t in parser.Maybe()
                   from ts in Rest().ZeroOrMore()
                   select (t switch {
                       Some<T> s => new T[] { s.Item },
                       None<T> => new T[0],
                       _ => throw new Exception("Unexpected Option case"),
                   }).Concat(ts);
        }
    }

    public class Grammar {

        private static Parser<Unit> LParen() => '('.Punct();
        private static Parser<Unit> RParen() => ')'.Punct();
        private static Parser<Unit> Colon() => ':'.Punct();
        private static Parser<Unit> OrBar() => '|'.Punct();
        private static Parser<Unit> Comma() => ','.Punct();
        private static Parser<Unit> LCurl() => '{'.Punct();
        private static Parser<Unit> RCurl() => '}'.Punct();
        private static Parser<Unit> LAngle() => '<'.Punct();
        private static Parser<Unit> RAngle() => '>'.Punct();
        private static Parser<Unit> Equal() => '='.Punct();
        private static Parser<Unit> SemiColon() => ';'.Punct();
        private static Parser<Unit> DoubleQuote() => '"'.Punct();
        private static Parser<Unit> DoubleArrow() => Expect("=>").Trim().Select(x => Unit());
        private static Parser<Unit> Arrow() => Expect("->").Trim().Select(x => Unit());

        public Result<IEnumerable<TopLevel>> Parse(string s) {
            var input = new Input(s);

            var p = TypeDefineParser().Or(LetStatementParser()).ZeroOrMore();

            switch (p.Parse(input)) {
                case Ok<IEnumerable<TopLevel>> o: 
                    switch (Any().Parse(input)) {
                        case Ok<char> o2: 
                            return new Err<IEnumerable<TopLevel>>(
                                new ParseError( $"Expected end of file but found {o2.Item}"
                                              , input.Index
                                              , input.Index
                                              , input.Text
                                              ));
                        case Err<char> e: return o;
                        default : throw new Exception("Unexpected Result case");
                    }

                case Err<IEnumerable<TopLevel>> e: return e;
                default : throw new Exception("Unexpected Result case");
            }
        }

        private Parser<TopLevel> TypeDefineParser() {

            Parser<DefineConstructor> Constructor() {
                Parser<DefineConstructor> Empty() 
                    => from id in IdentifierParser()
                       where char.IsUpper(id[0])
                       select new DefineConstructor(id, new SType[0]);
                Parser<DefineConstructor> Paramed()
                    => from id in IdentifierParser()
                       where char.IsUpper(id[0])
                       from lp in LParen()
                       from ps in TypeParser().List()
                       from rp in RParen()
                       select new DefineConstructor(id, ps);

                return Paramed().Or(Empty());
            }

            return from t in Expect("type").Trim()
                   from name in IdentifierParser()
                   where char.IsUpper(name[0])
                   from e in Equal()
                   from cons in Constructor().List("|")
                   from semi in SemiColon() 
                   select new TypeDefine(name, cons) as TopLevel;
        }

        private Parser<TopLevel> LetStatementParser() {
            Parser<SType> LetType() 
                => from c in Colon()
                   from t in TypeParser()
                   select t;

            return from l in Expect("let").Trim()
                   from variable in IdentifierParser()
                   from type in LetType().Maybe()
                   from e in Equal() 
                   from valueExpr in ExprParser()
                   from semi in SemiColon() 
                   select new LetStatement(variable, type, valueExpr) as TopLevel;
        }

        private Parser<Expr> IntegerParser()  
            => (from c in Any()
               where char.IsNumber(c)
               select c).OneoOrMore()
                        .Select(cs => new string(cs.ToArray()))
                        .Select(str => int.Parse(str))
                        .Select( i => new Integer(i) as Expr )
                        .Trim();

        private Parser<Expr> BoolParser() 
            => Expect("false").Or(Expect("true")).Select(str => new Bool(str == "true") as Expr).Trim();

        private Parser<Expr> StrParser() {
            static Parser<char> EscapeParser() 
                => (from slash in Expect("\\")
                   from other in Expect("t")
                                 .Or(Expect("n"))
                                 .Or(Expect("r"))
                                 .Or(Expect("\\"))
                                 .Or(Expect("\""))
                   select other).Select( c => c switch {
                       "t" => '\t',
                       "n" => '\n',
                       "r" => '\r',
                       "\\" => '\\',
                       "\"" => '"',
                       _ => throw new Exception("Impossible escape character encountered"),
                   });

            static Parser<char> NotQuote() 
                => from c in Any()
                   where c != '"'
                   select c;

            return (from q1 in DoubleQuote() 
                   from cs in EscapeParser().Or(NotQuote()).ZeroOrMore()
                   from q2 in DoubleQuote() 
                   select new Str(new string(cs.ToArray())) as Expr).Trim();
        }

        private Parser<string> IdentifierParser() {
            static Parser<char> Rest() 
                => from c in Any()
                   where Char.IsLetterOrDigit(c) || c == '_'
                   select c;

            return (from first in Any()
                   where first == '_' || Char.IsLetter(first)
                   from rest in Rest().ZeroOrMore()
                   select new string(rest.Prepend(first).ToArray())).Trim();
        }

        private Parser<Expr> VarParser() 
            => from id in IdentifierParser()
               where char.IsLower(id[0])
               select new Variable(id) as Expr;

        private Parser<Expr> LetExprParser() {
            Parser<SType> LetType() 
                => from c in Colon()
                   from t in TypeParser()
                   select t;

            return from l in Expect("let").Trim()
                   from variable in IdentifierParser()
                   from type in LetType().Maybe()
                   from e in Equal() 
                   from valueExpr in ExprParser()
                   from i in Expect("in").Trim()
                   from bodyExpr in ExprParser()
                   select new LetExpr(variable, type, valueExpr, bodyExpr) as Expr;
        }

        private Parser<Expr> LambdaExprParser() {
            Parser<SType> ArrowType() 
                => from a in Arrow()
                   from t in TypeParser()
                   select t;
            Parser<SType> ColonType() 
                => from c in Colon()
                   from t in TypeParser()
                   select t;
            Parser<(string, Option<SType>)> Parameter() 
                => from id in IdentifierParser()
                   from t in ColonType().Maybe()
                   select (id, t);

            return from o1 in OrBar()
                   from parameters in Parameter().ZeroOrMore()
                   from o2 in OrBar()
                   from returnType in ArrowType().Maybe()
                   from expr in ExprParser()
                   select new LambdaExpr(parameters, returnType, expr) as Expr;
        }

        private Parser<Expr> ConstructorExprParser() {
            Parser<Expr> ParamConstructor() 
                => from id in IdentifierParser()
                   where char.IsUpper(id[0])
                   from lp in LParen()
                   from es in ExprParser().List()
                   from rp in RParen()
                   select new ConstructorExpr(id, es) as Expr;
            Parser<Expr> EmptyConstructor() 
                => from id in IdentifierParser()
                   where char.IsUpper(id[0])
                   select new ConstructorExpr(id, new Expr[0]) as Expr;

            return ParamConstructor().Or(EmptyConstructor()); 
        }

        private Parser<Expr> TupleExprParser() 
            => from lp in LParen()
               from es in ExprParser().List()
               from rp in RParen()
               select new TupleExpr(es) as Expr;

        private Parser<Expr> MatchExprParser() {
            Parser<(Pattern, Expr)> Case() 
                => from pat in PatternParser()
                   from arrow in DoubleArrow() 
                   from expr in ExprParser()
                   select (pat, expr);

            return from m in Expect("match").Trim()
                   from expr in ExprParser()
                   from lc in LCurl()
                   from ps in Case().List()
                   from c in Comma() 
                   from rc in RCurl()
                   select new MatchExpr(expr, ps) as Expr;
        }

        private Parser<Expr> ExprParser() {
            Parser<Expr> Call() {
                Parser<Expr> Callables() 
                    // The TupleExpr includes just paren expr
                    // which I want to be able to call in case you put a 
                    // lambda or a match in there or a let that returns
                    // a lambda or whatever
                    => TupleExprParser().Or(VarParser());

                return from e in Callables()
                       from lp in LParen()
                       from parameters in ExprParser().List()
                       from rp in RParen()
                       select new CallExpr(e, parameters) as Expr;
            }

            return Call()
                    .Or(TupleExprParser())
                    .Or(MatchExprParser())
                    .Or(IntegerParser())
                    .Or(StrParser())
                    .Or(BoolParser())
                    .Or(LetExprParser())
                    .Or(ConstructorExprParser())
                    .Or(LambdaExprParser())
                    .Or(VarParser());
        }

        private Parser<Pattern> PatternParser() {
            Parser<Pattern> WildCardParser() => Expect("_").Trim().Select( x => new WildCard() as Pattern);
            Parser<Pattern> VariableParser() 
                => from id in IdentifierParser()
                   where char.IsLower(id[0])
                   select new VariablePattern(id) as Pattern;
            Parser<Pattern> ParamConstructor() 
                => from id in IdentifierParser()
                   where char.IsUpper(id[0])
                   from lp in LParen()
                   from ps in PatternParser().List()
                   from rp in RParen()
                   select new ConstructorPattern(id, ps) as Pattern;
            Parser<Pattern> EmptyConstructor() 
                => IdentifierParser().Select( id => new ConstructorPattern(id, new Pattern[0]) as Pattern);
                
            return WildCardParser().Or(VariableParser()).Or(ParamConstructor()).Or(EmptyConstructor());
        }

        private Parser<SType> TypeParser() {
            Parser<SType> SimpleType() 
                => from id in IdentifierParser()
                   where char.IsUpper(id[0])
                   select new SimpleType(id) as SType;
            
            Parser<SType> GenericType() 
                => from id in IdentifierParser()
                   where char.IsLower(id[0]) 
                   select new GenericType(id) as SType;

            Parser<SType> TupleType() {
                Parser<IEnumerable<SType>> Types() 
                    => from ts in TypeParser().List()
                       select ts;
                return from lp in LParen()
                       from ts in Types()
                       from rp in RParen()
                       select new TupleType(ts) as SType;
            }

            Parser<SType> Index() {

                return (from id in IdentifierParser()
                   where char.IsUpper(id[0])
                   from la in LAngle()
                   from ts in TypeParser().List()
                   from ra in RAngle()
                   select new IndexedType(id, ts) as SType).Trim();
            }
                       
            Parser<SType> ArrowCombinator(Parser<SType> parser) 
                => from s in parser 
                   from a in Arrow()
                   from t in TypeParser().Trim()
                   select new ArrowType(s, t) as SType;

            return ArrowCombinator(Index())
                    .Or(ArrowCombinator(TupleType()))
                    .Or(ArrowCombinator(SimpleType()))
                    .Or(ArrowCombinator(GenericType()))
                    .Or(TupleType())
                    .Or(Index())
                    .Or(SimpleType())
                    .Or(GenericType());
        }
    }
}
