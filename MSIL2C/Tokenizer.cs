﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2C
{
    public class Tokenizer
    {
        public delegate string Token(string line);
        Dictionary<string, Token> Tokens;
        Stack<string> Depth;

        public Tokenizer()
        {
            Tokens = new Dictionary<string, Token>();
            #region Tokens
            Tokens[".namespace"] = (string s) => {
                string name = s.Remove(".namespace").Trim();
                Depth.Push("</namespace>");
                return "<namespace NAME=\"" + name + "\">";
            };
            Tokens[".class"] = (string s) =>
            {
                string[] tmp = s.Split(' ');
                Depth.Push("</class>");
                return "<class NAME=\"" + tmp[tmp.Length - 1] + "\">";
            };
            Tokens[".method"] = (string s) =>
            {
                s = s.Remove(".method").Remove("hidebysig").Trim();
                string[] nxtLine = (s + " " + PeekNextLine().Remove("cil managed").Remove(0, 7).Trim()).Split(' ');
                Depth.Push("</method>");
                return "<method VISIBILITY=\"" + nxtLine[0] + "\" RETURN=\"" + nxtLine[2] + "\" SCOPE=\"" + nxtLine[1] + "\" NAME=\"" + nxtLine[3] + nxtLine[4] + "\">";
            };
            Tokens["IL_"] = (string s) =>
                {
                    return "<IL instruction=\"" + s.Remove(s.Split(':')[0] + ":").Trim() + "\"></IL>";
                };
            Tokens["}"] = (string s) =>
                {
                    //We're out one level
                    if (Depth.Count != 0) return Depth.Pop();
                    else return "";
                };
            #endregion
        }


        int lineNum = 0;
        int peekLineNum = 0;
        string[] lines;

        public string PeekNextLine()
        {
            peekLineNum++;
            return lines[lineNum + peekLineNum].Replace("\"", "\\\"").Trim();
        }

        public string Tokenize(string code)
        {
            lines = code.Split('\n');
            StringBuilder final = new StringBuilder();
            Depth = new Stack<string>();

            final.Append("<MSIL>\r\n");

            for (lineNum = 0; lineNum < lines.Length; lineNum++)
            {
                //skip empty lines
                if (lines[lineNum].Trim() != string.Empty)
                {
                    //Remove all whitespace from the string
                    string line = lines[lineNum].Replace("\"", "&quot;").Trim();
                    //Check if the line is of any interest to us
                    foreach (string key in Tokens.Keys)
                    {
                        //Reset the peek variables
                        peekLineNum = 0;

                        //if it is, call the appropriate handler and update the tokens
                        if (line.StartsWith(key))
                        {
                            string f = Tokens[key](line);
                            if(!string.IsNullOrWhiteSpace(f))final.AppendLine(f);
                        }
                    }
                }
            }

            final.Append("</MSIL>");
            return final.ToString();
        }


    }
}
