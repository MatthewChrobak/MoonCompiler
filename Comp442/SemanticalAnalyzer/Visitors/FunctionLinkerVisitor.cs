﻿using Errors;
using SyntacticAnalyzer.Nodes;
using SyntacticAnalyzer.Semantics;
using System.Linq;

namespace SemanticalAnalyzer.Visitors
{
    public class FunctionLinkerVisitor : Visitor
    {
        private SymbolTable GlobalScope;

        public FunctionLinkerVisitor(SymbolTable globalScope)
        {
            this.GlobalScope = globalScope;
        }

        public override void Visit(FuncDef funcDef)
        {
            // We don't need to handle free-functions.
            if (funcDef.ScopeResolution == null) {
                return;
            }
            
            // Make sure the class exists.
            var classScope = this.GlobalScope.Get($"{funcDef.ScopeResolution.ID}-{Classification.Class}");
            if (classScope != null) {

                // Make sure the function exists.
                var function = classScope.Link.Get($"{funcDef.FunctionName}-{Classification.Function}");
                if (function != null) {

                    // Get the return type and the parameters.
                    var types = function.Type.Split('-');
                    var returnType = types[0];
                    var parameters = types[1].Split(',');

                    // Make sure the return type matches with the number of parameters
                    if (funcDef.ReturnType == returnType) {
                        if (funcDef.Parameters.Count == parameters.Count()) {

                            bool valid = true;

                            // Make sure each parameter matches.
                            for (int i = 0; i < funcDef.Parameters.Count; i++) {

                                string expectedType = funcDef.Parameters[i].Type;
                                string receivedType = parameters[i].Replace("[]", string.Empty);

                                // Apply pseudo type checking.
                                if (expectedType != receivedType) {
                                    //ErrorManager.Add($"Invalid parameter type: Expected {expectedType}, got {receivedType}");
                                    valid = false;
                                    break;
                                }

                                if (funcDef.Parameters[i].Dimensions.Count != parameters[i].Count(val => val == '[')) {

                                    // Invalid parameter.
                                    valid = false;
                                    break;
                                }
                            }

                            if (valid) {
                                function.Link = funcDef.Entry.Link;
                            }
                        }
                    }
                }
            }
        }
    }
}
