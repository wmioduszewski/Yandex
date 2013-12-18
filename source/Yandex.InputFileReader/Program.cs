﻿using System;
using System.Diagnostics;

namespace Yandex.InputFileReader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string data = "train";
            InputFileReader[] readers = new InputFileReader[]
            {
                //new TopUrlsGetter(@"D:\Downloads\EDWD\"+data+"_top_urls_1.txt"),
                //new TopTermsGetter(@"D:\Downloads\EDWD\"+data+"_top_terms_1.txt"),
                //new TopDomainsGetter(@"D:\Downloads\EDWD\"+data+"_top_domains_1.txt"),
                //new QueriesExtractor(@"D:\Downloads\EDWD\"+data+"_queries"),
                new UsersNTerms(@"D:\Downloads\EDWD\" + data + "_users2terms.txt"),
            };

            String filename = @"D:\Downloads\EDWD\" + data + "_tr";

            foreach (var reader in readers)
            {
                var watch = Stopwatch.StartNew();
                using (InputFileOpener opener = new InputFileOpener(filename,
                    reader))
                {
                    opener.read();
                }
                watch.Stop();
                Console.WriteLine("Time {0}", watch.Elapsed);
            }

            Console.ReadLine();
        }
    }
}