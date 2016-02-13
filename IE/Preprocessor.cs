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
    public class Preprocessor
    {
        private Article articleCurrent;
        private List<Token> listLatestTokenizedArticle;
        private List<Token> listWhoCandidates;

        public Preprocessor()
        {
            listLatestTokenizedArticle = new List<Token>();
            listWhoCandidates = new List<Token>();
        }

        #region Setters
        public void setCurrent(Article pArticle)
        {
            articleCurrent = pArticle;
        }
        #endregion

        #region Getters
        public Article getCurrent()
        {
            return articleCurrent;
        }

        public List<Token> getLatestTokenizedArticle()
        {
            return listLatestTokenizedArticle;
        }

        public List<Token> getWhoCandidates()
        {
            return listWhoCandidates;
        }
        #endregion

        public List<Token> preprocess()
        {
            if (articleCurrent == null)
            {
                return null;
            }

            listLatestTokenizedArticle = new List<Token>();

            performTokenizationAndSS();
            performNER();
            performPOST();
            performWS();
            performTokenizeAnnotations();
            performCandidateSelection();

            foreach (var token in listLatestTokenizedArticle)
            {
                System.Console.WriteLine("Value: " + token.Value);
                System.Console.WriteLine("Sentence: " + token.Sentence);
                System.Console.WriteLine("Position: " + token.Position);
                System.Console.WriteLine("NER: " + token.NamedEntity);
                System.Console.WriteLine("POS: " + token.PartOfSpeech);
                System.Console.WriteLine("WS: " + token.Frequency);
                System.Console.WriteLine("=====\n");
            }

            return listLatestTokenizedArticle;
        }

        /// <summary>
        /// Perform tokenization on all the given article's annotations.
        /// </summary>
        private void performTokenizeAnnotations()
        {
            performWhoTokenization();
        }

        private void performCandidateSelection()
        {
            CandidateSelector selector = new CandidateSelector();
            listWhoCandidates = selector.performWhoCandidateSelection(listLatestTokenizedArticle);
        }

        #region Article Preprocessing Functions
        private void performTokenizationAndSS()
        {
            var sentences = MaxentTagger.tokenizeText(new java.io.StringReader(articleCurrent.Body)).toArray();
            int sentenceCounter = 1;
            int positionCounter = 1;
            foreach (java.util.ArrayList sentence in sentences)
            {
                foreach (var word in sentence)
                {
                    var newToken = new Token(word.ToString(), positionCounter);
                    newToken.Sentence = sentenceCounter;
                    listLatestTokenizedArticle.Add(newToken);
                    positionCounter++;
                }
                sentenceCounter++;
            }
        }

        private void performNER()
        {
            java.util.List tokens;
            List<string> values = new List<string>();
            object[] nerValues;
            var classifier = CRFClassifier.getClassifierNoExceptions(@"..\..\NERModel\filipino.ser.gz");

            foreach (Token token in listLatestTokenizedArticle)
            {
                values.Add(token.Value);
            }

            tokens = Sentence.toCoreLabelList(values.ToArray());

            nerValues = classifier.classifySentence(tokens).toArray();

            //System.Console.WriteLine("{0}\n", classifier.classifyToString(article.Body));

            for (int i = 0; i < listLatestTokenizedArticle.Count; i++)
            {
                //System.Console.WriteLine(((CoreLabel)nerValues[i]).get(typeof(CoreAnnotations.AnswerAnnotation)) + " - " + ((CoreLabel)nerValues[i]).toShorterString());
                //NamedEntity nerValue;
                //System.Enum.TryParse(((CoreLabel)nerValues[i]).get(typeof(CoreAnnotations.AnswerAnnotation)).ToString(), out nerValue);

                listLatestTokenizedArticle[i].NamedEntity = ((CoreLabel)nerValues[i]).get(typeof(CoreAnnotations.AnswerAnnotation)).ToString();
            }
        }

        private void performPOST()
        {
            //Path to Filipino Tagger Model
            String modelPath = @"..\..\POSTagger\filipino.tagger";
            MaxentTagger tagger = new MaxentTagger(modelPath);

            //Get all tokens and segregate them into lists based on sentence number
            List<List<Token>> segregatedTokenLists = listLatestTokenizedArticle
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
                        foreach (var token in listLatestTokenizedArticle)
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

        private void performWS()
        {
            Dictionary<string, int> frequencies = new Dictionary<string, int>();

            foreach (Token token in listLatestTokenizedArticle)
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

            foreach (Token token in listLatestTokenizedArticle)
            {
                token.Frequency = frequencies[token.Value];
            }
        }
        #endregion

        #region Annotation Preprocessing Functions
        private void performWhoTokenization()
        {
            String who = articleCurrent.Annotation.Who;
            string[] whoAnnotations = null;

            whoAnnotations = who.Split(';');

            for (int r = 0; r < whoAnnotations.Length; r++)
            {
                if (whoAnnotations[r][0] == ' ')
                {
                    whoAnnotations[r] = whoAnnotations[r].Substring(1);
                }
                System.Console.WriteLine("WHO ANNOTATIONS-" + whoAnnotations[r]);
            }
        }
        #endregion
    }
}
