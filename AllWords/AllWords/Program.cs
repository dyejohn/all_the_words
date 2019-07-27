using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AllWords
{
    class Program
    {
        static void Main(string[] args)
        {
            // read all the words and put them in a list.

            var reader = new System.IO.StreamReader("words_alpha.txt");
            int i = 0;
            var wordList = new Dictionary<int, string>( reader.ReadToEnd().Split('\n').Select(w => new KeyValuePair<int,string>(i++, w.Replace("\r",""))));
            reader.Close();

            //whats the longest word?
            var maxWordSize = wordList.Values.Max(x => x.Length);

            // index the word list.
            // letter, the position in the word, and the word in the list it matches.
            Dictionary<char, Dictionary<int, List<int>>> index = new Dictionary<char, Dictionary<int, List<int>>>();


            // flesh out the index;
            foreach (char item in "abcdefghijklmnopqrstuvwxyz")
            {
                index.Add(item, new Dictionary<int, List<int>>());
                
                for(int j=0; j<maxWordSize; j++)
                {
                    index[item].Add(j, new List<int>());
                }
            }

            // for each word, for each letter, record in the list.
            foreach(int item in wordList.Keys)
            {
                for(int j=0; j<wordList[item].Length; j++)
                {
                    index[wordList[item][j]][j].Add(item);
                }
            }



            string oldChoices = "";

            Console.WriteLine("ready");
            string input = "";
            while(input.ToLower() != "exit")
            {
                Console.Write("search pattern:");
                input = Console.ReadLine();
                (string choices,string pattern) = CleanInput(input);

                if(choices == "*" && oldChoices.Length > 0)
                {
                    choices = oldChoices;
                }

                int patternSize = 0;

                if (int.TryParse(pattern, out patternSize ))
                {
                    pattern = new string('_', patternSize);
                }

                // alright. find all the words that match the pattern before blanks.
                var patternFilteredList = CreateFilteredList(index, wordList, pattern);

                var potentialLetters = choices.ToList();

                if(potentialLetters.Count() != pattern.Where(x=> x == '_').Count())
                {
                    foreach(var item in pattern.Where(x=>x != '_').ToList())
                    {
                        potentialLetters.Remove(item);
                    }
                }

                oldChoices = choices;

                var patternWords = new List<string>();
                patternWords.AddRange(FillInSpaces(potentialLetters, pattern));

                // see if any of the patternwords are in patternfilteredlist.
                var matchedWords = patternWords.Where(x => patternFilteredList.Contains(x)).Distinct().ToList();

                if (matchedWords.Count() == 0)
                {
                    Console.WriteLine("no matched words");
                }
                else
                {
                    Console.WriteLine("Matches:");
                    foreach(var word in matchedWords)
                    {
                        Console.WriteLine(word + ",");
                    }
                    Console.WriteLine();
                }

            }
        }

        static List<string> FillInSpaces(List<char> choices, string current)
        {
            if(!current.Contains("_"))
            {
                return new List<string>() { current };
            }
            List<string> results = new List<string>();

            foreach(char choice in choices)
            {
                List<char> subChoices = new List<char>(choices);
                subChoices.Remove(choice);
                var subCurrent = swapFirstCharactor(current, '_', choice);

                results.AddRange(FillInSpaces(subChoices, subCurrent));
            }

            return results;
        }

        static string swapFirstCharactor(string fullString, char find, char replacement)
        {
            char[] response = new char[fullString.Length];
            bool found = false;

            for(int i=0; i<fullString.Length;i++)
            {
                if(found)
                {
                    // don't compare just copy
                    // to speed it up, hopefully
                    response[i] = fullString[i];
                }
                else
                {
                    if(fullString[i] == find)
                    {
                        response[i] = replacement;
                        found = true;
                    }
                    else
                    {
                        response[i] = fullString[i];
                    }
                }
            }

            return new string(response);
        }

        static List<string> CreateFilteredList(Dictionary<char, Dictionary<int, List<int>>> index, Dictionary<int,string> wordList,  string pattern)
        {
            var result = new List<int>(wordList.Keys);

            for(int i=0;i<pattern.Length;i++)
            {
                var letter = pattern[i];

                if (letter != '_')
                {
                    var lettergroup = index[letter][i];
                    var crossoverList = lettergroup.Where(x => result.Contains(x)).ToList();
                    result = crossoverList;
                }

            }

            // turn result into actual words
            var resultWords = result.Select(x => wordList[x]).Distinct().ToList();

            return resultWords;
        }

        static (string, string) CleanInput(string input)
        {
            var separator = ";";
            if(input.Contains("|"))
            {
                separator = "|";
            }

            var splitInput = input.Split(separator);

            return (splitInput[0], splitInput[1]);
        }
    }
}
