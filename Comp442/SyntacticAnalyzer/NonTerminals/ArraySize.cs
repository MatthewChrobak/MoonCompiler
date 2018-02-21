﻿namespace SyntacticAnalyzer.Parser
{
    public partial class Parser
    {
        private bool ArraySize()
        {
            string first = "[";
            this.SkipErrors(first);

            var lookaheadToken = this.TokenStream.Peek();
            string lookahead = lookaheadToken.AToCCFormat();

            if (first.HasToken(lookahead)) {
                this.ApplyDerivation("arraySize -> '[' 'intNum' ']'");
                if (Match("[") & Match("intNum") & Match("]")) {
                    return true;
                }
            }
            
            return false;
        }
    }
}
