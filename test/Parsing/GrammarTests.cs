using System;
using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;

using test.TestUtil;

using sand.Parsing;
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

            _output.WriteLine(GenAst.GenConstructorId().Gen(n));

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
