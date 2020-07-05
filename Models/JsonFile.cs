using System.Collections.Generic;

namespace dsevents.Models
{    
    public class Process    {
        public string ID { get; set; } 

    }

    public class Channel    {
        public string ID { get; set; } 
        public string From { get; set; } 
        public string To { get; set; } 

    }

    public class Message    {
        public string ID { get; set; } 

    }

    public class Event    {
        public string ID { get; set; } 
        public int Seq { get; set; } 
        public string ProcessID { get; set; } 
        public string ChannelID { get; set; } 
        public string MessageID { get; set; } 

    }

    public class JsonRoot    {
        public List<Process> Processes { get; set; } 
        public List<Channel> Channels { get; set; } 
        public List<Message> Messages { get; set; } 
        public List<Event> Events { get; set; } 

    }
}