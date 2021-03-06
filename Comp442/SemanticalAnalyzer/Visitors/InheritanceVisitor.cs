﻿using Errors;
using SyntacticAnalyzer.Nodes;
using SyntacticAnalyzer.Semantics;
using System.Collections.Generic;

namespace SemanticalAnalyzer.Visitors
{
    public class InheritanceVisitor : Visitor
    {
        private Dictionary<string, ClassDecl> classes = new Dictionary<string, ClassDecl>();
        private SymbolTable GlobalScope;

        public InheritanceVisitor(SymbolTable globalScope)
        {
            this.GlobalScope = globalScope;
        }

        public override void Visit(Program node)
        {
            // Determines whether or not a class contains full implementation.
            Dictionary<string, object> Done = new Dictionary<string, object>();

            // This is done after we visited all the classes.
            foreach (var classEntry in classes.Values) {

                var visiting = new Stack<ClassDecl>();
                var visited = new Dictionary<string, object>();
                var implementation = new SymbolTable();
                var focus = classEntry;
                
                implementation.AddRange(classEntry.Table.GetAll(), classEntry.Location);

                do {
                    visited.Add(focus.ClassName, null);
                    foreach (var parent in focus.InheritingClasses.IDs) {
                        if (Done.ContainsKey(parent)) {
                            implementation.AddRange(classes[parent].Table.GetAll(), classEntry.Location, true);
                        } else {
                            if (visited.ContainsKey(parent)) {

                                ErrorManager.Add($"Circular dependancy deteced between {focus.ClassName} and {parent}.", focus.Location);
                                visiting.Clear();
                                break;
                            } else {
                                if (classes.ContainsKey(parent)) {
                                    implementation.AddRange(classes[parent].Table.GetAll(), classEntry.Location);
                                    visiting.Push(classes[parent]);
                                } else {
                                    ErrorManager.Add($"The class {parent} cannot be found or is not defined.", focus.Location);
                                }
                            }
                        }
                    }

                    if (visiting.Count > 0) {
                        focus = visiting.Pop();
                    } else {
                        focus = null;
                    }
                } while (focus != null);

                Done.Add(classEntry.ClassName, null);
                classEntry.Table = implementation;
            }

            foreach (var classEntry in GlobalScope.GetAll(Classification.Class)) {
                classEntry.Link = classes[classEntry.ID].Table;
            }
        }

        public override void Visit(ClassDecl classDecl)
        {
            if (classes.ContainsKey(classDecl.ClassName)) {
                ErrorManager.Add($"Multiple class declaration of class {classDecl.ClassName}.", classDecl.Location);
                return;
            }
            classes.Add(classDecl.ClassName, classDecl);
        }
    }
}
