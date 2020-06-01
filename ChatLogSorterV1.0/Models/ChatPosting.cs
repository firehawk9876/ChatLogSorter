using System;

namespace Models
{
    public class ChatPosting
    {
        public string DateAndTime { get; set; }
        public string Person { get; set; }
        public string Posting { get; set; }

        //feel free to seperate the date and time into their own properties below

        public override string ToString()
        {
            return $"{DateAndTime} > User: {Person} >{Posting}";
        }
    }
}