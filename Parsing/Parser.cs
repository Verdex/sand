
using System;

using sand.Util;
using static sand.Util.ResultEx;

namespace sand.Parsing {
    public static class ParserEx {
        public static Parser<B> Select<A, B>(this Parser<A> target, Func<A, B> map)  
            => new Parser<B>( input => target.Parse(input) switch {
                Ok<A> o => Ok(map(o.Item)),
                Err<A> e => Err<B>(e.Error),
                _ => throw new Exception("Select unexpected Result Case"),
            } );

        public static Parser<R> SelectMany<A, B, R>(this Parser<A> target, Func<A, Parser<B>> next, Func<A, B, R> final)  
            => new Parser<R>( input => {
                var result = target.Parse(input);
                return result switch {
                    Ok<A> o => next(o.Item).Parse(input) switch {
                        Ok<B> ob => Ok(final( o.Item, ob.Item )),
                        Err<B> eb => Err<R>(eb.Error),
                        _ => throw new Exception("SelectMany unexpected Result Case"),
                    },
                    Err<A> e => Err<R>(e.Error),
                    _ => throw new Exception("SelectMany unexpected Result Case"),
                };
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