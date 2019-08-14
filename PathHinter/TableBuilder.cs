using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathHinter
{
    public static class TableBuilder
    {
        /// <summary>
        /// Draws a collection of items as a table in the console.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="padding">The padding.</param>
        public static void DrawAsTable<T>(this IEnumerable<T> collection, int padding = 3)
        {
            var list = collection.ToList();

            int maxLength = list.Max(item => item.ToString().Length) + padding;

            int columns = (int)Math.Floor((double)Console.WindowWidth / maxLength); // Horizontal
            int rows = (int)Math.Ceiling((double)list.Count / columns); // Vertical

            var builder = new StringBuilder();
            for (int i = 0; i < rows; i++)
            {
                string curLine = string.Empty;

                for (int j = 0; j < columns; j++)
                {
                    int index = j + i * columns;

                    if (index >= list.Count)
                        continue;

                    string item = list[index].ToString();
                    curLine += item.PadRight(maxLength);
                }

                builder.AppendLine(curLine);
            }

            Console.WriteLine();
            Console.WriteLine(builder.ToString());
        }
    }
}