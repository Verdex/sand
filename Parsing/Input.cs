
using sand.Util;
using static sand.Util.OptionEx;
using static sand.Util.ResultEx;

namespace sand.Parsing {
    public record RestorePoint(int index);
    
    public class Input {
        private readonly string _text;
        private int _index;
        public Input(string text) {
            _text = text;
            _index = 0;
        }

        public RestorePoint CreateRestore() => new RestorePoint(_index);
        public void Restore(RestorePoint rp) {
            _index = rp.index;
        }

        public Option<char> GetChar() {
            if (_index >= _text.Length) {
                return None<char>();
            }

            var ret = _text[_index];
            _index++;

            return Some(ret);
        }

        public Result<string> Expect(string value) {
            var s = _index;
            var e = _index + value.Length;
            var target = _text[s..e];
            if ( target == value ) {
                return Ok(value);
            }
            return Err<string>(new ParseError( $"Expected {value}, but found {target}."
                                             , s 
                                             , e
                                             , _text
                                             ));
        }
    }    
}