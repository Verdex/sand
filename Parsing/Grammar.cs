
using System;
using System.Linq;
using System.Collections.Generic;

using sand.Util;
using static sand.Parsing.ParserEx;
using static sand.Util.ResultEx;
using static sand.Util.OptionEx;

namespace sand.Parsing {
    public class Grammar {

        private Parser<IEnumerable<T>> List<T>(Parser<T> parser) {
            Parser<char> Comma() => Expect(",").Select(x => '\0').Trim();

            Parser<T> TComma() 
                => from t in parser.Trim()
                   from comma in Comma().Trim()
                   select t;
            
            return from ts in TComma().ZeroOrMore()
                   from t in parser.Trim()
                   select ts.Append(t);
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

            return (from q1 in Expect("\"")
                   from cs in EscapeParser().Or(NotQuote()).ZeroOrMore()
                   from q2 in Expect("\"")
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
            Parser<string> Colon() => Expect(":").Trim();
            Parser<Option<SType>> LetType() 
                => (from c in Colon()
                   from t in TypeParser()
                   select t).Maybe();

            return from l in Expect("let").Trim()
                   from variable in IdentifierParser()
                   from type in LetType()
                   from e in Expect("=").Trim()
                   from valueExpr in ExprParser()
                   from i in Expect("in").Trim()
                   from bodyExpr in ExprParser()
                   select new LetExpr(variable, type, valueExpr, bodyExpr) as Expr;
        }

        private Parser<Expr> LambdaExprParser() {
            Parser<string> OrBar() => Expect("|").Trim();
            Parser<string> Colon() => Expect(":").Trim();
            Parser<string> Arrow() => Expect("->").Trim();
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
            Parser<string> LParen() => Expect("(").Trim();
            Parser<string> RParen() => Expect(")").Trim();
            Parser<Expr> ParamConstructor() 
                => from id in IdentifierParser()
                   where char.IsUpper(id[0])
                   from lp in LParen()
                   from es in List(ExprParser())
                   from rp in RParen()
                   select new ConstructorExpr(id, es) as Expr;
            Parser<Expr> EmptyConstructor() 
                => IdentifierParser().Select( id => new ConstructorExpr(id, new Expr[0]) as Expr );

            return ParamConstructor().Or(EmptyConstructor()); 
        }

        private Parser<Expr> TupleExprParser() {
            Parser<string> LParen() => Expect("(").Trim();
            Parser<string> RParen() => Expect(")").Trim();

            return from lp in LParen()
                   from es in List(ExprParser())
                   from rp in RParen()
                   select new TupleExpr(es) as Expr;
        }

        private Parser<Expr> MatchExprParser() {
            Parser<string> LCurl() => Expect("{").Trim();
            Parser<string> RCurl() => Expect("}").Trim();
            Parser<(Pattern, Expr)> Case() 
                => from pat in PatternParser()
                   from arrow in Expect("=>").Trim()
                   from expr in ExprParser()
                   select (pat, expr);

            return from m in Expect("match").Trim()
                   from lc in LCurl()
                   from ps in List(Case())
                   from rc in RCurl()
                   select new MatchExpr(ps) as Expr;
        }

        private Parser<Expr> ExprParser() {
            Parser<string> LParen() => Expect("(").Trim();
            Parser<string> RParen() => Expect(")").Trim();

            Parser<Expr> Call() {
                Parser<Expr> Callables() 
                    // The TupleExpr includes just paren expr
                    // which I want to be able to call in case you put a 
                    // lambda or a match in there or a let that returns
                    // a lambda or whatever
                    => TupleExprParser().Or(VarParser());

                return from e in Callables()
                       from lp in LParen()
                       from parameters in ExprParser().ZeroOrMore()
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
            Parser<string> LParen() => Expect("(").Trim();
            Parser<string> RParen() => Expect(")").Trim();
            Parser<Pattern> ParamConstructor() 
                => from id in IdentifierParser()
                   where char.IsUpper(id[0])
                   from lp in LParen()
                   from ps in List(PatternParser())
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
                Parser<char> LParen() => Expect("(").Select(x => '\0').Trim();
                Parser<char> RParen() => Expect(")").Select(x => '\0').Trim();
                Parser<IEnumerable<SType>> Types() 
                    => from ts in List(TypeParser())
                       select ts;
                return from lp in LParen()
                       from ts in Types()
                       from rp in RParen()
                       select new TupleType(ts) as SType;
            }

            Parser<char> Arrow() => Expect("->").Select(x => '\0').Trim();

            Parser<SType> Index() {
                Parser<char> LAngle() => Expect("<").Select(x => '\0').Trim();
                Parser<char> RAngle() => Expect(">").Select(x => '\0').Trim();

                return (from id in IdentifierParser()
                   where char.IsUpper(id[0])
                   from la in LAngle()
                   from ts in List(TypeParser())
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
