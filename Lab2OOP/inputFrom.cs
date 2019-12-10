using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab2OOP
{
    public static class InputForm
    {
        public static string Show(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 300,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label()
            {
                Left = 30,
                Top = 40,
                Text = text,
                Width = 200
            };
            RichTextBox textBox = new RichTextBox()
            {
                Left = 30,
                Top = textLabel.Bottom + 10,
                Width = 200,
                Height = 30,
                Font = new Font("Arial", 14),
            };
            Button confirmation = new Button()
            {
                Text = "Submit",
                Left = textBox.Right - 50,
                Width = 60,
                Height = 25,
                Top = textBox.Bottom + 10,
                DialogResult = DialogResult.OK
            };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
