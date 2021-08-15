
using sand.Util;

namespace sand.Parsing {
    public interface Expr { }

    public record Integer(int i) : Expr;
    public record Str(string s) : Expr;
    public record Bool(bool b) : Expr;

}