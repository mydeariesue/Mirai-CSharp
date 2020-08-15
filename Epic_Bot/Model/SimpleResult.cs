using System;
using System.Collections.Generic;
using System.Text;

namespace Epic_Bot.Model
{
    public class SimpleResult
    {
        public int status { get; set; }
        public SimpleResultData data { get; set; }
    }

    public class SimpleResultData
    {
        public string message { get; set; }
        public int code { get; set; }
        public bool success { get; set; }
    }
}
