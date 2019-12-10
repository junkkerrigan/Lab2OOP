using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Antlr4;
using Antlr4.Runtime;

namespace Lab2OOP
{
    public partial class Form1 : Form
    {
        TextBox ExpressionTextBox;
        Panel MainPanel;
        Label Rows, Cols;
        Button AddRow, OpenAboutButton, Clear, AddColumn, RemoveRow, RemoveColumn, SaveToButton, Open;
        public ElectronicTable Table { get; set; }
        bool TableChanged = false;
        void AddMainPanel()
        {
            MainPanel = new Panel()
            {
                Location = new Point(60, 20),
                Size = new Size(ClientSize.Width - 120, 80),
            };
            ExpressionTextBox = new TextBox()
            {
                Location = new Point(0, 5),
                Font = new Font("Arial", 20),
                Width = 250,
            };
            ExpressionTextBox.Leave += (s, e) =>
            {
                if (Table.CurCell == null) return;
                string oldExpr = Table.CurCell.Expression;
                Table.CurCell.Expression = ExpressionTextBox.Text;
                try
                {
                    Table.CalculateAllCells(Table.CurCell);
                }
                catch (Exception ex)
                {
                    if (ex.Data.Contains("Type"))
                    {
                        MessageBox.Show($"Wrong expression: {ex.Data["Type"]}", "Error");
                    }
                    else
                    {
                        MessageBox.Show($"Wrong expression: impossible to recognize formula", "Error");
                    }
                    Table.CurCell.Expression = oldExpr;
                    Table.CalculateAllCells(Table.CurCell);
                }
                TableChanged = true;
                Text = "*Table";
            };
            AddTableButtons();
            AddFileButtons();
            MainPanel.Controls.Add(ExpressionTextBox);
            Controls.Add(MainPanel);
        }
        void AddTable(ElectronicTable table)
        {
            if (table == null)
            {
                Table = new ElectronicTable(20, 10)
                {
                    Location = new Point(40, MainPanel.Bottom + 20),
                    Size = new Size(ClientSize.Width - 80,
                        ClientSize.Height - MainPanel.Bottom - 80),
                };
            }
            else
            {
                Table.SuspendLayout();
                Controls.Remove(Table);
                table.Location = new Point(30, MainPanel.Bottom + 30);
                table.Size = new Size(ClientSize.Width - 60,
                    ClientSize.Height - MainPanel.Bottom - 60);
                Table = table;
            }

            Table.CellEnter += (s, e) =>
            {
                ElectronicTableCell selected = Table.Cell(e.RowIndex, e.ColumnIndex) as ElectronicTableCell;
                ExpressionTextBox.Text = Table.Cell(e.RowIndex, e.ColumnIndex).Expression;
            };
            Table.CellBeginEdit += (s, e) =>
            {
                try
                {
                    Table.CurCell.Value = ExpressionTextBox.Text =
                        Table.Cell(e.RowIndex, e.ColumnIndex).Expression;
                }
                catch (IndexOutOfRangeException)
                {
                }
            };
            Table.CellEndEdit += (s, e) =>
            {
                ElectronicTableCell changed = Table.Cell(e.RowIndex, e.ColumnIndex);
                string oldExpr = changed.Expression;
                changed.Expression = (string)changed.Value;
                try
                {
                    Table.CalculateAllCells(changed);
                }
                catch (Exception ex)
                {
                    if (ex.Data.Contains("Type"))
                    {
                        MessageBox.Show($"Wrong expression: {ex.Data["Type"]}", "Error");

                    }
                    else
                    {
                        MessageBox.Show($"Wrong expression", "Error");

                    }
                    changed.Expression = oldExpr;
                    Table.CalculateAllCells(changed);
                }
                TableChanged = true;
                Text = "*Table";
            };
            Controls.Add(Table);
            Table.ResumeLayout();
        }
        void AddTableButtons()
        {
            Rows = new Label()
            {
                Location = new Point(
                    ExpressionTextBox.Right + Math.Max(20, 
                        (ClientRectangle.Right - 900 - ExpressionTextBox.Right) / 2)
                    , 15),
                Text = "Rows:",
                Font = new Font("Arial", 14),
                Size = new Size(70, 30),
            };
            AddRow = new Button()
            {
                Text = "Add",
                Location = new Point(Rows.Right, 10),
                Size = new Size(50, 30),
            };
            AddRow.Click += (s, e) => Table.AddRow(1);
            RemoveRow = new Button()
            {
                Text = "Remove",
                Location = new Point(AddRow.Right + 10, 10),
                Size = new Size(60, 30),
            };
            RemoveRow.Click += (s, e) =>
            {
                string input = InputForm.Show("Select which row to remove:", "Removing");
                if (input == "") return;
                try
                {
                    int num = Convert.ToInt32(input);
                    Table.DeleteRow(num);
                }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show("Wrong number", "Wrong number",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch
                {
                    MessageBox.Show("Wrong input", "Wrong input",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            Cols = new Label()
            {
                Location = new Point(RemoveRow.Right + 40, 15),
                Text = "Columns:",
                Font = new Font("Arial", 14),
                Size = new Size(100, 30),
            };
            AddColumn = new Button()
            {
                Text = "Add",
                Location = new Point(Cols.Right, 10),
                Size = new Size(50, 30),
            };
            AddColumn.Click += (s, e) => Table.AddColumn(1);
            RemoveColumn = new Button()
            {
                Text = "Remove",
                Location = new Point(AddColumn.Right + 10, 10),
                Size = new Size(60, 30),
            };
            RemoveColumn.Click += (s, e) =>
            {
                string input = InputForm.Show("Select which column to remove:", "Removing");
                if (input == "") return;
                try
                {
                    Table.DeleteColumn(input);
                }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show("Wrong number of column", "Wrong number",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch
                {
                    MessageBox.Show("Wrong input", "Wrong input",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            MainPanel.Controls.Add(Rows);
            MainPanel.Controls.Add(AddRow);
            MainPanel.Controls.Add(RemoveRow);
            MainPanel.Controls.Add(Cols);
            MainPanel.Controls.Add(AddColumn);
            MainPanel.Controls.Add(RemoveColumn);
        }
        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!TableChanged) return;
            var isSave = MessageBox.Show("Save the table?", "Saving", MessageBoxButtons.YesNoCancel);
            if (isSave == DialogResult.Yes)
            {
                SaveToButton.PerformClick();
            }
            else if (isSave == DialogResult.Cancel) e.Cancel = true;
        }
        void AddFileButtons()
        {
            OpenAboutButton = new Button()
            {
                Location = new Point(
                    RemoveColumn.Right + Math.Max(20, 
                        (ClientRectangle.Right - 900 - ExpressionTextBox.Right) / 2 - 30)
                    , 10),
                Size = new Size(80, 30),
                Text = "About",
                Font = new Font("Arial", 14),
            };
            OpenAboutButton.Click += (s, e) =>
            {
                MessageBox.Show("Сивокобильска Іра, К-25, варіант №5\nІндивідуальне завдання: *, /, +, -, div, mod, ^, mmax(), mmin()",
                        "About", MessageBoxButtons.OK);
            };
            SaveToButton = new Button()
            {
                Location = new Point(OpenAboutButton.Right + 15, OpenAboutButton.Top),
                Size = new Size(80, 30),
                Text = "Save",
                Font = new Font("Arial", 14),
            };
            SaveToButton.Click += (s, e) =>
            {
                var saveTo = new SaveFileDialog()
                {
                    Filter = "Text files (*.txt)|*.txt",
                };
                if (saveTo.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveTo.FileName, Table.ConvertToText());
                }
                TableChanged = false;
                Text = "Table";
            };
            Open = new Button()
            {
                Location = new Point(SaveToButton.Right + 15, OpenAboutButton.Top),
                Size = new Size(80, 30),
                Text = "Open",
                Font = new Font("Arial", 14),
            };
            Open.Click += (s, e) =>
            {
                if (TableChanged)
                {
                    var isSave = MessageBox.Show("Do you wanna save this table before you open other?", "Saving", MessageBoxButtons.YesNoCancel);
                    if (isSave == DialogResult.Yes)
                    {
                        SaveToButton.PerformClick();
                    }
                }
                var openFrom = new OpenFileDialog()
                {
                    Filter = "Text files (*.txt)|*.txt",
                };
                if (openFrom.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        AddTable(
                            ElectronicTable.FillFromText(
                                File.ReadAllText(openFrom.FileName)));
                    }
                    catch
                    {
                        MessageBox.Show($"Wrong file: {openFrom.FileName}", "Wrong file");
                    }
                }
            };
            Clear = new Button()
            {
                Location = new Point(Open.Right + 15, OpenAboutButton.Top),
                Size = new Size(70, 30),
                Text = "Clear",
                Font = new Font("Arial", 14),
            };
            Clear.Click += (s, e) =>
            {
                Table.Clear();
            };
            MainPanel.Controls.Add(OpenAboutButton);
            MainPanel.Controls.Add(SaveToButton);
            MainPanel.Controls.Add(Open);
            MainPanel.Controls.Add(Clear);
        }
        public Form1()
        {
            Text = "Table";
            AddMainPanel();
            AddTable(null);
            Resize += Form1_SizeChanged;
            FormClosing += Form1_FormClosing;
        }
        void Form1_SizeChanged(object sender, EventArgs e)
        {
            MainPanel.Width = ClientSize.Width - 120;

            Rows.Left = ExpressionTextBox.Right + Math.Max(20,
                        (ClientRectangle.Right - 900 - ExpressionTextBox.Right) / 2);
            AddRow.Left = Rows.Right;
            RemoveRow.Left = AddRow.Right + 10;
            Cols.Left = RemoveRow.Right + 40;
            AddColumn.Left = Cols.Right;
            RemoveColumn.Left = AddColumn.Right + 10;
            
            OpenAboutButton.Left = RemoveColumn.Right + Math.Max(20, 
                (ClientRectangle.Right - 900 - ExpressionTextBox.Right) / 2 - 30);
            SaveToButton.Left = OpenAboutButton.Right + 15;
            Open.Left = SaveToButton.Right + 15;
            Clear.Left = Open.Right + 15;

            Table.Size = new Size(ClientSize.Width - 80,
                    ClientSize.Height - MainPanel.Bottom - 80);
        }
    }
}
