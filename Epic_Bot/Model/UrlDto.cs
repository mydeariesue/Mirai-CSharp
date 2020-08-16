using System;
using System.Collections.Generic;
using System.Text;

namespace Epic_Bot.Model
{
    public class UrlDto
    {
        public string Url { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }

    public class ApiUrl
    {
        public string Url { get; set; }
        public string Type { get; set; }
        public string Order { get; set; }
    }
}
