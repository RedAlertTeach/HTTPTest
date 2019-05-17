using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using Newtonsoft.Json;


namespace HTTPTest
{
    [ServiceContract]

    // define the interface for the service contract
    public interface IService
    {
        [OperationContract]
        // WebInvoke maps to HTTP POST
        [WebInvoke]

        // endpoint to accept a JSON payload and count words per sentence
        string word_count_per_sentence(string s);

        [OperationContract]
        [WebInvoke]

        // endpoint to accept a JSON payload and count total letters
        string total_letter_count(string s);
    }

    // implement the serice contract interface
    public class Service : IService
    {
        public string word_count_per_sentence(string s)
        {
            // deserialize the JSON payload to a key/value pair object
            KeyValuePair keyValuePair = JsonConvert.DeserializeObject<KeyValuePair>(s);

            // get the key
            string key = keyValuePair.key;

            // create a new SentenceCounts object to hold the counts for all sentences
            SentenceCounts sentenceCounts = new SentenceCounts();

            // set the key in the SentenceCounts object
            sentenceCounts.key = key;

            // split the value from the key/value pair into sentences (defined by '?', '!', '.')
            string[] sentences = keyValuePair.value.Split(new char[] { '?', '!', '.' }, StringSplitOptions.RemoveEmptyEntries);

            // go through and get the word count for each sentence in the array
            foreach (string sentence in sentences)
            {
                // split each sentence into words, defined by ' ' and '-' (hardcoded Gettysburg address contains dashes)
                string[] words = sentence.Split(new char[] { ' ', '-'}, StringSplitOptions.RemoveEmptyEntries);

                // use LINQ query to get the key matching word count for each sentence
                int wordCount = (from word in words
                                 where word.ToLower() == key.ToLower()   // allow for any case
                                 select word).Count();

                // create a new SentenceCount object with the matching word count and sentence text
                SentenceCount sentenceCount = new SentenceCount();
                sentenceCount.count = wordCount;
                sentenceCount.sentence = sentence;

                // add the SenctenceCount object to the SentenceCounts list
                sentenceCounts.counts.Add(sentenceCount);
                
            }
            // turn the SentenceCounts object back into a JSON payload
            return JsonConvert.SerializeObject(sentenceCounts, Formatting.Indented);
        }

        public string total_letter_count(string s)
        {
            // deserialize the JSON payload to a key/value pair object
            KeyValuePair keyValuePair = JsonConvert.DeserializeObject<KeyValuePair>(s);

            // declare a new LetterCounts object to hold a list of all letters and their counts
            LetterCounts letterCounts = new LetterCounts();

            
                        
            // set the key (??) 
            letterCounts.key = keyValuePair.key;

            // iterate through each letter in the text 
            foreach (char c in keyValuePair.value)
            {
                // see if the LetterCounts list already contains the letter

                // only get counts for alphabetic characters
                if (char.IsLetter(c))
                {
                    // get the letter count using a LINQ query
                    int count = (from n in keyValuePair.value
                                 where char.ToLower(n) == char.ToLower(c)    // match all cases
                                 select n).Count();


                    // each letter and its count will be a new object (LetterCount)
                    LetterCount letterCount = new LetterCount();

                    // set the letter and get its count from a LINQ query on the entire text value
                    letterCount.letter = char.ToLower(c);
                    letterCount.count = count;

                    // add the LetterCount object to the LetterCounts list
                    letterCounts.AddToList(letterCount);
                }
            }

            // turn the LetterCounts object back into a JSON payload
            return JsonConvert.SerializeObject(letterCounts, Formatting.Indented);

        }

    } 
    // class to hold the key/value pair
    public class KeyValuePair
    {
        public string key { get; set; }
        public string value { get; set; }
        
    }

    // class that holds the list of sentences and their key-matching counts
    public class SentenceCounts
    {
        public string key { get; set; }
        public List<SentenceCount> counts = new List<SentenceCount>();

    }

    // class that holds one sentence and its key-matching count
    public class SentenceCount
    {
        public string sentence { get; set; }
        public int count{ get; set; }
     
    }

    // class that holds the list of letter counts
    public class LetterCounts
    {
        public string key { get; set; }    // not sure why we need this key, but it is in the spec
        public List<LetterCount> counts = new List<LetterCount>();

        // use this string to determine if we already have an entry for the given letter
        private string alphabet = "abcdefghijklmnopqrstuvwxyz"; 


        public void AddToList(LetterCount lc)
        {
            // if the alphabet string still contains the requested letter, we don't have an entry for it
            // so we can add it
            // replace the given letter in the alphabet string with a space
            // so next time we don't add the object again
            if (alphabet.Contains(lc.letter))
            {
                counts.Add(lc);
                alphabet = alphabet.Replace(lc.letter, ' ');
            }
        }
    }

    // class that holds each letter and its count
    public class LetterCount
    {
      //  public string sentence { get; set; }
        public char letter { get; set; }
        public int count { get; set; }

    }
    // runner
    class Program
    {
        static void Main(string[] args)
        {
            // create a new web services host using our Service class and the localhost Uri
            WebServiceHost host = new WebServiceHost(typeof(Service), new Uri("http://localhost:8000/"));

            // make sure we allow for errors
            try
            {
                // create a WebHTTPBinding for JSON
                ServiceEndpoint ep = host.AddServiceEndpoint(typeof(IService), new WebHttpBinding(), "");
                host.Open();

                // create a channel factory so we can open a channel
                using (ChannelFactory<IService> cf = new ChannelFactory<IService>(new WebHttpBinding(), "http://localhost:8000"))
                {
                    cf.Endpoint.Behaviors.Add(new WebHttpBehavior());

                    // associate our service with a channel
                    IService channel = cf.CreateChannel();

                    // declare a KeyValuePair to hold the key/value pair
                    KeyValuePair kvInput = new KeyValuePair();

                    // search for the word "we" in the Gettysburg Address
                    kvInput.key = "we";
                    kvInput.value = @"Fourscore and seven years ago our fathers brought forth on this continent, " +
                @"a new nation, conceived in Liberty, and dedicated to the proposition that all men are " +
                @"created equal.Now we are engaged in a great civil war, testing whether that nation, or any " +
                @"nation so conceived and so dedicated, can long endure. We are met on a great battle-field of " +
                @"that war. We have come to dedicate a portion of that field, as a final resting place for those " +
                @"who here gave their lives that that nation might live. It is altogether fitting and proper that " +
                @"we should do this.But, in a larger sense, we cannot dedicate-we cannot consecrate-we cannot " +
                @"hallow-this ground. The brave men, living and dead, who struggled here, have consecrated it, " +
                @"far above our poor power to add or detract. The world will little note, nor long remember what " +
                @"we say here, but it can never forget what they did here. It is for us the living, rather, to be " +
                @"dedicated here to the unfinished work which they who fought here have thus far so nobly advanced. " +
                @"It is rather for us to be here dedicated to the great task remaining before us-that from these " +
                @"honored dead we take increased devotion to that cause for which they gave the last full measure " +
                @"of devotion-that we here highly resolve that these dead shall not have died in vain-that this " +
                @"nation, under God, shall have a new birth of freedom-and that government of the people, by the " +
                @"people, for the people shall not perish from the earth.";


                    // turn the key/value pair object into a JSON payload (string)
                    string json = JsonConvert.SerializeObject(kvInput, Formatting.Indented);

                    // declare a string to hold the results of our POST calls
                    string s;

                    // send the JSON payload to the channel via POST (WebInvoke)
                    // to calculate the word count per sentence
                    s = channel.word_count_per_sentence(json);

                    // show the results
                    Console.WriteLine("The word counts per sentence are:\n{0}", s);
                    Console.WriteLine("");

                    // send the JSON payload to the channel via POST (WebInvoke)
                    // to count the occurrences of each letter
                    s = channel.total_letter_count(json);

                    // show the results
                    Console.WriteLine("The total letter counts are:\n{0}", s);
                    Console.WriteLine("");
                }
                // do not terminate the application until the user hits enter
                Console.WriteLine("Press <ENTER> to terminate");
                Console.ReadLine();

                // close the web services host
                host.Close();
            }
            catch (CommunicationException cex)
            {
                Console.WriteLine("An exception occurred:\n {0}\nPress enter to terminate.", cex.Message);
                // allow the user time to read the exception text if one occurs
                Console.ReadLine();

                // clean up the web services host even if an exception occurs
                host.Abort();
            }
        }
    }
}
