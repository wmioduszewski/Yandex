﻿using System;
using System.IO;

namespace Yandex.Utils
{
    public static class PathResolver
    {
        /// <summary>
        /// UserMatrixOutput_processed property
        /// </summary>
        public static readonly string UserMatrixOutputProcessed = GetPath("UserMatrixOutput_processed");

        /// <summary>
        /// The UserMatrix property.
        /// </summary>
        public static readonly string UserMatrix = GetPath("UserMatrix");

        /// <summary>
        /// The UserMatrixOutput property.
        /// </summary>
        public static readonly string UserMatrixOutput = GetPath("UserMatrixOutput");

        /// <summary>
        /// The path to train binary processed file
        /// </summary>
        public static readonly string TrainProcessedFile = GetPath("TrainProcessedFile");

        /// <summary>
        /// The path to folder where parts of the processed file will be stored
        /// </summary>
        public static readonly string DataPartsFolder = GetPath("PartFilesOutputFolder");

        private const string LogMapPath = @"C:\$EDWD_logs\LogsMap.txt";
        
        private static string GetPath(string pathId)
        {
            if (pathId == null) throw new ArgumentNullException("pathId");

            var reader = new StreamReader(LogMapPath);
            string res = null;
            while (reader.Peek() != -1)
            {
                var prop = reader.ReadLine();
                var commentIndex = prop.IndexOf('/');
                if (commentIndex > 0)
                    prop = prop.Substring(0, commentIndex);
                var arr = prop.Split('=');
                if (!pathId.Equals(arr[0].Trim())) continue;
                res = arr[1].Trim();
                break;
            }
            if (string.IsNullOrWhiteSpace(res)) throw new Exception("Property " + pathId + " not found in " + LogMapPath);
            
            return res;
        }
    }
}