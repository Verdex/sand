
using System;
using System.Collections.Generic;

using sand.Parsing;
using sand.Util;

using static sand.Parsing.ParserEx;

namespace sand
{
    class Program
    {
        static void Main(string[] args)
        {
            var g = new Grammar();
            var x = g.Parse(@"

let x = 5 in print(x);

            ");

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
