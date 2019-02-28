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
        }

        private class PhotoMapped
        {
            public int Index { get; set; }
            public bool IsVertical { get; set; }
            public int[] Tags { get; set; }
        }

        private class Slide
        {
            public PhotoMapped[] photos { get; set; }
            public int[] Tags { get; set; }
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
            var mappedPhotos = MapPhotos(Photos);

            Slides = TransformToSlides(mappedPhotos);

            var resultSlides = MakeGreedySlideshow(Slides);

            string result = CreateResultString(resultSlides);

            WriteResult(result, Path.GetFileName(fileName));

            watch.Stop();
            Console.WriteLine($"Time elapsed: \t {watch.ElapsedMilliseconds}");

            Console.WriteLine("-----------------------");
        }

        private static Slide[] MakeGreedySlideshow(Slide[] inputSlides)
        {
            var inputList = inputSlides.ToList();
            var result = new List<Slide>();

            result.Add(inputList[0]);
            inputList.RemoveAt(0);

            while (inputList.Any())
            {
                var batch = inputList.Take(Math.Min(1000, inputList.Count)).ToList();

                while (batch.Any())
                {
                    Slide nextSlide = null;
                    Slide currentSlide = result.Last();

                    int maxScore = ScoreSlides(currentSlide, batch[0]);
                    int i = 1;
                    for (; i < batch.Count; i++)
                    {
                        int score = ScoreSlides(currentSlide, batch[i]);
                        if (maxScore < score)
                        {
                            maxScore = score;
                            if (maxScore >= (int)(currentSlide.Tags.Length / 2))
                                break;
                        };
                    }

                    nextSlide = batch[i - 1];

                    result.Add(nextSlide);
                    batch.Remove(nextSlide);
                    inputList.Remove(nextSlide);

                    Console.WriteLine($"{inputList.Count()} to go...");
                }
            }

            return result.ToArray();
        }

        private static int ScoreSlides(Slide current, Slide other)
        {
            int equalCount = 0;
            int i = 0;
            int j = 0;

            if (current.Tags[current.Tags.Length - 1] < other.Tags[0]
            || other.Tags[other.Tags.Length - 1] < current.Tags[0])
                return 0;

            while (i < current.Tags.Length && j < other.Tags.Length)
            {
                if (current.Tags[i] == other.Tags[j])
                {
                    equalCount++;
                    i++;
                    j++;
                }
                else if (current.Tags[i] > other.Tags[j])
                {
                    j++;
                }
                else
                {
                    i++;
                }
            }

            return Math.Min(equalCount, Math.Min(current.Tags.Length - equalCount, other.Tags.Length - equalCount));
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

        private static Slide[] TransformToSlides(PhotoMapped[] photos)
        {
            PhotoMapped previousVertical = null;
            var slides = new List<Slide>();
            for (int i = 0; i < photos.Length; i++)
            {
                if (photos[i].IsVertical)
                {
                    if (previousVertical != null)
                    {
                        slides.Add(new Slide
                        {
                            photos = new PhotoMapped[] { previousVertical, photos[i] },
                            Tags = previousVertical.Tags.Concat(photos[i].Tags).Distinct().ToArray()
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
                    slides.Add(new Slide
                    {
                        photos = new PhotoMapped[] { photos[i] },
                        Tags = photos[i].Tags
                    });
                }
            }

            return slides.ToArray();
        }
        private static PhotoMapped[] MapPhotos(Photo[] photos)
        {
            var dictionary = new Dictionary<string, int>();
            var index = 0;
            var result = new PhotoMapped[photos.Length];
            foreach (var photo in photos)
            {
                foreach (var tag in photo.Tags)
                {
                    if (!dictionary.ContainsKey(tag))
                    {
                        dictionary.Add(tag, index);
                        index++;
                    }
                }
            }

            result = photos.Select(photo => new PhotoMapped
            {
                Index = photo.Index,
                IsVertical = photo.IsVertical,
                Tags = photo.Tags.Select(tag => dictionary[tag]).OrderBy(x => x).ToArray()
            }).ToArray();


            return result;
        }

        private static string CreateResultString(Slide[] slides)
        {
            string result = $"{Slides.Count()}{Environment.NewLine}";
            foreach (var slide in Slides)
            {
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
