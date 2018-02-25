﻿using System;
using System.Xml.Serialization;

namespace SyntacticAnalyzer.Nodes
{
    // TODO: XmlInclude all of that in Factor
    [Serializable]
    public class MultOp
    {
        [XmlElement(type: typeof(AddOp), elementName: "LeftAddOp")] // arithExpr
        [XmlElement(type: typeof(RelExpr), elementName: "LeftRelationalExpression")] // expr
        [XmlElement(type: typeof(MultOp), elementName: "LeftMultOp")] // term
        [XmlElement(type: typeof(Var), elementName: "LeftVariable")] // factor
        [XmlElement(type: typeof(string), elementName: "LeftNumber")] // factor
        [XmlElement(type: typeof(FCall), elementName: "LeftFunctionCall")] // factor
        [XmlElement(type: typeof(Not), elementName: "LeftNotFactor")] // factor
        [XmlElement(type: typeof(Sign), elementName: "LeftSignFactor")] // factor
        public object LHS { get; set; } // factor

        public string Operator { get; set; }

        [XmlElement(type: typeof(AddOp), elementName: "RightAddOp")] // arithExpr
        [XmlElement(type: typeof(RelExpr), elementName: "RightRelationalExpression")] // expr
        [XmlElement(type: typeof(MultOp), elementName: "RightMultOp")] // term
        [XmlElement(type: typeof(Var), elementName: "RightVariable")] // factor
        [XmlElement(type: typeof(string), elementName: "RightNumber")] // factor
        [XmlElement(type: typeof(FCall), elementName: "RightFunctionCall")] // factor
        [XmlElement(type: typeof(Not), elementName: "RightNotFactor")] // factor
        [XmlElement(type: typeof(Sign), elementName: "RightSignFactor")] // factor
        public object RHS { get; set; } // factor

        public override string ToString()
        {
            return $"({LHS} {Operator} {RHS})";
        }
    }
}
