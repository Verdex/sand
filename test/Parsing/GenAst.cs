
using System;
using System.Linq;
using System.Collections.Generic;

using test.TestUtil;
using static test.TestUtil.NoiseGeneratorEx;

using sand.Parsing;

namespace test.Parsing {

    public static class GenAst {

        private static NoiseGenerator<char> GenUpperLetter() 
            => new NoiseGenerator<char>( noise => (char)noise.InRange(65, 90) );

        private static NoiseGenerator<char> GenLowerLetter() 
            => new NoiseGenerator<char>( noise => (char)noise.InRange(97, 122) );

        public static NoiseGenerator<string> GenTypeId() {
            return from f in GenUpperLetter()
                   from r in Or(GenLowerLetter(), GenUpperLetter()).OneOrMore()
                   select new string((new char[] { f }).Concat(r).ToArray());
        }

        public static NoiseGenerator<string> GenConstructorId() => GenTypeId();
    }
}