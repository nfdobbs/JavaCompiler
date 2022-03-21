//Nathan Dobbs
//Parser Class
//Also prints intermediate code to .TAC and then create assembly language from the TAC

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Parser
{
    class Parser
    {
        //Parser Settings
        public bool advancedSymbolTable = false;


        //Output
        public string fileOutput;
        public string tempOutput = "";
        public string urnaryOutput = "";
        public string paramOutput = "";
        public string lastTemp = "";

        private Tokens savedType = Tokens.UNKNOWN_TOKEN;
        private Tokens pastToken = Tokens.UNKNOWN_TOKEN;
        private string pastLexeme = "";
        private int depth = 1;
        private SymTab symTab = new SymTab();
        private SymTab symTab2 = new SymTab();

        private LexicalAnalyzer Lexi;
        private Tokens token;

        private int offset = -2;

        private int tempCount;
        private string filesName = "";

        private int literalCount = 0;
        private string exprReturn = "0";

        private Generator gen;


        //Function Called to Start Parse
        public void Parse()
        {
            Prog();
            Console.WriteLine("\nFile Parse Successful");
        }

        //Constructor for Parser
        public Parser(string fileName)
        {
            filesName = fileName;
            Lexi = new LexicalAnalyzer(fileName);
        }

        private bool Match(Tokens desiredToken)
        {
            if (token == desiredToken)
            { 
                pastToken = token;
                pastLexeme = Lexi.lexeme;
                token = Lexi.GetNextToken();
                return true;
            }
            else
                return false;
        }

        //Private Functions Below Implement Java Grammar
        private void Prog()
        {
            token = Lexi.GetNextToken();
            MoreClasses();
            MainClass();

            if (Match(Tokens.END_OF_FILE_TOKEN) == false)
                ErrorMessage(Tokens.END_OF_FILE_TOKEN);

            //Showing depth 1
            symTab.WriteTable(depth, advancedSymbolTable);

            //Writing to File
            Console.WriteLine("Intermediate Code wrote to: " + filesName.Remove(filesName.Length - 5) + ".TAC");
            File.WriteAllText(filesName.Remove(filesName.Length - 5) + ".TAC", fileOutput);

            Console.WriteLine("=====================================");
            Console.WriteLine("                 TAC                 ");
            Console.WriteLine("=====================================\n");
            Console.WriteLine(fileOutput);
            Console.WriteLine("=====================================");
            Console.WriteLine("               END TAC               ");
            Console.WriteLine("=====================================");

            gen = new Generator(filesName.Remove(filesName.Length - 5) + ".TAC", symTab2);
            gen.generate();

            Console.WriteLine("Deleting TAC\n");
            Console.WriteLine("Final Code At: " + filesName.Remove(filesName.Length - 5) + ".ASM");
            File.Delete(filesName.Remove(filesName.Length - 5) + ".TAC");

        }

        private void MainClass()
        {
            TableEntry savedClass = new TableEntry();
            if (Match(Tokens.FINAL_TOKEN) == false)
                ErrorMessage(Tokens.FINAL_TOKEN);

            if (Match(Tokens.CLASS_TOKEN) == false)
                ErrorMessage(Tokens.CLASS_TOKEN);

            if (Match(Tokens.IDENTIFIER_TOKEN) == false)
                ErrorMessage(Tokens.IDENTIFIER_TOKEN);

            //Adding Main Class to Symbol Table
            if (symTab.LookUp(pastLexeme) == null || symTab.LookUp(pastLexeme).depth != depth)
            {
                symTab.Insert(pastLexeme, pastToken, depth);
                savedClass = symTab.LookUp(pastLexeme);
                savedClass.typeOfEntry = EntryType.CLASS;
            }

            else
            {
                Console.WriteLine("Error Line-" + Lexi.lineNum + ": \"" + pastLexeme + "\" already declared");
                Environment.Exit(-3);
            }

            if (Match(Tokens.LEFT_BRACE_TOKEN) == false)
                ErrorMessage(Tokens.LEFT_BRACE_TOKEN);

            if (Match(Tokens.PUBLIC_TOKEN) == false)
                ErrorMessage(Tokens.PUBLIC_TOKEN);

            if (Match(Tokens.STATIC_TOKEN) == false)
                ErrorMessage(Tokens.STATIC_TOKEN);

            if (Match(Tokens.VOID_TOKEN) == false)
                ErrorMessage(Tokens.VOID_TOKEN);

            if (Match(Tokens.MAIN_TOKEN) == false)
                ErrorMessage(Tokens.MAIN_TOKEN);

            depth++;
            //Adding Class to Symbol Table
            if (symTab.LookUp("main") == null || symTab.LookUp("main").depth != depth)
            {
                symTab.Insert("main", pastToken, depth);
                symTab.LookUp("main").typeOfEntry = EntryType.FUNCTION;
                symTab.LookUp("main").Union.function.ReturnType = VariableType.VOID_TYPE;
                symTab.LookUp("main").Union.function.sizeOfParameters = 2;
                symTab.LookUp("main").Union.function.sizeOfLocal = 0;
                savedClass.classMethodList.AddFirst("main");
            }

            else
            {
                Console.WriteLine("Error Line-" + Lexi.lineNum + ": \"" + pastLexeme + "\" already declared");
                Environment.Exit(-3);
            }
            
            fileOutput += "Proc " + "main" + "\n";

            if (Match(Tokens.LEFT_PARENTHESIS_TOKEN) == false)
                ErrorMessage(Tokens.LEFT_PARENTHESIS_TOKEN);

            if (Match(Tokens.STRING_TOKEN) == false)
            {
                ErrorMessage(Tokens.STRING_TOKEN);
            }

            if (Match(Tokens.LEFT_BRACKET_TOKEN) == false)
                ErrorMessage(Tokens.LEFT_BRACKET_TOKEN);

            if (Match(Tokens.RIGHT_BRACKET_TOKEN) == false)
                ErrorMessage(Tokens.RIGHT_BRACKET_TOKEN);

            if (Match(Tokens.IDENTIFIER_TOKEN) == false)
                ErrorMessage(Tokens.IDENTIFIER_TOKEN);

            depth++;
            //Adding Class to Symbol Table
            if (symTab.LookUp(pastLexeme) == null || symTab.LookUp(pastLexeme).depth != depth)
            {
                symTab.Insert(pastLexeme, pastToken, depth);
                symTab.LookUp(pastLexeme).typeOfEntry = EntryType.VARIABLE;
                symTab.LookUp(pastLexeme).Union.var.offset = 0;
                symTab.LookUp(pastLexeme).Union.var.typeOfVariable = VariableType.STRING_TYPE;
                symTab.LookUp(pastLexeme).Union.var.size = 2;
            }

            else
            {
                Console.WriteLine("Error Line-" + Lexi.lineNum + ": \"" + pastLexeme + "\" already declared");
                Environment.Exit(-3);
            }

            if (Match(Tokens.RIGHT_PARENTHESIS_TOKEN) == false)
                ErrorMessage(Tokens.RIGHT_PARENTHESIS_TOKEN);


            if (Match(Tokens.LEFT_BRACE_TOKEN) == false)
                ErrorMessage(Tokens.LEFT_BRACE_TOKEN);

            SeqOfStatements();

            fileOutput += "Endp " + "main" + "\n";

            //Size of Local Would be Set Here

            if (Match(Tokens.RIGHT_BRACE_TOKEN) == false)
                ErrorMessage(Tokens.RIGHT_BRACE_TOKEN);
            //Printing
            symTab.WriteTable(depth, advancedSymbolTable);
            symTab.DeleteDepth(depth);
            depth--;

            if (Match(Tokens.RIGHT_BRACE_TOKEN) == false)
                ErrorMessage(Tokens.RIGHT_BRACE_TOKEN);

            //Printing
            symTab.WriteTable(depth, advancedSymbolTable);
            symTab.DeleteDepth(depth);
            depth--;
        }

        private void MoreClasses()
        {
            if (Match(Tokens.CLASS_TOKEN) == true)
            {
                ClassDecl();
                MoreClasses();
                return;
            }
            else
                return;
        }

        private void ClassDecl()
        {
            TableEntry savedClass = new TableEntry();

            if (Match(Tokens.IDENTIFIER_TOKEN) == false)
                ErrorMessage(Tokens.IDENTIFIER_TOKEN);

            //Adding Class to Symbol Table
            if (symTab.LookUp(pastLexeme) == null || symTab.LookUp(pastLexeme).depth != depth)
            {
                symTab.Insert(pastLexeme, pastToken, depth);
                savedClass = symTab.LookUp(pastLexeme);
                savedClass.typeOfEntry = EntryType.CLASS;
            }

            else
            {
                Console.WriteLine("Error Line-" + Lexi.lineNum + ": \"" + pastLexeme + "\" already declared");
                Environment.Exit(-3);
            }

            //class token already checked in More Classes so we can continue with idt            
            if (Match(Tokens.EXTENDS_TOKEN) == true)
            {
                if (Match(Tokens.IDENTIFIER_TOKEN) == false)
                    ErrorMessage(Tokens.IDENTIFIER_TOKEN);

            }

            if (Match(Tokens.LEFT_BRACE_TOKEN) == false)
                ErrorMessage(Tokens.LEFT_BRACE_TOKEN);

          
            depth++;
            offset = -2;
            VarDecl();
            savedClass.Union.classData.sizeOfVar = offset;
            //Reseting Offset
            offset = -2;
            MethodDecl();

            if (Match(Tokens.RIGHT_BRACE_TOKEN) == false)
                ErrorMessage(Tokens.RIGHT_BRACE_TOKEN);

            symTab.WriteTable(depth, advancedSymbolTable);
            //Write all methods and variables to linked lists
            symTab.populateLists(savedClass, depth);
            symTab.DeleteDepth(depth);
            depth--;
        }

        private void VarDecl()
        {
            if (Type() == true)
            {
                savedType = pastToken;
                IdentifierList();

                if (Match(Tokens.SEMICOLON_TOKEN) == false)
                    ErrorMessage(Tokens.SEMICOLON_TOKEN);

                VarDecl();
            }

            else if (Match(Tokens.FINAL_TOKEN) == true)
            {
                TableEntry savedConst = new TableEntry();

                if (Type() == false)
                {
                    Console.WriteLine("Error Line-" + Lexi.lineNum + " Expected: int, boolean, or void Found: " + ErrorString(token));
                    Environment.Exit(-404);
                }

                savedType = pastToken;

                if (Match(Tokens.IDENTIFIER_TOKEN) == false)
                    ErrorMessage(Tokens.IDENTIFIER_TOKEN);

                symTab.Insert(pastLexeme, pastToken, depth);
                savedConst = symTab.LookUp(pastLexeme);
                savedConst.Union.constant.typeOfConstant = getReturnType(savedType);

                if (Match(Tokens.ASSIGNMENT_OPERATOR_TOKEN) == false)
                    ErrorMessage(Tokens.ASSIGNMENT_OPERATOR_TOKEN);

                if (Match(Tokens.NUMBER_TOKEN) == false)
                    ErrorMessage(Tokens.NUMBER_TOKEN);

                if (savedType == Tokens.FLOAT_TOKEN)
                {
                    savedConst.Union.constant.valueOfConstant.valueR = (float)Lexi.valueR;
                }
                else if (savedType == Tokens.INT_TOKEN)
                    savedConst.Union.constant.valueOfConstant.value = Lexi.value;

                if (Match(Tokens.SEMICOLON_TOKEN) == false)
                    ErrorMessage(Tokens.SEMICOLON_TOKEN);

                VarDecl();
            }

            //For Empty Documentation
            else
                return;
        }

        private void IdentifierList()
        {
            if (Match(Tokens.IDENTIFIER_TOKEN) == false)
                ErrorMessage(Tokens.IDENTIFIER_TOKEN);

            //Try to Add Variable
            AddVariable(offset);
            offset -= GetOffset(savedType);

            if (Match(Tokens.COMMA_TOKEN) == true)
            {
                IdentifierList();
            }
        }

        private bool Type()
        {
            if (Match(Tokens.INT_TOKEN) == true)
                return true;

            else if (Match(Tokens.BOOLEAN_TOKEN) == true)
                return true;

            else if (Match(Tokens.VOID_TOKEN) == true)
                return true;

            else
                return false;
        }

        private void MethodDecl()
        {
            TableEntry savedMethod = new TableEntry();
            Tokens returnType = Tokens.UNKNOWN_TOKEN;


            if (Match(Tokens.PUBLIC_TOKEN) == true)
            {
                if (Type() == false)
                {
                    Console.WriteLine("Error Line-" + Lexi.lineNum + " Expected: int, boolean, or void Found: " + ErrorString(token));
                    Environment.Exit(-404);
                }

                returnType = pastToken;

                if (Match(Tokens.IDENTIFIER_TOKEN) == false)
                    ErrorMessage(Tokens.IDENTIFIER_TOKEN);


                //Adding Method to Symbol Table
                if (symTab.LookUp(pastLexeme) == null || symTab.LookUp(pastLexeme).depth != depth)
                {
                    symTab.Insert(pastLexeme, pastToken, depth);
                    //Add Entry Type
                    symTab.LookUp(pastLexeme).typeOfEntry = EntryType.FUNCTION;
                    //Add Return Type
                    symTab.LookUp(pastLexeme).Union.function.ReturnType = getReturnType(returnType);
                }

                else
                {
                    Console.WriteLine("Error Line-" + Lexi.lineNum + ": \"" + pastLexeme + "\" already declared");
                    Environment.Exit(-3);
                }

                savedMethod = symTab.LookUp(pastLexeme);

                //Adding Strings for Proc and Endp
                fileOutput += "Proc " + savedMethod.Lexeme + "\n";
                


                if (Match(Tokens.LEFT_PARENTHESIS_TOKEN) == false)
                    ErrorMessage(Tokens.LEFT_PARENTHESIS_TOKEN);

                //Changing Depth For Formal List
                depth++;
                FormalList();
                //Saving ParamSize
                savedMethod.Union.function.sizeOfParameters = offset;

                symTab2.Insert(savedMethod.Lexeme, savedMethod.Token, savedMethod.depth);
                symTab2.LookUp(savedMethod.Lexeme).Union.function.sizeOfParameters = savedMethod.Union.function.sizeOfParameters;
                //Console.WriteLine(savedMethod.literal);
                //Reseting offset to -2 for local params
                offset = -2;

                if (Match(Tokens.RIGHT_PARENTHESIS_TOKEN) == false)
                    ErrorMessage(Tokens.RIGHT_PARENTHESIS_TOKEN);

                if (Match(Tokens.LEFT_BRACE_TOKEN) == false)
                    ErrorMessage(Tokens.LEFT_BRACE_TOKEN);

                VarDecl();
                SeqOfStatements();

                if (Match(Tokens.RETURN_TOKEN) == false)
                    ErrorMessage(Tokens.RETURN_TOKEN);

                Expr();
                fileOutput += "_AX = " + getBP(exprReturn) +"\n";
                exprReturn = "0";

                //Console.WriteLine(lastTemp);

                if (Match(Tokens.SEMICOLON_TOKEN) == false)
                    ErrorMessage(Tokens.SEMICOLON_TOKEN);

                if (Match(Tokens.RIGHT_BRACE_TOKEN) == false)
                    ErrorMessage(Tokens.RIGHT_BRACE_TOKEN);

                //Setting Size of Local
                savedMethod.Union.function.sizeOfLocal = -1*(offset + 2);
                symTab2.LookUp(savedMethod.Lexeme).Union.function.sizeOfLocal = savedMethod.Union.function.sizeOfLocal;
                symTab2.LookUp(savedMethod.Lexeme).typeOfEntry = EntryType.FUNCTION;
                //Print Depth
                symTab.WriteTable(depth, advancedSymbolTable);
                //Delete Depth
                symTab.DeleteDepth(depth);
                //Lowering the Depth
                depth--;
                tempCount = 0;

                fileOutput += "Endp " + savedMethod.Lexeme + "\n";
                fileOutput += "\n";
                MethodDecl();
            }

            //Documents the null option
            else
                return;
        }

        private void FormalList()
        {
            offset = 0;
            if (Type() == true)
            {
                offset = 0;
                //Saving type for formal list
                savedType = pastToken;

                if (Match(Tokens.IDENTIFIER_TOKEN) == false)
                    ErrorMessage(Tokens.IDENTIFIER_TOKEN);

                //Otherwise we can add the idt, offset must be zero as formalList only deals with first formal var
                AddVariable(0 + 4);
                //Change offset
                offset += GetOffset(savedType);

                FormalRest();
            }

            //Documents null option
            else
                return;
        }

        private void FormalRest()
        {
            if (Match(Tokens.COMMA_TOKEN) == true)
            {
                if (Type() == false)
                {
                    Console.WriteLine("Error Line-" + Lexi.lineNum + " Expected: int, boolean, or void Found: " + ErrorString(token));
                    Environment.Exit(-404);
                }

                savedType = pastToken;

                if (Match(Tokens.IDENTIFIER_TOKEN) == false)
                    ErrorMessage(Tokens.IDENTIFIER_TOKEN);

                //Good to Add IDT
                AddVariable(offset+4);
                //Change Offset
                offset += GetOffset(savedType);


                FormalRest();
            }
        }

        //Continued Grammar for Statements/Expressions
        private void SeqOfStatements()
        {
            if (Statement() == true)
            {
                if (Match(Tokens.SEMICOLON_TOKEN) == false)
                {
                    Console.WriteLine("Error Line - " + Lexi.lineNum + ": Expected ; after " + ErrorString(pastToken));
                    Environment.Exit(-404);
                }

           
                StatTail();
            }
            //Represents Null Option
        }

        private bool Expr()
        {
            //Console.WriteLine("Expr");
            if (Relation() == true)
            {
                return true;
            }
            //Null option/
            return false;
        }

        private bool Statement()
        {
            if (AssignStat() == true)
            {
                return true;
            }

            else if (IOStat() == true)
            {
                return true;
            }

            return false;
        }

        private void StatTail()
        {
            if (Statement() == true)
            {
                if (Match(Tokens.SEMICOLON_TOKEN) == false)
                    Console.WriteLine("Error Line - " + Lexi.lineNum + ": Expected ; after " + ErrorString(pastToken));

                StatTail();
            }
            //Represents Null Option
        }

        private bool AssignStat()
        {

            string savedIdt;
            if (Match(Tokens.IDENTIFIER_TOKEN) == true)
            {
                if (symTab.LookUp(pastLexeme) == null)
                {
                    Console.WriteLine("Error Line-" + Lexi.lineNum + ": " + pastLexeme + " is an undeclared variable");
                    Environment.Exit(-404);
                }

                
                savedIdt = pastLexeme;

                if (symTab.LookUp(pastLexeme).typeOfEntry == EntryType.CLASS)
                {
                    MethodCall();
                    return true;
                }
               
                if (Match(Tokens.ASSIGNMENT_OPERATOR_TOKEN) == false)
                    ErrorMessage(Tokens.ASSIGNMENT_OPERATOR_TOKEN);

                if (Expr() == true)
                {
                    if (urnaryOutput != "")
                    {
                        string tempName = getBP(newTemp().Lexeme);

                        fileOutput += tempName + " = " + getBP(urnaryOutput) + "\n";
                        fileOutput += getBP(savedIdt) + " = " + getBP(tempName) + "\n";
                        urnaryOutput = "";
                    }

                    else
                    {
                        int tempNum = tempCount - 1;
                        fileOutput += getBP(savedIdt) + " = " + getBP("_T" + (tempNum).ToString()) + "\n";
                        tempOutput = "";
                    }

                    exprReturn = savedIdt;
                }

                else if(MethodCall() == true)
                {
                    fileOutput += getBP(savedIdt) + "=_AX";
                }

                else
                {
                    Console.WriteLine("Error Line-" + Lexi.lineNum + ": " + "Nothing is assigned to variable");
                    Environment.Exit(-404);
                }


                return true;
            }

            return false;
        }

        private bool IOStat()
        {
            if(InStat() == false && OutStat() == false)
            {
                return false;
            }
            
            else
                return true;
        }

        //Additional grammers for IO
        private bool InStat()
        {
            if (Match(Tokens.READ_TOKEN) == true)
            {
                if (Match(Tokens.LEFT_PARENTHESIS_TOKEN) == false)
                {
                    ErrorMessage(Tokens.LEFT_PARENTHESIS_TOKEN);
                    Environment.Exit(-404);
                }

                IdList();

                if (Match(Tokens.RIGHT_PARENTHESIS_TOKEN) == false)
                {
                    ErrorMessage(Tokens.RIGHT_PARENTHESIS_TOKEN);
                    Environment.Exit(-404);
                }

                return true;
            }
            else
                return false;
        }

        private void IdList()
        {
            if(Match(Tokens.IDENTIFIER_TOKEN) == false)
            {
                ErrorMessage(Tokens.IDENTIFIER_TOKEN);
                Environment.Exit(-404);
            }

            fileOutput += "rdi " + getBP(pastLexeme) + "\n";

            IdListTail();
        }

        private void IdListTail()
        {
            if(Match(Tokens.COMMA_TOKEN) == true)
            {
                if (Match(Tokens.IDENTIFIER_TOKEN) == false)
                {
                    ErrorMessage(Tokens.IDENTIFIER_TOKEN);
                    Environment.Exit(-404);
                }

                fileOutput += "rdi " + getBP(pastLexeme) + "\n";

                IdListTail();
            }

            //Represents Null Options
        }

        private bool OutStat()
        {
            if (Match(Tokens.WRITE_TOKEN) == true)
            {
                if (Match(Tokens.LEFT_PARENTHESIS_TOKEN) == false)
                {
                    ErrorMessage(Tokens.LEFT_PARENTHESIS_TOKEN);
                    Environment.Exit(-404);
                }

                WriteList();

                if (Match(Tokens.RIGHT_PARENTHESIS_TOKEN) == false)
                {
                    ErrorMessage(Tokens.RIGHT_PARENTHESIS_TOKEN);
                    Environment.Exit(-404);
                }

                return true;
            }

            else if (Match(Tokens.WRITELN_TOKEN) == true)
            {
                if (Match(Tokens.LEFT_PARENTHESIS_TOKEN) == false)
                {
                    ErrorMessage(Tokens.LEFT_PARENTHESIS_TOKEN);
                    Environment.Exit(-404);
                }

                WriteList();

                if (Match(Tokens.RIGHT_PARENTHESIS_TOKEN) == false)
                {
                    ErrorMessage(Tokens.RIGHT_PARENTHESIS_TOKEN);
                    Environment.Exit(-404);
                }

                fileOutput += "wrln\n";

                return true;
            }

            else
                return false;
        }

        private void WriteList()
        {
            WriteToken();
            WriteListTail();
        }

        private void WriteListTail()
        {
            if(Match(Tokens.COMMA_TOKEN) == true)
            {
                WriteToken();
                WriteListTail();
            }

            //Represents the Null Option
        }

        private void WriteToken()
        {
            TableEntry entry;
            string name = "";

            if (Match(Tokens.NUMBER_TOKEN) == true)
            {
                fileOutput += "wri " + Lexi.value + "\n";
            }

            else if (Match(Tokens.LITERAL_TOKEN) == true)
            {
                name = "S" + literalCount.ToString();
                symTab2.Insert(name, pastToken, 1);
                literalCount = literalCount + 1;

                entry = symTab2.LookUp(name);
                entry.typeOfEntry = EntryType.LITERAL;
                entry.literal = pastLexeme;

                fileOutput += "wrs " + name + "\n";
                
            }

            else if(Match(Tokens.IDENTIFIER_TOKEN) == true)
            {
                fileOutput += "wri " + getBP(pastLexeme) + "\n";
            }
        }

        private bool Relation()
        {
            if (SimpleExpr() == true)
            {
                //tempOutput += temp.Lexeme;
                return true;
            }

            else
                return false;
        }

        private bool SimpleExpr()
        {
            string holdString;
            holdString = Term();
            if (holdString != null)
            {
                if (MoreTerm(holdString) == false)
                {
                    if(symTab.LookUp(holdString) == null)
                    urnaryOutput = holdString;
                }


               return true;
            }

            else
            {
                return false;
            }
        }

        private bool MoreTerm(string termString)
        {
            string holdString;
            string tempName;

            //Console.WriteLine("TermString");
            //Console.WriteLine(termString);



            if (Addop() == true)
            {
                termString += " " + pastLexeme + " ";
                holdString = Term();
                
                if (holdString == null)
                {
                    Console.WriteLine("Error Line-" + Lexi.lineNum + " Expected Term");
                    Environment.Exit(-404);
                }

                tempName = getBP(newTemp().Lexeme);
                termString += holdString;
                fileOutput += tempName + " = " + getBP(termString) + "\n";
                exprReturn = tempName; //Only the last called = will get set to exprReturn
                MoreTerm(holdString);

                return true;
            }

            // fileOutput += tempName + "=" + termString;

            //Null Option
            return false;
        }

        private string Term()
        {
            string tempString;
            if(Factor() == true)
            {
                MoreFactor(tempOutput);
                tempString = tempOutput;
                //Console.WriteLine(tempString);
                tempOutput = "";
                return tempString;
            }

            return null;

        }

        private void MoreFactor(string holdString)
        {
            if (Mulop() == true)
            {
                string savedMulop = " " + pastLexeme + " ";
                tempOutput = "";
                if(Factor() == false)
                {
                    Console.WriteLine("Error Line-" + Lexi.lineNum + " Expected Factor");
                    Environment.Exit(-404);
                }


                string tempName = getBP(newTemp().Lexeme);

                fileOutput += tempName + " = " + holdString + savedMulop + tempOutput + "\n";

                tempOutput = tempName;
                
                MoreFactor(tempName);
                
            }
            //Null Options
        }

        private bool Factor()
        {
            if (Match(Tokens.IDENTIFIER_TOKEN) == true)
            {
                if (symTab.LookUp(pastLexeme) == null)
                {
                    Console.WriteLine("Error Line-" + Lexi.lineNum + ": " + pastLexeme + " is an undeclared variable");
                    Environment.Exit(-404);
                }
                tempOutput += getBP(pastLexeme);
                //tempOutput += pastLexeme;
                return true;
            }

            else if (Match(Tokens.NUMBER_TOKEN) == true)
            {
                tempOutput += Lexi.value.ToString();
                return true;
            }
            
            else if (Match(Tokens.LEFT_PARENTHESIS_TOKEN) == true)
            {
                string savedOutput = tempOutput;
                Console.WriteLine(savedOutput);
                tempOutput = "";
                Expr();
                if (Match(Tokens.RIGHT_PARENTHESIS_TOKEN) == false)
                    ErrorMessage(Tokens.RIGHT_PARENTHESIS_TOKEN);
                tempOutput = lastTemp;

                return true;
            }

            else if (Lexi.lexeme == "!")
            {
                pastToken = token;
                pastLexeme = Lexi.lexeme;
                token = Lexi.GetNextToken();

                Factor();
                return true;
            }

            else if (SignOp() == true)
            {
                Factor();
                fileOutput += getBP(newTemp().Lexeme) + " = 0-" + tempOutput + "\n";
                return true;
            }

            else if (Match(Tokens.TRUE_TOKEN) == true)
            {
                return true;
            }

            else if (Match(Tokens.FALSE_TOKEN) == true)
            {
                return true;
            }

            else
                return false;
            
        }

        private bool Addop()
        {
            if (Match(Tokens.ADDITION_OPERATOR_TOKEN) == true)
            {
                //tempOutput += pastLexeme;
                return true;
            }
            else
                return false;
        }

        private bool Mulop()
        {
            if (Match(Tokens.MULTIPLICATION_OPERATOR_TOKEN) == true)
            {
                tempOutput += pastLexeme;
                return true;
            }

            else
                return false;
        }

        private bool SignOp()
        {
            if(Lexi.lexeme == "-")
            {

                //Console.WriteLine("Sign");
                pastToken = token;
                pastLexeme = Lexi.lexeme;
                token = Lexi.GetNextToken();
                return true;
            }

            return false;
        }

        //Adding Even More Grammar Rules
        private bool MethodCall()
        {
            //Class Name Already found in Assign Stat
            if (Match(Tokens.PERIOD_TOKEN) == false)
            {
                ErrorMessage(Tokens.PERIOD_TOKEN);
            }

            if (Match(Tokens.IDENTIFIER_TOKEN) == false)
                ErrorMessage(Tokens.IDENTIFIER_TOKEN);

            string savedIdt = pastLexeme;

            if (Match(Tokens.LEFT_PARENTHESIS_TOKEN) == false)
                ErrorMessage(Tokens.LEFT_PARENTHESIS_TOKEN);

            Params();
            fileOutput += paramOutput;
            paramOutput = "";

            fileOutput += "call " + savedIdt + "\n";


                

                if (Match(Tokens.RIGHT_PARENTHESIS_TOKEN) == false)
                    ErrorMessage(Tokens.RIGHT_PARENTHESIS_TOKEN);

                return true;
        }

        private bool ClassName()
        {
            if(Match(Tokens.IDENTIFIER_TOKEN) == true)
            {
                //Need to Check for class
                return true;
            }

            return false;
        }

        private bool Params()
        {
            if (Match(Tokens.IDENTIFIER_TOKEN) == true)
            {
                paramOutput += "push " + getBP(pastLexeme) + "\n";
                ParamsTail();
                return true;
            }

            else if (Match(Tokens.NUMBER_TOKEN) == true)
            {
                paramOutput += Lexi.value.ToString() + "\n";
                ParamsTail();
                return true;
            }

            else
            {
                return true;
            }
        }

        private bool ParamsTail()
        {
            if (Match(Tokens.COMMA_TOKEN) == true)
            {
                if (Match(Tokens.IDENTIFIER_TOKEN) == true)
                {
                    paramOutput = paramOutput.Insert(0, "push " + getBP(pastLexeme) + "\n");
                    ParamsTail();
                    return true;
                }

                else if (Match(Tokens.NUMBER_TOKEN) == true)
                {
                    paramOutput = paramOutput.Insert(0, "push " + Lexi.value.ToString() + "\n");
                    ParamsTail();
                    return true;
                }

                else
                {
                    Console.WriteLine("Error Line-" + Lexi.lineNum + ": Expecting Another Parameter");
                    Environment.Exit(-404);
                }
                
                return false;
            }

            else
                return true;
        }
       
        //Error Generating Function for Parser
        private string ErrorString(Tokens token)
        {
            //return "";

            switch (token)
            {
                case Tokens.CLASS_TOKEN:
                    return "class";
                case Tokens.FINAL_TOKEN:
                    return "final";
                case Tokens.FLOAT_TOKEN:
                    return "float";
                case Tokens.PUBLIC_TOKEN:
                    return "public";
                case Tokens.STATIC_TOKEN:
                    return "static";
                case Tokens.VOID_TOKEN:
                    return "void";
                case Tokens.MAIN_TOKEN:
                    return "main";
                case Tokens.STRING_TOKEN:
                    return "String";
                case Tokens.EXTENDS_TOKEN:
                    return "extends";
                case Tokens.RETURN_TOKEN:
                    return "return";
                case Tokens.INT_TOKEN:
                    return "int";
                case Tokens.BOOLEAN_TOKEN:
                    return "boolean";
                case Tokens.IF_TOKEN:
                    return "if";
                case Tokens.ELSE_TOKEN:
                    return "else";
                case Tokens.WHILE_TOKEN:
                    return "while";
                case Tokens.SYSTEM_OUT_PRINTLN_TOKEN:
                    return "System.out.println";
                case Tokens.LENGTH_TOKEN:
                    return "length";
                case Tokens.TRUE_TOKEN:
                    return "true";
                case Tokens.FALSE_TOKEN:
                    return "false";
                case Tokens.THIS_TOKEN:
                    return "this";
                case Tokens.WRITE_TOKEN:
                    return "write";
                case Tokens.WRITELN_TOKEN:
                    return "writeln";
                case Tokens.READ_TOKEN:
                    return "read";
                case Tokens.NEW_TOKEN:
                    return "new";
                case Tokens.IDENTIFIER_TOKEN:
                    return "identifier";
                case Tokens.NUMBER_TOKEN:
                    return "number";
                case Tokens.LITERAL_TOKEN:
                    return "literal";
                case Tokens.RELATIONAL_OPERATOR_TOKEN:
                    return "==, !=, <, <=, >, or >=";
                case Tokens.ADDITION_OPERATOR_TOKEN:
                    return "+, -, or ||";
                case Tokens.MULTIPLICATION_OPERATOR_TOKEN:
                    return "*, /, or &&";
                case Tokens.ASSIGNMENT_OPERATOR_TOKEN:
                    return "=";
                case Tokens.LEFT_PARENTHESIS_TOKEN:
                    return "(";
                case Tokens.RIGHT_PARENTHESIS_TOKEN:
                    return ")";
                case Tokens.LEFT_BRACE_TOKEN:
                    return "{";
                case Tokens.RIGHT_BRACE_TOKEN:
                    return "}";
                case Tokens.COMMA_TOKEN:
                    return ",";
                case Tokens.SEMICOLON_TOKEN:
                    return ";";
                case Tokens.PERIOD_TOKEN:
                    return ".";
                case Tokens.QUOTE_TOKEN:
                    return "\"";
                case Tokens.LEFT_BRACKET_TOKEN:
                    return "[";
                case Tokens.RIGHT_BRACKET_TOKEN:
                    return "]";
                case Tokens.END_OF_FILE_TOKEN:
                    return "EOF";
                default:
                    return "unknown symbol";
            }



        }

        private void ErrorMessage(Tokens expectedToken)
        {
            Console.WriteLine("Error Line-" + Lexi.lineNum + " Expected: " + ErrorString(expectedToken) + ", Found: " + ErrorString(token) + " (\"" + Lexi.lexeme + "\")");
            Environment.Exit(-404);
        }

        //Adds Variable to symbol table
        private void AddVariable(int offset)
        {
            TableEntry entry = new TableEntry();
            //Otherwise we can add the idt
            if (symTab.LookUp(pastLexeme) == null || symTab.LookUp(pastLexeme).depth != depth)
            {
                symTab.Insert(pastLexeme, pastToken, depth);
                entry = symTab.LookUp(pastLexeme);
                //Add Entry Type
                entry.typeOfEntry = EntryType.VARIABLE;
                entry.Union.var.offset = offset;
                entry.Union.var.typeOfVariable = getReturnType(savedType);
                entry.Union.var.size = GetOffset(savedType);

            }

            else
            {
                Console.WriteLine("Error Line-" + Lexi.lineNum + ": Variable \"" + pastLexeme + "\" already declared");
                Environment.Exit(-3);
            }
        }
        //Returns offset based on data type
        private int GetOffset(Tokens type)
        {
            switch (type)
            {
                case Tokens.FLOAT_TOKEN:
                    return 2;
                case Tokens.INT_TOKEN:
                    return 2;
                case Tokens.BOOLEAN_TOKEN:
                    return 1;
                default:
                    Console.WriteLine("Improper Token sent to offset func: " + ErrorString(type));
                    Environment.Exit(-444);
                    return -444;
            }
        }
        //Returns variableType based on token
        private VariableType getReturnType(Tokens type)
        {
            switch (type)
            {
                case Tokens.VOID_TOKEN:
                    return VariableType.VOID_TYPE;
                case Tokens.BOOLEAN_TOKEN:
                    return VariableType.BOOLEAN_TYPE;
                case Tokens.FLOAT_TOKEN:
                    return VariableType.FLOAT_TYPE;
                case Tokens.INT_TOKEN:
                    return VariableType.INTEGER_TYPE;
                default:
                    Console.WriteLine("Improper token to getReturnType");
                    Environment.Exit(-4);
                    return VariableType.VOID_TYPE;
            }
        }

        string getBP(string name)
        {
            if (symTab.LookUp(name) == null)
            {
                return name;
            }

            else if (symTab.LookUp(name).typeOfEntry == EntryType.CONSTANT)
            {
                return symTab.LookUp(name).Union.constant.valueOfConstant.value.ToString();
            }

            else
            {
                if(symTab.LookUp(name).Union.var.offset < 0)
                    return "_BP" + symTab.LookUp(name).Union.var.offset.ToString().PadRight(4);
                else
                     return "_BP" + "+" + symTab.LookUp(name).Union.var.offset.ToString().PadRight(4);
            }

        }

        TableEntry newTemp()
        {
            string name = "_T" + tempCount.ToString();
            lastTemp = name;
            tempCount++;

            symTab.Insert(name, Tokens.IDENTIFIER_TOKEN, depth);

            symTab.LookUp(name).typeOfEntry = EntryType.VARIABLE;
            symTab.LookUp(name).Union.var.offset = offset;
            offset -= 2;

            return symTab.LookUp(name);
        }
    }
}
