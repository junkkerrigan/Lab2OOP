using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2OOP
{
    public static class ColumnSys26
    {
        public static int Sys26ToNum(string inSys26)
        {
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int num = 0, pow = 1;
            for (int i = inSys26.Length - 1; i >= 0; i--)
            {
                num += (alphabet.IndexOf(inSys26[i]) + 1) * pow;
                pow *= 26;
            }
            return num;
        }
        public static string NumToSys26(int num)
        {
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string title = "";
            //if (num <= 0) throw new ArgumentException();
            while (num > 0)
            {
                if (num % 26 == 0)
                {
                    num /= 26;
                    num--;
                    title = "Z" + title;
                }
                else
                {
                    title = alphabet[num % 26 - 1] + title;
                    num /= 26;
                }
            }
            return title;
        }
    }
}
