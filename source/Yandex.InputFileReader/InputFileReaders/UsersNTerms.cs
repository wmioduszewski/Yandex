﻿using System;
using System.Collections.Generic;
using System.IO;
using Yandex.Utils;

namespace Yandex.InputFileReader
{
    public class UsersNTerms : InputFileReader
    {
        private List<List<int>> usersTerms = null;
        private List<int> currentList;
        private string outputFile;

        public UsersNTerms(String outputFile)
        {
            this.outputFile = outputFile;
        }

        public override void onBeginRead()
        {
            usersTerms = new List<List<int>>();
            currentList = null;
        }

        public List<int> getList(int userId)
        {
            while (usersTerms.Count < userId)
                usersTerms.Add(null);

            if (usersTerms.Count == userId)
                usersTerms.Add(new List<int>());

            List<int> list = usersTerms[userId];
            if (list == null)
            {
                list = new List<int>();
                usersTerms[userId] = list;
            }

            return list;
        }

        public override void onMetadata(BufferedBinaryReader reader)
        {
            // TYPE
            reader.ReadByte();

            // SESSION_ID
            reader.ReadInt32();

            // DAY
            reader.ReadInt32();

            // USER
            int userId = reader.ReadInt32();
            currentList = getList(userId);
        }

        public override void onQueryAction(BufferedBinaryReader reader)
        {
            // TYPE
            reader.ReadByte();

            // SESSION_ID
            reader.ReadInt32();

            // TIME
            reader.ReadInt32();
            // SERPID
            reader.ReadInt32();
            // QUERYID
            reader.ReadInt32();

            for (int i = reader.ReadInt32(); i > 0; i--)
            {
                // TERM ID
                int termId = reader.ReadInt32();
                currentList.Add(termId);
            }

            for (int i = reader.ReadInt32(); i > 0; i--)
            {
                // URL_ID
                reader.ReadInt32();

                // DOMAIN_ID
                reader.ReadInt32();
            }
        }

        public override void onEndRead()
        {
            List<Tuple<int, List<int>>> tmpList = new List<Tuple<int, List<int>>>();

            for (int i = 0; i < usersTerms.Count; i++)
            {
                var list = usersTerms[i];
                if (list == null)
                    continue;
                usersTerms[i] = null;
                tmpList.Add(new Tuple<int, List<int>>(i, list));
            }

            tmpList.Sort((o1, o2) =>
            {
                int c1 = o1.Item2.Count;
                int c2 = o2.Item2.Count;
                if (c1 < c2)
                    return 1;
                if (c1 > c2)
                    return -1;
                return o1.Item1 - o2.Item1;
            }
                );

            usersTerms = null;
            GC.Collect();

            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                int counter = 0;

                foreach (var element in tmpList)
                {
                    counter++;
                    if (counter%10000 == 0)
                    {
                        Console.Write("Finalizing: {0} %\r", (100.0f*counter/tmpList.Count).ToString("0.000"));
                    }

                    List<int> list = element.Item2;
                    list.Sort();
                    List<Tuple<int, int>> occurrences = new List<Tuple<int, int>>();
                    int index = 0;
                    while (index < list.Count)
                    {
                        int start = index;
                        while (index < list.Count - 1)
                        {
                            if (list[index] != list[index + 1])
                                break;
                            index++;
                        }

                        occurrences.Add(new Tuple<int, int>(list[index], index - start + 1));
                        index++;
                    }

                    writer.WriteLine("User {0}:", element.Item1);
                    foreach (var occurrence in occurrences)
                        writer.WriteLine("{0}\t{1}", occurrence.Item1, occurrence.Item2);
                    writer.WriteLine();
                }
            }

            tmpList = null;
            GC.Collect();
        }
    }
}