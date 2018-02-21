﻿namespace SyntacticAnalyzer.Parser
{
    public partial class Parser
    {
        private bool Type()
        {
            string first = "id int float";
            this.SkipErrors(first);

            var lookaheadToken = this.TokenStream.Peek();
            string lookahead = lookaheadToken.AToCCFormat();

            if ("id".HasToken(lookahead)) {
                this.ApplyDerivation("type -> 'id'");
                if (Match("id")) {
                    return true;
                }
            }

            if ("int float".HasToken(lookahead)) {
                this.ApplyDerivation("type -> type_NoID");
                if (Type_NoID()) {
                    return true;
                }
            }

            return false;
        }
    }
}
