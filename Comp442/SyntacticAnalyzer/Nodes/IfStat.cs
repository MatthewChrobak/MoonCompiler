﻿using SyntacticAnalyzer.Semantics;
using System;
using System.Xml.Serialization;

namespace SyntacticAnalyzer.Nodes
{
    [Serializable]
    public class IfStat : Node, IVisitable
    {
        [XmlElement(type: typeof(AddOp), elementName: "AddOp")] // arithExpr
        [XmlElement(type: typeof(RelExpr), elementName: "RelationalExpression")] // expr
        [XmlElement(type: typeof(MultOp), elementName: "MultOp")] // term
        [XmlElement(type: typeof(Var), elementName: "Variable")] // factor
        [XmlElement(type: typeof(Integer), elementName: "Integer")] // factor
        [XmlElement(type: typeof(Float), elementName: "Float")] // factor
        [XmlElement(type: typeof(FCall), elementName: "FunctionCall")] // factor
        [XmlElement(type: typeof(Not), elementName: "NotFactor")] // factor
        [XmlElement(type: typeof(Sign), elementName: "SignFactor")] // factor
        public Node Condition { get; set; }
        public StatBlock TrueBlock { get; set; }
        public StatBlock ElseBlock { get; set; }

        // Just used for serialization.
        public IfStat() : base((-1, -1))
        {

        }

        public IfStat((int, int) location) : base(location)
        {
        }

        public void Accept(Visitor visitor)
        {
            visitor.PreVisit(this);

            if (this.Condition is IVisitable condition) {
                condition?.Accept(visitor);
            }
            visitor.PostConditionalVisit(this);
            this.TrueBlock?.Accept(visitor);

            visitor.PreElseBlockVisit(this);

            this.ElseBlock?.Accept(visitor);

            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"if ({Condition}) then {TrueBlock} else {ElseBlock}";
        }
    }
}
