
using System;

namespace sand.Parsing {
    public static class Displayer {
        public static string Display(TopLevel tl) 
            => tl switch {
                TypeDefine td => Display(td),
                LetStatement ls => Display(ls),
                _ => throw new Exception("Unexpected TopLevel case"),
            };
        
        public static string Display(TypeDefine td)
            => "";

        public static string Display(LetStatement ls) 
            => "";
    }
}