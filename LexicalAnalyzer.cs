//Lexical Analyzer Class

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Parser
{
    public enum Tokens
    {
        CLASS_TOKEN,
        FINAL_TOKEN,
        FLOAT_TOKEN,
        PUBLIC_TOKEN,
        STATIC_TOKEN,
        VOID_TOKEN,
        MAIN_TOKEN,
        STRING_TOKEN,
        EXTENDS_TOKEN,
        RETURN_TOKEN,
        INT_TOKEN,
        BOOLEAN_TOKEN,
        IF_TOKEN,
        ELSE_TOKEN,
        WHILE_TOKEN,
        SYSTEM_OUT_PRINTLN_TOKEN,
        LENGTH_TOKEN,
        TRUE_TOKEN,
        FALSE_TOKEN,
        THIS_TOKEN,
        WRITE_TOKEN,
        WRITELN_TOKEN,
        READ_TOKEN,
        NEW_TOKEN,
        IDENTIFIER_TOKEN,
        NUMBER_TOKEN,
        LITERAL_TOKEN,
        RELATIONAL_OPERATOR_TOKEN,
        ADDITION_OPERATOR_TOKEN,
        MULTIPLICATION_OPERATOR_TOKEN,
        ASSIGNMENT_OPERATOR_TOKEN,
        LEFT_PARENTHESIS_TOKEN,
        RIGHT_PARENTHESIS_TOKEN,
        LEFT_BRACE_TOKEN,
        RIGHT_BRACE_TOKEN,
        COMMA_TOKEN,
        SEMICOLON_TOKEN,
        PERIOD_TOKEN,
        QUOTE_TOKEN,
        LEFT_BRACKET_TOKEN,
        RIGHT_BRACKET_TOKEN,
        END_OF_FILE_TOKEN,
        UNKNOWN_TOKEN
    }

    public class LexicalAnalyzer
    {
        public Tokens token;
        public string lexeme;
        public int value = 0;
        public double valueR = 0.0;
        public string literal;
        
        public bool isFloat = false;
        private string[] reservedWords = new string[(int)Tokens.NEW_TOKEN + 1];
        private string file;
        public int lineNum;

        //Constructor for Class
        public LexicalAnalyzer(string fileName)
        {
            lineNum = 1;
            PopulateReservedWords();
            
            //Reading File
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File " + fileName + " not found program will now exit");
                Environment.Exit(-1);
            }

            file = File.ReadAllText(fileName);

            token = Tokens.UNKNOWN_TOKEN;
        }

        private void PopulateReservedWords()
        {
            reservedWords[(int)Tokens.CLASS_TOKEN] = "class";
            reservedWords[(int)Tokens.FINAL_TOKEN] = "final";
            reservedWords[(int)Tokens.FLOAT_TOKEN] = "float";
            reservedWords[(int)Tokens.PUBLIC_TOKEN] = "public";
            reservedWords[(int)Tokens.STATIC_TOKEN] = "static";
            reservedWords[(int)Tokens.VOID_TOKEN] = "void";
            reservedWords[(int)Tokens.MAIN_TOKEN] = "main";
            reservedWords[(int)Tokens.STRING_TOKEN] = "String";
            reservedWords[(int)Tokens.EXTENDS_TOKEN] = "extends";
            reservedWords[(int)Tokens.RETURN_TOKEN] = "return";
            reservedWords[(int)Tokens.INT_TOKEN] = "int";
            reservedWords[(int)Tokens.BOOLEAN_TOKEN] = "boolean";
            reservedWords[(int)Tokens.IF_TOKEN] = "if";
            reservedWords[(int)Tokens.ELSE_TOKEN] = "else";
            reservedWords[(int)Tokens.WHILE_TOKEN] = "while";
            reservedWords[(int)Tokens.SYSTEM_OUT_PRINTLN_TOKEN] = "System.out.println";
            reservedWords[(int)Tokens.LENGTH_TOKEN] = "length";
            reservedWords[(int)Tokens.TRUE_TOKEN] = "true";
            reservedWords[(int)Tokens.FALSE_TOKEN] = "false";
            reservedWords[(int)Tokens.THIS_TOKEN] = "this";
            reservedWords[(int)Tokens.NEW_TOKEN] = "new";
            reservedWords[(int)Tokens.WRITE_TOKEN] = "write";
            reservedWords[(int)Tokens.WRITELN_TOKEN] = "writeln";
            reservedWords[(int)Tokens.READ_TOKEN] = "read";
        }

        private void ProccessToken()
        {
            int incrementer = 0;
            string temp = "";

            #region Reserved_Words
            //Checking for Reserverd Words
            foreach (string value in reservedWords)
            {
                if (file.StartsWith(value) == true && (value.Length == file.Length || file[value.Length] == '\n' || file[value.Length] == ' ' || file[value.Length] == '(' || file[value.Length] == ';'))
                {
                    temp = value;
                    token = (Tokens)incrementer;
                    lexeme = value;
                    file = file.Remove(0, value.Length);
                    return;
                }
                incrementer = incrementer + 1;
            }
            #endregion

            #region Numbers
            if (char.IsDigit(file[0]) == true)
            {
                bool tempBool = false;
                string tempString = "";
                bool exitBool = false;

                while (file.Length > 0 && ((char.IsDigit(file[0]) == true) || file[0] == '.') && exitBool == false)
                {
                    if (file[0] == '.')
                    {
                        if (file.Length > 1 && char.IsDigit(file[1]) == true && tempBool == false)
                        {
                            tempBool = true;
                            tempString = tempString + file[0].ToString();
                            file = file.Remove(0, 1);
                        }
                        else
                            exitBool = true;
                    }


                    else
                    {
                        tempString = tempString + file[0].ToString();
                        file = file.Remove(0, 1);
                    }

                        
                }

                if (tempBool == false)
                {
                    value = Int32.Parse(tempString);
                    token = Tokens.NUMBER_TOKEN;
                    isFloat = false;
                }

                if(tempBool == true)
                {
                    valueR = Double.Parse(tempString);
                    token = Tokens.NUMBER_TOKEN;
                    isFloat = true;
                }
                return;
            }
            #endregion

            #region Literals

            if(file[0] == '"')
            {
               // Console.WriteLine(file);
                int i = 1;
                bool exitBool = false;
                while (i < file.Length && exitBool == false)
                {
                    if (file[i] == '"' || file[i] == '\n')
                        exitBool = true;
                    i++;
                }

               

                if (file[i-1] == '"')
                {
                    token = Tokens.LITERAL_TOKEN;
                    lexeme = file.Substring(0,i);
                    file = file.Remove(0, i);
                    return;
                }

                if(file[i-1] == '\n')
                {
                    Console.WriteLine("Warning: Expected Quote");
                }

                if(i == file.Length)
                {
                    Console.WriteLine("Warning: Expected Quote");
                }

            }

            #endregion

            #region Relational_Operators
            //Checking for Relational_Operators
            if (file.StartsWith("==") == true)
            {
                lexeme = "==";
                token = Tokens.RELATIONAL_OPERATOR_TOKEN;
                file = file.Remove(0, 2);
                return;
            }

            else if(file.StartsWith("!=") == true)
            {
                token = Tokens.RELATIONAL_OPERATOR_TOKEN;
                lexeme = "!=";
                file = file.Remove(0, 2);
                return;
            }
            else if (file.StartsWith("<=") == true)
            {
                token = Tokens.RELATIONAL_OPERATOR_TOKEN;
                lexeme = "<=";
                file = file.Remove(0, 2);
                return;
            }
        
            else if (file.StartsWith(">=") == true)
            {
                token = Tokens.RELATIONAL_OPERATOR_TOKEN;
                lexeme = ">=";
                file = file.Remove(0, 2);
                return;
            }

            else if (file[0] == '<' )
            {
                token = Tokens.RELATIONAL_OPERATOR_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            else if (file[0] == '>')
            {
                token = Tokens.RELATIONAL_OPERATOR_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }
            #endregion

            #region Addition_Operators
            if (file.StartsWith("||") == true)
            {
                token = Tokens.ADDITION_OPERATOR_TOKEN;
                lexeme = "||";
                file = file.Remove(0, 2);
                return;
            }

            else if (file[0] == '+')
            {
                token = Tokens.ADDITION_OPERATOR_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            else if (file[0] == '-')
            {
                token = Tokens.ADDITION_OPERATOR_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }
            #endregion

            #region Multiplication_Operator
            if (file.StartsWith("&&") == true)
            {
                token = Tokens.MULTIPLICATION_OPERATOR_TOKEN;
                lexeme = "&&";
                file = file.Remove(0, 2);
                return;
            }

            else if (file[0] == '*')
            {
                token = Tokens.MULTIPLICATION_OPERATOR_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            else if (file[0] == '/')
            {
                token = Tokens.MULTIPLICATION_OPERATOR_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }
            #endregion

            #region Symbols
            //Check Other Symbols
            if (file[0] == '(')
            {
                token = Tokens.LEFT_PARENTHESIS_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            else if(file[0] == ')')
            {
                token = Tokens.RIGHT_PARENTHESIS_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            else if (file[0] == '{')
            {
                token = Tokens.LEFT_BRACE_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            else if (file[0] == '}')
            {
                token = Tokens.RIGHT_BRACE_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            else if (file[0] == ',')
            {
                token = Tokens.COMMA_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            else if (file[0] == ';')
            {
                token = Tokens.SEMICOLON_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            else if (file[0] == '.')
            {
                token = Tokens.PERIOD_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            else if (file[0] == '"')
            {
                token = Tokens.QUOTE_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            else if (file[0] == '[')
            {
                token = Tokens.LEFT_BRACKET_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            else if (file[0] == ']')
            {
                token = Tokens.RIGHT_BRACKET_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            #endregion

            if (file[0] == '=')
            {
                token = Tokens.ASSIGNMENT_OPERATOR_TOKEN;
                lexeme = file[0].ToString();
                file = file.Remove(0, 1);
                return;
            }

            #region Identifier
            //Must be Identifier
            if (char.IsLetter(file[0]) == true)
            {
                token = Tokens.IDENTIFIER_TOKEN;
                string tempString = "";
                while (file.Length != 0 && tempString.Length < 33 && (file[0] != ' ' || file[0] != '\n' || file[0] != '\t') && (char.IsLetterOrDigit(file[0]) == true) || file[0] == '_')
                {
                    tempString = tempString + file[0];
                    file = file.Remove(0, 1);
                }
                lexeme = tempString;
                return;
            }
            #endregion

            token = Tokens.UNKNOWN_TOKEN;
            lexeme = file[0].ToString();
            file = file.Remove(0, 1);

        }

        public Tokens GetNextToken()
        {
            while (file.Length != 0 && (file[0] == ' ' || file[0] == '\n' || file[0] == (char)13 || file[0] == '\t'))
            {
                if (file[0] == '\n')
                    lineNum = lineNum + 1;
                file = file.Remove(0, 1);
            }
            
            while (file.StartsWith("//") == true || file.StartsWith("/*") == true)
            {

                if (file.StartsWith("//") == true)
                {
                    while (file.Length != 0 && file[0] != '\n')
                    {
                        file = file.Remove(0, 1);
                    }

                    file = file.Remove(0, 1);
                    lineNum = lineNum + 1;
                }


                if (file.StartsWith("/*") == true)
                {
                    while (file.Length != 0 && (file.StartsWith("*/") != true))
                    {
                        if (file[0] == '\n')
                        {
                            lineNum = lineNum + 1;
                        }
                        file = file.Remove(0, 1);
                    }

                    if (file.StartsWith("*/") == true)
                    {
                        file = file.Remove(0, 2);
                        if (file.StartsWith("\n") == true)
                        {
                            lineNum = lineNum + 1;
                            file = file.Remove(0, 2);
                        }
                    }

                    
                }

                while (file.Length != 0 && (file[0] == ' ' || file[0] == '\n' || file[0] == (char)13 || file[0] == '\t'))
                {
                    if (file[0] == '\n')
                        lineNum = lineNum + 1;
                    file = file.Remove(0, 1);
                }
            }
            
            if (file.Length != 0)
                ProccessToken();
            else
                return Tokens.END_OF_FILE_TOKEN;

            return token;
        }
    }
}
