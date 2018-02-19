﻿using LexicalAnalyzer;

namespace SyntacticAnalyzer.Parser
{
    public partial class Parser
    {
        private bool StatBlock()
        {
            var lookaheadToken = this._tokenStream.Peek();
            string lookahead = lookaheadToken.AToCC();

            if ("{".HasToken(lookahead)) {
                this.ApplyDerivation("statBlock -> '{' infStatement '}'");
                if (Match("{") && InfStatement() && Match("}")) {
                    return true;
                }
            }

            if ("id if for get put return".HasToken(lookahead)) {
                this.ApplyDerivation("statBlock -> statement");
                if (Statement()) {
                    return true;
                }
            }

            if ("else ;".HasToken(lookahead)) {
                this.ApplyDerivation("statBlock -> EPSILON");
                return true;
            }

            return false;
        }
    }
}
