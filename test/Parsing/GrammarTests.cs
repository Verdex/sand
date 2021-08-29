using System;
using System.Collections.Generic;

using Xunit;

using sand.Parsing;
using sand.Util;

namespace test.Parsing
{
    public class GrammarTests 
    {
        [Fact]
        public void Test1()
        {

            var g = new Grammar();
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
