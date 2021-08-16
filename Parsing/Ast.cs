
using System.Collections.Generic;

using sand.Util;

namespace sand.Parsing {

    public interface TopLevel { }
    public class LetStatement : TopLevel { }
    public class TypeDefine : TopLevel { }

    public interface SType { }
    public record SimpleType(string name) : SType;
    public record IndexedType(string name, IEnumerable<SType> parameters) : SType;
    public record ArrowType(SType source, SType destination) : SType;
    public record TupleType(IEnumerable<SType> parameters) : SType;

    public interface Expr { }
    public record Integer(int i) : Expr;
    public record Str(string s) : Expr;
    public record Bool(bool b) : Expr;
    public record Variable(string name) : Expr;
    public record LetExpr(string variable, Expr value, Expr body) : Expr { }
    public class LambdaExpr : Expr { }
    public class CallExpr : Expr { }
    public class ConstructorExpr : Expr { }
    public class MatchExpr : Expr { }
}