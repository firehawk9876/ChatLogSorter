using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Sorter
{
    public static class Globals
    {
        static public string SaveFilePath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName + @"\Maps\Paths.txt";
    }
    class Sorter
    {
        static void PromptSetLogPath()
        {
            Console.WriteLine("\nIn order to work, I need to know where to look for the logs\n");
            Console.WriteLine("Could you paste down below the absolute path to the logs? (can copy paste from the log properties)");
            Console.WriteLine(@"should be somewhere like... Documents\eve\logs\chatlogs");
            Console.WriteLine("\nI won't work properly if pointed in the wrong place (duh)\n");
            string userin = Console.ReadLine();
            if (Path.IsPathFullyQualified(userin))
            {
                File.WriteAllText(Globals.SaveFilePath, (userin.Substring(userin.LastIndexOf(@"\")) == @"\Chatlogs") ? userin : userin + @"\Chatlogs");
            }
            else
            {
                Console.Clear();
                Console.WriteLine("that wasn't a fully qualified path");
                PromptSetLogPath();
            }
            Console.Clear();
        }
        static string PromptSearchDay()
        {
            Console.WriteLine("enter a day to find the logs for (format: dd/mm/yyyy):");
            getinput:
            string userin = Console.ReadLine();
            if(Regex.Matches(userin, "/").Count == 2){
                return FormatDay(userin);
            }
            else
            {
                Console.WriteLine("I need at least 2 \"/\"s");
                goto getinput;
            }
        }
        static string FormatDay(string input)
        {
            string day, month, year;
            day = (input[..(input.IndexOf('/'))].Length < 2) ? '0' + input[..(input.IndexOf('/'))] : input[..(input.IndexOf('/'))];
            month = (input[(input.IndexOf('/') + 1)..input.LastIndexOf('/')].Length < 2) ? '0' + input[(input.IndexOf('/') + 1)..input.LastIndexOf('/')] : input[(input.IndexOf('/') + 1)..input.LastIndexOf('/')];
            year = (input.Substring(input.LastIndexOf('/') + 1).Length == 2) ? Convert.ToString(DateTime.Now.Year % 100) + input.Substring(input.LastIndexOf('/') + 1) : input.Substring(input.LastIndexOf('/') + 1);
            return year + month + day;
        }
        static LogFile FormatFile(string Item)
        {
            List<ChatPosting> postings = new List<ChatPosting>();
            foreach (string post in File.ReadAllLines(Item).Where(x=>!x.StartsWith(' ')&&x!=""&&x[..2].Contains('[')))
            {
                postings.Add(new ChatPosting {
                    DateAndTime=post[(post.IndexOf('[')+1)..(post.IndexOf(']')-1)].Trim(),
                    Person=post[(post.IndexOf(']')+1)..(post.IndexOf('>')-1)].Trim(),
                    Posting=post[(post.IndexOf('>')+1)..]
                });
            }

            return new LogFile() {
                FileName = Item.Substring(Item.LastIndexOf('\\') + 1), Contents=postings
            };
        }
        static void Initilize()
        {
            //grabs the location of the saved path
            Console.WriteLine("Hello, this is a chat log sorter for eve onlines chat logs.");
            Thread.Sleep(3000);
            Console.Clear();

            if (File.Exists(Globals.SaveFilePath) && new FileInfo(Globals.SaveFilePath).Length > 0)
            {
                bool flag = false;

                do
                {
                    Console.Write("I have a path from a previous run. Shall I use that? Y/N: ");
                    switch (Console.ReadLine())
                    {
                        case "y":
                        case "Y":
                            Console.WriteLine($"\nChecking from {File.ReadAllText(Globals.SaveFilePath)}");
                            Thread.Sleep(3000);
                            Console.Clear();
                            flag = true;
                            break;
                        case "n":
                        case "N":
                            PromptSetLogPath();
                            flag = true;
                            break;
                        default:
                            Console.WriteLine("I need a 'y' or a 'n' bub...");
                            break;
                    }
                } while (!flag);
            }
            else
            {
                //if lookup file needs a path. get it here
                PromptSetLogPath();
            }

        }
        static List<LogFile> GetDaysLogs(string DirectoryPath, string Date)
        {
            List<LogFile> FoundFiles = new List<LogFile>();
            foreach(string file in Directory.GetFiles(DirectoryPath,".",SearchOption.AllDirectories).Where(x=>x.Contains(Date)))
            {
                FoundFiles.Add(FormatFile(file));
            }
            return FoundFiles;
        }
        static List<LogFile> SortFiles(List<LogFile> logFiles, string[] Chats,string[] Players, string[] MessageContents)
        {
            if (!(Chats.Length == 1 && Chats[0] == ""))
            {
                logFiles = logFiles.Where(x => Chats.Any(y => x.FileName.Contains(y))).ToList();
            }
            if (!(Players.Length == 1 && Players[0] == ""))
            {
                foreach (LogFile item in logFiles)
                {
                    item.Contents = item.Contents.Where(x => Players.Any(y => x.Person == y)).ToList();
                }
            }
            if (!(MessageContents.Length == 1 && MessageContents[0] == ""))
            {
                foreach(LogFile item in logFiles)
                {
                    item.Contents = item.Contents.Where(x => MessageContents.Any(y => x.Posting.Contains(y))).ToList();
                }
            }
            logFiles = logFiles.Where(x => x.Contents.Count > 0).ToList();

            return logFiles;
        }

        static void Main(string[] args)
        {

            Initilize();

            //find which day to find files for
            bool getday;
            do
            {
                getday = false;

                string SearchDate = PromptSearchDay();
                Console.Clear();
                //get those files
                List<LogFile> FoundFiles = GetDaysLogs(File.ReadAllText(Globals.SaveFilePath), SearchDate);
                Console.WriteLine("getting files...");
                Console.Clear();
                //prompt the user the program is ready to sort the found files
                Console.WriteLine("I'm ready to start sorting!\n");
                bool getparams;
                do
                {
                    getparams = false;
                    //get the sorting params from the user
                    Console.WriteLine("What do you want to sort by? (case sensitive)");
                    Console.WriteLine("(leave empty to skip or comma separated to add multiple)");
                    Console.Write("Chat channel name(s): ");
                    string[] SortChatNames = Console.ReadLine().Split(",");
                    Console.Write("PlayerName(s): ");
                    string[] SortPlayerNames = Console.ReadLine().Split(",");
                    Console.Write("Message Contents: ");
                    string[] SortMessageContents = Console.ReadLine().Split(",");
                    Console.Clear();

                    //start sorting and give results
                    Console.WriteLine("Here's what I found:");
                    var SortedFiles = SortFiles(FoundFiles, SortChatNames, SortPlayerNames, SortMessageContents);
                    foreach (LogFile logFile in SortedFiles)
                    {
                        Console.WriteLine("\n-------------------------" + logFile.FileName + ":---------------------");
                        foreach (var contents in logFile.Contents)
                        {
                            Console.WriteLine("\t" + contents);
                        }
                    }
                    Console.WriteLine("\n\nWhat would you like to do next:");
                    Console.WriteLine("\t1)Search a different date");
                    Console.WriteLine("\t2)do another search on the same day");
                    Console.WriteLine("\tanything else) exit");
                    switch (Console.ReadLine())
                    {
                        case "1":
                            getday = true;
                            break;
                        case "2":
                            getparams = true;
                            break;
                        default:
                            break;
                    }
                    Console.Clear();
                    GC.Collect();
                } while (getparams);       
            } while (getday);
            Console.WriteLine("Goodbye");
            Thread.Sleep(2000);
        }
    }
}
