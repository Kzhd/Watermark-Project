using System.Collections.Generic;

namespace Email.DEO
{
    public class  EmailDetails
    {
        public Recipients Recipients { get; set; }   
        public string Subject { get; set; }  
        public string Body { get; set; }
        public int Priority { get; set; }   
        public IDictionary<string,string> Attachments { get; set; }
    }
}