﻿using SyntacticAnalyzer.Parser;
using System;
using System.IO;
using System.Linq;

namespace Assignment2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            while (true) {
                Console.Write("Please enter a filename for input: ");
                string file = Console.ReadLine();

                if (!File.Exists(file)) {
                    Console.WriteLine("File does not exist.");
                    continue;
                }
                var parser = new Parser(File.ReadAllLines(file));

                bool valid = parser.Parse();
                
                Console.WriteLine($"Valid program: {parser.Verify()}");

                Console.WriteLine(parser.Derivations.Last().Derivation);
            }
        }
    }
}
