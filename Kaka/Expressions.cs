using System.Collections.Generic;

 namespace KakaExpressions
{

    public enum ExprType 
    {
        UNARY, BINARY, KEYWORD, LITERAL, IDENTIFIER,
    }

    public abstract record Node { public abstract R Accept<R>(Visitor<R> vistor); }

    // This is a helper record that holds the contents of a keyword argument declaration
    public record ArgumentsList(List<(Message Key, AbstractExpr Value)> pairs) : Node { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitArgumentsList(this); };

    // Defining the types of exressions: An expression is anything that can be evaluated 
    // to yeild a value  
    public abstract record AbstractExpr : Node;
    public record Expression(ExprType type, Expression expr) : AbstractExpr { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitExpression(this); };

    // The three message types
    public record Unary(Expression reciever, Message message) : AbstractExpr { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitUnary(this); };
    public record Binary(Expression reciever, Message message, Expression right) : AbstractExpr { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBinary(this); };
    public record Keyword(Expression reciever, List<(KeywordMessage, Expression)> keywordPairs) : AbstractExpr { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitKeyword(this); };

    public record Identifier(string Name) : AbstractExpr { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitIdentifier(this); };

    // Literals 
    public record Double(double Value) : AbstractExpr { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitDouble(this); };
    public record Integer(int Value) : AbstractExpr { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitInteger(this); };
    public record KString(string Value) : AbstractExpr { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitString(this); };
    public record KList(List<object> Value) : AbstractExpr { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitList(this); };
    public record KDict(Dictionary<object, object> Value) : AbstractExpr { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitDict(this); };
    public record Message(string message) : AbstractExpr { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitMessage(this); };
    public record KeywordMessage(string message) : AbstractExpr { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitKeywordMessage(this); };
    public record Block(List<Identifier> Args, List<Node> Code) : AbstractExpr { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBlock(this); };

    // Assignments may not have a body, in which case they are just declarations
    public record Assignment(Identifier Name, AbstractExpr Value) : Node { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitAssignment(this); };

    public record ClassDeclaration(
        Identifier Name,
        Identifier Inherit,
        Dictionary<Identifier, AbstractExpr> Fields,
        Dictionary<Identifier, MethodDef> Methods
    ) : Node { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitClassDeclaration(this); };

    public abstract record AbstractMethodDef : Node;
    public record MethodDef(ExprType type, AbstractMethodDef Method) : AbstractMethodDef { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitMethodDef(this); };

    public record UniaryMethod(Message Message, List<Expression> Code) : AbstractMethodDef { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitUnaryMethod(this); };
    public record BinaryMethod(Message Message, Identifier Right, List<Expression> Code) : AbstractMethodDef { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBinaryMethod(this); };
    public record KeywordMethod(List<Identifier> Args, List<Expression> Code) : AbstractMethodDef { public override R Accept<R>(Visitor<R> visitor) => visitor.VisitKeywordMethod(this); };

    public interface Visitor<R>
    {
        R VisitArgumentsList(ArgumentsList args);

        R VisitExpression(Expression expression);

        R VisitUnary(Unary unary);
        R VisitBinary(Binary binary);
        R VisitKeyword(Keyword keyword);

        R VisitIdentifier(Identifier identifier);

        R VisitDouble(Double kdouble);
        R VisitInteger(Integer integer);
        R VisitString(KString kstring);
        R VisitList(KList list);
        R VisitDict(KDict dict);
        R VisitMessage(Message message);
        R VisitKeywordMessage(KeywordMessage message);
        R VisitBlock(Block block);

        R VisitAssignment(Assignment assignment);
        
        R VisitClassDeclaration(ClassDeclaration classDec);

        R VisitMethodDef(MethodDef methodDef);
        R VisitUnaryMethod(UniaryMethod unaryMethod);
        R VisitBinaryMethod(BinaryMethod binaryMethod);
        R VisitKeywordMethod(KeywordMethod keywordMethod);
    }
}