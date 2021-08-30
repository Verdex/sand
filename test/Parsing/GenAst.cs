
using System;
using System.Linq;
using System.Collections.Generic;

using test.TestUtil;
using static test.TestUtil.NoiseGeneratorEx;

using sand.Parsing;

namespace test.Parsing {

    public static class GenAst {

        private static NoiseGenerator<T> GenConst<T>(T item) => new NoiseGenerator<T>( noise => item );

        private static NoiseGenerator<char> GenUpperLetter() 
            => new NoiseGenerator<char>( noise => (char)noise.InRange(65, 90) );

        private static NoiseGenerator<char> GenLowerLetter() 
            => new NoiseGenerator<char>( noise => (char)noise.InRange(97, 122) );

        private static NoiseGenerator<char> GenNumber() 
            => new NoiseGenerator<char>( noise => (char)noise.InRange(48, 57) );

        private static NoiseGenerator<string> GenTypeId() 
            => from f in GenUpperLetter()
               from r in Or(GenLowerLetter(), GenUpperLetter(), GenNumber()).ZeroOrMore()
               select new string( r.Prepend(f).ToArray() );
        
        private static NoiseGenerator<string> GenGenericTypeId() 
            => from f in GenLowerLetter()
               from r in Or(GenLowerLetter(), GenUpperLetter(), GenNumber()).ZeroOrMore()
               select new string( r.Prepend(f).ToArray() );

        private static NoiseGenerator<string> GenConstructorId() => GenTypeId();

        private static NoiseGenerator<string> GenVariableId() 
            => from f in Or(GenLowerLetter(), GenConst('_'))
               from r in Or(GenLowerLetter(), GenUpperLetter(), GenNumber(), GenConst('_')).ZeroOrMore()
               select new string( r.Prepend(f).ToArray() );

        public static NoiseGenerator<Expr> GenInt()
            => new NoiseGenerator<Expr>( noise => new Integer((int)noise.Max(999999)) );

        public static NoiseGenerator<Expr> GenBool() => Or(GenConst(new Bool(true) as Expr), GenConst(new Bool(false) as Expr));

        public static NoiseGenerator<Expr> GenString() => GenConst(new Str("") as Expr); 

        public static NoiseGenerator<Expr> GenVariable() 
            => from id in GenVariableId()
               select new Variable(id) as Expr;

        public static NoiseGenerator<Expr> GenTuple() 
            => from es in GenExpr().ZeroOrMore()
               select new TupleExpr(es) as Expr;
        

        public static NoiseGenerator<Expr> GenExpr() 
            => Or( GenVariable()
                 , GenString()
                 , GenBool()
                 , GenInt()
                 );

    }
}