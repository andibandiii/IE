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

            tokenizeAndSS();
            performNER();
            performPOST();
            performWS();
            performTokenizeAnnotations();
            performWhoCandidateSelection();

            foreach (var token in tokenizedArticle)
            {
                System.Console.WriteLine("Value: " + token.Value);
                System.Console.WriteLine("Sentence: " + token.Sentence);
                System.Console.WriteLine("Position: " + token.Position);
                System.Console.WriteLine("NER: " + token.NamedEntity);
                System.Console.WriteLine("POS: " + token.PartOfSpeech);
                System.Console.WriteLine("WS: " + token.Frequency);
                System.Console.WriteLine("=====\n");
            }
        }

        static void tokenizeAndSS()
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

        static void performNER()
        {
            java.util.List tokens;
            List<string> values = new List<string>();
            object[] nerValues;
            var classifier = CRFClassifier.getClassifierNoExceptions(@"..\..\NERModel\filipino.ser.gz");

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
            string[] whoAnnotations = null;
            
            whoAnnotations = who.Split(';');

            for (int r=0; r< whoAnnotations.Length; r++)
            {
                if (whoAnnotations[r][0] == ' ')
                {
                    whoAnnotations[r] = whoAnnotations[r].Substring(1);
                }
                System.Console.WriteLine("WHO ANNOTATIONS-" + whoAnnotations[r]);
            }    
            
                    
        }

        static void performWhoCandidateSelection()
        {
            candidates = new List<Token>();
            int startIndex = 0;
            int endIndex = 0;
            
            string strValue = "";
            int tempWs = 0;
            for (int i=0; i < tokenizedArticle.Count; i++)
            {
                
                if(tokenizedArticle[i].NamedEntity == "PER" || tokenizedArticle[i].NamedEntity == "ORG")
                {
                    startIndex = i;
                    strValue = tokenizedArticle[i].Value;
                    tempWs = tokenizedArticle[i].Frequency;

                    while(tokenizedArticle[i].NamedEntity == tokenizedArticle[i+1].NamedEntity)
                    {
                        i++;
                        strValue += " " + tokenizedArticle[i].Value;
                        if (tokenizedArticle[i].Frequency > tempWs)
                        {
                            tempWs = tokenizedArticle[i].Frequency;
                        }
                    }
                    
                    endIndex = i;

                    var newToken = new Token(strValue, tokenizedArticle[startIndex].Position);
                    newToken.Sentence = tokenizedArticle[i].Sentence;
                    newToken.NamedEntity = tokenizedArticle[i].NamedEntity;
                    newToken.PartOfSpeech = tokenizedArticle[i].PartOfSpeech;
                    newToken.Frequency = tempWs;
                    candidates.Add(newToken);
                }
            }

            for (int can = 0; can < candidates.Count; can++)
            {
                for (int a = 0; a < can; a++)
                {
                    if (candidates[can].Value.Equals(candidates[a].Value))
                    {
                        candidates.RemoveAt(can);
                        if (can > 0)
                        {
                            can--;
                        }
                        break;
                    }
                }
            }

            foreach (var candidate in candidates)
            {
                System.Console.WriteLine("CANDIDATE " + candidate.Value);
            }
        }
    }
}
