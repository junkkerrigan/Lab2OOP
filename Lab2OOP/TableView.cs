using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Antlr4.Runtime;

namespace Lab2
{
    public class MyParsErrListener : IAntlrErrorListener<IToken>
    {
        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            console.log("SE");
            throw new Exception();
        }
    }

    public class TableView : DataGridView
    {
        public TableView(int rows, int cols) : base()
        {
            AllowUserToAddRows = false;
            MultiSelect = false;
            AddColumns(cols);
            AddRows(rows);
            RowHeadersWidth = 60;
            DefaultCellStyle.Font = new Font("Times New Roman", 12);
            ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        public CellView CurCell
        {
            get
            {
                if (SelectedCells.Count == 0) return null;
                return SelectedCells[0] as CellView;
            }
        }
        public CellView Cell(int rNum, int cNum)
        {
            return Rows[rNum].Cells[cNum] as CellView;
        }
        public void AddRows(int count)
        {
            for (int i = 0; i < count; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.HeaderCell.Value = (RowCount + 1).ToString();
                Rows.Add(row);
            }
        }
        public void AddColumns(int cnt)
        {
            for (int i = 0; i < cnt; i++)
            {
                DataGridViewColumn col = new DataGridViewColumn(new CellView());
                col.HeaderCell.Value = Sys26.NumToSys26(ColumnCount + 1);
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
                foreach (var d in cell.Connected)
                {
                    if (d.RowIndex == idx) continue;
                    canDelete = false;
                    break;
                }
            }
            if (canDelete)
            {
                RestoreRowTitles(idx);
                Rows.RemoveAt(idx);
                return;
            }
            var isDelete = MessageBox.Show("Removing row will lead to some cells" +
                " to become empty. Remove row?", "Danger",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (isDelete == DialogResult.No) return;
            try
            {
                for (int i = 0; i < Columns.Count; i++)
                {
                    var cell = Cell(idx, i);
                    EmptyConnections(cell);
                }
                RestoreRowTitles(idx);
                Rows.RemoveAt(idx);
            }
            catch (Exception ex)
            {
                console.log($"in DelRow: {ex.Message}");
            }
        }
        public void DeleteColumn(string name)
        {
            int idx;
            try
            {
                idx = int.Parse(name);
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
                foreach (var d in cell.Connected)
                {
                    if (d.ColumnIndex == idx) continue;
                    canDelete = false;
                    break;
                }
            }
            if (canDelete)
            {
                RestoreColTitles(idx);
                Columns.RemoveAt(idx);
                return;
            }
            var isDelete = MessageBox.Show("Removing column will lead to some cells" +
                " to become empty. Remove column?", "Danger",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (isDelete == DialogResult.No) return;
            try
            {
                for (int i = 0; i < RowCount; i++)
                {
                    var cell = Cell(i, idx);
                    EmptyConnections(cell);
                }
                RestoreColTitles(idx);
                Columns.RemoveAt(idx);
            }
            catch (Exception ex)
            {
                console.log($"in DelCol: {ex.Message}");
            }
        }
        void RestoreRowTitles(int deleted)
        {
            for (int i = RowCount - 1; i > deleted; i--)
            {
                Rows[i].HeaderCell.Value = Rows[i - 1].HeaderCell.Value;
            }
        }
        void RestoreColTitles(int deleted)
        {
            for (int i = ColumnCount - 1; i > deleted; i--)
            {
                Columns[i].HeaderCell.Value = Columns[i - 1].HeaderCell.Value;
            }
        }
        void ProcessCell(CellView cell)
        {
            try
            {
                var inputStream = new AntlrInputStream(cell.Expression);
                var lexer = new Lab2GrammarLexer(inputStream);
                var commonTokenStream = new CommonTokenStream(lexer);
                var parser = new Lab2GrammarParser(commonTokenStream);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(new MyParsErrListener());
                var expr = parser.unit();
                cell.Value = (new AntlrVisitor(cell)).Visit(expr);
            }
            catch
            {
                throw;
            }
            cell.Recalculated = true;
            foreach (var dep in cell.Connected)
            {
                ProcessCell(dep);
            }
        }
        void EmptyConnections(CellView cur)
        {
            cur.Value = cur.Expression = "";
            foreach (var d in cur.Connected)
            {
                EmptyConnections(d);
            }
            cur.Connected.Clear();
            cur.Connections.Clear();
        }
        public string ToFile()
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
        public static TableView FillFromFile(string data)
        {
            try
            {
                var reader = new StringReader(data);
                var sizes = reader.ReadLine();
                var n = int.Parse(sizes.Substring(0, sizes.IndexOf(' ')));
                var m = int.Parse(sizes.Substring(sizes.IndexOf(' ') + 1));
                console.log($"created table with sizes {n} and {m}");
                TableView table = new TableView(n, m);
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        table.Cell(i, j).Expression = reader.ReadLine();
                        console.log($"in cell ({i},{j}) now {table.Cell(i, j).Expression}");
                    }
                }
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        table.Recalculate(table.Cell(i, j));
                        console.log($"value of ({i},{j}) now {table.Cell(i, j).Value}");
                    }
                }
                return table;
            }
            catch
            {
                throw;
            }
        }
        public void Recalculate(CellView updated)
        {
            foreach (DataGridViewRow r in Rows)
            {
                foreach (CellView c in r.Cells)
                {
                    c.Recalculated = false;
                }
            }
            foreach (var d in updated.Connections)
            {
                d.Connected.Remove(updated);
            }
            updated.Connections.Clear();
            if (string.IsNullOrWhiteSpace(updated.Expression))
            {
                EmptyConnections(updated);
                return;
            }
            try
            {
                ProcessCell(updated);
            }
            catch
            {
                throw;
            }
        }
    }
}
