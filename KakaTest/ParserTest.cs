using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using KakaParser;
using KakaLexer;
using KakaIndenter;
using KakaExpressions;

namespace ParserTest 
{
    public class ParserTests
    {

        private List<Node> Parse(string input)
        {
            var tokens = new Lexer(input).Lex();
            var results = new Parser(tokens).Parse();
            return results;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(7)]
        public void Integers_Parse_Correctly(int pairKey)
        {
            var pairs = new TestPairs();
            (string input, Node expected) = pairs.pairs[pairKey];
            var result = Parse(input)[0];

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public void Doubles_Parse_Correctly(int pairKey)
        {
            var pairs = new TestPairs();
            (string input, Node expected) = pairs.pairs[pairKey];
            var result = Parse(input)[0];

            Assert.Equal(result, expected);
        }

    }

    // For each test I will want to have a pair that is the expected AST plus the input
    // We can then parse everything and compare the actual AST vs the expected AST
    class TestPairs
    {
        public Dictionary<int, (string Input, Node Expected)> pairs = new Dictionary<int, (string Input, Node Expected)>
        {
            {1, ("1", new Integer(1))},

            {2, ("1+1",
                new Binary(
                    new Integer(1),
                    new Message("+"),
                    new Integer(1)
                ))
            },

            {3, ("1000009878987", new Integer(1000009878987))},

            {4, ("1.0", new KDouble(1.0))},
            {5, ("1.", new KDouble(1.0))},
            {6, ("-1.", new KDouble(-1.0))},
            {7, ("-1", new Integer(-1))},

        };

    }
}