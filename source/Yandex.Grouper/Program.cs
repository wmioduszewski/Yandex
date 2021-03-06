﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Yandex.Grouper
{
    internal class Program
    {
        private const string connstr =
            "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=postgres;";

        private static void Main(string[] args)
        {
            var watch = Stopwatch.StartNew();
            string schema = "train";
            using (var writer = new StreamWriter(@"D:\Downloads\group_result_" + schema + ".txt", true))
            {
                //writer.WriteLine(DateTime.Now);
                const int N_THREADS = 8;
                Semaphore end = new Semaphore(0, N_THREADS);
                for (int i = 0; i < N_THREADS; i++)
                {
                    int id = i;
                    int min = (int) (i/(float) (N_THREADS)*200) + 1;
                    int max = (int) ((i + 1)/(float) (N_THREADS)*200) + 1;
                    new Thread(delegate()
                    {
                        using (var g = new Grouper(connstr, schema, writer, id, min, max))
                        {
                            g.group();
                            end.Release();

                            lock (Console.Out)
                            {
                                Console.SetCursorPosition(0, id);
                                Console.WriteLine("Processing ends");
                            }
                        }
                    }).Start();
                }

                for (int i = 0; i < N_THREADS; i++)
                    end.WaitOne();
            }

            Console.SetCursorPosition(0, 10);
            Console.WriteLine("Total time: " + watch.Elapsed);
            Console.ReadLine();
        }
    }
}