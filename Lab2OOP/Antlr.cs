using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;

namespace Lab2
{
    public class AntlrVisitor : Lab2GrammarBaseVisitor<int>
    {
        CellView CalculatingCell;
        int LeftOperand(Lab2GrammarParser.ExprContext context)
        {
            return Visit(context.GetRuleContext<Lab2GrammarParser.ExprContext>(0));
        }
        int RightOperand(Lab2GrammarParser.ExprContext context)
        {
            return Visit(context.GetRuleContext<Lab2GrammarParser.ExprContext>(1));
        }
        public AntlrVisitor(CellView calculating) : base()
        {
            CalculatingCell = calculating;
            console.log($"CalculatingCell: {calculating}");
            console.log("Dependecies:");
            foreach (var d in calculating.Connections)
            {
                console.log(d);
            }
            console.log("Connected:");
            foreach (var d in calculating.Connected)
            {
                console.log(d);
            }
        }
        public override int VisitUnit(Lab2GrammarParser.UnitContext context)
        {
            try
            {
                console.log(context.GetText());
                int ans = Visit(context.expr());
                console.log($"{ans}");
                return ans;
            }
            catch
            {
                throw;
            }
        }
        public override int VisitNum([NotNull] Lab2GrammarParser.NumContext context)
        {
            int ans = int.Parse(context.GetText());
            console.log($"num {ans}");
            return ans;
        }
        public override int VisitAdditionOrSubtraction(Lab2GrammarParser.AdditionOrSubtractionContext context)
        {
            int l = LeftOperand(context), r = RightOperand(context);
            int ans = 0;
            if (context.operatorToken.Type == Lab2GrammarLexer.ADDITION)
            {
                ans = l + r;
                console.log($"plus {ans}");
            }
            else if (context.operatorToken.Type == Lab2GrammarLexer.SUBTRACTION)
            {
                ans = l - r;
                console.log($"minus {ans}");
            }
            return ans;
        }
        public override int VisitMultiplicationOrDivision(Lab2GrammarParser.MultiplicationOrDivisionContext context)
        {
            int l = LeftOperand(context), r = RightOperand(context);
            int ans = 0;
            if (context.operatorToken.Type == Lab2GrammarLexer.MULTIPLICATION)
            {
                ans = l * r;
                console.log($"mult {ans}");
            }
            else if (context.operatorToken.Type == Lab2GrammarLexer.DIV)
            {
                if (r == 0)
                {
                    var ex = new Exception();
                    ex.Data.Add("Type", "div on 0");
                    throw ex;
                }
                ans = l / r;
                console.log($"div {ans}");
            }
            return ans;
        }
        public override int VisitMod([NotNull] Lab2GrammarParser.ModContext context)
        {
            var l = LeftOperand(context);
            var r = RightOperand(context);
            if (r == 0)
            {
                var ex = new DivideByZeroException();
                ex.Data.Add("Type", "mod on 0");
                throw ex;
            }
            int ans = l % r;
            console.log($"mod {ans}");
            return ans;
        }
        public override int VisitPower([NotNull] Lab2GrammarParser.PowerContext context)
        {
            var l = LeftOperand(context);
            var r = RightOperand(context);
            if (r < 0)
            {
                var ex = new ArgumentOutOfRangeException();
                ex.Data.Add("Type", "negative value of power");
                throw ex;
            }
            if (r == 0 && l == 0)
            {
                var ex = new ArgumentOutOfRangeException();
                ex.Data.Add("Type", "null in null power");
                throw ex;
            }
            int ans = (int)Math.Pow(l, r);
            console.log($"pow {ans}");
            return ans;
        }
        public override int VisitBrackets([NotNull] Lab2GrammarParser.BracketsContext context)
        {
            int ans = Visit(context.expr());
            console.log($"par {ans}");
            return ans;
        }
        public override int VisitNegNum([NotNull] Lab2GrammarParser.NegNumContext context)
        {
            int ans = int.Parse(context.GetText());
            console.log($"neg {ans}");
            return ans;
        }
        public override int VisitCellRef([NotNull] Lab2GrammarParser.CellRefContext context)
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
                console.log($"{cellRef} {ans}");
                return ans;
            }
            catch
            {
                throw;
            }
        }
        public override int VisitInvalid([NotNull] Lab2GrammarParser.InvalidContext context)
        {
            throw new Exception();
        }
        public override int VisitInc([NotNull] Lab2GrammarParser.IncContext context)
        {
            int ans = Visit(context.expr()) + 1;
            console.log($"inc {ans}");
            return ans;
        }
        public override int VisitDec([NotNull] Lab2GrammarParser.DecContext context)
        {
            int ans = Visit(context.expr()) - 1;
            console.log($"dic {ans}");
            return ans;
        }
    }
}
