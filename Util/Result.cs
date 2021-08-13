namespace sand.Util {
    public interface Error { }

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
    }
}