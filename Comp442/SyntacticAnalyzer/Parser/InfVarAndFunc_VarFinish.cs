﻿using LexicalAnalyzer;

namespace SyntacticAnalyzer.Parser
{
    public partial class Parser
    {
        private bool InfVarAndFunc_VarFinish()
        {
            var lookaheadToken = this._tokenStream.Peek();
            string lookahead = AtoCC.Convert(lookaheadToken);

            if ("; [".HasToken(lookahead)) {
                if (InfArraySize() && Match(";") && InfVarAndFunc_VarStart()) {
                    return true;
                }
            }

            if ("(".HasToken(lookahead)) {
                if (InfVarAndFunc_FuncFinish()) {
                    return true;
                }
            }

            return false;
        }
    }
}