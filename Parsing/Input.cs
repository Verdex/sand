
using sand.Util;
using static sand.Util.OptionEx;
using static sand.Util.ResultEx;

namespace sand.Parsing {
    public record RestorePoint(int index);
    
    public class Input {
        public string Text { get; }
        private int _index;
        public Input(string text) {
            Text = text;
            _index = 0;
        }

        public RestorePoint CreateRestore() => new RestorePoint(_index);
        public void Restore(RestorePoint rp) {
            _index = rp.index;
        }

        public Option<char> GetChar() {
            if (_index >= Text.Length) {
                return None<char>();
            }

            var ret = Text[_index];
            _index++;

            return Some(ret);
        }

        public Result<string> Expect(string value) {
            if ( value.Length + _index > Text.Length ) {
                return Err<string>(new EndOfFileError());
            }

            var s = _index;
            var e = _index + value.Length;
            var target = Text[s..e];
            if ( target == value ) {
                _index += value.Length;
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