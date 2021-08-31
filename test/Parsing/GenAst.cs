
using System;
using System.Linq;
using System.Collections.Generic;

using test.TestUtil;
using static test.TestUtil.NoiseGeneratorEx;

using sand.Parsing;
using sand.Util;
using static sand.Util.OptionEx;

namespace test.Parsing {

    public static class GenAst {

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
            => from f in Or(GenLowerLetter(), '_'.GenConst())
               from r in Or(GenLowerLetter(), GenUpperLetter(), GenNumber(), '_'.GenConst()).ZeroOrMore()
               select new string( r.Prepend(f).ToArray() );

        public static NoiseGenerator<Expr> GenInt()
            => new NoiseGenerator<Expr>( noise => new Integer((int)noise.Max(999999)) );

        public static NoiseGenerator<Expr> GenBool() => Or((new Bool(true) as Expr).GenConst(), (new Bool(false) as Expr).GenConst());

        public static NoiseGenerator<Expr> GenString() => (new Str("") as Expr).GenConst(); 

        public static NoiseGenerator<Expr> GenVariable() 
            => from id in GenVariableId()
               select new Variable(id) as Expr;

        public static NoiseGenerator<Expr> GenTuple() 
            => from es in GenExpr().ZeroOrMore()
               select new TupleExpr(es) as Expr;

        public static NoiseGenerator<Expr> GenLet() 
            => from varId in GenVariableId()
               from type in GenType().Maybe()
               from value in GenExpr()
               from body in GenExpr()
               select new LetExpr(varId, type, value, body) as Expr;

        public static NoiseGenerator<Expr> GenLambda() {
            static NoiseGenerator<(string, Option<SType>)> GenParameter() 
                => from id in GenVariableId()
                   from type in GenType().Maybe()
                   select (id, type);

            return from parameters in GenParameter().ZeroOrMore()
                   from retType in GenType().Maybe()
                   from body in GenExpr()
                   select new LambdaExpr(parameters, retType, body) as Expr;
        }

        public static NoiseGenerator<Expr> GenCall() 
            => from f in Or(GenTuple(), GenVariable())
               from parameters in GenExpr().ZeroOrMore()
               select new CallExpr(f, parameters) as Expr;

        public static NoiseGenerator<Expr> GenConstructor() 
            => from id in GenConstructorId()
               from parameters in GenExpr().ZeroOrMore()
               select new ConstructorExpr(id, parameters) as Expr;

        public static NoiseGenerator<Expr> GenMatch() {
            static NoiseGenerator<(Pattern, Expr)> GenCase() 
                => from p in GenPattern()
                   from e in GenExpr()
                   select (p, e);

            return from e in GenExpr()
                   from cases in GenCase().OneOrMore()
                   select new MatchExpr(e, cases) as Expr;
        }

        public static NoiseGenerator<Expr> GenExpr() 
            => Or( GenVariable()
                 , GenString()
                 , GenBool()
                 , GenInt()
                 , GenVariable()
                 , GenTuple()
                 , GenLet()
                 , GenLambda()
                 , GenCall()
                 , GenConstructor()
                 , GenMatch()
                 );

        public static NoiseGenerator<Pattern> GenVariablePattern() 
            => from id in GenVariableId()
               select new VariablePattern(id) as Pattern;

        public static NoiseGenerator<Pattern> GenConstructorPattern()
            => from id in GenConstructorId()
               from parameters in GenPattern().OneOrMore()
               select new ConstructorPattern(id, parameters) as Pattern;

        public static NoiseGenerator<Pattern> GenPattern() 
            => Or( (new WildCard() as Pattern).GenConst()
                 , GenVariablePattern()
                 , GenConstructorPattern()
                 );

        public static NoiseGenerator<SType> GenSimpleType()
            => from id in GenTypeId()
               select new SimpleType(id) as SType;

        public static NoiseGenerator<SType> GenGenericType() 
            => from id in GenGenericTypeId()
               select new GenericType(id) as SType;

        public static NoiseGenerator<SType> GenIndexedType()
            => from id in GenTypeId()
               from types in GenType().OneOrMore()
               select new IndexedType(id, types) as SType;

        public static NoiseGenerator<SType> GenArrowType()
            => from source in GenType()
               from destination in GenType()
               select new ArrowType(source, destination) as SType;

        public static NoiseGenerator<SType> GenTupleType() 
            => from parameters in GenType().ZeroOrMore()
               select new TupleType(parameters) as SType;

        public static NoiseGenerator<SType> GenType() 
            => Or( GenSimpleType() 
                 , GenGenericType()
                 , GenIndexedType()
                 , GenArrowType()
                 , GenTupleType()
                 );
    }
}