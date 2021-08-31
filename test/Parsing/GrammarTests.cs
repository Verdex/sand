using System;
using System.Linq;
using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;

using test.TestUtil;
using static test.Parsing.GenAst;

using sand.Parsing;
using static sand.Parsing.Displayer;
using sand.Util;


namespace test.Parsing
{
    public class GrammarTests 
    {
        private readonly ITestOutputHelper _output;

        public GrammarTests(ITestOutputHelper output) {
            _output = output;
        }

        [Fact]
        public void Test1()
        {
            var n = new Noise(17);

            var input = string.Join("\n", GenTopLevel(3).OneOrMore().Gen(n).Select(tl => tl.Display()).ToArray());

            var g = new Grammar();

            var x = g.Parse(input);

            switch (x) {
                case Ok<IEnumerable<TopLevel>> o: 
                    foreach(var w in o.Item) {
                        _output.WriteLine(Displayer.Display(w));
                        _output.WriteLine("\n");
                    }
                    break;
                case Err<IEnumerable<TopLevel>> e: 
                    _output.WriteLine($"{e.Error.Report()}");
                    break;
                default : 
                    _output.WriteLine("Default?");
                    break;
            }

            Assert.False(true);
        }
    }
}
