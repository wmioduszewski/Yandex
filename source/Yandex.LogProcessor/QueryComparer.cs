﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Yandex.LogProcessor
{
    public partial class QueryComparer
    {
        //@"H:\Projects\EDWD\Yandex\data\logs\log-example.txt";
        //@"C:\Users\Wojciech\Desktop\test_output\test_output.txt";
        private static readonly string mode = "test";
        private string _path = @"C:\Users\Wojciech\Desktop\test_output\" + mode + "_output.txt";
        private string _outputPath = @"C:\Users\Wojciech\Desktop\" + mode + "_out.txt";
        private List<int>[] _topUrlsAndTermsQueries;

        public QueryComparer()
        {
            try
            {
                using (StreamReader reader = new StreamReader(@"C:\paths.txt"))
                {
                    _path = reader.ReadLine() + mode + "_output.txt";
                    _outputPath = reader.ReadLine() + mode + "_out.txt";
                }
            }
            catch
            {
            }
        }

        public QueryComparer(string path, string outputPath)
        {
            this._path = path;
            this._outputPath = outputPath;
        }

        public HashSet<int> ReadQueryList()
        {
            _topUrlsAndTermsQueries = new List<int>[200];
            var textReader = new StreamReader(_path);
            var queries = new HashSet<int>();
            var globalStopwatch = new Stopwatch();
            var segmentStopwatch = new Stopwatch();

            for (int i = 0; i < 200; i++)
            {
                _topUrlsAndTermsQueries[i] = new List<int>();
            }

            //******************
            //* urls segment   *
            //******************

            Console.Write("Processing urls... ");
            globalStopwatch.Start();
            segmentStopwatch.Start();

            //eliminate count info
            var line = textReader.ReadLine();
            while (!line.Equals(""))
            {
                line = textReader.ReadLine();
            }

            //read url queries
            for (int i = 0; i < 100; i++)
            {
                //skip the line with description
                var debug = textReader.ReadLine();

                var tmp = textReader.ReadLine();
                while (tmp != null && !tmp.Equals(""))
                {
                    var url = Convert.ToInt32(tmp);
                    _topUrlsAndTermsQueries[i].Add(url);
                    queries.Add(url);
                    tmp = textReader.ReadLine();
                }
            }
            segmentStopwatch.Stop();
            Console.WriteLine("took " + segmentStopwatch.Elapsed.TotalSeconds + "s.");
            Console.WriteLine("Url queries count: " + queries.Count);


            //******************
            //* terms segment  *
            //******************

            Console.Write("Processing terms... ");
            segmentStopwatch.Restart();

            //eliminate count info
            line = textReader.ReadLine();
            while (!line.Equals(""))
            {
                line = textReader.ReadLine();
            }

            //read term queries
            for (int i = 0; i < 100; i++)
            {
                //skip the line with description
                var debug = textReader.ReadLine();

                var tmp = textReader.ReadLine();
                while (tmp != null && !tmp.Equals(""))
                {
                    var term = Convert.ToInt32(tmp);
                    _topUrlsAndTermsQueries[i + 100].Add(term);
                    queries.Add(term);
                    tmp = textReader.ReadLine();
                }
            }

            segmentStopwatch.Stop();
            globalStopwatch.Stop();
            textReader.Close();
            Console.WriteLine("took " + segmentStopwatch.Elapsed.TotalSeconds + "s.");
            Console.WriteLine("Total count: " + queries.Count);
            Console.WriteLine("Total time: " + globalStopwatch.Elapsed.TotalSeconds + "s.");

            return queries;
        }

        public Dictionary<int, List<int>> CreateQueryLists(HashSet<int> queries)
        {
            Console.Write("Computing queries lists... ");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var queriesWithUrlsTermsMapped = new Dictionary<int, List<int>>();

            //foreach top 100 url and top 100 term
            for (int i = 0; i < _topUrlsAndTermsQueries.Length; i++)
            {
                //foreach query which occured in current url/term
                for (int j = 0; j < _topUrlsAndTermsQueries[i].Count; j++)
                {
                    var query = _topUrlsAndTermsQueries[i][j];
                    if (!queriesWithUrlsTermsMapped.ContainsKey(query))
                    {
                        queriesWithUrlsTermsMapped.Add(query, new List<int>());
                    }
                    queriesWithUrlsTermsMapped[query].Add(i);
                }
            }

            foreach (var query in queriesWithUrlsTermsMapped)
            {
                query.Value.Sort();
            }

            stopwatch.Stop();
            Console.WriteLine("took " + stopwatch.Elapsed.TotalSeconds + "s.");
            return queriesWithUrlsTermsMapped;
        }

        private static float CompareTwoLists(List<int> list1, List<int> list2)
        {
            int i = 0, j = 0;
            int sum = 0;
            int all = 0;
            while (i < list1.Count && j < list2.Count)
            {
                if (list1[i] != list2[j])
                {
                    if (list1[i] < list2[j])
                        i++;
                    else
                        j++;
                    all++;
                }
                else
                {
                    sum += 2;
                    all += 2;
                    i++;
                    j++;
                }
            }
            all += list1.Count - i;
            all += list2.Count - j;

            return sum/(float) all;
        }

        public void CompareQueries(Dictionary<int, List<int>> queries)
        {
            const int TO_COMPARE = 100;
            const int N_NEIGHBOURS = 50;
            const int MAX_IN_PROGRESS = 16;

            Console.Write("Comparing queries... ");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var writer = new StreamWriter(_outputPath);

            Semaphore finished = new Semaphore(0, TO_COMPARE);
            Semaphore inProgress = new Semaphore(MAX_IN_PROGRESS - 1, MAX_IN_PROGRESS);

            int count = 0;
            foreach (var q1 in queries)
            {
                if (++count > TO_COMPARE)
                    break;

                KeyValuePair<int, List<int>> element = q1;

                new Thread(delegate()
                {
                    List<Tuple<int, float>> sims = new List<Tuple<int, float>>();
                    foreach (var q2 in queries)
                    {
                        if (q1.Key == q2.Key)
                            continue;
                        var res = CompareTwoLists(q1.Value, q2.Value);
                        sims.Add(new Tuple<int, float>(q2.Key, res));
                        if (sims.Count > 5*N_NEIGHBOURS)
                        {
                            sims.Sort((o1, o2) => { return (int) (1000000*(o2.Item2 - o1.Item2)); });
                            sims.RemoveRange(N_NEIGHBOURS, sims.Count - N_NEIGHBOURS);
                        }
                    }

                    sims.Sort((o1, o2) => { return (int) (1000000*(o2.Item2 - o1.Item2)); });
                    sims.RemoveRange(N_NEIGHBOURS, sims.Count - N_NEIGHBOURS);

                    lock (writer)
                    {
                        writer.WriteLine(q1.Key + ":");
                        foreach (var sim in sims)
                            writer.WriteLine(sim.Item1 + "\t" + sim.Item2);
                        writer.WriteLine();
                    }

                    inProgress.Release();
                    finished.Release();
                }).Start();

                inProgress.WaitOne();
            }

            for (int i = 0; i < TO_COMPARE; i++)
                finished.WaitOne();

            stopwatch.Stop();
            writer.Close();
            Console.WriteLine("took " + stopwatch.Elapsed.TotalSeconds + "s.");
        }
    }
}