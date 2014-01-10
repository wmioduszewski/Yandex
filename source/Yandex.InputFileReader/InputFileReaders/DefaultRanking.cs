﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Yandex.Utils;
using Yandex.Utils.UserActions;

namespace Yandex.InputFileReader.InputFileReaders
{
    public class DefaultRanking : InputFileReader
    {
        private int _sessionId;
        private StreamWriter _writer;
        private readonly string _output;

        public DefaultRanking(string output)
        {
            _output = output;
        }

        public override void onBeginRead()
        {
            _writer = new StreamWriter(_output);
        }

        public override void onQueryAction(QueryAction queryAction)
        {
            if (queryAction.type == 2)
            {
                for(int i=0;i<queryAction.nUrls;i++)
                _writer.WriteLine(_sessionId +""+ queryAction.urls[i]);
            }
        }

        public override void onMetadata(Metadata metadata)
        {
            _sessionId = metadata.sessionId;
        }

        public override void onEndRead()
        {
            _writer.Flush();
            _writer.Close();
        }
    }
}