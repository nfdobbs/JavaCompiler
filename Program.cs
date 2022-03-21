
//Nathan Dobbs
//Lexical Analyzer
//Analyzes Java files and determines if text is valid java grammar, makes use of the LexicalAnalyzer class to identify tokens, has advanced symbol table with more data
//Available by adding Y or y after file on command line
//Now will also print intermediate code file to a .TAC of same name
//SDSU Compiler Construction

using System;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = "";
            bool advSym = false;

            //Setup//
            if (args.Length == 0)
            {
                Console.WriteLine("Please Specify Program File: ");
                fileName = Console.ReadLine();
            }

            else if (args.Length == 1)
            {
                Console.WriteLine("Program Using Program File: " + args[0]);
                Console.WriteLine();
                fileName = args[0];
            }

            else if(args.Length == 2)
            {
                Console.WriteLine("Program Using Program File: " + args[0]);
                if (args[1] == "y" || args[1] == "Y")
                {
                    advSym = true;
                   
                }
                Console.WriteLine();
                fileName = args[0];
            }

            else
                Environment.Exit(-2);

            if (advSym == false)
                Console.WriteLine("Program Printing Basic Symbol Table");
            else
                Console.WriteLine("Program Printing Advanced Symbol Table");

            Parser Pars = new Parser(fileName);
            Pars.advancedSymbolTable = advSym;
            Pars.Parse();

        }
        
        
    }
}
