using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using KakaLexer;

namespace LexerTest
{
    public class TokenTest
    {
        static List<Token> Tokens(TokenType[] types)
        {
            var results = new List<Token>();
            foreach (var type in types) 
            {
                results.Add(new Token(type, "", null, 0));
            }
            results.Add(new Token(TokenType.EOF, "", null, 0));
            return results;
        }

        static bool CheckTypes(List<Token> left, List<Token> right)
        {
            return left.Zip(right).All(pair => pair.Item1.type == pair.Item2.type);
        }

        private static string StripQuotes(string input) => input.Trim(new char[] {'\"'});

        [Theory]
        [InlineData("!", new TokenType[] {TokenType.BANG})]
        [InlineData("!=", new TokenType[] {TokenType.BANG_EQUAL})]
        [InlineData("", new TokenType[] {})]
        [InlineData("<=", new TokenType[] {TokenType.LESS_EQUAL})]
        [InlineData("[ * ]", new TokenType[] {TokenType.LEFT_BRACKET, TokenType.STAR, TokenType.RIGHT_BRACKET})]
        [InlineData("[*]", new TokenType[] {TokenType.LEFT_BRACKET, TokenType.STAR, TokenType.RIGHT_BRACKET})]
        public void Token_Types_Are_Correct(string input, TokenType[] tokens)
        {
            var lexer = new Lexer(input);
            var testTokens = Tokens(tokens);

            var resultTokens = lexer.Lex();

            Assert.True(CheckTypes(testTokens, resultTokens));
        }

        [Theory]
        [InlineData("self", new TokenType[] {TokenType.SELF})]
        [InlineData("foo", new TokenType[] {TokenType.IDENTIFIER})]
        [InlineData("selfish", new TokenType[] {TokenType.IDENTIFIER})]
        [InlineData("selfish class", new TokenType[] {TokenType.IDENTIFIER, TokenType.CLASS})]
        public void Keywords_and_Identifiers_are_Correct(
            string input,
            TokenType[] tokens
        )
        {
            var lexer = new Lexer(input);
            var testTokens = Tokens(tokens);

            var resultTokens = lexer.Lex();

            Assert.True(CheckTypes(testTokens, resultTokens));
        }


        [Theory]
        [InlineData("\"self\"", new TokenType[] {TokenType.STRING})]
        [InlineData("\"self\" self", new TokenType[] {TokenType.STRING, TokenType.SELF})]
        public void Strings_are_Correct(
            string input,
            TokenType[] tokens
        )
        {
            var lexer = new Lexer(input);
            var testTokens = Tokens(tokens);

            var resultTokens = lexer.Lex();

            Assert.True(CheckTypes(testTokens, resultTokens));
        }

        [Theory]
        [InlineData("\"test\"")]
        [InlineData("\"\"")]
        [InlineData("\"Multi word string\"")]
        public void String_Contents_are_Correct(string input)
        {
            var lexer = new Lexer(input);
            var expected = StripQuotes(input);

            List<Token> resultTokens = lexer.Lex();

            Assert.Equal(expected, resultTokens[0].literal);
        }

        [Theory]
        [InlineData("foo:", "foo")]
        [InlineData("foo:bar", "foo")]
        public void Keywords_are_Lexed(string input, string expected)
        {
            var lexer = new Lexer(input);
            
            List<Token> resultsTokens = lexer.Lex();

            Token kwToken = resultsTokens[0];
            Assert.Equal(TokenType.KEYWORD, kwToken.type);
            Assert.Equal(expected, kwToken.literal);
        }

        [Fact]
        public void Identifier_After_Keyword_is_Lexed()
        {
            string expected = "bar";
            var lexer = new Lexer($"foo:{expected}");
            
            List<Token> resultsTokens = lexer.Lex();

            Token token = resultsTokens[1];
            Assert.Equal(TokenType.IDENTIFIER, token.type);
            Assert.Equal(expected, token.lexeme);
        }
    }
}
