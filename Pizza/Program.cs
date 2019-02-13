using System;
using System.IO;
using System.Linq;
using System.Text;

namespace GHC2019
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputFiles = Directory.GetFiles($"{Environment.CurrentDirectory}\\pizza data");

            foreach (var file in inputFiles)
            {
                HandleInput(file);
            }

            Console.ReadLine();
        }

        static int Rows;
        static int Columns;
        static int MinIngredientOfEach;
        static int MaxIngredientsPerSlice;

        enum Ingredient
        {
            Tomato,
            Mushroom
        }

        static Ingredient[,] Pizza;

        private static void HandleInput(string input)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var fileName = Path.GetFileNameWithoutExtension(input);
            Console.WriteLine("-----------------------");
            Console.WriteLine($"Handeling: \t {fileName}");

            var lines = File.ReadAllLines(input);

            var firstLine = lines.First().Split(" ");
            var i = 0;
            Rows = int.Parse(firstLine[i++]);
            Columns = int.Parse(firstLine[i++]);
            MinIngredientOfEach = int.Parse(firstLine[i++]);
            MaxIngredientsPerSlice = int.Parse(firstLine[i++]);

            Pizza = new Ingredient[Rows, Columns];

            for (int r = 1; r < lines.Length; r++)
            {
                for (int c = 0; c < lines[r].Length; c++)
                {
                    if (lines[r][c] == 'T')
                        Pizza[r - 1, c] = Ingredient.Tomato;
                    else if (lines[r][c] == 'M')
                        Pizza[r - 1, c] = Ingredient.Mushroom;
                    else
                        throw new InvalidOperationException();
                }
            }

            string result = SlicePizza();
            result = result.Count(c => c == '\r') + Environment.NewLine + result;


            WriteResult(result, Path.GetFileName(fileName));

            watch.Stop();
            Console.WriteLine($"Time elapsed: \t {watch.ElapsedMilliseconds}");

            Console.WriteLine("-----------------------");
        }

        private static string SlicePizza()
        {
            int maxRows = MaxIngredientsPerSlice / 2;
            int maxColumns = 2;

            int MushroomsOnSlice = 0;
            int TomatosOnSlice = 0;

            var logBuilder = new StringBuilder();
            // foreach (var carLog in log)
            // {
            //     logBuilder.AppendLine($"{carLog.Value.Count} {string.Join(' ', carLog.Value.Select(r => r.Id))}");
            // }    

            int startCol = 0;
            for (int col = 1; col < Columns && startCol < Columns; col++)
            {
                if (SliceContainsMinimumIngredients(startCol, 0, col, Rows - 1))
                {
                    logBuilder.AppendLine($"{startCol} {Rows - 1} {col} {Rows - 1}");
                    startCol = col;
                }
            }

            return logBuilder.ToString();



        }

        private static bool SliceContainsMinimumIngredients(int xStart, int yStart, int xEnd, int yEnd)
        {
            int foundMushroom = 0;
            int foundTomato = 0;

            for (int col = xStart; col < xEnd; col++)
            {
                for (int row = yStart; row < yEnd; row++)
                {
                    if (Pizza[row, col] == Ingredient.Mushroom)
                        foundMushroom++;
                    else
                        foundTomato++;

                    if (foundTomato >= MinIngredientOfEach && foundMushroom >= MinIngredientOfEach)
                        return true;
                }
            }

            return false;
        }

        private static void WriteResult(string output, string fileName)
        {
            File.WriteAllText($"Output\\{fileName}.out", output);
            Console.WriteLine($"Done with: \t {fileName}");
        }
    }
}
