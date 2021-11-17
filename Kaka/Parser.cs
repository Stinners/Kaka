using System;
using System.Linq;
using System.Collections.Generic;

using KakaLexer;
using KakaExpressions;

namespace KakaParser
{
    public class Parser
    {
        private readonly List<Token> tokens;
        private List<Node> output = new List<Node>();
        private int current = 0;
        private int start = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        private bool IsNotAtEnd { get { return current < tokens.Count; } }

        private Token Advance() {
            return IsNotAtEnd 
                   ? tokens[current++]
                   : tokens.Last();
        }

        private Token Peek { get { return IsNotAtEnd ? tokens[current] : tokens.Last(); } }

        private bool Match(TokenType type, out Token nextToken) 
        {
            nextToken = Peek;
            bool isMatch = nextToken.type == type;
            if (isMatch) Advance();
            return isMatch;
        }

        #region TerminalLiterals

        private void ParseInteger(Token token)
        {
            output.Add(new Integer((long)(token.literal!)));
        }

        private void ParseDouble(Token token)
        {
            output.Add(new KDouble((double)(token.literal!)));
        }

        #endregion

        private void ParseToken()
        {
            Token token = Advance();
            switch (token.type)
            {
                case TokenType.INTEGER: ParseInteger(token); break;
                case TokenType.FLOAT: ParseDouble(token); break;
                default: break;
            }
        }

        public List<Node> Parse()
        {
            while (IsNotAtEnd)
            {
                start = current;
                ParseToken();

            }
            return output;
        }
    }

}