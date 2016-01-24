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

                token.Sentence = ctr;
                isPreceded = false;
            }
        }

        static void performNER()
        {
            java.util.List tokens;
            List<string> values = new List<string>();
            object[] nerValues;
            var classifier = CRFClassifier.getClassifierNoExceptions(@"..\..\NERModel\filipino.all.4class.distsim.crf.ser.gz");

            foreach (Token token in tokenizedArticle)
            {
                values.Add(token.Value);
            }

            tokens = Sentence.toCoreLabelList(values.ToArray());

            nerValues = classifier.classifySentence(tokens).toArray();

            for(int i = 0; i < tokenizedArticle.Count; i++)
            {
                NamedEntity nerValue;
                System.Enum.TryParse(((CoreLabel)nerValues[i]).ner(), out nerValue);

                tokenizedArticle[i].NamedEntity = nerValue;
            }
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
            Dictionary<string, int> frequencies = new Dictionary<string, int>();

            foreach(Token token in tokenizedArticle)
            {
                if(frequencies.ContainsKey(token.Value))
                {
                    frequencies[token.Value]++;
                }
                else
                {
                    frequencies[token.Value] = 1;
                }
            }

            foreach(Token token in tokenizedArticle)
            {
                token.Frequency = frequencies[token.Value];
            }
        }
    }
}
