
using System;

using sand.Util;
using static sand.Util.ResultEx;

namespace sand.Parsing {
    public static class ParserEx {
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