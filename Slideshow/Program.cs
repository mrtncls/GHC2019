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

        private class Photo
        {
            public int Index { get; set; }
            public bool IsVertical { get; set; }
            public string[] Tags { get; set; }
            public int[] HashedTags { get; set; }

        }

        private class Slide
        {
            public Slide(Photo horizontal)
            {
                photos = new Photo[] { horizontal };
                HashedTags = horizontal.HashedTags;
                ResultString = horizontal.Index.ToString();
            }
            public Slide(Photo vertical1, Photo vertical2)
            {
                photos = new Photo[] { vertical1, vertical2 };
                HashedTags = vertical1.HashedTags.Concat(vertical2.HashedTags).Distinct().ToArray();
                Array.Sort(HashedTags);
                ResultString = $"{vertical1.Index} {vertical2.Index}";
            }
            public string ResultString { get; set; }
            public Photo[] photos { get; set; }
            public int[] HashedTags { get; set; }
        }

        private class Score
        {
            public int scoreInt { get; set; }
            public Slide Slide { get; set; }
        }

        private static void HandleInput(string input)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var fileName = Path.GetFileNameWithoutExtension(input);
            Console.WriteLine("-----------------------");
            Console.WriteLine($"Handeling: \t {fileName}");

            var lines = File.ReadAllLines(input);

            Photos = ReadLinesIntoArray(lines);

            Console.WriteLine($"Processing {Photos.Length} photo's...");

            HashTags();

            Console.WriteLine($"Tags hashed. Creating slides...");

            TransformToSlides();

            Console.WriteLine($"Sorting {Slides.Length} slides...");

            MakeGreedySlideshow();

            Console.WriteLine($"Writing {Slides.Length} slides to outputfile...");

            string result = CreateResultString();

            WriteResult(result, Path.GetFileName(fileName));

            watch.Stop();
            Console.WriteLine($"Time elapsed: \t {watch.ElapsedMilliseconds}");

            Console.WriteLine("-----------------------");
        }

        private static void MakeGreedySlideshow()
        {
            int totalScore = 0;

            Console.WriteLine($"Sorting by tags...");

            Slides = Slides.OrderByDescending(s => s.HashedTags.Length).ToArray();

            var inputList = Slides.ToList();
            var result = new List<Slide>();

            Console.WriteLine($"Comparing tags...");

            result.Add(inputList[0]);
            inputList.RemoveAt(0);

            while (inputList.Any())
            {
                var batch = inputList.Take(Math.Min(3000, inputList.Count)).ToList();
                int batchSize = batch.Count;

                while (batch.Count > batchSize / 2)
                {
                    Slide currentSlide = result.Last();

                    Slide nextSlide = batch[0];
                    int maxScore = ScoreSlides(currentSlide, nextSlide);

                    for (int i = 1; i < batch.Count; i++)
                    {                        
                        int score = ScoreSlides(currentSlide, batch[i]);
                        if (maxScore < score)
                        {
                            nextSlide = batch[i];
                            maxScore = score;
                            if (maxScore >= (int)(currentSlide.HashedTags.Length / 2))
                                break;
                        }
                        else if (maxScore >= (int)(batch[i].HashedTags.Length / 2))
                        {
                            break;
                        }
                    }

                    totalScore += maxScore;

                    result.Add(nextSlide);
                    batch.Remove(nextSlide);
                    inputList.Remove(nextSlide);                    

                    //Console.WriteLine($"Score: {totalScore} | {inputList.Count()} slides to sort...");
                }

                Console.WriteLine($"Score: {totalScore} | {inputList.Count()} slides to sort...");
            }

            Console.WriteLine($"Score: {totalScore} | {inputList.Count()} slides to sort...");

            Slides = result.ToArray();
        }

        private static int ScoreSlides(Slide current, Slide other)
        {
            int equalCount = 0;
            int i = 0;
            int j = 0;

            if (current.HashedTags[current.HashedTags.Length - 1] < other.HashedTags[0]
            || other.HashedTags[other.HashedTags.Length - 1] < current.HashedTags[0])
                return 0;

            while (i < current.HashedTags.Length && j < other.HashedTags.Length)
            {
                if (current.HashedTags[i] == other.HashedTags[j])
                {
                    equalCount++;
                    i++;
                    j++;
                }
                else if (current.HashedTags[i] > other.HashedTags[j])
                {
                    j++;
                }
                else
                {
                    i++;
                }
            }

            return Math.Min(equalCount, Math.Min(current.HashedTags.Length - equalCount, other.HashedTags.Length - equalCount));
        }


        private static Photo[] ReadLinesIntoArray(string[] lines)
        {
            var photos = new Photo[int.Parse(lines.First())];

            for (int p = 1; p < lines.Length; p++)
            {
                int index = p - 1;

                var line = lines[p].Split(" ");
                photos[index] = new Photo
                {
                    Index = index,
                    IsVertical = line[0] == "V",
                    Tags = line.Skip(2).ToArray()
                };
            }

            return photos;
        }

        private static void TransformToSlides()
        {
            Photo previousVertical = null;
            var slides = new List<Slide>();
            for (int i = 0; i < Photos.Length; i++)
            {
                var photo = Photos[i];

                if (photo.IsVertical)
                {
                    if (previousVertical != null)
                    {
                        slides.Add(new Slide(previousVertical, photo));
                        previousVertical = null;
                    }
                    else
                    {
                        previousVertical = photo;
                    }
                }
                else
                {
                    slides.Add(new Slide(photo));
                }
            }

            Slides = slides.ToArray();
        }
        private static void HashTags()
        {
            int totalTags = 0;

            var tagHashMap = new Dictionary<string, int>();
            var nextHash = 0;

            for (int i = 0; i < Photos.Length; i++)
            {
                var photo = Photos[i];

                totalTags += Photos[i].Tags.Length;

                photo.HashedTags = new int[photo.Tags.Length];

                for (int j = 0; j < photo.Tags.Length; j++)
                {
                    var tag = photo.Tags[j];

                    if (!tagHashMap.ContainsKey(tag))
                        tagHashMap.Add(tag, nextHash++);

                    photo.HashedTags[j] = tagHashMap[tag];
                }       

                Array.Sort(photo.HashedTags);
            }

            Console.WriteLine($"Found {totalTags} tags of which {tagHashMap.Keys.Count} are unique");
        }

        private static string CreateResultString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Slides.Length}{Environment.NewLine}");
            foreach (var slide in Slides)
            {
                sb.Append($"{slide.ResultString}{Environment.NewLine}");
            }

            return sb.ToString();
        }

        private static void WriteResult(string output, string fileName)
        {
            File.WriteAllText($"Output\\{fileName}.out", output);
            Console.WriteLine($"Done with: \t {fileName}");
        }
    }
}
