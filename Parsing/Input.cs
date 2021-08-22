
using sand.Util;
using static sand.Util.OptionEx;
using static sand.Util.ResultEx;

namespace sand.Parsing {
    public record RestorePoint(int index);
    
    public class Input {
        public string Text { get; }
        public int Index { get; }
        public Input(string text) {
            Text = text;
            Index = 0;
        }

        public RestorePoint CreateRestore() => new RestorePoint(Index);
        public void Restore(RestorePoint rp) {
            Index = rp.index;
        }

        public Option<char> GetChar() {
            if (Index >= Text.Length) {
                return None<char>();
            }

            var ret = Text[Index];
            Index++;

            return Some(ret);
        }

        public Result<string> Expect(string value) {
            if ( value.Length + Index > Text.Length ) {
                return Err<string>(new EndOfFileError());
            }

            var s = Index;
            var e = Index + value.Length;
            var target = Text[s..e];
            if ( target == value ) {
                Index += value.Length;
                return Ok(value);
            }
            return Err<string>(new ParseError( $"Expected {value}, but found {target}."
                                             , s 
                                             , e
                                             , Text
                                             ));
        }
    }    
}