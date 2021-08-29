
using System;

namespace sand.Util {
    public interface Result<T> { }

    public class Ok<T> : Result<T> {
        public readonly T Item;
        public Ok(T item) {
            Item = item;
        }
    }

    public class Err<T> : Result<T> {
        public readonly Error Error;
        public Err(Error error) {
            Error = error;
        }
    }

    public static class ResultEx {
        public static Result<T> Ok<T>(T item) => new Ok<T>(item);
        public static Err<T> Err<T>(Error error) => new Err<T>(error);
        public static Result<B> Select<A, B>(this Result<A> target, Func<A, B> f) 
            => target switch {
                Ok<A> o => Ok(f(o.Item)),
                Err<A> e => Err<B>(e.Error),
                _ => throw new Exception("Unexpected Result case"),
            };
    }
}