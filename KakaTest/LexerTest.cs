using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using KakaLexer;

namespace LexerTest
{
    public class TokenTest
    {
        /* =========== Helper Functions ===================*/

        static List<Token> Tokens(TokenType[] types)
        {
            return types.Select(type => new Token(type, "", null, 0))
                        .Append(new Token(TokenType.EOF, "", null, 0))
                        .ToList();
        }

        static bool CheckTypes(List<Token> left, List<Token> right)
        {
            return left.Zip(right).All(pair => pair.Item1.type == pair.Item2.type);
        }

        private static string StripQuotes(string input) => input.Trim(new char[] {'\"'});

        /* =========== Tests ===================*/

        [Theory]
        [InlineData("!", new TokenType[] {TokenType.BANG})]
        [InlineData(".", new TokenType[] {TokenType.DOT})]
        [InlineData("", new TokenType[] {})]
        [InlineData("<= !=", new TokenType[] {TokenType.LESS_EQUAL, TokenType.BANG_EQUAL})]
        [InlineData("@{", new TokenType[] {TokenType.AT_LEFT_BRACE})]
        [InlineData("[ * ]", new TokenType[] {TokenType.LEFT_BRACKET, TokenType.STAR, TokenType.RIGHT_BRACKET})]
        [InlineData("[*]", new TokenType[] {TokenType.LEFT_BRACKET, TokenType.STAR, TokenType.RIGHT_BRACKET})]
        [InlineData("class 3.12 \n \"test\"", new TokenType[] {TokenType.CLASS, TokenType.FLOAT, TokenType.NEWLINE, TokenType.STRING})]
        [InlineData("#", new TokenType[] {TokenType.COMMENT})]
        [InlineData("#test", new TokenType[] {TokenType.COMMENT})]
        [InlineData("#test\n   test", new TokenType[] {TokenType.COMMENT, TokenType.NEWLINE, TokenType.IDENTIFIER})]
        public void Token_Types_Are_Correct(string input, TokenType[] tokens)
        {
            var lexer = new Lexer(input);
            var testTokens = Tokens(tokens);

            var resultTokens = lexer.Lex();

            Assert.True(CheckTypes(testTokens, resultTokens));
        }

        [Theory]
        [InlineData("$")]
        [InlineData("?")]
        public void Invalid_Tokens_Throw(string input)
        {
            var lexer = new Lexer(input);

            Assert.Throws<Exception>(() => lexer.Lex());
            
        }

        [Theory]
        [InlineData("self", new TokenType[] {TokenType.SELF})]
        [InlineData("using", new TokenType[] {TokenType.USING})]
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

        [Theory]
        [InlineData("3.14", 3.14)]
        [InlineData(".14", 0.14)]
        [InlineData("3.", 3.0)]
        public void Doubles_are_Parsed(string input, double expected)
        {
            var lexer = new Lexer(input);

            List<Token> resultsTokens = lexer.Lex();

            Token token = resultsTokens[0];
            Assert.Equal(TokenType.FLOAT, token.type);
            Assert.Equal(expected, token.literal);
        }

        [Theory]
        [InlineData("3", 3)]
        [InlineData("10", 10)]
        public void Integers_are_Parsed(string input, long expected)
        {
            var lexer = new Lexer(input);

            List<Token> resultsTokens = lexer.Lex();

            Token token = resultsTokens[0];
            Assert.Equal(TokenType.INTEGER, token.type);
            Assert.Equal(expected, token.literal);
        }
        
        [Theory]
        [InlineData(".3.")]
        [InlineData("1..0")]
        [InlineData("1.0.")]
        public void Only_1_Dot_Allowed(string input)
        {
            var lexer = new Lexer(input);

            Assert.Throws<Exception>(() => lexer.Lex());
        }

        // We need some text at the end of these since the lexer won't capture 
        // the last newline of the file
        [Theory]
        [InlineData("\ntext", 1)]
        [InlineData("\n   text", 4)]
        [InlineData("  \n   text", 4)]
        [InlineData("\n   \ttext", 5)]
        public void Counts_Spaces_After_Newlines(string input, int expected)
        {
            var lexer = new Lexer(input);

            Token token = lexer.Lex()[0];

            Assert.Equal(expected, token.lexeme.Length);
            Assert.Equal(2, token.line);
            Assert.Equal(TokenType.NEWLINE, token.type);
        }
    }
}
