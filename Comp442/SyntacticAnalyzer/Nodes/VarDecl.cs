﻿namespace SyntacticAnalyzer.Nodes
{
    public class VarDecl
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public DimList Dimentions { get; set; }
    }
}
