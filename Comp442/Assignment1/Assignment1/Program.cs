﻿using LexicalAnalyzer;
using System;
using System.Collections.Generic;
using System.IO;

namespace Assignment1
{
    public class Program
    {
        private const string _inputFile = "input.txt";

        static Program()
        {
            if (!File.Exists(_inputFile)) {
                File.Create(_inputFile).Close();
            }
        }

        private static void Main(string[] args)
        {
            while (true) {
                Console.Write("Please enter a filename for input: ");
                string file = Console.ReadLine();

                if (!File.Exists(file)) {
                    Console.WriteLine("File does not exist.");
                    continue;
                }

                Console.WriteLine();
                var tokenizer = new Tokenizer(File.ReadAllText(file).Replace('\r', '\0'));

                var tokens = new List<Token>();

                Token token;
                do {
                    token = tokenizer.NextToken();
                    tokens.Add(token);
                    Console.Write($"[{token.Type}:{token.TokenContent.Replace("\n", "\\n")}:{token.SourceLocation}] ");
                } while (token.Type != TokenType.EndOfStream);
                Console.WriteLine("\n");

                tokenizer.Dispose(new FileInfo(file).Name.Replace(".", "_report."));

                File.WriteAllText(new FileInfo(file).Name.Replace(".", "_AtoCC."), new AtoCC().Convert(tokens.ToArray()));
            }
        }
    }

}
