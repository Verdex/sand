
using System;

namespace sand.Parsing {
    public static class Displayer {
        public static string Display(TopLevel input) 
            => input switch {
                TypeDefine x => Display(x),
                LetStatement x => Display(x),
                _ => throw new Exception("Unexpected TopLevel case"),
            };
        
        public static string Display(TypeDefine input)
            => "";

        public static string Display(LetStatement input) 
            => "";

        public static string Display(DefineConstructor input) 
            => "";

        public static string Display(SType input) 
            => input switch {
                SimpleType x => Display(x),
                GenericType x => Display(x),
                IndexedType x => Display(x),
                ArrowType x => Display(x),
                TupleType x => Display(x),
                _ => throw new Exception("unexpected SType case"),
            };

        public static string Display(SimpleType input)
            => "";

        public static string Display(GenericType input) 
            => "";

        public static string Display(IndexedType input) 
            => "";

        public static string Display(ArrowType input)
            => "";

        public static string Display(TupleType input) 
            => "";

        public static string Display(Expr input)
            => input switch {
                Integer x => Display(x),
                Str x => Display(x),
                Bool x => Display(x),
                Variable x => Display(x),
                TupleExpr x => Display(x),
                LetExpr x => Display(x),
                LambdaExpr x => Display(x),
                CallExpr x => Display(x),
                ConstructorExpr x => Display(x),
                MatchExpr x => Display(x),
                _ => throw new Exception("unexpected expr case"),
            };

        public static string Display(Integer input) => $" {input.i} ";
        public static string Display(Str input) => $" \"{input.s}\" ";
        public static string Display(Bool input) => $" {input.b} ";
        public static string Display(Variable input) => $" {input.name} ";
        public static string Display(TupleExpr input) => "";
        public static string Display(LetExpr input) => "";
        public static string Display(LambdaExpr input) => "";
        public static string Display(CallExpr input) => "";
        public static string Display(ConstructorExpr input) => "";
        public static string Display(MatchExpr input) => "";

        public static string Display(Pattern input) 
            => input switch {
                WildCard x => Display(x),
                VariablePattern x => Display(x),
                ConstructorPattern x => Display(x),
                _ => throw new Exception("unexpected pattern case"),
            };

        public static string Display(WildCard input) => "";
        public static string Display(VariablePattern input) => "";
        public static string Display(ConstructorPattern input) => "";
    }
}