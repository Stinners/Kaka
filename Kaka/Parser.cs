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
        private List<Expression> output = new List<Expression>();
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        private bool IsNotAtEnd { get { return current > tokens.Count; } }

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

        public List<Expression> Parse()
        {
            return output;
        }
    }

}