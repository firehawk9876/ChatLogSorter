using System.Collections.Generic;

namespace Models
{
    public class LogFile
    {
        public string FileName { get; set; }
        public List<ChatPosting> Contents { get; set; }
    }
}
