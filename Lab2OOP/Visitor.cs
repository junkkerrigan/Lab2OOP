using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Lab2OOP
{
    public class ParsingErrorListener : IAntlrErrorListener<IToken>
    {
        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Console.WriteLine("SE");
            throw new Exception();
        }
    }

    public class ParsingVisitor : GrammarBaseVisitor<int>
    {
        ElectronicTableCell CalculatingCell;

        public ParsingVisitor(ElectronicTableCell calculating) : base()
        {
            CalculatingCell = calculating;
            Console.WriteLine($"CalculatingCell: {calculating}");
            Console.WriteLine("Dependecies:");
            foreach (var d in calculating.Dependecies)
            {
                Console.WriteLine(d);
            }
            Console.WriteLine("Depended:");
            foreach (var d in calculating.Depended)
            {
                Console.WriteLine(d);
            }
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
                Console.WriteLine($"div(/) {ans}");
            }
            else if (context.operation.Type == GrammarLexer.MULTIPLICATION_SIGN)
            {
                ans = l * r;
                Console.WriteLine($"mult {ans}");
            }
            return ans;
        }
        public override int VisitInBrackets([NotNull] GrammarParser.InBracketsContext context)
        {
            int ans = Visit(context.expression());
            Console.WriteLine($"par {ans}");
            return ans;
        }
        public override int VisitNumber([NotNull] GrammarParser.NumberContext context)
        {
            int ans = int.Parse(context.GetText());
            Console.WriteLine($"num {ans}");
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
                int colNum = ColumnSys26.Sys26ToNum(cellRef.Substring(0, column)) - 1;
                int rowNum = int.Parse(cellRef.Substring(column)) - 1;
                ElectronicTableCell cell = CalculatingCell.Table.Cell(rowNum, colNum);
                if (cell.CurrentlyCalculating)
                {

                    var ex = new Exception();
                    ex.Data.Add("Type", "cell loop");
                    throw ex;
                }
                cell.Depended.Add(CalculatingCell);
                CalculatingCell.Dependecies.Add(cell);
                cell.CurrentlyCalculating = true;
                int ans = cell.Count();
                cell.CurrentlyCalculating = false;
                Console.WriteLine($"Value of {cellRef} is {ans}");
                return ans;
            }
            catch
            {
                throw;
            }
        }
        public override int VisitInPower([NotNull] GrammarParser.InPowerContext context)
        {
            var l = Left(context);
            var r = Right(context);
            int ans;
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
        public override int VisitWrong([NotNull] GrammarParser.WrongContext context)
        {
            throw new Exception();
        }
        public override int VisitOnDiv([NotNull] GrammarParser.OnDivContext context)
        {
            int l = Left(context), r = Right(context);
            int ans = 0;

            if (r == 0)
            {
                var ex = new Exception();
                ex.Data.Add("Type", "div on null");
                throw ex;
            }
            else
            {
                ans = l / r;
            }
            Console.WriteLine($"div {ans}");
            return ans;
        }
        public override int VisitMaxOf([NotNull] GrammarParser.MaxOfContext context)
        {
            int ans = int.MinValue;
            foreach (var operand in context.expression())
            {
                ans = Math.Max(Visit(operand), ans);
            }
            Console.WriteLine($"max {ans}");
            return ans;
        }
        public override int VisitNegativeNumber([NotNull] GrammarParser.NegativeNumberContext context)
        {
            int ans = int.Parse(context.GetText());
            Console.WriteLine($"neg {ans}");
            return ans;
        }
        public override int VisitMinOf([NotNull] GrammarParser.MinOfContext context)
        {
            int ans = int.MaxValue;
            foreach (var operand in context.expression())
            {
                ans = Math.Min(Visit(operand), ans);
            }
            Console.WriteLine($"min {ans}");
            return ans;
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
