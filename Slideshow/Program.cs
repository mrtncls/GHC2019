using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace GHC2019
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputFiles = Directory.GetFiles($"{Environment.CurrentDirectory}\\Input");

            foreach (var file in inputFiles)
            {
                HandleInput(file);
            }

            Console.ReadLine();
        }

        static int NumOfPhotos => Photos.Length;
        static Photo[] Photos;
        static Slide[] Slides;
        private class Photo {
            public int Index {get;set;}
            public bool IsVertical {get;set;}
            public string[] Tags {get;set;}
        }

        private class Slide {
            public Photo[] photos {get;set;}
        }

        private static void HandleInput(string input)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var fileName = Path.GetFileNameWithoutExtension(input);
            Console.WriteLine("-----------------------");
            Console.WriteLine($"Handeling: \t {fileName}");

            var lines = File.ReadAllLines(input);

            Photos = ReadLinesIntoArray(lines);
            
            Slides = MakeSlideshow(Photos);
            
            string result = CreateResultString(Slides);

            WriteResult(result, Path.GetFileName(fileName));

            watch.Stop();
            Console.WriteLine($"Time elapsed: \t {watch.ElapsedMilliseconds}");

            Console.WriteLine("-----------------------");
        }

        

        private static Photo[] ReadLinesIntoArray(string[] lines)
        {
            var photos = new Photo[int.Parse(lines.First())];

            for (int p = 1; p < lines.Length; p++)
            {
                int index = p -1;

                var line = lines[p].Split(" ");
                photos[index] = new Photo{
                    Index = index,
                    IsVertical = line[0] == "V",
                    Tags = line.Skip(2).ToArray()
                };
            }

            return photos;
        }

       private static Slide[] MakeSlideshow(Photo[] photos)
       {
           Photo previousVertical = null;
           var slides = new List<Slide>();
           for (int i = 0; i < photos.Length; i++)
           {
               if(photos[i].IsVertical){
                   if (previousVertical != null) {
                        slides.Add(new Slide {
                            photos = new Photo[] { previousVertical, photos[i] }
                        });
                        previousVertical = null;
                   }
                   else
                   {
                       previousVertical = photos[i];
                   }
               }
               else 
               {
                   slides.Add(new Slide {
                       photos = new Photo[] { photos[i] }
                   });
               }
           }

           return slides.ToArray();
       }

       private static string CreateResultString(Slide[] slides) {
            string result = $"{Slides.Count()}{Environment.NewLine}"; 
            foreach(var slide in Slides) {
                result = $"{result}{string.Join(" ", slide.photos.Select(p => p.Index))}{Environment.NewLine}";
            }

            return result;
       }

        private static void WriteResult(string output, string fileName)
        {
            File.WriteAllText($"Output\\{fileName}.out", output);
            Console.WriteLine($"Done with: \t {fileName}");
        }
    }
}
