﻿namespace SyntacticAnalyzer.Parser
{
    public partial class Parser
    {
        private bool FuncBody()
        {
            string first = "{";
            this.SkipErrors(first);

            var lookaheadToken = this.TokenStream.Peek();
            string lookahead = lookaheadToken.AToCCFormat();

            if (first.HasToken(lookahead)) {
                this.ApplyDerivation("funcBody -> '{' infVarAndState '}'");

                Match("{");
                InfVarAndState();
                Match("}");
            }

            return false;
        }
    }
}
