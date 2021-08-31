
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

        public static NoiseGenerator<Expr> GenTuple(int depth) 
            => from es in GenExpr(depth).ZeroOrMore()
               select new TupleExpr(es) as Expr;

        public static NoiseGenerator<Expr> GenLet(int depth) 
            => from varId in GenVariableId()
               from type in GenType(depth).Maybe()
               from value in GenExpr(depth)
               from body in GenExpr(depth)
               select new LetExpr(varId, type, value, body) as Expr;

        public static NoiseGenerator<Expr> GenLambda(int depth) {
            static NoiseGenerator<(string, Option<SType>)> GenParameter(int depth) 
                => from id in GenVariableId()
                   from type in GenType(depth).Maybe()
                   select (id, type);

            return from parameters in GenParameter(depth).ZeroOrMore()
                   from retType in GenType(depth).Maybe()
                   from body in GenExpr(depth)
                   select new LambdaExpr(parameters, retType, body) as Expr;
        }

        public static NoiseGenerator<Expr> GenCall(int depth) 
            => from f in Or(GenTuple(depth), GenVariable())
               from parameters in GenExpr(depth).ZeroOrMore()
               select new CallExpr(f, parameters) as Expr;

        public static NoiseGenerator<Expr> GenConstructor(int depth) 
            => from id in GenConstructorId()
               from parameters in GenExpr(depth).ZeroOrMore()
               select new ConstructorExpr(id, parameters) as Expr;

        public static NoiseGenerator<Expr> GenMatch(int depth) {
            static NoiseGenerator<(Pattern, Expr)> GenCase(int depth) 
                => from p in GenPattern(depth)
                   from e in GenExpr(depth)
                   select (p, e);

            return from e in GenExpr(depth)
                   from cases in GenCase(depth).OneOrMore()
                   select new MatchExpr(e, cases) as Expr;
        }

        public static NoiseGenerator<Expr> GenExpr(int depth)  {
            if (depth <= 0) {
                return Or( GenVariable()
                         , GenString()
                         , GenBool()
                         , GenInt()
                         );
            }

            return  Or( GenVariable()
                      , GenString()
                      , GenBool()
                      , GenInt()
                      , GenTuple(depth - 1)
                      , GenLet(depth - 1)
                      , GenLambda(depth - 1)
                      , GenCall(depth - 1)
                      , GenConstructor(depth - 1)
                      , GenMatch(depth - 1)
                      );
        }

        public static NoiseGenerator<Pattern> GenVariablePattern() 
            => from id in GenVariableId()
               select new VariablePattern(id) as Pattern;

        public static NoiseGenerator<Pattern> GenConstructorPattern(int depth)
            => from id in GenConstructorId()
               from parameters in GenPattern(depth).OneOrMore()
               select new ConstructorPattern(id, parameters) as Pattern;

        public static NoiseGenerator<Pattern> GenPattern(int depth) {
            if (depth <= 0) {
                return Or( (new WildCard() as Pattern).GenConst(), GenVariablePattern() );
            }

            return Or( (new WildCard() as Pattern).GenConst()
                     , GenVariablePattern()
                     , GenConstructorPattern(depth - 1)
                     );
        }

        public static NoiseGenerator<SType> GenSimpleType()
            => from id in GenTypeId()
               select new SimpleType(id) as SType;

        public static NoiseGenerator<SType> GenGenericType() 
            => from id in GenGenericTypeId()
               select new GenericType(id) as SType;

        public static NoiseGenerator<SType> GenIndexedType(int depth)
            => from id in GenTypeId()
               from types in GenType(depth).OneOrMore()
               select new IndexedType(id, types) as SType;

        public static NoiseGenerator<SType> GenArrowType(int depth)
            => from source in GenType(depth)
               from destination in GenType(depth)
               select new ArrowType(source, destination) as SType;

        public static NoiseGenerator<SType> GenTupleType(int depth) 
            => from parameters in GenType(depth).ZeroOrMore()
               select new TupleType(parameters) as SType;

        public static NoiseGenerator<SType> GenType(int depth) {
            if (depth <= 0) {
                return Or(GenSimpleType(), GenGenericType());
            }
            return Or( GenSimpleType() 
                     , GenGenericType()
                     , GenIndexedType(depth - 1)
                     , GenArrowType(depth - 1)
                     , GenTupleType(depth - 1)
                     );
        } 

        public static NoiseGenerator<TopLevel> GenLetStatement(int depth) 
            => from id in GenVariableId()
               from type in GenType(depth).Maybe()
               from body in GenExpr(depth)
               select new LetStatement(id, type, body) as TopLevel;

        public static NoiseGenerator<TopLevel> GenTypeDefine(int depth) {
            static NoiseGenerator<DefineConstructor> GenCon(int depth)
                => from id in GenConstructorId()
                   from ps in GenType(depth).ZeroOrMore()
                   select new DefineConstructor(id, ps);
            
            return from id in GenTypeId()
                   from cons in GenCon(depth).ZeroOrMore()
                   select new TypeDefine(id, cons) as TopLevel;
        }

        public static NoiseGenerator<TopLevel> GenTopLevel(int depth)
            => Or( GenLetStatement(depth)
                 , GenTypeDefine(depth)
                 );
    }
}