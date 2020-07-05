using System;
using dsevents.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Linq;

namespace dsevents
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2) {
                Console.WriteLine("the number of arguments should be 2");
                Console.WriteLine("dsevents <type> <event>");
                return 1;
            }
            string typeArgument = args[0];
            string eventArgument = args[1];
            Models.JsonRoot jsonObject = ReadJsonInput();
            WorkByType(typeArgument, eventArgument, jsonObject);
            return 0;
        }

        private static Models.JsonRoot ReadJsonInput()
        {
            string lineInput;
            string jsonFileContent = "";
            do {
                lineInput = Console.ReadLine();
                jsonFileContent += lineInput;
            } while (lineInput != null);
            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true
            };
            return JsonSerializer.Deserialize <Models.JsonRoot>(jsonFileContent, options);
        }

        private static void WorkByType (string typeArgument, string eventArgument, Models.JsonRoot jsonObject) 
        {
            List<string> events = new List<string>();
            switch (typeArgument)
            {
                case "past":
                    findPastEvents(eventArgument, jsonObject, events);
                    break;
                case "future":
                    findFutureEvents(eventArgument, jsonObject, events);
                    break;
                case "concurrent":
                    findConcurrentEvents(eventArgument, jsonObject, events);
                    break;
                default:
                    Console.WriteLine("Type not found");
                    return;
            }
            events.Remove(eventArgument);
            events = events.Distinct().ToList();
            if (events.Count > 0) {
                PrintEvents(events);
            } else {
                Console.WriteLine("Events not found");
            }
        }

        private static void findPastEvents(string eventArgument, Models.JsonRoot jsonObject, List<string> events)
        {
            int eventArgumentId = findEventIdByName(eventArgument, jsonObject);
            if (eventArgumentId == -1) {
                Console.WriteLine("Event by argumnet not found");
                return;
            }
            for (int j = eventArgumentId; jsonObject.Events[j].Seq != 1; j--)
            {
                if (j - 1 >= 0)
                {
                    events.Add(jsonObject.Events[j - 1].ID);
                }

                if (jsonObject.Events[j].ChannelID != null)
                {
                    for (int k = 0; k < jsonObject.Events.Count; k++)
                    {
                        if (jsonObject.Events[j].ChannelID == jsonObject.Events[k].ChannelID &&
                            jsonObject.Events[j].ID != jsonObject.Events[k].ID)
                        {
                            if (isConnectionEvents(k, jsonObject))
                            {
                                events.Add(jsonObject.Events[k].ID);
                                findPastEvents(jsonObject.Events[k].ID, jsonObject, events);
                            }
                        }
                    }
                }
            }
        }
        private static void findFutureEvents(string eventArgument, Models.JsonRoot jsonObject, List<string> events)
        {
            int eventArgumentId = findEventIdByName(eventArgument, jsonObject);
            if (eventArgumentId == -1) {
                Console.WriteLine("Event by argumnet not found");
                return;
            }
                    
            for (int j = eventArgumentId; (jsonObject.Events[eventArgumentId].ProcessID == jsonObject.Events[j].ProcessID) || (j + 1  >= jsonObject.Events.Count); j++)
            {
                if (j <= jsonObject.Events.Count + 1)
                {
                    events.Add(jsonObject.Events[j].ID);
                }

                if (jsonObject.Events[j].ChannelID != null)
                {
                    for (int k = 0; k < jsonObject.Events.Count; k++)
                    {
                        if (jsonObject.Events[j].ChannelID == jsonObject.Events[k].ChannelID &&
                            jsonObject.Events[j].ID != jsonObject.Events[k].ID)
                        {
                            if (!isConnectionEvents(k, jsonObject))
                            {
                                events.Add(jsonObject.Events[k].ID);
                                findFutureEvents(jsonObject.Events[k].ID, jsonObject, events);
                            }
                        }
                    }
                }
            }
        }

        private static void findConcurrentEvents(string eventArgument, Models.JsonRoot jsonObject, List<string> events) 
        {
            List<string> diffEvents = new List<string>();
            findPastEvents(eventArgument, jsonObject, diffEvents);
            findFutureEvents(eventArgument, jsonObject, diffEvents);
            diffEvents = diffEvents.Distinct().ToList();
            if (diffEvents.Count > 0) {
                for (int i = 0; i < jsonObject.Events.Count; i++) {
                    if (diffEvents.IndexOf(jsonObject.Events[i].ID) == -1) {
                        events.Add(jsonObject.Events[i].ID);
                    }
                }
            } 
        }
        private static bool isConnectionEvents(int pos, Models.JsonRoot jsonObject)
        {
            for (int i = 0; i < jsonObject.Channels.Count; i++)
            {
                if (jsonObject.Channels[i].ID == jsonObject.Events[pos].ChannelID && jsonObject.Channels[i].From == jsonObject.Events[pos].ProcessID)
                {
                    return true;
                }
            }
            return false;
        }

        private static int findEventIdByName(string eventArgument, Models.JsonRoot jsonObject) 
        {
            for (int i = 0; i < jsonObject.Events.Count; i++) {
                if (jsonObject.Events[i].ID == eventArgument) {
                    return i;
                }
            } 
            return -1;  
        }

        private static void PrintEvents(List<string> events)
        {
            foreach (var str in events)
            {
                Console.Write(str + " ");
            }
            Console.WriteLine();
        }
    }
}
