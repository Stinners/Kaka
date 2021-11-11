using System;
using System.Text;
using System.Collections.Generic;

namespace KakaLexer
{
    public enum TokenType 
    {
        // Single character tokens 
        LEFT_PAREN, RIGHT_PAREN,
        LEFT_BRACE, RIGHT_BRACE,
        LEFT_BRACKET, RIGHT_BRACKET,
        PLUS, MINUS, SLASH, STAR, DOT,
        COMMA, SEMICOLON, PIPE,

        // One or two character tokens 
        BANG, BANG_EQUAL,
        EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,
        AT_LEFT_BRACE,

        // Atomic Literals 
        IDENTIFIER, STRING,  KEYWORD, RESERVED,
        INTEGER, FLOAT,

        // Language Keywords 
        CLASS, SELF, USING,

        // Comments 
        COMMENT,

        // Newlines 
        NEWLINE, INDENT, DEDENT,

        EOF,
    }

    public readonly struct Token
    {
        public readonly TokenType type;
        public readonly String lexeme;
        public readonly object literal;
        public readonly int line;

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }
    }

    public class Lexer 
    {
        private Stack<int> indentationStack = new Stack<int>(new int[] {0});
        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();

        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Lexer (string source)
        {
            this.source = source;
        }

        private readonly Dictionary<string, TokenType> reserved = new Dictionary<string, TokenType>
        {
            {  "self",  TokenType.SELF },
            {  "class", TokenType.CLASS },
            {  "using", TokenType.USING },
        };
        private bool IsNotAtEnd { get { return current < source.Length; } }

        private char Advance() => source[current++];

        private void AddToken(TokenType type) => AddToken(type, null);

        private void AddToken(TokenType type, object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }

        // Return the next charter if not at end, otherwise, return 
        // a null byte 
        private char Peek
        {
            get { return IsNotAtEnd ? source[current] : '\0';}
        }

        private char PeekNext
        {
            get 
            {
                if (current + 1 > source.Length) return '\0';
                return source[current + 1];
            }
        }

        // Checks for a character and advances only if it finds the 
        // expected value
        private bool Match(char expected)
        {
            if (IsNotAtEnd && source[current] == expected)
            {
                current++;
                return true;
            }
            else 
            {
                return false;
            }
        }

        private bool IsDigitOrDot(char c) => Char.IsDigit(c) || c == '.';

        private bool IdentifierStart(char c) => Char.IsLetter(c) || c == '_';
        private bool IdentifierInner(char c) => IdentifierStart(c) || Char.IsDigit(c);


        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                // Single character tokens
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case '[': AddToken(TokenType.LEFT_BRACKET); break;
                case ']': AddToken(TokenType.RIGHT_BRACKET); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '+': AddToken(TokenType.PLUS); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '/': AddToken(TokenType.SLASH); break;
                case '*': AddToken(TokenType.STAR); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '|': AddToken(TokenType.PIPE); break;

                // One or two character tokens
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '@': 
                    if (Match('{'))
                    {
                        AddToken(TokenType.AT_LEFT_BRACE);
                    }
                    else 
                    {
                        throw new Exception($"Invalid Character: {c}");
                    }
                    break;
                case '#': ScanComment(); break;

                // Whitespace 
                case ' ':  break;
                case '\r': break;
                case '\t': break;

                // Newline and Leading Whitespace
                case '\n': ScanNewLine(); break;

                // Strings 
                // TODO handle string escape characters
                case '"': ScanString(); break;

                default:
                    if (IdentifierStart(c)) 
                    {
                        ScanIdentifier();
                    } 
                    else if (IsDigitOrDot(c))
                    {
                        ScanNumber(c);
                    }
                    else
                    {
                        throw new Exception($"Invalid Character: {c}");
                    }
                    break;
            }
        }

        // TODO allow identifiers with internal dots
        /* This scans something that looks initally like an identifier
           It can be either: a keyword - if it ends with a colon 
                             a reserved word - if it's in the 'reserved' dictionary 
                             an identifier - otherwise 
        */
        private void ScanIdentifier()
        {
            TokenType type;

            while (IdentifierInner(Peek)) Advance();

            string text = source.Substring(start, current - start);
            if (Match(':'))
            {
                type = TokenType.KEYWORD;
            } 
            else if (!reserved.TryGetValue(text, out type))
            {
                type = TokenType.IDENTIFIER;
            }
            AddToken(type, text);
        }

        // TODO support exponential notation 
        private void ScanNumber(char c)
        {
            bool isFloat = c == '.';
            while (IsDigitOrDot(Peek))
            {
                if (Peek == '.')
                {
                    if (!isFloat) isFloat = true;
                    else throw new Exception("Invalid Number literal");
                }
                Advance();
            }

            string text = source.Substring(start, current - start);
            if (!isFloat && Int64.TryParse(text, out long intNumber)) 
            {
                AddToken(TokenType.INTEGER, intNumber);
            }
            else if (Double.TryParse(text, out double floatNumber))
            {
                AddToken(TokenType.FLOAT, floatNumber);
            }
            else if (text == ".")
            {
                AddToken(TokenType.DOT);
            }
            else 
            {
                throw new Exception("Could not parse number");
            }
        }

        // Possibly make a IndentStack class, that takes a Token and returns if it's an indent or a dedent
        // This can then be reused in the parser class
        private void ScanNewLine() 
        {
            line++;
            // TODO prevent mixing spaces and tabs
            while (Peek == ' ' || Peek == '\t')
            {
                Advance();
            }

            if (IsNotAtEnd)
            {
                AddToken(TokenType.NEWLINE);
            }
        }

        private void ScanString() 
        {
            while (Peek != '"' && IsNotAtEnd)
            {
                if (Peek == '\n') line++;
                Advance();
            }

            if (IsNotAtEnd) 
            {
                // Advance past the closing '"'
                Advance();

                // Exclude the leading and ending quotes
                string text = source.Substring(start+1, current - start - 2);
                AddToken(TokenType.STRING, text);
            }
            else 
            {
                throw new Exception("Unterminated String Literal");
            }

        }

        private void ScanComment()
        {
            while (Peek != '\n' && IsNotAtEnd)
            {
                Advance();
            }

            AddToken(TokenType.COMMENT);
        }

        public List<Token> Lex()
        {
            while (IsNotAtEnd)
            {
                start = current;
                ScanToken();
            }
            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }
    }
}

