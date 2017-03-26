using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLDR_Client
{
    public class RequestObject
    {
        public string content { get; set; }
    }

    public class ResponseObject
    {
        public string summary { get; set; }
        public IEnumerable<KeyPhrase> key_phrases { get; set; }
    }
}
