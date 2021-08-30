
using System;
using System.Linq;
using System.Collections.Generic;

using sand.Util;
using static sand.Util.OptionEx;

namespace test.TestUtil {
    public record Noise(ulong seed) {

        public ulong Any() {
            var x = seed + 17;
            
            x ^= x << 13;
            x ^= x >> 7;
            x ^= x << 17;

            return x;
        }

        public Noise Next() => this with {seed = this.Any()};

        public ulong InRange(ulong min, ulong max) {
            var m = (max - min) + 1;
            return this.Any() % m + min;
        }

        public ulong Max(ulong max) => InRange(0, max);
    }

    public class NoiseGenerator<T> {
        private readonly Func<Noise, T> _gen;
        public NoiseGenerator(Func<Noise, T> gen) {
            _gen = gen;
        }

        public T Gen(Noise noise) => _gen(noise);
    }

    public static class NoiseGeneratorEx {
        private const ulong Length = 7;

        public static NoiseGenerator<B> Select<A, B>(this NoiseGenerator<A> gen, Func<A, B> map)
            => new NoiseGenerator<B>(noise => map(gen.Gen(noise)));

        public static NoiseGenerator<R> SelectMany<A, B, R>(this NoiseGenerator<A> gen, Func<A, NoiseGenerator<B>> next, Func<A, B, R> final)  
            => new NoiseGenerator<R>(noise => {
                var a = gen.Gen(noise);
                var b = next(a).Gen(noise.Next());
                return final(a, b);
            });

        public static NoiseGenerator<T> GenConst<T>(this T item) => new NoiseGenerator<T>( noise => item );

        public static NoiseGenerator<T> Or<T>(params NoiseGenerator<T>[] targets) 
            => new NoiseGenerator<T>(noise => targets[noise.Max((ulong)targets.Length - 1)].Gen(noise.Next()));

        public static NoiseGenerator<IEnumerable<T>> OneOrMore<T>(this NoiseGenerator<T> target)  {
            static IEnumerable<T> F(Noise noise, NoiseGenerator<T> target) {
                var i = noise.InRange(1, Length) + 1;
                while(i >= 1) {
                    noise = noise.Next();
                    yield return target.Gen(noise);
                    i--;
                }
            }

            return new NoiseGenerator<IEnumerable<T>>( noise => F(noise, target).ToArray() );
        }

        public static NoiseGenerator<IEnumerable<T>> ZeroOrMore<T>(this NoiseGenerator<T> target) {
            static IEnumerable<T> F(Noise noise, NoiseGenerator<T> target) {
                var i = noise.Max(Length) + 1;
                while(i >= 1) {
                    noise = noise.Next();
                    yield return target.Gen(noise);
                    i--;
                }
            }

            return new NoiseGenerator<IEnumerable<T>>( noise => F(noise, target).ToArray() );
        }

        public static NoiseGenerator<Option<T>> Maybe<T>(this NoiseGenerator<T> target) 
            => Or(target.Select(t => Some(t)), None<T>().GenConst());
                
    }
}