
using System;
using System.Linq;
using System.Collections.Generic;

using test.TestUtil;

using sand.Parsing;

namespace test.Parsing {

    public static class GenAst {

        private const ulong Length = 7;

        private static char GenUpperLetter(Noise noise) 
            => (char)noise.InRange(65, 90);

        private static char GenLowerLetter(Noise noise) 
            => (char)noise.InRange(97, 122);

        private static Func<Noise, T> Or<T>(params Func<Noise, T>[] targets) 
            => noise => targets[noise.Max((ulong)targets.Length - 1)](noise.Next());

        private static Func<Noise, IEnumerable<T>> OneOrMore<T>(Func<Noise, T> target) {
            static IEnumerable<T> F(Noise noise, Func<Noise, T> target) {
                var i = noise.InRange(1, Length) + 1;
                while(i >= 1) {
                    noise = noise.Next();
                    yield return target(noise);
                    i--;
                }
            }

            return noise => F(noise, target);
        }

        private static Func<Noise, IEnumerable<T>> ZeroOrMore<T>(Noise noise, Func<Noise, T> target) {
            static IEnumerable<T> F(Noise noise, Func<Noise, T> target) {
                var i = noise.Max(Length) + 1;
                while(i >= 1) {
                    noise = noise.Next();
                    yield return target(noise);
                    i--;
                }
            }

            return noise => F(noise, target);
        }

        public static string GenConstructorId(Noise noise) {
            var f = GenUpperLetter(noise);
            var r = OneOrMore(Or(GenLowerLetter, GenUpperLetter))(noise.Next());
            return new string((new char[] { f }).Concat(r).ToArray());
        }
    }

}