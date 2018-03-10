﻿using Errors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalyzer.Semantics
{
    public class SymbolTable
    {
        private Dictionary<string, TableEntry> _entries { get; set; }

        public SymbolTable()
        {
            this._entries = new Dictionary<string, TableEntry>();
        }

        public void Add(TableEntry value, bool overrideEntry = false)
        {
            if (value == null) {
                return;
            }

            string key = value.ToString();

            if (this._entries.ContainsKey(key)) {
                if (overrideEntry) {
                    return;
                }
                ErrorManager.Add($"The {value.Classification} {value.ID} is already defined in this scope.", (0, 0));
                return;
            }

            this._entries.Add(key, value);
        }

        public void AddRange(IEnumerable<TableEntry> entries, bool overrideEntries = false)
        {
            if (entries == null) {
                return;
            }
            foreach (var entry in entries) {
                this.Add(entry, overrideEntries);
            }
        }

        public TableEntry Get(string key)
        {
            if (this._entries.ContainsKey(key)) {
                return this._entries[key];
            }
            Console.WriteLine("Could not find " + key);
            return null;
        }

        public IEnumerable<TableEntry> GetAll(Classification? type = null)
        {
            if (type == null) {
                return this._entries.Values;
            }
            return this._entries.Where(val => val.Value.Classification == type).Select(val => val.Value);
        }
    }
}