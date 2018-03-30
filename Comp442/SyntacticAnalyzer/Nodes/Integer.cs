﻿using SyntacticAnalyzer.Semantics;
using System;

namespace SyntacticAnalyzer.Nodes
{
    [Serializable]
    public class Integer : Node, IVisitable
    {
        public string Value { set; get; }

        public Integer((int, int) location) : base(location)
        {
            this.SemanticalType = "int";
            this.IsLiteral = true;
        }

        public void Accept(Visitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}
