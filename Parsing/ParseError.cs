
using System.Linq;

using sand.Util;

namespace sand.Parsing {
    public record ParsePredicateError(string retType, int index) : Error {
        public string Report() => $"Parsing predicate failed for type {retType} at {index}";
    }
    public class EndOfFileError : Error { 
        public string Report() => "Encountered unexpected end of file";
    }
    public class ParseError : Error {
        private readonly string _message;
        private readonly int _start;
        private readonly int _end;
        private readonly string _text;
        public ParseError(string message, int start, int end, string text) {
            _message = message;
            _start = start;
            _end = end;
            _text = text;
        }
        public string Report() {
            static bool EndLine(char c) => c == '\n' || c == '\r';

            var startOfLine = _start;
            var endOfLine = _end;
            while (startOfLine > 0 && !EndLine(_text[startOfLine])) {
                startOfLine--;
            }
            while (endOfLine < _text.Length && !EndLine(_text[endOfLine])) {
                endOfLine++;
            }
            var errorLine = _text[startOfLine..endOfLine];
            var underLine = new string('-', _start - startOfLine);
            var arrowLine = new string('^', _end - _start);

            var lineCount = _text[.._start].ToCharArray()
                                           .Where(EndLine)
                                           .ToArray()
                                           .Length;

            return $"{_message}\nAt Line Number:{lineCount}\n\n{errorLine}\n{underLine}{arrowLine}\n";
        }
    }
}