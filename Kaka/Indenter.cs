/* This file defines a class that runs after the Tokenizer and handles syntacticaly relevant 
   whitespace.

   It scans all newline tokens and performs the following transformations: 
    1: Remove any newline characters immediatly followed by another newline 
    2: Replace any newlines representing increases in indentation with an INDENT token 
    3: Replace any newlines respesenting a valid decrease in indentation with a DEDENT token 
    4: Raise an exception if a newline represents an invalid decrease in indentation 
    5: Raise an exception is spaces and tabs are found mixed in a single file

A decrease in indentation is considered valid iff it reduces the indentation to a previously 
seen level 
*/


using System;
using System.Linq;
using System.Collections.Generic;

using KakaLexer;

namespace KakaIndenter 
{
    public class Indenter 
    {

        private enum IndentType 
        {
            UNKNOWN, SPACES, TABS,
        }

        private Stack<int> indentStack = new Stack<int>(new int[] { 0 });
        private readonly List<Token> input;
        private List<Token> output;
        private int current = 0;
        private IndentType indentType = IndentType.UNKNOWN;

        public Indenter(List<Token> tokens)
        {
            input = tokens;
            output = new List<Token>(tokens.Count);
        }

        private Token Advance() => input[current++];

        private Token EOF() => new Token(TokenType.EOF, "", null, 1);

        private bool IsNotAtEnd  { get { return current < input.Count; } }

        private Token Peek { get { return IsNotAtEnd ? input[current] : EOF(); } }

        private Token NewType(Token t, TokenType type) => 
            new Token(type, t.lexeme, t.literal, t.line);

        /* Ensures that indents do not contain a mix of spaces and tabs,
           the indent type for a file is set when then first space or 
           tab is encountered, and mist be consistent after that */
        private void PreventSpaceTabMixing(Token token)
        {
            if (token.lexeme.Length <= 1) return;

            if (indentType == IndentType.UNKNOWN)
            {
                indentType = token.lexeme[1] switch 
                {
                    ' ' => IndentType.SPACES,
                    '\t' => IndentType.TABS,
                    _ => throw new Exception($"Invalid character on line {token.line}")
                };
            }

            bool invalidIndent = indentType switch
            {
                IndentType.SPACES => token.lexeme.Contains('\t'),
                IndentType.TABS =>   token.lexeme.Contains(' '),
                _ => true,
            };

            if (invalidIndent) throw new Exception($"Mixed spaces and tabs on line ${token.line}");
        }

        // Catches changes in indentation and converts to the appropriate token,
        // Also raises en exception on invalid changes in indentation
        // i.e. Enforces rules 2-4 above
        private List<Token> ConvertToIndentDedent(Token token)
        {
            var tokens = new List<Token>();

            int spaces = token.lexeme.Length - 1;
            if (spaces > indentStack.Peek()) 
            {
                indentStack.Push(spaces);
                tokens.Add(NewType(token, TokenType.INDENT));
            }
            else if (spaces < indentStack.Peek())
            {
                if (!indentStack.Contains(spaces)) throw new Exception($"Invalid Dedent line ${token.line}");
                while (indentStack.Peek() != spaces)
                {
                    indentStack.Pop();
                    tokens.Add(NewType(token, TokenType.DEDENT));
                }
            }
            else
            {
                tokens.Add(token);
            }
            return tokens;
        }

        private void ProcessNewLineToken(Token token)
        {
            // Ignore newlines for empty lines
            if (Peek.type == TokenType.NEWLINE) return;
            if (Peek.type == TokenType.COMMENT) return;

            PreventSpaceTabMixing(token);
            var tokens = ConvertToIndentDedent(token);

            output.AddRange(tokens);
        }

        public List<Token> Indent() 
        {
            while (IsNotAtEnd)
            {
                Token token = Advance();
                if (token.type != TokenType.NEWLINE)
                {
                    output.Add(token);
                }
                else 
                {
                    ProcessNewLineToken(token);
                }
            }
            return output;
        }
    }
}