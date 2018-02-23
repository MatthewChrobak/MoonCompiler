﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SyntacticAnalyzer.Nodes
{
    [Serializable]
    public class StatBlock
    {
        [XmlArrayItem(type:typeof(VarDecl), elementName:"VarDecl")]
        [XmlArrayItem(type:typeof(AssignStat), elementName:"Assignment")]
        [XmlArrayItem(type:typeof(IfStat), elementName:"If")]
        [XmlArrayItem(type:typeof(ForStat), elementName:"For")]
        [XmlArrayItem(type:typeof(GetStat), elementName:"Get")]
        [XmlArrayItem(type: typeof(PutStat), elementName: "Put")]
        [XmlArrayItem(type: typeof(ReturnStat), elementName: "Return")]
        public List<object> Statements { get; set; } = new List<object>();

        public override string ToString()
        {
            return "{\n" + String.Join(string.Empty, Statements.Select(val => val + "\n")) + "\n}";
        }
    }
}
