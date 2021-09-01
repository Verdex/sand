using System;
using System.Linq;
using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;

using test.TestUtil;
using static test.Parsing.GenAst;

using sand.Parsing;
using static sand.Parsing.Displayer;
using sand.Util;


namespace test.Parsing{
    public class GrammarTests {
        private readonly ITestOutputHelper _output;

        public GrammarTests(ITestOutputHelper output) {
            _output = output;
        }

        [Theory]
        [InlineData("let x : Y<a> -> a = ();")]
        [InlineData("let x : Y<Z<a>> = ();")]
        [InlineData("let x = | _ | \"\";")]
        [InlineData("let x = 123456;")]
        [InlineData("let x = | | ();")]
        [InlineData("let x = | w, y | ();")]
        [InlineData("let x = | w : (a, b) | ();")]
        [InlineData("let x = | w : (a, b), xyz | ();")]
        public void ShouldParseSingleTopLevelItem(string input) {
            var g = new Grammar();

            var ast = g.Parse(input);

            Assert.True(ast is Ok<IEnumerable<TopLevel>>);
        }

        [Theory]
        [InlineData(637660367133469093)]
        [InlineData(637660368893334849)]
        public void ShouldParseKnownProblematicSeed(ulong seed) {
            var g = new Grammar();

            var n = new Noise(seed);

            var input = string.Join("\n", GenTopLevel(3).OneOrMore().Gen(n).Select(tl => tl.Display()).ToArray());

            var ast = g.Parse(input);

            Assert.True(ast is Ok<IEnumerable<TopLevel>>, $"Encountered parse failure from seed {seed}");

            switch(ast) {
                case Ok<IEnumerable<TopLevel>> o: 
                    var output = string.Join("\n", o.Item.Select( tl => tl.Display() ).ToArray());
                    Assert.True(input == output, $"Gen->Parse->Display failed at seed {seed}");
                    break;
                default:
                    Assert.True(false, $"ast somehow failed at seed {seed}");
                    break;
            }
        }

        [Fact]
        public void ShouldParseRandomProgram() {
            var g = new Grammar();

            var initial = new Noise((ulong)DateTime.Now.Ticks);

            foreach( ulong seed in Enumerable.Range(0, 50).Select(_ => { initial = initial.Next(); return initial.Any(); }) ) {
                var n = new Noise(seed);

                var input = string.Join("\n", GenTopLevel(3).OneOrMore().Gen(n).Select(tl => tl.Display()).ToArray());

                var ast = g.Parse(input);

                Assert.True(ast is Ok<IEnumerable<TopLevel>>, $"Encountered parse failure from seed {seed}");

                switch(ast) {
                    case Ok<IEnumerable<TopLevel>> o: 
                        var output = string.Join("\n", o.Item.Select( tl => tl.Display() ).ToArray());
                        Assert.True(input == output, $"Gen->Parse->Display failed at seed {seed}");
                        break;
                    default:
                        Assert.True(false, $"ast somehow failed at seed {seed}");
                        break;
                }
            }
        }
    }
}
