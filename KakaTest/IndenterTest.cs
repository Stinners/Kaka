using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using KakaLexer;

/* Note this doesn;t actually call Indenter directly, it just tests the functionality
   of the lexer that is provided by the indenter class */
namespace IndenterTest 
{
    public class IndenterTests 
    {
        // TODO tidy up these utility functions - Should probably move into a superclass
        static bool CheckTypes(List<Token> left, List<Token> right)
        {
            return left.Zip(right).All(pair => pair.Item1.type == pair.Item2.type);
        }

        static List<Token> Tokens(TokenType[] types)
        {
            return types.Select(type => new Token(type, "", null, 0))
                        .Append(new Token(TokenType.EOF, "", null, 0))
                        .ToList();
        }

        private void CheckInput(string input, TokenType[] expected)
        {
            var lexer = new Lexer(input);
            var resultTokens = lexer.Lex();
            List<Token> expectedTokens = Tokens(expected);

            Assert.True(CheckTypes(resultTokens, expectedTokens));
        }


        /* ============= Tests ===================*/
        [Theory]
        [InlineData("\n\ntest", new TokenType[] { TokenType.NEWLINE, TokenType.IDENTIFIER})]
        [InlineData("\n#test\ntest", new TokenType[] { TokenType.COMMENT, TokenType.NEWLINE, TokenType.IDENTIFIER })]
        [InlineData("\n\n\n\n.", new TokenType[] { TokenType.NEWLINE, TokenType.DOT})]
        public void Removes_Excess_Newlines(string input, TokenType[] expected)
        {
            CheckInput(input, expected);
        }

        [Theory]
        [InlineData("\n\t .")]
        [InlineData("\n \t.")]
        [InlineData("\n .\n\t.")]
        public void Rejects_Mixed_Tabs_and_Spaces(string input)
        {
            var lexer = new Lexer(input);

            Assert.Throws<Exception>(() => lexer.Lex());
        }

        [Theory]
        [InlineData("\n.\n   .", new TokenType[] { TokenType.NEWLINE, TokenType.DOT, TokenType.INDENT, TokenType.DOT})]
        [InlineData("\n  \n.\n   .", new TokenType[] { TokenType.NEWLINE, TokenType.DOT, TokenType.INDENT, TokenType.DOT})]
        [InlineData("\n\t\t\n.\n   .", new TokenType[] { TokenType.NEWLINE, TokenType.DOT, TokenType.INDENT, TokenType.DOT})]
        public void Handles_Indents(string input, TokenType[] expected)
        {
            CheckInput(input, expected);
        }

        [Theory]
        [InlineData("\n    .\n.", new TokenType[] { TokenType.INDENT, TokenType.DOT, TokenType.DEDENT, TokenType.DOT})]
        [InlineData("\n .\n  .\n .", new TokenType[] { TokenType.INDENT, TokenType.DOT, TokenType.INDENT, TokenType.DOT, TokenType.DEDENT, TokenType.DOT})]
        [InlineData("\n .\n  .\n.", new TokenType[] { TokenType.INDENT, TokenType.DOT, TokenType.INDENT, TokenType.DOT, TokenType.DEDENT, TokenType.DEDENT, TokenType.DOT})]
        public void Handles_Valid_Dedent(string input, TokenType[] expected)
        {
            CheckInput(input, expected);
        }

        [Theory]
        [InlineData("\n    .\n .")]
        public void Rejects_Invalid_Dedent(string input)
        {
            var lexer = new Lexer(input);

            Assert.Throws<Exception>(() => lexer.Lex());
        }
    }
}