﻿using SyntacticAnalyzer.Nodes;
using SyntacticAnalyzer.Semantics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGeneration.Visitors
{
    public class NewMoonVisitor : Visitor
    {
        private SymbolTable GlobalScope;
        private SymbolTable FunctionScope;
        private SymbolTable ClassInstanceScope;

        public NewMoonVisitor(SymbolTable globalScope)
        {
            this.GlobalScope = globalScope;

            // Reset the instructions.
            InstructionStream.Instructions.Clear();
            InstructionStream.Add(File.ReadAllLines("puts.m").Where(line => line.Trim().Length != 0).ToArray(), "Link the puts library");
        }

        private void Load(Node node, string register)
        {
            InstructionStream.Add($"lw {register}, {node.stackOffset}(r14)", $"Loading the value of {node}");

            if (!node.IsLiteral) {
                InstructionStream.Add($"lw {register}, 0({register})", $"Pointer detected. Dereferencing {node}");
            }
        }

        private void LoadAndStore(int sourceAddresss, int destinationAddress, int copySizeInBytes, bool derefSource = false, bool derefDest = false, string comment = "")
        {
            for (int i = 0; i < copySizeInBytes; i += 4) {
                InstructionStream.Add($"lw r1, {sourceAddresss + (i)}(r14)", $"{comment} - loading {i} of {copySizeInBytes}");
                if (derefSource) {
                    InstructionStream.Add("lw r1, 0(r1)", "Pointer detected. Dereferencing.");
                }
                if (derefDest) {
                    InstructionStream.Add($"lw r2, {destinationAddress}(r14)");
                    InstructionStream.Add($"sw {i}(r2), r1", "Pointer detected. Storing the value in the dereferenced location.");
                } else {
                    InstructionStream.Add($"sw {destinationAddress + (i)}(r14), r1", $"{comment} - storing {i} of {copySizeInBytes}");
                }
            }
        }

        private void LoadAndStore(Node source, Node destination, int sizeInWords, string comment = "")
        {
            this.LoadAndStore(source.stackOffset, destination.stackOffset, sizeInWords, !source.IsLiteral, !destination.IsLiteral, comment);
        }

        private void LoadAndStore(Node source, int destinationAddress, int sizeInWords, string comment = "")
        {
            this.LoadAndStore(source.stackOffset, destinationAddress, sizeInWords, !source.IsLiteral, false, comment);
        }

        private void LoadAndStore(int sourceAddress, Node destination, int sizeInWords, string comment = "")
        {
            this.LoadAndStore(sourceAddress, destination.stackOffset, sizeInWords, false, !destination.IsLiteral, comment);
        }



        public override void PreVisit(FuncDef funcDef)
        {
            this.FunctionScope = this.GlobalScope.Get(funcDef.Entry.ID, Classification.Function).Link;

            this.ClassInstanceScope = new SymbolTable();
            if (funcDef.ScopeResolution != null) {
                this.ClassInstanceScope = this.GlobalScope.Get(funcDef.ScopeResolution.ID.Replace(':', '_'), Classification.Class).Link;
            }

            InstructionStream.Add($"function_{funcDef.Entry.ID.Replace(':', '_')} sw 0(r14), r15", $"Store the return address.");
        }

        public override void Visit(FuncDef funcDef)
        {
            InstructionStream.Add(new string[] {
                "lw r15, 0(r14)",
                "jr r15"
            }, "Load the return address, and return.");
        }


        public override void PreVisit(MainStatBlock mainStatBlock)
        {
            this.FunctionScope = GlobalScope.Get("main", Classification.Function).Link;
            this.ClassInstanceScope = new SymbolTable();

            InstructionStream.Add("entry");
        }

        public override void Visit(MainStatBlock mainStatBlock)
        {
            InstructionStream.Add("hlt");

            int programSize = InstructionStream.Instructions.Count * 4;
            int indexToInsert = InstructionStream.Instructions.IndexOf("entry") + 1;
            InstructionStream.Instructions.Insert(indexToInsert, $"addi r14, r0, {programSize}  % Set the stack pointer");
        }


        public override void Visit(Integer integer)
        {
            var bytes = BitConverter.GetBytes(int.Parse(integer.Value));

            for (int i = 0; i < bytes.Length; i++) {
                InstructionStream.Add(new string[] {
                    $"addi r1, r0, {bytes[i]}",
                    $"sb {integer.stackOffset + i}(r14), r1"
                }, $"Storing {bytes[i]}");
            }
        }

        public override void Visit(Sign sign)
        {
            int factor = sign.SignSymbol == "-" ? -1 : 1;

            InstructionStream.Add(new string[] {
                $"lw r2, {sign.Factor.stackOffset}(r14)",
                $"muli r2, r2, {factor}",
                $"sw {sign.stackOffset}(r14), r2"
            }, $"Calculating {sign}");
        }

        public override void Visit(AddOp addOp)
        {
            string instruction = addOp.Operator == "+" ? "add" : "sub";

            this.Load(addOp.LHS, "r2");
            this.Load(addOp.RHS, "r3");

            InstructionStream.Add(new string[] {
                $"{instruction} r1, r2, r3",
                $"sw {addOp.stackOffset}(r14), r1"
            }, $"Calculating {addOp.ToString()}");
        }

        public override void Visit(MultOp multOp)
        {
            string instruction = multOp.Operator == "*" ? "mul" : "div";

            this.Load(multOp.LHS, "r2");
            this.Load(multOp.RHS, "r3");

            InstructionStream.Add(new string[] {
                $"{instruction} r1, r2, r3",
                $"sw {multOp.stackOffset}(r14), r1"
            }, $"Calculating {multOp.ToString()}");
        }


        public override void PreVisit(Var var)
        {
            string lastDatatype = string.Empty;

            for (int i = 0; i < var.Elements.Count; i++) {
                var element = var.Elements[i];
                if (element is DataMember member) {
                    lastDatatype = member.SemanticalType;

                    this.SubVisit(member);
                }

                if (element is FCall call) {
                    if (lastDatatype.Length != 0) {
                        call.Id = lastDatatype + "::" + call.Id;
                    }

                    lastDatatype = call.SemanticalType;


                    if (lastDatatype.Length != 0) {
                        InstructionStream.Add($"addi r7, r14, 0", "This is to calculate the data offset for any potential member functions.");

                        for (int x = 0; x < i; x++) {
                            if (var.Elements[x] is DataMember calcMem) {
                                InstructionStream.Add($"lw r8, {calcMem.stackOffset}(r14)", "This is to calculate the next data offset AFTER a data member");
                                InstructionStream.Add($"add r7, r7, r8");
                            }
                            if (var.Elements[x] is FCall calcFunc) {
                                InstructionStream.Add($"addi r7, r14, {calcFunc.stackOffset}", "This is to calculate the next data offset AFTER a function call.");
                            }
                        }

                        for (int x = 0; x < call.MemberMemorySize; x += 4) {
                            InstructionStream.Add(new string[] {
                                $"lw r1, {x}(r7)",
                                $"sw {this.FunctionScope.GetStackFrameSize() + 4 + call.NodeMemorySize + x}(r14), r1"
                            });
                        }
                    }


                    this.SubVisit(call);

                    // Check if we need to retrieve the member
                    if (lastDatatype.Length != 0) {
                        InstructionStream.Add($"addi r7, r14, 0", "This is to calculate the data offset for any potential member functions.");

                        for (int x = 0; x < i; x++) {
                            if (var.Elements[x] is DataMember calcMem) {
                                InstructionStream.Add($"lw r8, {calcMem.stackOffset}(r14)", "This is to calculate the next data offset AFTER a data member");
                                InstructionStream.Add($"add r7, r7, r8");
                            }
                            if (var.Elements[x] is FCall calcFunc) {
                                InstructionStream.Add($"addi r7, r14, {calcFunc.stackOffset}", "This is to calculate the next data offset AFTER a function call.");
                            }
                        }

                        for (int y = 0; y < call.MemberMemorySize; y += 4) {
                            InstructionStream.Add(new string[] {
                                $"lw r1, {this.FunctionScope.GetStackFrameSize() + 4 + call.NodeMemorySize + y}(r14)",
                                $"sw {y}(r7), r1"
                            });
                        }
                    }

                }
            }

            
        }

        public void SubVisit(FCall fCall)
        {
            int stackFrameSize = this.FunctionScope.GetStackFrameSize();
            var scope = this.GlobalScope.Get(fCall.Id, Classification.Function).Link;
            int paramOffset = stackFrameSize + 4 + scope.Get("retval", Classification.SubCalculationStackSpace).EntryMemorySize;

            foreach (var expression in fCall.Parameters.Expressions) {
                this.LoadAndStore(expression, paramOffset, expression.NodeMemorySize, $"Passing parameter {expression}");
                paramOffset += expression.NodeMemorySize;
            }

            // PASS THE "THIS" VALUE??
            // Whatever's in R7 is what we're calling from.
            


            InstructionStream.Add($"addi r14, r14, {stackFrameSize}");
            InstructionStream.Add($"jl r15, function_{fCall.Id.Replace(':', '_')}", $"Call the function {fCall.Id}");
            InstructionStream.Add($"subi r14, r14, {stackFrameSize}");


            this.LoadAndStore(stackFrameSize + 4, fCall, fCall.NodeMemorySize, "Retrieve the returnvalue");
        }

        public void SubVisit(DataMember dataMember)
        {
            InstructionStream.Add($"addi r1, r0, {dataMember.baseOffset}", $"Start to calculate the offset for {dataMember}");

            if (dataMember?.Indexes?.Expressions != null) {
                var maxIndexes = dataMember.MaxSizeDimensions;
                
                for (int i = 0; i < dataMember.Indexes.Expressions.Count; i++) {
                    var exp = dataMember.Indexes.Expressions[i];

                    InstructionStream.Add($"lw r2, {exp.stackOffset}(r14)", $"Getting the index [{exp}]");

                    if (!exp.IsLiteral) {
                        InstructionStream.Add($"lw r2, 0(r2)", "Pointer deteced - dereferencing");
                    }

                    InstructionStream.Add($"muli r2, r2, {dataMember.NodeMemorySize}", $"Multiply by the size.");

                    for (int sizeIndex = i + 1; sizeIndex < dataMember.Indexes.Expressions.Count; sizeIndex++) {
                        InstructionStream.Add($"muli r2, r2, {maxIndexes[sizeIndex]}", $"Multiply by the chunk size {maxIndexes[sizeIndex]}");
                    }

                    InstructionStream.Add($"add r1, r1, r2", $"Add the index {dataMember.Indexes.Expressions[i]}");
                }
            }

            for (int i = 0; i < dataMember.NodeMemorySize; i += 4) {
                if (i != 0) {
                    InstructionStream.Add($"addi r1, r1, 4");
                }
                InstructionStream.Add($"sw {dataMember.stackOffset + i}(r14), r1");
            }
        }

        public override void Visit(Var var)
        {
            InstructionStream.Add($"addi r1, r14, 0", $"Calculating the REAL offset for {var}");

            foreach (var entry in var.Elements) {
                if (entry is DataMember member) {
                    InstructionStream.Add(new string[] {
                        $"lw r2, {member.stackOffset}(r14)",
                        $"add r1, r1, r2"
                    });
                }

                if (entry is FCall call) {
                    InstructionStream.Add(new string[] {
                        $"addi r1, r14, {call.stackOffset}"
                    }, "Get the function call's pointer");
                }
            }

            if (var.Elements.Last() as DataMember != null) {
                for (int i = 0; i < var.NodeMemorySize; i += 4) {
                    if (i != 0) {
                        InstructionStream.Add($"addi r1, r1, 4");
                    }
                    InstructionStream.Add($"sw {var.stackOffset + i}(r14), r1");
                }
                var.IsLiteral = false;
            } else {
                for (int i = 0; i < var.NodeMemorySize; i += 4) {
                    InstructionStream.Add(new string[] {
                        $"lw r2, {var.Elements.Last().stackOffset + i}(r14)",
                        $"sw {var.stackOffset + i}(r14), r2"
                    });
                }
                var.IsLiteral = true;
            }
        }



        public override void Visit(RelExpr relExpr)
        {
            string instruction = relExpr.RelationOperator;

            switch (relExpr.RelationOperator) {
                case "==":
                    instruction = "ceq";
                    break;
                case "<>":
                    instruction = "cne";
                    break;
                case "<":
                    instruction = "clt";
                    break;
                case ">":
                    instruction = "cgt";
                    break;
                case "<=":
                    instruction = "cle";
                    break;
                case ">=":
                    instruction = "cge";
                    break;
            }

            this.Load(relExpr.LHS, "r2");
            this.Load(relExpr.RHS, "r3");
            InstructionStream.Add(new string[] {
                $"{instruction} r1, r2, r3",
                $"sw {relExpr.stackOffset}(r14), r1"
            }, $"Evaluate {relExpr}");
        }

        public override void Visit(Not not)
        {
            this.Load(not.Factor, "r1");
            InstructionStream.Add(new string[] {
                "not r1, r1",
                $"sw {not.stackOffset}(r14), r1"
            }, $"Invert the sign");
        }



        public override void PostConditionalVisit(IfStat ifStat)
        {
            string elseIdentifier = $"else_{ifStat.Location.line}_{ifStat.Location.column}";

            this.Load(ifStat.Condition, "r1");
            InstructionStream.Add($"bz r1, {elseIdentifier}", $"If {ifStat.Condition}.");
        }

        public override void PreElseBlockVisit(IfStat ifStat)
        {
            string elseIdentifier = $"else_{ifStat.Location.line}_{ifStat.Location.column}";
            string endifIdentifier = $"endif_{ifStat.Location.line}_{ifStat.Location.column}";

            InstructionStream.Add($"j {endifIdentifier}", "Go to the end of the else block.");
            InstructionStream.Add($"{elseIdentifier}    nop", "Start the else block");
        }

        public override void Visit(IfStat ifStat)
        {
            string endifIdentifier = $"endif_{ifStat.Location.line}_{ifStat.Location.column}";
            InstructionStream.Add($"{endifIdentifier}    nop", "End the else block");
        }

        public override void PostInitializationVisit(ForStat forStat)
        {
            string startForIdentifier = $"for_{forStat.Location.line}_{forStat.Location.column}";
            string conditionIdentifier = $"forcond_{forStat.Location.line}_{forStat.Location.column}";

            this.LoadAndStore(forStat.Initialization, forStat.stackOffset, forStat.NodeMemorySize, $"{forStat.Type} {forStat.Id} = {forStat.Initialization}");

            InstructionStream.Add($"j {conditionIdentifier}");
            InstructionStream.Add($"{startForIdentifier}    nop", "Start the for loop");
        }

        public override void PostUpdateVisit(ForStat forStat)
        {
            string conditionIdentifier = $"forcond_{forStat.Location.line}_{forStat.Location.column}";

            InstructionStream.Add($"{conditionIdentifier}   nop");
        }

        public override void PostForLoopConditionalVisit(ForStat forStat)
        {
            string endForIdentifier = $"endfor_{forStat.Location.line}_{forStat.Location.column}";
            
            this.Load(forStat.Condition, "r1");
            InstructionStream.Add($"bz r1, {endForIdentifier}");
        }

        public override void Visit(ForStat forStat)
        {
            string startForIdentifier = $"for_{forStat.Location.line}_{forStat.Location.column}";
            string endForIdentifier = $"endfor_{forStat.Location.line}_{forStat.Location.column}";

            InstructionStream.Add(new string[] {
                $"j {startForIdentifier}",
                $"{endForIdentifier}    nop"
            });
        }



        public override void Visit(ReturnStat returnStat)
        {
            this.LoadAndStore(returnStat.ReturnValueExpression, 4, returnStat.ReturnValueExpression.NodeMemorySize, $"Returning {returnStat.ReturnValueExpression}");

            InstructionStream.Add(new string[] {
                "lw r15, 0(r14)",
                "jr r15"
            }, "Load the return address, and return.");
        }

        public override void Visit(PutStat putStat)
        {
            int thisFrameSize = this.FunctionScope.GetStackFrameSize();

            this.LoadAndStore(putStat.Expression, thisFrameSize + 4, putStat.Expression.NodeMemorySize, $"Store the put value of {putStat.Expression}");
            InstructionStream.Add($"addi r14, r14, {thisFrameSize}");
            InstructionStream.Add($"jl r15, puti_func");
            InstructionStream.Add($"subi r14, r14, {thisFrameSize}");
        }

        public override void Visit(AssignStat assignStat)
        {
            this.LoadAndStore(assignStat.ExpressionValue, assignStat.Variable, assignStat.ExpressionValue.NodeMemorySize, $"{assignStat}");
        }
    }
}
