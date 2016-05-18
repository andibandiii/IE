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
        private List<Candidate> listWhoCandidates;
        private List<Candidate> listWhenCandidates;
        private List<Candidate> listWhereCandidates;
        private List<List<Token>> listWhatCandidates;
        private List<List<Token>> listWhyCandidates;
        private CRFClassifier nerClassifier;
        private MaxentTagger posTagger;

        private readonly String nerModelPath = @"..\..\NERModel\filipino.ser.gz";
        private readonly String posModelPath = @"..\..\POSTagger\filipino.tagger";

        public Preprocessor()
        {
            listLatestTokenizedArticle = new List<Token>();
            listWhoCandidates = new List<Candidate>();
            listWhenCandidates = new List<Candidate>();
            listWhereCandidates = new List<Candidate>();
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

        public List<Candidate> getWhoCandidates()
        {
            return listWhoCandidates;
        }

        public List<Candidate> getWhenCandidates()
        {
            return listWhenCandidates;
        }

        public List<Candidate> getWhereCandidates()
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
        public float[][] performAnnotationAssignment()
        {
            float[][] statistics = new float[3][];
            if (annotationCurrent == null)
            {
                return null;
            }

            statistics[0] = performMultipleAnnotationAssignment("WHO");
            statistics[1] = performMultipleAnnotationAssignment("WHEN");
            statistics[2] = performMultipleAnnotationAssignment("WHERE");

            return statistics;
        }


        private void performCandidateSelection()
        {
            CandidateSelector selector = new CandidateSelector();
            listWhoCandidates = selector.performWhoCandidateSelection(listLatestTokenizedArticle, articleCurrent.Title);
            listWhenCandidates = selector.performWhenCandidateSelection(listLatestTokenizedArticle, articleCurrent.Title);
            listWhereCandidates = selector.performWhereCandidateSelection(listLatestTokenizedArticle, articleCurrent.Title);
            listWhatCandidates = selector.performWhatCandidateSelection(listLatestTokenizedArticle, articleCurrent.Title);
            listWhyCandidates = selector.performWhyCandidateSelection(listLatestTokenizedArticle, articleCurrent.Title);
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
        private float[] performMultipleAnnotationAssignment(String annotationType)
        {
            float[] statistics = new float[3] { 0, 0, 0}; //[0] = recall, [1] = precision, [2]  total
            int totalMatch = 0;
            annotationType = annotationType.ToUpper();
            if (annotationType != "WHO" && annotationType != "WHEN" && annotationType != "WHERE")
            {
                return statistics;
            }

            String strAnnotation = "";
            Action<string> assignmentMethod = null;
            string[] arrAnnotations = null;
            bool foundMatchingCandidate = false;
            switch (annotationType)
            {
                case "WHO":
                    strAnnotation = annotationCurrent.Who;
                    assignmentMethod = annotation =>
                    {
                        foreach (var candidate in listWhoCandidates)
                        {
                            if (candidate.Value == annotation)
                            {
                                candidate.IsWho = true;
                                foundMatchingCandidate = true;
                                //System.Console.WriteLine("WHO\nBEFORE: " + (((candidate.Position - 2) >= 0) ? listLatestTokenizedArticle[candidate.Position - 2].Value : "N/A"));
                                //string[] temp = candidate.Value.Split(' ');
                                //System.Console.WriteLine("AFTER: " + (((candidate.Position + temp.Length - 1) <= listLatestTokenizedArticle.Count()) ? listLatestTokenizedArticle[candidate.Position + temp.Length - 1].Value : "N/A"));
                                break;
                            }
                        }
                    };
                    break;
                case "WHEN":
                    strAnnotation = annotationCurrent.When;
                    assignmentMethod = annotation =>
                    {
                        foreach (var candidate in listWhenCandidates)
                        {
                            if (candidate.Value == annotation)
                            {
                                candidate.IsWhen = true;
                                foundMatchingCandidate = true;
                                //System.Console.WriteLine("WHEN\nBEFORE: " + (((candidate.Position - 2) >= 0) ? listLatestTokenizedArticle[candidate.Position - 2].Value : "N/A"));
                                //string[] temp = candidate.Value.Split(' ');
                                //System.Console.WriteLine("AFTER: " + (((candidate.Position + temp.Length - 1) <= listLatestTokenizedArticle.Count()) ? listLatestTokenizedArticle[candidate.Position + temp.Length - 1].Value : "N/A"));
                                break;
                            }
                        }
                    };
                    break;
                case "WHERE":
                    strAnnotation = annotationCurrent.Where;
                    assignmentMethod = annotation =>
                    {
                        foreach (var candidate in listWhereCandidates)
                        {
                            if (candidate.Value == annotation)
                            {
                                candidate.IsWhere = true;
                                foundMatchingCandidate = true;
                                //System.Console.WriteLine("WHERE\nBEFORE: " + (((candidate.Position - 2) >= 0) ? listLatestTokenizedArticle[candidate.Position - 2].Value : "N/A"));
                                //string[] temp = candidate.Value.Split(' ');
                                //System.Console.WriteLine("AFTER: " + (((candidate.Position + temp.Length - 1) <= listLatestTokenizedArticle.Count()) ? listLatestTokenizedArticle[candidate.Position + temp.Length - 1].Value : "N/A"));
                                break;
                            }
                        }
                    };
                    break;
            }

            if (strAnnotation.Count() <= 0 || strAnnotation == "N/A")
            {
                return statistics;
            }

            arrAnnotations = strAnnotation.Split(';');

            for (int r = 0; r < arrAnnotations.Length; r++)
            {
                if (arrAnnotations[r].Length > 0 && arrAnnotations[r][0] == ' ')
                {
                    arrAnnotations[r] = arrAnnotations[r].Substring(1);
                }

                ////System.Console.WriteLine(annotationType + " ANNOTATIONS-" + arrAnnotations[r]);
                if (assignmentMethod != null)
                {
                    assignmentMethod(arrAnnotations[r]);
                }

                if (!foundMatchingCandidate)
                {
                    int i = -1;
                    String[] wordForWordAnnotation = arrAnnotations[r].Split(' ');
                    for (int ctr = 0; ctr < listLatestTokenizedArticle.Count; ctr++)
                    {
                        if (wordForWordAnnotation[0].Contains(listLatestTokenizedArticle[ctr].Value))
                        {
                            i = ctr;
                            break;
                        }
                    }

                    if (i > -1)
                    {
                        //add as candidate
                        int startIndex = i;
                        int tempWs = listLatestTokenizedArticle[i].Frequency;

                        for (int ctr = startIndex; ctr < startIndex + wordForWordAnnotation.Count(); ctr++)
                        {
                            if (ctr < listLatestTokenizedArticle.Count && listLatestTokenizedArticle[ctr].Frequency > tempWs)
                            {
                                tempWs = listLatestTokenizedArticle[ctr].Frequency;
                            }
                        }

                        var newToken = new Candidate(arrAnnotations[r], listLatestTokenizedArticle[startIndex].Position, startIndex + wordForWordAnnotation.Count() - 1);
                        newToken.Sentence = listLatestTokenizedArticle[i].Sentence;
                        newToken.NamedEntity = listLatestTokenizedArticle[i].NamedEntity;
                        newToken.PartOfSpeech = listLatestTokenizedArticle[i].PartOfSpeech;
                        newToken.Frequency = tempWs;
                        switch (annotationType)
                        {
                            case "WHO":
                                newToken.IsWho = true;
                                listWhoCandidates.Add(newToken);
                                break;
                            case "WHEN":
                                newToken.IsWhen = true;
                                listWhenCandidates.Add(newToken);
                                break;
                            case "WHERE":
                                newToken.IsWhere = true;
                                listWhereCandidates.Add(newToken);
                                break;
                        }
                    }
                }
                else
                {
                    totalMatch += 1;
                    foundMatchingCandidate = false;
                }
            }

            System.Console.WriteLine("Annotations Count: {0}", arrAnnotations.GetLength(0));
            statistics[2] += 1;
            statistics[0] = (float) totalMatch / arrAnnotations.GetLength(0);
            switch (annotationType)
            {
                case "WHO":
                    statistics[1] = (float) totalMatch / listWhoCandidates.Count;
                    System.Console.WriteLine("Total Match: {0}, Who Candidates Count: {1}", totalMatch, listWhoCandidates.Count);
                    break;
                case "WHEN":
                    statistics[1] = (float) totalMatch / listWhenCandidates.Count;
                    System.Console.WriteLine("Total Match: {0}, When Candidates Count: {1}", totalMatch, listWhenCandidates.Count);
                    break;
                case "WHERE":
                    statistics[1] = (float) totalMatch / listWhereCandidates.Count;
                    System.Console.WriteLine("Total Match: {0}, Where Candidates Count: {1}", totalMatch, listWhereCandidates.Count);
                    break;
            }
            return statistics;
        }
        #endregion
    }
}
