
using System;
using System.Linq;

using sand.Util;
using static sand.Parsing.ParserEx;
using static sand.Util.ResultEx;
using static sand.Util.OptionEx;

namespace sand.Parsing {
    public class Grammar {

        private Parser<Integer> IntegerParser()  
            => (from c in Any()
               where char.IsNumber(c)
               select c).OneoOrMore()
                        .Select(cs => new string(cs.ToArray()))
                        .Select(str => int.Parse(str))
                        .Select( i => new Integer(i) );

        private Parser<Bool> BoolParser() 
            => Expect("false").Or(Expect("true")).Select(str => new Bool(str == "true"));

        private Parser<Str> StrParser() {
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

            return from q1 in Expect("\"")
                   from cs in EscapeParser().Or(NotQuote()).ZeroOrMore()
                   from q2 in Expect("\"")
                   select new Str(new string(cs.ToArray()));
        }

        private Parser<string> IdentifierParser() {
            static Parser<char> Rest() 
                => from c in Any()
                   where Char.IsLetterOrDigit(c) || c == '_'
                   select c;

            return from first in Any()
               where first == '_' || Char.IsLetter(first)
               from rest in Rest().ZeroOrMore()
               select new string(rest.Prepend(first).ToArray());
        }

        private Parser<Variable> VarParser() => IdentifierParser().Select(s => new Variable(s));

        private Parser<Expr> ParseExpr() {
            return null;
        }
    }
}
