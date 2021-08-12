
namespace sand.Util {
    public interface Option<T> { }

    public class Some<T> : Option<T> {
        public readonly T Item;
        public Some(T item) {
            Item = item;
        }
    }

    public class None<T> : Option<T> { }

    public static class OptionEx {
        public static Option<T> Some<T>( T item ) => new Some<T>(item);
        public static Option<T> None<T>() => new None<T>();
    }
}