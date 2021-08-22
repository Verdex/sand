
using System;
using System.Linq;
using System.Collections.Generic;

using sand.Util;
using static sand.Util.OptionEx;

namespace sand.Parsing {
    public static class Displayer {
        public static string Display(this TopLevel input) 
            => input switch {
                TypeDefine x => Display(x),
                LetStatement x => Display(x),
                _ => throw new Exception("Unexpected TopLevel case"),
            };
        
        public static string Display(this TypeDefine input)
            => "";

        public static string Display(this LetStatement input) 
            => "";

        public static string Display(this DefineConstructor input) 
            => "";

        public static string Display(this SType input) 
            => input switch {
                SimpleType x => Display(x),
                GenericType x => Display(x),
                IndexedType x => Display(x),
                ArrowType x => Display(x),
                TupleType x => Display(x),
                _ => throw new Exception("unexpected SType case"),
            };

        public static string Display(this SimpleType input)
            => $" {input.name} ";

        public static string Display(this GenericType input) 
            => $" {input.name} ";

        public static string Display(this IndexedType input) 
            => $" {input.name}< {input.parameters.Display(Display)} > ";

        public static string Display(this ArrowType input)
            => $" {input.source} -> {input.destination} ";

        public static string Display(this TupleType input) 
            => $" ( {input.parameters.Display(Display)} ) ";

        public static string Display(this Expr input)
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

        public static string Display(this Integer input) => $" {input.i} ";
        public static string Display(this Str input) => $" \"{input.s}\" ";
        public static string Display(this Bool input) => $" {input.b} ";
        public static string Display(this Variable input) => $" {input.name} ";
        public static string Display(this TupleExpr input) => $" ( {input.parameters.Display(Display)} ) ";
        public static string Display(this LetExpr input) 
            => $"let {input.variable} {input.type.Display( x => $" : {x.Display()}" )} = {input.value.Display()} \nin {input.body.Display()}\n";
        public static string Display(this LambdaExpr input) 
            => $" | {input.parameters.Display(x => $" {x.Item1} {x.Item2.Display(x => $" : {x.Display()} " )} ") } | {input.returnType.Display(x => $" -> {x.Display()} ")} {input.body.Display()} ";
        public static string Display(this CallExpr input) => $" {input.funcExpr.Display()}( {input.parameters.Display(Display)} ) ";
        public static string Display(this ConstructorExpr input) {
            if (input.parameters.Any()) {
                return $" {input.name}( {input.parameters.Display(Display)} )";
            }
            else {
                return $" {input.name} ";
            }
        } 
        public static string Display(this MatchExpr input) 
            => $" match {input.expr.Display()} {{\n {input.patterns.Display(x => $" {x.Item1.Display()} => {x.Item2.Display()}\n")}}}";

        public static string Display(this Pattern input) 
            => input switch {
                WildCard x => Display(x),
                VariablePattern x => Display(x),
                ConstructorPattern x => Display(x),
                _ => throw new Exception("unexpected pattern case"),
            };

        public static string Display(this WildCard input) => " _ ";
        public static string Display(this VariablePattern input) => $" {input.name} ";
        public static string Display(this ConstructorPattern input) 
            => $" {input.name}( {input.parameters.Display(Display)}) ";
        
        private static string Display<T>(this IEnumerable<T> target, Func<T, string> display, string sep = ", ") 
            => target.Select(x => display(x)).Join(sep);

        private static string Display<T>(this Option<T> target, Func<T, string> display)
            => display switch {
                Some<T> s  => display(s.Item),
                None<T> n => "",
                _ => throw new Exception("Unexpected Option case"),
            };

        private static string Join(this IEnumerable<string> target, string sep) 
            => string.Join(sep, target);
    }
}