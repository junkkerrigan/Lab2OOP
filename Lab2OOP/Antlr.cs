using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;

namespace Lab2OOP
{
    public class ParsingVisitor : GrammarBaseVisitor<int>
    {
        CellView CalculatingCell;

        public ParsingVisitor(CellView calculating) : base()
        {
            CalculatingCell = calculating;
            Console.WriteLine($"CalculatingCell: {calculating}");
            Console.WriteLine("Dependecies:");
            foreach (var d in calculating.Connections)
            {
                Console.WriteLine(d);
            }
            Console.WriteLine("Connected:");
            foreach (var d in calculating.Connected)
            {
                Console.WriteLine(d);
            }
        }
        
        public override int VisitRule(GrammarParser.RuleContext context)
        {
            try
            {
                Console.WriteLine(context.GetText());
                int ans = Visit(context.expression());
                Console.WriteLine($"{ans}");
                return ans;
            }
            catch
            {
                throw;
            }
        }
        public override int VisitNumber([NotNull] GrammarParser.NumberContext context)
        {
            int ans = int.Parse(context.GetText());
            Console.WriteLine($"num {ans}");
            return ans;
        }
        public override int VisitAdditionSubtraction(GrammarParser.AdditionSubtractionContext context)
        {
            int l = Left(context), r = Right(context);
            int ans = 0;
            
            if (context.operation.Type == GrammarLexer.MINUS_SIGN)
            {
                ans = l - r;
                Console.WriteLine($"minus {ans}");
            }
            else if (context.operation.Type == GrammarLexer.PLUS_SIGN)
            {
                ans = l + r;
                Console.WriteLine($"plus {ans}");
            }
            return ans;
        }
        public override int VisitMultiplicationDivision(GrammarParser.MultiplicationDivisionContext context)
        {
            int l = Left(context), r = Right(context);
            int ans = 0;
            
            if (context.operation.Type == GrammarLexer.DIVISION_SIGN)
            {
                if (r == 0)
                {
                    var ex = new Exception();
                    ex.Data.Add("Type", "division on null");
                    throw ex;
                }
                else
                {
                    ans = l / r;
                }
                Console.WriteLine($"div {ans}");
            }
            else if (context.operation.Type == GrammarLexer.MULTIPLICATION_SIGN)
            {
                ans = l * r;
                Console.WriteLine($"mult {ans}");
            }
            return ans;
        }
        public override int VisitOnModulo([NotNull] GrammarParser.OnModuloContext context)
        {
            var l = Left(context);
            var r = Right(context);
            int ans = 0;
            if (r == 0)
            {
                var ex = new DivideByZeroException();
                ex.Data.Add("Type", "on modulo 0");
                throw ex;
            }
            else
            {
                ans = l % r;
            }
            Console.WriteLine($"mod {ans}");
            return ans;
        }
        public override int VisitInPower([NotNull] GrammarParser.InPowerContext context)
        {
            var l = Left(context);
            var r = Right(context);
            int ans = 0;
            if (r < 0)
            {
                var ex = new ArgumentOutOfRangeException();
                ex.Data.Add("Type", "expression in negative power");
                throw ex;
            }
            else if (r == 0 && l == 0)
            {
                var ex = new ArgumentOutOfRangeException();
                ex.Data.Add("Type", "null expression in null power");
                throw ex;
            }
            else
            {
                ans = (int)Math.Pow(l, r);
            }
            Console.WriteLine($"pow {ans}");
            return ans;
        }
        public override int VisitInBrackets([NotNull] GrammarParser.InBracketsContext context)
        {
            int ans = Visit(context.expression());
            Console.WriteLine($"par {ans}");
            return ans;
        }
        public override int VisitNegativeNumber([NotNull] GrammarParser.NegativeNumberContext context)
        {
            int ans = int.Parse(context.GetText());
            Console.WriteLine($"neg {ans}");
            return ans;
        }
        public override int VisitCell([NotNull] GrammarParser.CellContext context)
        {
            try
            {
                string cellRef = context.GetText();
                int column = 0;
                while (65 <= (int)cellRef[column] && (int)cellRef[column] <= 90)
                    column++;
                int colNum = Sys26.Sys26ToNum(cellRef.Substring(0, column)) - 1;
                int rowNum = int.Parse(cellRef.Substring(column)) - 1;
                CellView cell = CalculatingCell.Table.Cell(rowNum, colNum);
                if (cell.Visited)
                {
                    var ex = new Exception();
                    ex.Data.Add("Type", "cell loop");
                    throw ex;
                }
                cell.Connected.Add(CalculatingCell);
                CalculatingCell.Connections.Add(cell);
                cell.Visited = true;
                int ans = cell.Evaluate();
                cell.Visited = false;
                Console.WriteLine($"{cellRef} {ans}");
                return ans;
            }
            catch
            {
                throw;
            }
        }
        public override int VisitWrong([NotNull] GrammarParser.WrongContext context)
        {
            throw new Exception();
        }
 
        int Left(GrammarParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<GrammarParser.ExpressionContext>(0));
        }
        int Right(GrammarParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<GrammarParser.ExpressionContext>(1));
        }
    }
}
