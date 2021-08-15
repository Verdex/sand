
using System;
using System.Linq;
using System.Collections.Generic;

using sand.Util;
using static sand.Util.ResultEx;
using static sand.Util.OptionEx;

namespace sand.Parsing {
    public static class ParserEx {

        public static Parser<char> Any() 
            => new Parser<char>(input => input.GetChar() switch {
                Some<char> c => Ok(c.Item),
                None<char> => Err<char>(new EndOfFileError()),
                _ => throw new Exception("Unexpected Option case")
            });

        public static Parser<string> Expect(string s) 
            => new Parser<string>(input => input.Expect(s));

        public static Parser<T> Or<T>( this Parser<T> target, Parser<T> other ) 
            => new Parser<T>( input => {
                var rp = input.CreateRestore();
                switch (target.Parse(input)) {
                    case Ok<T> o: 
                        return o;
                    case Err<T> e:
                        input.Restore(rp);
                        return other.Parse(input); 
                    default:
                        throw new Exception("Or unexpected Result case");
                }
            } );

        public static Parser<IEnumerable<T>> ZeroOrMore<T>( this Parser<T> target ) 
            => new Parser<IEnumerable<T>>( input => {
                var ret = new List<T>();
                var again = true;
                do {
                    var rp = input.CreateRestore();
                    switch (target.Parse(input)) {
                        case Ok<T> o:
                            ret.Add(o.Item);
                            break;
                        case Err<T> e:
                            input.Restore(rp);
                            again = false;
                            break;
                        default:
                            throw new Exception("ZeroOrMore unexpected Result case");
                    }
                } while(again);
                return Ok<IEnumerable<T>>(ret);
            });

        public static Parser<IEnumerable<T>> OneoOrMore<T>( this Parser<T> target ) 
            => from initial in target
            from rest in target.ZeroOrMore()
            select rest.Prepend(initial);

        public static Parser<Option<T>> Maybe<T>( this Parser<T> target ) 
            => new Parser<Option<T>>( input => target.Parse(input) switch {
                Ok<T> o => Ok(Some(o.Item)),
                Err<T> e => Ok(None<T>()),
                _ => throw new Exception("Unexpected Result case"),
            });

        public static Parser<T> Where<T>(this Parser<T> target, Func<T, bool> predicate) 
            => new Parser<T>( input => {
                var rp = input.CreateRestore();
                switch (target.Parse(input)) {
                    case Ok<T> o:
                        if(predicate(o.Item)) {
                            return Ok(o.Item);
                        }
                        else {
                            var e = Err<T>(new ParseError( $"Parsing predicate failed for type {typeof(T).ToString()}"
                                                         , rp.index
                                                         , rp.index
                                                         , input.Text
                                                         ));
                            input.Restore(rp);
                            return e;
                        }
                    case Err<T> e:
                        input.Restore(rp);
                        return e;
                    default: 
                        throw new Exception("Unexpected Result case");
                }
            });

        public static Parser<B> Select<A, B>(this Parser<A> target, Func<A, B> map)  
            => new Parser<B>( input => {
                var rp = input.CreateRestore();
                switch (target.Parse(input)) {
                    case Ok<A> o: 
                        return Ok(map(o.Item));
                    case Err<A> e: 
                        input.Restore(rp);
                        return Err<B>(e.Error);
                    default: 
                        throw new Exception("Select unexpected Result Case");
                }
            } );

        public static Parser<R> SelectMany<A, B, R>(this Parser<A> target, Func<A, Parser<B>> next, Func<A, B, R> final)  
            => new Parser<R>( input => {
                var rp1 = input.CreateRestore();
                var result1 = target.Parse(input);
                switch (result1) {
                    case Ok<A> o: 
                        var rp2 = input.CreateRestore();
                        var result2 = next(o.Item).Parse(input);
                        switch (result2) {
                            case Ok<B> ob: 
                                return Ok(final( o.Item, ob.Item ));
                            case Err<B> eb: 
                                input.Restore(rp2);
                                return Err<R>(eb.Error);
                            default: 
                                throw new Exception("SelectMany unexpected Result Case");
                        }
                    case Err<A> e:
                        input.Restore(rp1); 
                        return Err<R>(e.Error);
                    default:
                        throw new Exception("SelectMany unexpected Result Case");
                }
            } );
    }

    public class Parser<T> {
        private readonly Func<Input, Result<T>> _parser;
        public Parser(Func<Input, Result<T>> parser) {
            _parser = parser;
        }

        public Result<T> Parse(Input input) => _parser(input);
    }

}