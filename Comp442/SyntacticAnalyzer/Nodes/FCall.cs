﻿using System;

namespace SyntacticAnalyzer.Nodes
{
    [Serializable]
    public class FCall
    {
        public string Id { get; set; }
        public AParams Parameters { get; set; }

        public override string ToString()
        {
            return $"{Id}({Parameters})";
        }
    }
}
