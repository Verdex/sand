
using System;
using System.Linq;
using System.Collections.Generic;

using sand.Util;
using static sand.Parsing.ParserEx;
using static sand.Util.ResultEx;
using static sand.Util.OptionEx;

namespace sand.Parsing {
    public class Grammar {

        Parser<IEnumerable<T>> List<T>(Parser<T> parser) {
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

        private Parser<Expr> VarParser() => IdentifierParser().Select(s => new Variable(s) as Expr).Trim();

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

        private Parser<Expr> ExprParser() {
            return null;
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
