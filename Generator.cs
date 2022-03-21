//Nathan Dobbs
//Compiler Construction
//This generates assembly from a provided TAC

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Parser
{
    class Generator
    {
        private string[] code;

        private string finalCode = "";
        private string header = "";
        private string fileN = "";
        SymTab symTable;

        public Generator(string fileName, SymTab symTab)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File " + fileName + " not found program will now exit");
                Environment.Exit(-1);
            }

            fileN = fileName;
            code = File.ReadAllLines(fileName);
            symTable = symTab;
        }

        public void generate()
        {
            header += ".model small\n";
            header += ".stack 100h\n";
            header += ".data\n";

            //Doing Final Code
            finalCode += "start PROC\n";
            finalCode += "\tmov ax, @data\n";
            finalCode += "\tmov ds, ax\n";
            finalCode += "\tcall main\n";
            finalCode += "\tmov ah, 04ch\n";
            finalCode += "\tmov al,0\n";
            finalCode += "\tint 21h\n";
            finalCode += "start ENDP\n";
            
            

            for(int i = 0; i < code.Length; i++)
            {
                lineHandle(code[i]);
            }

            header += ".code\n";
            header += "include io.asm\n\n";

            finalCode += "END start";
            finalCode = header + finalCode;
            //Console.WriteLine(finalCode);

            File.WriteAllText(fileN.Remove(fileN.Length - 4) + ".ASM", finalCode);
        }

        public void lineHandle(string line)
        {
           string[] words = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            //Dealing with Writing Strings
            if (line.StartsWith("wrs"))
            {
                finalCode += "\tmov dx,offset " + words[words.Length - 1] + "\n";
                header += words[words.Length - 1] + " DB " + symTable.LookUp(words[words.Length - 1]).literal + ",\"$\"\n";
                finalCode += "\tcall writestr\n";
            }

            //Dealing with Writing Ints
            else if (line.StartsWith("wri"))
            {
                finalCode += "\tmov dx," + bracketer(words[words.Length - 1]) + "\n";
                finalCode += "\tcall writeint\n";
            }
            //Dealing with Writing NewLines
            else if (line.StartsWith("wrln"))
            {
                finalCode += "\tcall writeln\n";
            }

            //Dealing with Reading Ints
            else if (line.StartsWith("rdi"))
            {
                finalCode += "\tcall readint\n";
                finalCode += "\tmov " + bracketer(words[words.Length - 1]) + ",bx\n";
            }

            //Dealing with Proc
            else if (line.StartsWith("Proc"))
            {
                finalCode += "\n" + words[words.Length - 1] + " PROC\n";
                if (words[words.Length - 1] != "main")
                {
                    finalCode += "\tpush bp\n";
                    finalCode += "\tmov bp,sp\n";
                    finalCode += "\tsub sp," + symTable.LookUp(words[words.Length - 1]).Union.function.sizeOfLocal.ToString() + "\n";
                }
            }

            else if (line.StartsWith("Endp"))
            {
                if (words[words.Length - 1] != "main")
                {
                    finalCode += "\tadd sp," + symTable.LookUp(words[words.Length - 1]).Union.function.sizeOfLocal.ToString() + "\n";
                    finalCode += "\tpop bp\n";
                    finalCode += "\tret " + symTable.LookUp(words[words.Length - 1]).Union.function.sizeOfParameters.ToString() + "\n";
                }
                else
                    finalCode += "\tret\n";

                finalCode += words[words.Length - 1] + " ENDP\n";

            }

            //Dealing With returning _AX
            else if (line.StartsWith("_AX"))
            {
                finalCode += "\tmov ax," + bracketer(words[words.Length - 1]) + "\n";
            }

            else if(line.StartsWith("call"))
            {
                finalCode += "\t" + line + "\n";
            }

            else if(line.StartsWith("push"))
            {
                finalCode += "\tpush " + bracketer(words[words.Length - 1]) + "\n";
            }

            else if(line.StartsWith("_"))
            {
                for(int i = 0; i<words.Length; i++)
                {
                    if (words[i] == "+")
                    {
                        finalCode += "\tmov ax," + bracketer(words[i - 1]) + "\n";
                        finalCode += "\tadd ax," + bracketer(words[i + 1]) + "\n";
                        finalCode += "\tmov " + bracketer(words[0]) + ",ax\n";
                        return;
                    }

                    else if(words[i] == "-")
                    {
                        finalCode += "\tmov ax," + bracketer(words[i - 1]) + "\n";
                        finalCode += "\tadd ax," + bracketer(words[i + 1]) + "\n";
                        finalCode += "\tmov ax," + bracketer(words[0]) + "\n";
                        return;
                    }

                    else if(words[i] == "*")
                    {
                        finalCode += "\tmov ax," + bracketer(words[i - 1]) + "\n";
                        finalCode += "\tmov bx," + bracketer(words[i + 1]) + "\n";
                        finalCode += "\timul bx\n";
                        finalCode += "\tmov " + bracketer(words[0]) + ",ax\n";
                        return;
                    }

                    else if(words[i] == "/")
                    {
                        finalCode += "\tmov ax," + bracketer(words[i - 1]) + "\n";
                        finalCode += "\tmov bx," + bracketer(words[i + 1]) + "\n";
                        finalCode += "\tidiv bx\n";
                        finalCode += "\tmov " + bracketer(words[0]) + ",ax\n";
                        return;
                    }
                }


                if (words[1] == "=" && words.Length == 3)
                {
                    finalCode += "\tmov ax," + bracketer(words[2]) + "\n";
                    finalCode += "\tmov " + bracketer(words[0]) + ",ax\n"; 
                }

            }
        }

        public string bracketer(string index)
        {
            if (index.StartsWith("_") == true)
            {
                index = index.Remove(0, 1);
                index = index.Insert(0, "[");
                index = index.Insert(index.Length, "]");
                return index;
            }

            else
                return index;

        }
    }
}
