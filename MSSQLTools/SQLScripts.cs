using System;
using System.Collections.Generic;

namespace MSSQLTools
{
    partial class Program
    {
        public class SQLScripts : List<string>
        {
            public int _tabs;

            public SQLScripts(int tabs = 0)
            {
                _tabs = tabs;
            }
            
            public SQLScripts Add(int tabs, string text)
            {
                base.Add($"{new String('\t', tabs + _tabs)}{text}");

                return this;
            }

            public void Save(string path)
            {
                System.IO.File.WriteAllLines(path, this);
            }
        }
    }
}
