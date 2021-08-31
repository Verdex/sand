using System;
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

            foreach (var f in GenTopLevel(3).OneOrMore().Gen(n)) {

                _output.WriteLine(f.Display());
            }

            var g = new Grammar();

            Assert.False(true);

            var x = g.Parse("");

            switch (x) {
                case Ok<IEnumerable<TopLevel>> o: 
                    foreach(var w in o.Item) {
                        Console.WriteLine(Displayer.Display(w));
                        Console.WriteLine("\n");
                    }
                    break;
                case Err<IEnumerable<TopLevel>> e: 
                    Console.WriteLine($"{e.Error.Report()}");
                    break;
                default : 
                    Console.WriteLine("Default?");
                    break;
            }
        }
    }
}
