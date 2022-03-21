//Nathan Dobbs
//Compiler Constructions
//Assignment 4

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Parser
{

    #region substructs for TableEntry
    //Definintion of "Union" used by TableEntry for the alternate types
    [StructLayout(LayoutKind.Explicit)]
    public struct TypeUnion
    {
        [FieldOffset(0)]
        public Variable var;
        [FieldOffset(0)]
        public Constant constant;
        [FieldOffset(0)]
        public Function function;
        [FieldOffset(0)]
        public Class classData;
    }
    public struct Variable
    {
        public VariableType typeOfVariable;
        public int offset;
        public int size;
    }
    public struct Constant
    {
        public VariableType typeOfConstant;
        public int offsetConstant;
        public Value valueOfConstant;
    }
    
    [StructLayout(LayoutKind.Explicit)]
    public struct Value
    {
        [FieldOffset(0)]
        public int value;
        [FieldOffset(0)]
        public float valueR;
    }

    public struct Function
    {
        public int sizeOfLocal;
        public int sizeOfParameters;
        public VariableType ReturnType;
    }

    public struct Class
    {
        public int sizeOfVar;
    }

    public enum VariableType { BOOLEAN_TYPE, INTEGER_TYPE, FLOAT_TYPE, VOID_TYPE, STRING_TYPE}
    
    public enum EntryType {CONSTANT, VARIABLE, FUNCTION, CLASS, LITERAL}
    
    #endregion
    //Entry for Symbol Table
    public class TableEntry
    {
        public Tokens Token;
        public string Lexeme;
        public int depth;
        public TableEntry next;

        public EntryType typeOfEntry;
        public TypeUnion Union;
        public string literal;
        
        //Code Enters BreakPoint unless I put all class/pointer stuff outside of the outside the "Union"
        //public LinkedList<VariableType> functionParamList;
        public LinkedList<string> classVariableList = new LinkedList<string>();
        public LinkedList<string> classMethodList = new LinkedList<string>();

    }

    //Linked List for Symbol Table
    public class Linked
    {
        public TableEntry head;
    }

    //Functions for the Symbol/HashTable
    class SymTab
    {

        private const int TABLE_SIZE = 223;
        public Linked[] hashTable = new Linked[TABLE_SIZE];

        //Returns Pointer towards lexeme
        public TableEntry LookUp(string lex)
        {
            for (int i = 0; i < TABLE_SIZE; i++)
            {
                if (hashTable[i] != null)
                {
                    TableEntry location = hashTable[i].head;
                    while (location != null)
                    {
                        if (location.Lexeme == lex)
                            return location;

                        location = location.next;
                    }
                }
            }

            return null;
        }

        public TableEntry LookUpSpecial(string lex, int depthNum)
        {
            for (int i = 0; i < TABLE_SIZE; i++)
            {
                if (hashTable[i] != null)
                {
                    TableEntry location = hashTable[i].head;
                    while (location != null)
                    {
                        if (location.Lexeme == lex && location.depth == depthNum)
                            return location;

                        location = location.next;
                    }
                }
            }

            return null;
        }
        //Class Constructor
        public SymTab()
        {
            for (int i = 0; i < TABLE_SIZE; i++)
            {
                hashTable[i] = new Linked();
            }
        }

        //Adds entry to Symbol Table
        public void Insert(string lex, Tokens token, int depth)
        {
            TableEntry add = new TableEntry();
            add.Lexeme = lex;
            add.Token = token;
            add.depth = depth;
            add.next = null;

            int hashLocation = Hash(lex);

            if (hashTable[hashLocation].head == null)
                hashTable[hashLocation].head = add;

            else
            {
                add.next = hashTable[hashLocation].head;
                hashTable[hashLocation].head = add;
            }
        }

        //Prints Table at given depth
        public void WriteTable(int depth, bool advancedSymTab)
        {
            Console.WriteLine("=====================================");
            Console.WriteLine(" Displaying Symbol Table at Depth: " + depth);
            Console.WriteLine("=====================================");

            for(int i = 0; i < TABLE_SIZE; i++)
            {
                if (hashTable[i] != null)
                {
                    TableEntry location = hashTable[i].head;
                    while (location != null)
                    {
                        if (location.depth == depth)
                        {
                            Console.WriteLine(" Lexeme: \"" + location.Lexeme + "\" Type: " + TypeOfEntryString(location.typeOfEntry));
                            if(location.typeOfEntry == EntryType.FUNCTION && advancedSymTab == true)
                            {
                                Console.WriteLine(" ---Size of Parameters: " + location.Union.function.sizeOfParameters);
                                Console.WriteLine(" ---Size of Local Variables: " +location.Union.function.sizeOfLocal);
                            }

                            else if (location.typeOfEntry == EntryType.VARIABLE && advancedSymTab == true)
                            {
                                Console.WriteLine(" ---Data Type: " + TypeOfVar(location.Union.var.typeOfVariable));
                                Console.WriteLine(" ---Size: " + location.Union.var.size);
                                Console.WriteLine(" ---Offset: " + location.Union.var.offset);
                            }

                            else if(location.typeOfEntry == EntryType.CLASS && advancedSymTab == true)
                            {
                                Console.WriteLine(" ---Method Names: " + String.Join(", ", location.classMethodList));
                                Console.WriteLine(" ---Variable Names: " + String.Join(", ", location.classVariableList));
                            }

                            else if (location.typeOfEntry == EntryType.CONSTANT && advancedSymTab == true)
                            {
                                Console.WriteLine(" ---Constant Type: " + TypeOfVar(location.Union.constant.typeOfConstant));
                                Console.Write(" ---Value: ");
                                if (location.Union.constant.typeOfConstant == VariableType.FLOAT_TYPE)
                                {
                                    Console.WriteLine(location.Union.constant.valueOfConstant.valueR);
                                }

                                else if (location.Union.constant.typeOfConstant == VariableType.INTEGER_TYPE)
                                    Console.WriteLine(location.Union.constant.valueOfConstant.value);
                            }

                        }

                        location = location.next;
                    }
                }
            }
            Console.WriteLine("");
        }
        //Returns String version of variableType
        private string TypeOfVar(VariableType type)
        {
            switch (type)
            {
                case VariableType.BOOLEAN_TYPE:
                    return "Boolean";
                case VariableType.FLOAT_TYPE:
                    return "Float";
                case VariableType.INTEGER_TYPE:
                    return "Integer";
                case VariableType.VOID_TYPE:
                    return "Void";
                default:
                    return "String[]";
            }
        }
        //Return String Version of EntryType
        public string TypeOfEntryString(EntryType type)
        {
            switch(type)
            {
                case EntryType.CONSTANT:
                    return "Constant";
                case EntryType.VARIABLE:
                    return "Variable";
                case EntryType.FUNCTION:
                    return "Function";
                case EntryType.LITERAL:
                    return "Literal";
                case EntryType.CLASS:
                    return "Class";
                default:
                    return "";
            }
        }

        //Deletes Entrys at a given depth
        public void DeleteDepth(int depth)
        {
            for (int i = 0; i < TABLE_SIZE; i++)
                //Only need to worry about non null lists
                if (hashTable[i].head != null)
                {
                    TableEntry location = hashTable[i].head;
                    TableEntry past = null;

                    //Dealing With Head
                    while (location != null && location.depth == depth)
                    {
                        hashTable[i].head = location.next;
                        location = hashTable[i].head;
                    }

                    //Dealing With NonHead
                    while (location != null)
                    {
                        while (location != null && location.depth != depth)
                        {
                            past = location;
                            location = location.next;
                        }

                        if (location != null)
                        {
                            past.next = location.next;

                            location = past.next;
                        }
                    }
                }
        }

        //Returns Lexemes hash
        private int Hash(string lexeme)
        {
            uint h = 0, g;

            for (int i = 0; i < lexeme.Length; i++)
            {
                h = (h << 4) + lexeme[i];
                if ((g = h & 0b_1111_0000_0000_0000_0000_0000_0000_0000) != 0)
                {
                    h = h ^ (g >> 24);
                    h = h ^ g;
                }
            }

            return (int)(h % TABLE_SIZE);
        }

        //Writes all entrys at a certain depth to a table entrys class lists
        public void populateLists(TableEntry entry, int depth)
        {
            for (int i = 0; i < TABLE_SIZE; i++)
            {
                if (hashTable[i] != null)
                {
                    TableEntry location = hashTable[i].head;
                    while (location != null)
                    {
                        if (location.depth == depth)
                        {
                            if(location.typeOfEntry == EntryType.FUNCTION)
                            {
                                entry.classMethodList.AddFirst(location.Lexeme);
                            }

                            else if(location.typeOfEntry == EntryType.VARIABLE)
                            {
                                entry.classVariableList.AddFirst(location.Lexeme);
                            }
                        }

                        location = location.next;
                    }
                }
            }
        }
    }
}
