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
        private Annotation annotationCurrent;
        private List<Token> listLatestTokenizedArticle;
        private List<Token> listWhoCandidates;
        private List<Token> listWhenCandidates;
        private List<Token> listWhereCandidates;
        private List<List<Token>> listWhatCandidates;
        private List<List<Token>> listWhyCandidates;
        private CRFClassifier nerClassifier;
        private MaxentTagger posTagger;

        private readonly String nerModelPath = @"..\..\NERModel\filipino.ser.gz";
        private readonly String posModelPath = @"..\..\POSTagger\filipino.tagger";

        public Preprocessor()
        {
            listLatestTokenizedArticle = new List<Token>();
            listWhoCandidates = new List<Token>();
            listWhenCandidates = new List<Token>();
            listWhereCandidates = new List<Token>();
            listWhatCandidates = new List<List<Token>>();
            listWhyCandidates = new List<List<Token>>();
            nerClassifier = CRFClassifier.getClassifierNoExceptions(nerModelPath);
            posTagger = new MaxentTagger(posModelPath);
        }

        #region Setters
        public void setCurrentArticle(Article pArticle)
        {
            articleCurrent = pArticle;
        }

        public void setCurrentAnnotation(Annotation pAnnotation)
        {
            annotationCurrent = pAnnotation;
        }
        #endregion

        #region Getters
        public Article getCurrentArticle()
        {
            return articleCurrent;
        }

        public Annotation getCurrentAnnotation()
        {
            return annotationCurrent;
        }

        public List<Token> getLatestTokenizedArticle()
        {
            return listLatestTokenizedArticle;
        }

        public List<Token> getWhoCandidates()
        {
            return listWhoCandidates;
        }

        public List<Token> getWhenCandidates()
        {
            return listWhenCandidates;
        }

        public List<Token> getWhereCandidates()
        {
            return listWhereCandidates;
        }

        public List<List<Token>> getWhatCandidates()
        {
            return listWhatCandidates;
        }

        public List<List<Token>> getWhyCandidates()
        {
            return listWhyCandidates;
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
            performCandidateSelection();

            foreach (var token in listLatestTokenizedArticle)
            {
                //System.Console.WriteLine("Value: " + token.Value);
                //System.Console.WriteLine("Sentence: " + token.Sentence);
                //System.Console.WriteLine("Position: " + token.Position);
                //System.Console.WriteLine("NER: " + token.NamedEntity);
                //System.Console.WriteLine("POS: " + token.PartOfSpeech);
                //System.Console.WriteLine("WS: " + token.Frequency);
                //System.Console.WriteLine("=====\n");
            }

            return listLatestTokenizedArticle;
        }

        /// <summary>
        /// Assign an article's token to whether or not it is part of a 5W.
        /// </summary>
        public void performAnnotationAssignment()
        {
            if (annotationCurrent == null)
            {
                return;
            }

            performMultipleAnnotationAssignment("WHO");
            performMultipleAnnotationAssignment("WHEN");
            performMultipleAnnotationAssignment("WHERE");
        }

        private void performCandidateSelection()
        {
            CandidateSelector selector = new CandidateSelector();
            listWhoCandidates = selector.performWhoCandidateSelection(listLatestTokenizedArticle);
            listWhenCandidates = selector.performWhenCandidateSelection(listLatestTokenizedArticle);
            listWhereCandidates = selector.performWhereCandidateSelection(listLatestTokenizedArticle);
            listWhatCandidates = selector.performWhatCandidateSelection(listLatestTokenizedArticle);
            listWhyCandidates = selector.performWhyCandidateSelection(listLatestTokenizedArticle);
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
            bool hasNickname = false;

            foreach (Token token in listLatestTokenizedArticle)
            {
                values.Add(token.Value);
            }

            tokens = Sentence.toCoreLabelList(values.ToArray());

            nerValues = nerClassifier.classifySentence(tokens).toArray();

            for (int i = 0; i < listLatestTokenizedArticle.Count; i++)
            {
                listLatestTokenizedArticle[i].NamedEntity = ((CoreLabel)nerValues[i]).get(typeof(CoreAnnotations.AnswerAnnotation)).ToString();
            }

            for (int i = 0; i < listLatestTokenizedArticle.Count; i++)
            {
                if ((i - 1) < 0) continue;
                if (listLatestTokenizedArticle[i].Value.Equals("``") && listLatestTokenizedArticle[i - 1].NamedEntity.Equals("PER"))
                {
                    hasNickname = true;
                }

                if (hasNickname)
                {
                    listLatestTokenizedArticle[i].NamedEntity = "PER";

                    if(listLatestTokenizedArticle[i - 1].Value.Equals("\'\'"))
                    {
                        hasNickname = false;
                    }
                }
            }
        }

        private void performPOST()
        {
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
                var taggedSentence = posTagger.tagSentence(entry.Value).toArray();
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
        private void performMultipleAnnotationAssignment(String annotationType = "WHO")
        {
            annotationType = annotationType.ToUpper();
            if (annotationType != "WHO" && annotationType != "WHEN" && annotationType != "WHERE")
            {
                return;
            }

            String strAnnotation = "";
            Action<string> assignmentMethod = null;
            string[] arrAnnotations = null;

            switch (annotationType)
            {
                case "WHO":
                    strAnnotation = annotationCurrent.Who;
                    assignmentMethod = annotation => {
                        foreach (var candidate in listWhoCandidates)
                        {
                            if (candidate.Value == annotation)
                            {
                                candidate.IsWho = true;
                                break;
                            }
                        }
                    };
                    break;
                case "WHEN":
                    strAnnotation = annotationCurrent.When;
                    assignmentMethod = annotation => {
                        foreach (var candidate in listWhenCandidates)
                        {
                            if (candidate.Value == annotation)
                            {
                                candidate.IsWhen = true;
                                break;
                            }
                        }
                    };
                    break;
                case "WHERE":
                    strAnnotation = annotationCurrent.Where;
                    assignmentMethod = annotation => {
                        foreach (var candidate in listWhereCandidates)
                        {
                            if (candidate.Value == annotation)
                            {
                                candidate.IsWhere = true;
                                break;
                            }
                        }
                    };
                    break;
            }

            if (strAnnotation.Count() <= 0 || strAnnotation == "N/A")
            {
                return;
            }

            arrAnnotations = strAnnotation.Split(';');

            for (int r = 0; r < arrAnnotations.Length; r++)
            {
                if (arrAnnotations[r][0] == ' ')
                {
                    arrAnnotations[r] = arrAnnotations[r].Substring(1);
                }

                ////System.Console.WriteLine(annotationType + " ANNOTATIONS-" + arrAnnotations[r]);
                if (assignmentMethod != null)
                {
                    assignmentMethod(arrAnnotations[r]);
                }
            }
        }
        #endregion
    }
}
