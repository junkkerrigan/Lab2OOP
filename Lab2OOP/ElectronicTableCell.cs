using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Antlr4.Runtime;

namespace Lab2OOP
{
    public class ElectronicTableCell : DataGridViewTextBoxCell
    {
        public ElectronicTable Table { 
            get
            {
                return DataGridView as ElectronicTable;
            }
        }
        public ElectronicTableCell() : base()
        {
        }
        public string Expression { get; set; } = "";
        public bool CurrentlyCalculating { get; set; } = false;
        public bool IsReevaluated { get; set; } = false;
        public HashSet<ElectronicTableCell> Dependecies { get; set; } = new HashSet<ElectronicTableCell>();
        public HashSet<ElectronicTableCell> Depended { get; set; } = new HashSet<ElectronicTableCell>();
        public int Count()
        {
            if (Expression == "")
            {
                var ex = new Exception();
                ex.Data.Add("Type", "reference to empty cell");
                throw ex;
            }
            if (IsReevaluated)
            {
                return (int)Value;
            }
            try
            {
                var inputStream = new AntlrInputStream(Expression);
                var lexer = new GrammarLexer(inputStream);
                var commonTokenStream = new CommonTokenStream(lexer);
                var parser = new GrammarParser(commonTokenStream);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(new ParsingErrorListener());
                var expr = parser.rule();
                int val = (new ParsingVisitor(this)).Visit(expr);
                IsReevaluated = true;
                return val;
            }
            catch
            {
                throw;
            }
        }
        public override object Clone()
        {
            var clone = base.Clone() as ElectronicTableCell;
            clone.Expression = Expression;
            return clone;
        }
    }

}