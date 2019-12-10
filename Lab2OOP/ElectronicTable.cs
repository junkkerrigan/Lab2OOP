using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Antlr4.Runtime;

namespace Lab2OOP
{
    public class ElectronicTable : DataGridView
    {
        public ElectronicTableCell Cell(int rNum, int cNum)
        {
            return Rows[rNum].Cells[cNum] as ElectronicTableCell;
        }
        public ElectronicTableCell CurCell
        {
            get
            {
                if (SelectedCells.Count == 0) return null;
                return SelectedCells[0] as ElectronicTableCell;
            }
        }
        public void AddRow(int count)
        {
            for (int i = 0; i < count; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.HeaderCell.Value = (RowCount + 1).ToString();
                Rows.Add(row);
            }
        }
        public ElectronicTable(int rows, int cols) : base()
        {
            AllowUserToAddRows = false;
            MultiSelect = false;
            AddColumn(cols);
            AddRow(rows);
            RowHeadersWidth = 60;
            DefaultCellStyle.Font = new Font("Arial", 16);
            ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        public void AddColumn(int cnt)
        {
            for (int i = 0; i < cnt; i++)
            {
                DataGridViewColumn col = new DataGridViewColumn(new ElectronicTableCell());
                col.HeaderCell.Value = ColumnSys26.NumToSys26(ColumnCount + 1);
                Columns.Add(col);
            }
        }
        public void DeleteRow(int idx)
        {
            idx--;
            bool canDelete = true;
            for (int i = 0; i < ColumnCount; i++)
            {
                var cell = Cell(idx, i);
                foreach (var d in cell.Depended)
                {
                    if (d.RowIndex == idx) continue;
                    canDelete = false;
                    break;
                }
            }
            if (canDelete)
            {
                ChangeRowTitles(idx);
                Rows.RemoveAt(idx);
                return;
            }
            var isDelete = MessageBox.Show("If you delete this row, cells that refers to it will become empty. Are you still wanna delete it?", "Danger",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (isDelete == DialogResult.No) return;
            try
            {
                for (int i = 0; i < Columns.Count; i++)
                {
                    var cell = Cell(idx, i);
                    ClearDependecies(cell);
                }
                ChangeRowTitles(idx);
                Rows.RemoveAt(idx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"in DelRow: {ex.Message}");
            }
        }
        public void Clear()
        {
            foreach (DataGridViewRow r in Rows)
            {
                foreach (ElectronicTableCell c in r.Cells)
                {
                    c.Value = c.Expression = "";
                }
            }
        }
        public void DeleteColumn(string num)
        {
            int idx;
            try
            {
                idx = int.Parse(num);
            }
            catch
            {
                throw new Exception();
            }
            if (idx < 1) throw new ArgumentOutOfRangeException();
            idx--;
            bool canDelete = true;
            for (int i = 0; i < RowCount; i++)
            {
                var cell = Cell(i, idx);
                foreach (var d in cell.Depended)
                {
                    if (d.ColumnIndex == idx) continue;
                    canDelete = false;
                    break;
                }
            }
            if (canDelete)
            {
                ChangeColTitles(idx);
                Columns.RemoveAt(idx);
                return;
            }
            var isDelete = MessageBox.Show("If you delete this column, cells that refers to it will become empty. Are you still wanna delete it?", "Danger",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (isDelete == DialogResult.No) return;
            try
            {
                for (int i = 0; i < RowCount; i++)
                {
                    var cell = Cell(i, idx);
                    ClearDependecies(cell);
                }
                ChangeColTitles(idx);
                Columns.RemoveAt(idx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"in DelCol: {ex.Message}");
            }
        }
        void ChangeRowTitles(int deleted)
        {
            for (int i = RowCount - 1; i > deleted; i--)
            {
                Rows[i].HeaderCell.Value = Rows[i - 1].HeaderCell.Value;
            }
        }
        void ChangeColTitles(int deleted)
        {
            for (int i = ColumnCount - 1; i > deleted; i--)
            {
                Columns[i].HeaderCell.Value = Columns[i - 1].HeaderCell.Value;
            }
        }
        void CalculateCellExpression(ElectronicTableCell cell)
        {
            try
            {
                var inputStream = new AntlrInputStream(cell.Expression);
                var lexer = new GrammarLexer(inputStream);
                var commonTokenStream = new CommonTokenStream(lexer);
                var parser = new GrammarParser(commonTokenStream);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(new ParsingErrorListener());
                var expr = parser.rule();
                cell.Value = (new ParsingVisitor(cell)).Visit(expr);
            }
            catch
            {
                throw;
            }
            cell.IsReevaluated = true;
            foreach (var dep in cell.Depended)
            {
                CalculateCellExpression(dep);
            }
        }
        void ClearDependecies(ElectronicTableCell cur)
        {
            cur.Value = cur.Expression = "";
            foreach (var d in cur.Depended)
            {
                ClearDependecies(d);
            }
            cur.Depended.Clear();
            cur.Dependecies.Clear();
        }
        public string ConvertToText()
        {
            string data = $"{RowCount} {ColumnCount}\n";
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    data += Cell(i, j).Expression + '\n';
                }
            }
            return data;
        }
        public void CalculateAllCells(ElectronicTableCell updated)
        {
            foreach (DataGridViewRow r in Rows)
            {
                foreach (ElectronicTableCell c in r.Cells)
                {
                    c.IsReevaluated = false;
                    c.CurrentlyCalculating = false;
                }
            }
            foreach (var d in updated.Dependecies)
            {
                d.Depended.Remove(updated);
            }
            updated.Dependecies.Clear();
            if (string.IsNullOrWhiteSpace(updated.Expression))
            {
                ClearDependecies(updated);
                return;
            }
            try
            {
                CalculateCellExpression(updated);
            }
            catch 
            {
                throw; // fix cell loop handling
            }
        }
        public static ElectronicTable FillFromText(string data)
        {
            try
            {
                var reader = new StringReader(data);
                var sizes = reader.ReadLine();
                var n = int.Parse(sizes.Substring(0, sizes.IndexOf(' ')));
                var m = int.Parse(sizes.Substring(sizes.IndexOf(' ') + 1));
                Console.WriteLine($"created table with sizes {n} and {m}");
                ElectronicTable table = new ElectronicTable(n, m);
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        table.Cell(i, j).Expression = reader.ReadLine();

                        Console.WriteLine($"in cell ({i},{j}) now {table.Cell(i, j).Expression}");
                    }
                }
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        table.CalculateAllCells(table.Cell(i, j));
                        Console.WriteLine($"value of ({i},{j}) now {table.Cell(i, j).Value}");
                    }
                }
                return table;
            }
            catch
            {
                throw;
            }
        }
    }
}
