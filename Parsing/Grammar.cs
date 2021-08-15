
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

        private Parser<Expr> ParseExpr() {
            return null;
        }
    }
}
