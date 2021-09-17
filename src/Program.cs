
using System;
using System.Linq;
using System.Collections.Generic;

using sand.Parsing;
using sand.Util;

using static sand.Parsing.ParserEx;

namespace sand
{
    class Program
    {
        private static List<Error> FindImportant(AggregateError error) {
            static IEnumerable<Error> Flat(Error error) =>
                error switch {
                    AggregateError ag => ag.Errors.SelectMany(Flat).Prepend(ag),
                    Error e => new [] { e },
                    _ => throw new Exception("Unexpected error type"),
                };

            return Flat(error).Where(e => e.Priority() == Importance.High).ToList();
        }

        static void Main(string[] args)
        {
            var g = new Grammar();
            var x = g.Parse(@"

type List = Cons(a, List<a>)
          | Nil
          ;

let x : Int = 4;

let count : List<a> -> Int = | list : List<a> | -> Int 
    match list {
        Cons(_, rest) => inc(count(rest)),
        Nil => zero(),
    }
    ;

let z = Cons( 1, NIL );

let ww = | h | h ;

let www = ""blarg"";

let u : (Int, Int, Int) = (1, 2, 3);

let g = 
    let q = 5 in
    let i = 6 in
    add(q, i);

let e =
    let j : Int = 5 in
    j;


let blahs : List< a -> List<Int> > = NIL;

let b = true;
let b2 = false;

            ");

            switch (x) {
                case Ok<IEnumerable<TopLevel>> o: 
                    foreach(var w in o.Item) {
                        Console.WriteLine(Displayer.Display(w));
                        Console.WriteLine("\n");
                    }
                    break;
                case Err<IEnumerable<TopLevel>> e: 
                    switch(e.Error) {
                        case AggregateError ag:
                            Console.WriteLine("blarg");
                            break;
                        case Error err:
                            Console.WriteLine($"{e.Error.Report()}");
                            break;
                        default:
                            Console.WriteLine("unknown error type");
                            break;
                    }
                    break;
                default : 
                    Console.WriteLine("Default?");
                    break;
            }
        }
    }
}
