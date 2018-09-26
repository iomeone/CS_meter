using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using DemoInfo;

namespace parse_demos
{
    public class Program
    {
        private const int NumConcurrentParsers = 10;

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("parse-demos.dll reads .dem files from an input folder and outputs csv files with training data to an output folder." +
                    " If the destination csv file already exists the corresponding demo file is not re-parsed and the existing file is untouched.");
                Console.WriteLine("");
                Console.WriteLine("Usage: dotnet parse-demos.dll <demos-folder> <csv-output-folder>");
                return;
            }

            DirectoryInfo inputFolder, outputFolder;

            try
            {
                inputFolder = new DirectoryInfo(args[0]);
                outputFolder = new DirectoryInfo(args[1]);
            }
            catch
            {
                Console.WriteLine("Directory is invalid");
                return;
            }

            if (!inputFolder.Exists)
            {
                Console.WriteLine("Input folder does not exist");
                return;
            }

            if (!outputFolder.Exists)
            {
                outputFolder.Create();
            }

            List<Task> jobs = new List<Task>();

            foreach (var demo in GetDemoFiles(inputFolder))
            {
                var newFilePath = Path.Combine(outputFolder.FullName, demo.Name + ".csv");

                if (File.Exists(newFilePath))
                {
                    continue;
                }

                if (jobs.Count >= NumConcurrentParsers)
                {
                    int i = Task.WaitAny(jobs.ToArray());
                    jobs.RemoveAt(i);
                }

                // The memory steram gets disposed by each individual task, so won't use a `using` block for it.
                var ms = new MemoryStream();

                using (var fs = demo.Open(FileMode.Open))
                {
                    Console.WriteLine("Reading demo file " + demo.Name);

                    fs.CopyTo(ms);
                    ms.Position = 0;

                    Console.WriteLine("Parsing demo file " + demo.Name);

                    jobs.Add(GetResultsFromDemo(ms, newFilePath));
                }
            }

            Task.WaitAll(jobs.ToArray());
        }

        private static IEnumerable<FileInfo> GetDemoFiles(DirectoryInfo directory)
            => directory.EnumerateFiles().Where(d => Path.GetExtension(d.Name).Equals(".dem"));

        private static Task GetResultsFromDemo(MemoryStream inputStream, string outputFile)
            => Task.Factory.StartNew(() =>
            {
                try
                {
                    using (var ms = new MatchScanner(inputStream))
                    {
                        var lst = ms.EnumerateTrainingResults().ToList();

                        using (var outputStream = new FileStream(outputFile, FileMode.Create))
                        using (var outputWriter = new StreamWriter(outputStream))
                        {
                            var serializer = new CsvWriter(outputWriter);
                            serializer.WriteRecords(lst);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unhandled exception -> " + e.GetType().ToString() + ": " + e.Message);
                }
                finally
                {
                    inputStream.Dispose();
                }
            }, TaskCreationOptions.LongRunning);
    }
}
