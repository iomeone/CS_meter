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

                jobs.Add(GetResultsFromDemo(demo, newFilePath));
            }

            Task.WaitAll(jobs.ToArray());
        }

        private static IEnumerable<FileInfo> GetDemoFiles(DirectoryInfo directory)
            => directory.EnumerateFiles().Where(d => Path.GetExtension(d.Name).Equals(".dem"));

        private static Task GetResultsFromDemo(FileInfo demoFile, string outputFile)
            => Task.Factory.StartNew(() =>
            {
                var ms = new MatchScanner(demoFile.FullName);
                var lst = ms.EnumerateTrainingResults().ToList();

                using (var fs = new FileStream(outputFile, FileMode.Create))
                using (var sw = new StreamWriter(fs))
                {
                    var serializer = new CsvWriter(sw);
                    serializer.WriteRecords(lst);
                }
            }, TaskCreationOptions.LongRunning);
    }
}
