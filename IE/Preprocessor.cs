using edu.stanford.nlp.ie.crf;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.process;
using edu.stanford.nlp.tagger.maxent;
using IE.Models;
using java.io;
using System;
using System.Collections.Generic;

namespace IE
{
    public static class Preprocessor
    {
        static Article article;
        static List<Token> tokenizedArticle;

        internal static void setArticle(Article inputArticle)
        {
            article = inputArticle;
        }

        internal static List<Token> getTokenizedArticle()
        {
            return tokenizedArticle;
        }

        internal static void preprocess()
        {
            if(article == null)
            {
                return;
            }

            tokenize();
            performSS();
            performNER();
            performPOST();
            performWS();
        }

        static void tokenize()
        {
            tokenizedArticle = new List<Token>();
            int ctr = 0;

            PTBTokenizer ptbt = new PTBTokenizer(new StringReader(article.Body), new CoreLabelTokenFactory(), "");

            while (ptbt.hasNext())
            {
                ctr++;
                CoreLabel label = ((CoreLabel)ptbt.next());
                tokenizedArticle.Add(new Token(label.toString(), ctr));
                
                System.Console.WriteLine(label);
            }
        }

        static void performSS()
        {
            int ctr = 1;
            bool isPreceded = false;

            foreach(Token token in tokenizedArticle)
            {
                token.Sentence = ctr;

                // Simple sentence segmentation
                if (token.Value.Equals('.'))
                {
                    isPreceded = true;
                    continue;
                }

                if(char.IsUpper(token.Value[0]) && isPreceded)
                { 
                    ctr++;
                }

                isPreceded = false;
            }
        }

        static void performNER()
        {
            // Path to the folder with classifiers models
            var jarRoot = @"..\..\..\..\paket-files\nlp.stanford.edu\stanford-ner-2015-12-09";
            var classifiersDirecrory = jarRoot + @"\classifiers";

            // Loading 3 class classifier model
            var classifier = CRFClassifier.getClassifierNoExceptions(
                classifiersDirecrory + @"\english.all.3class.distsim.crf.ser.gz");

            var s1 = "Good afternoon Rajat Raina, how are you today?";
            System.Console.WriteLine("{0}\n", classifier.classifyToString(s1));

            var s2 = "I go to school at Stanford University, which is located in California.";
            System.Console.WriteLine("{0}\n", classifier.classifyWithInlineXML(s2));

            System.Console.WriteLine("{0}\n", classifier.classifyToString(s2, "xml", true));
        }

        static void performPOST()
        {
            //Path to Filipino Tagger Model
            String modelPath = @"..\..\POSTagger\filipino.tagger";
            MaxentTagger tagger = new MaxentTagger(modelPath);

            //Sample Text to Tag
            String text = "Sinabi ni Pangulong Arroyo kahapon na inatasan niya si Vice President Noli de Castro na pumuntang Libya para tingnan ang posibilidad kung may mga oportunidad ng trabaho ang mga Pilipinong manggagawa sa bansa.";

            Dictionary<String, String> tokenToTag = new Dictionary<String, String>();

            //Segment text into sentences and break down each sentence into tokens
            var sentences = MaxentTagger.tokenizeText(new java.io.StringReader(text)).toArray();

            //Tag each sentence's tokens and add it to the Dictionary
            foreach (java.util.ArrayList sentence in sentences)
            {
                var taggedSentence = tagger.tagSentence(sentence).toArray();
                var convertedTaggedSentence = new List<String>();
                foreach (var word in taggedSentence)
                {
                    var splitWord = word.ToString().Split('/');
                    if (splitWord.Length >= 2)
                    {
                        tokenToTag[splitWord[0]] = splitWord[1];
                    }
                }
            }

            // TEMPORARY CODE FOR PRINTING OUT THIS FUNCTION'S RESULTS
            foreach (KeyValuePair<String, String> entry in tokenToTag)
            {
                System.Console.WriteLine(entry.Key + " - " + entry.Value);
            }
        }

        static void performWS()
        {

        }
    }
}
