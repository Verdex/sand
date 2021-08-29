
using System.Collections.Generic;

using sand.Util;

namespace sand.Parsing {

    public interface TopLevel { }
    public record LetStatement(string variable, Option<SType> type, Expr value) : TopLevel;
    public record TypeDefine( string name
                            , IEnumerable<DefineConstructor> constructors
                            ) : TopLevel;

    public record DefineConstructor(string name, IEnumerable<SType> parameters);

    public interface SType { }
    public record SimpleType(string name) : SType;
    public record GenericType(string name) : SType;
    public record IndexedType(string name, IEnumerable<SType> parameters) : SType;
    public record ArrowType(SType source, SType destination) : SType;
    public record TupleType(IEnumerable<SType> parameters) : SType;

    public interface Expr { }
    public record Integer(int i) : Expr;
    public record Str(string s) : Expr;
    public record Bool(bool b) : Expr;
    public record Variable(string name) : Expr;
    public record TupleExpr(IEnumerable<Expr> parameters) : Expr;
    public record LetExpr(string variable, Option<SType> type, Expr value, Expr body) : Expr;
    public record LambdaExpr(IEnumerable<(string, Option<SType>)> parameters, Option<SType> returnType, Expr body) : Expr;
    public record CallExpr(Expr funcExpr, IEnumerable<Expr> parameters) : Expr;
    public record ConstructorExpr(string name, IEnumerable<Expr> parameters) : Expr;
    public record MatchExpr(Expr expr, IEnumerable<(Pattern, Expr)> patterns) : Expr;

    public interface Pattern { }
    public record WildCard() : Pattern;
    public record VariablePattern(string name) : Pattern;
    public record ConstructorPattern(string name, IEnumerable<Pattern> parameters) : Pattern;
}