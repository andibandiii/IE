using edu.stanford.nlp.ie.crf;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.process;
using edu.stanford.nlp.tagger.maxent;
using IE.Models;
using java.io;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace IE
{
    public static class Preprocessor
    {
        static Article article;
        static List<Token> tokenizedArticle;
        static Annotation annotation;
        static List<Token> candidates;

        internal static void setArticle(Article inputArticle)
        {
            article = inputArticle;
        }

        internal static void setAnnotations(Annotation inputAnnotations)
        {
            annotation = inputAnnotations;
        }

        internal static List<Token> getTokenizedArticle()
        {
            return tokenizedArticle;
        }

        internal static List<Token> getCandidates()
        {
            return candidates;
        }

        internal static void preprocess()
        {
            if (article == null)
            {
                return;
            }

            StanfordTokenizeAndSS();
            //tokenize();
            //performSS();
            performNER();
            performPOST();
            performWS();
            performTokenizeAnnotations();
            performCandidateSelection();

            foreach (var token in tokenizedArticle)
            {
                System.Console.WriteLine("Value: " + token.Value);
                System.Console.WriteLine("Sentence: " + token.Sentence);
                System.Console.WriteLine("Position: " + token.Position);
                System.Console.WriteLine("NER: " + token.NamedEntity);
                System.Console.WriteLine("POS: " + token.PartOfSpeech);
                System.Console.WriteLine("WS: " + token.Frequency);
                System.Console.WriteLine("Who: " + token.IsWho);
                System.Console.WriteLine("=====\n");
            }
        }

        static void StanfordTokenizeAndSS()
        {
            tokenizedArticle = new List<Token>();
            var sentences = MaxentTagger.tokenizeText(new java.io.StringReader(article.Body)).toArray();
            int sentenceCounter = 1;
            int positionCounter = 1;
            foreach (java.util.ArrayList sentence in sentences)
            {
                foreach (var word in sentence)
                {
                    var newToken = new Token(word.ToString(), positionCounter);
                    newToken.Sentence = sentenceCounter;
                    tokenizedArticle.Add(newToken);
                    positionCounter++;
                }
                sentenceCounter++;
            }
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

                //System.Console.WriteLine(label);
            }
        }

        static void performSS()
        {
            int ctr = 1;
            bool isPreceded = false;

            foreach (Token token in tokenizedArticle)
            {
                // Simple sentence segmentation
                if (token.Value.Equals('.'))
                {
                    isPreceded = true;
                    continue;
                }

                if (char.IsUpper(token.Value[0]) && isPreceded)
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

            System.Console.WriteLine("{0}\n", classifier.classifyToString(article.Body));

            for (int i = 0; i < tokenizedArticle.Count; i++)
            {
                //System.Console.WriteLine(((CoreLabel)nerValues[i]).get(typeof(CoreAnnotations.AnswerAnnotation)) + " - " + ((CoreLabel)nerValues[i]).toShorterString());
                //NamedEntity nerValue;
                //System.Enum.TryParse(((CoreLabel)nerValues[i]).get(typeof(CoreAnnotations.AnswerAnnotation)).ToString(), out nerValue);

                tokenizedArticle[i].NamedEntity = ((CoreLabel)nerValues[i]).get(typeof(CoreAnnotations.AnswerAnnotation)).ToString();
            }
        }

        static void performPOST()
        {
            //Path to Filipino Tagger Model
            String modelPath = @"..\..\POSTagger\filipino.tagger";
            MaxentTagger tagger = new MaxentTagger(modelPath);

            //Get all tokens and segregate them into lists based on sentence number
            List<List<Token>> segregatedTokenLists = tokenizedArticle
                .GroupBy(token => token.Sentence)
                .Select(tokenGroup => tokenGroup.ToList())
                .ToList();

            //Convert the lists into a "CoreLabelList" and store in a Dictionary
            //Dictionary Key: Sentence Number
            //Dictionary Value: CoreLabelList
            Dictionary<int, java.util.List> tokenizedSentenceLists = new Dictionary<int, java.util.List>();
            foreach (List<Token> tokenList in segregatedTokenLists)
            {
                if (tokenList.Count > 0)
                {
                    var tokenToStringArray = tokenList.Select(token => token.Value).ToArray();
                    tokenizedSentenceLists[tokenList[0].Sentence] = Sentence.toCoreLabelList(tokenToStringArray);
                }
            }

            //Tag each sentence
            foreach (KeyValuePair<int, java.util.List> entry in tokenizedSentenceLists)
            {
                var taggedSentence = tagger.tagSentence(entry.Value).toArray();
                foreach (var word in taggedSentence)
                {
                    var splitWord = word.ToString().Split('/');
                    if (splitWord.Length >= 2)
                    {
                        foreach (var token in tokenizedArticle)
                        {
                            if ((token.PartOfSpeech == null || token.PartOfSpeech.Length <= 0) &&
                                token.Value.Trim() == splitWord[0].Trim() &&
                                token.Sentence == entry.Key)
                            {
                                token.PartOfSpeech = splitWord[1];
                                break;
                            }
                        }
                    }
                }
            }
        }

        static void performWS()
        {
            Dictionary<string, int> frequencies = new Dictionary<string, int>();

            foreach (Token token in tokenizedArticle)
            {
                if (frequencies.ContainsKey(token.Value))
                {
                    frequencies[token.Value]++;
                }
                else
                {
                    frequencies[token.Value] = 1;
                }
            }

            foreach (Token token in tokenizedArticle)
            {
                token.Frequency = frequencies[token.Value];
            }
        }

        static void performTokenizeAnnotations()
        {
            String who = annotation.Who;
            List<string> whoAnnotations = null;
            string[] whoAnnot = null;
            
            whoAnnot = who.Split(';');
            foreach (string a in whoAnnot)
            {
                whoAnnotations = a.Replace(",", "").Replace(":", "").Replace(".", "").Replace("-", "").Replace("\'", "").Replace("“", "").Replace("”", "").Replace("\"", "").Replace(" ", " ").Split(' ').ToList();
                
                for (int w=0; w < whoAnnotations.Count; w++)
                {
                    if (whoAnnotations[w].Equals(""))
                    {
                        whoAnnotations.RemoveAt(w);
                    }
                }

                int startIndex = 0;
                for (int i = 0; i < tokenizedArticle.Count; i++)
                {
                    if (whoAnnotations != null && (tokenizedArticle[i].Value.Equals(whoAnnotations[0])))
                    {
                        int ctr = 0;
                        for (int j = 0; j < whoAnnotations.Count; j++)
                        {
                            if (tokenizedArticle[i].Value.Equals(whoAnnotations[j]))
                            {
                                if (j == 0)
                                {
                                    startIndex = i;
                                }
                                i++;
                                while (!((tokenizedArticle[i].Value[0] >= 'A' && tokenizedArticle[i].Value[0] <= 'Z') || (tokenizedArticle[i].Value[0] >= 'a' && tokenizedArticle[i].Value[0] <= 'z')))
                                {
                                    i++;
                                    if (tokenizedArticle[i].Value.Equals(whoAnnotations[0]))
                                    { //If it matches the first word of the annotation again
                                        j = 0;
                                        ctr = 0;
                                        startIndex = i;
                                    }
                                }
                                ctr++;
                            }
                            if (ctr == whoAnnotations.Count)
                            {
                                while (startIndex < i)
                                {
                                    tokenizedArticle[startIndex].IsWho = true;
                                    startIndex++;
                                }
                                whoAnnotations = null;
                                break;
                            }
                        }
                    }
                }
            }
        }

        static void performCandidateSelection()
        {
            int startIndex = 0;
            int endIndex = 0;

            for (int i=0; i < tokenizedArticle.Count; i++)
            {
                if(tokenizedArticle[i].NamedEntity == "PER" || tokenizedArticle[i].NamedEntity == "ORG")
                {
                    startIndex = i;
                    

                }
            }
        }
    }
}
