﻿namespace SyntacticAnalyzer.Parser
{
    public partial class Parser
    {
        private bool FuncBody()
        {
            var lookaheadToken = this._tokenStream.Peek();
            string lookahead = lookaheadToken.AToCCFormat();

            if ("{".HasToken(lookahead)) {
                this.ApplyDerivation("funcBody -> '{' infVarAndState '}'");
                if (Match("{") && InfVarAndState() && Match("}")) {
                    return true;
                }
            }

            return false;
        }
    }
}