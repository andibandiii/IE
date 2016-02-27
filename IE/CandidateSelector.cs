using IE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IE
{
    class CandidateSelector
    {
        public List<Token> performWhoCandidateSelection(List<Token> tokenizedArticle)
        {
            List<Token> candidates = new List<Token>();
            for (int i = 0; i < tokenizedArticle.Count; i++)
            {
                i = getCandidateByNer("PER", i, candidates, tokenizedArticle);
                i = getCandidateByNer("ORG", i, candidates, tokenizedArticle);
            }

            for (int i = 0; i < tokenizedArticle.Count; i++)
            {
                i = getCandidateByPos("NNP", i, candidates, tokenizedArticle);
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

            //foreach (var candidate in candidates)
            //{
            //    //System.Console.WriteLine("WHO CANDIDATE " + candidate.Value);
            //}

            return candidates;
        }

        public List<Token> performWhenCandidateSelection(List<Token> tokenizedArticle)
        {
            List<Token> candidates = new List<Token>();
            String[] startMarkersExclusive = new String[] { "ang",
                "mula sa",
                "mula",
                "na",
                "noong",
                "nuong",
                "sa" };
            String[][] endMarkersExclusive = new String[][] { new String[] { "para"},
                new String[] { ","},
                new String[] { "."},
                new String[] { "ay"},
                new String[] { ",", "."},
                new String[] { ",", "."},
                new String[] { "ay", "upang", ",", "."} };
            String[] startMarkersInclusive = new String[] { "kamakalawa" };
            String[][] endMarkersInclusive = new String[][] { new String[] { "gabi", "umaga", "hapon" } };
            for (int i = 0; i < tokenizedArticle.Count; i++)
            {
                i = getCandidateByNer("DATE", i, candidates, tokenizedArticle);
                getCandidateByMarkers(startMarkersExclusive, endMarkersExclusive, null, i, candidates, tokenizedArticle, true);
                getCandidateByMarkers(startMarkersInclusive, endMarkersInclusive, null, i, candidates, tokenizedArticle, false);
            }

            for (int can = 0; can < candidates.Count; can++)
            {
                for (int a = 0; a < can; a++)
                {
                    if (candidates[can].Value != null && candidates[can].Value.Equals(candidates[a].Value))
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

            //foreach (var candidate in candidates)
            //{
            //    //System.Console.WriteLine("WHEN CANDIDATE " + candidate.Value);
            //}

            return candidates;
        }

        public List<Token> performWhereCandidateSelection(List<Token> tokenizedArticle)
        {
            List<Token> candidates = new List<Token>();
            String[] startMarkers = new String[5] { "ang",
                "nasa",
                "noong",
                "nuong",
                "sa" };
            String[][] endMarkers = new String[5][] { new String[] { "ay", "."},
                new String[] { "para"},
                new String[] { "."},
                new String[] { "."},
                new String[] { "para", "noong", "nuong","sa","kamakalawa","kamakala-wa","."} };
            String[][] enderMarkers = new String[5][] { new String[] { },
                new String[] { },
                new String[] { "sabado", "hapon","umaga","gabi","miyerkules","lunes","martes","huwebes","linggo","biyernes","alas","oras"},
                new String[] { "sabado", "hapon","umaga","gabi","miyerkules","lunes","martes","huwebes","linggo","biyernes","alas","oras"},
                new String[] { "sabado", "hapon","umaga","gabi","miyerkules","lunes","martes","huwebes","linggo","biyernes","alas","oras"} };
            for (int i = 0; i < tokenizedArticle.Count; i++)
            {
                i = getCandidateByNer("LOC", i, candidates, tokenizedArticle);
                getCandidateByMarkers(startMarkers, endMarkers, enderMarkers, i, candidates, tokenizedArticle, true);
            }

            for (int can = 0; can < candidates.Count; can++)
            {
                for (int a = 0; a < can; a++)
                {
                    if (candidates[can].Value != null && candidates[can].Value.Equals(candidates[a].Value))
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

            //foreach (var candidate in candidates)
            //{
            //    //System.Console.WriteLine("WHERE CANDIDATE " + candidate.Value);
            //}

            return candidates;
        }

        public List<List<Token>> performWhatCandidateSelection(List<Token> tokenizedArticle)
        {
            int maxNumberOfCandidates = 3;
            List<List<Token>> candidates = new List<List<Token>>();
            List<List<Token>> segregatedArticle = tokenizedArticle
                .GroupBy(token => token.Sentence)
                .Select(tokenGroup => tokenGroup.ToList())
                .ToList();

            for (int nI = 0; nI < Math.Min(maxNumberOfCandidates, segregatedArticle.Count()); nI++)
            {
                candidates.Add(segregatedArticle[nI]);
            }

            return candidates;
        }

        public List<List<Token>> performWhyCandidateSelection(List<Token> tokenizedArticle)
        {
            int maxNumberOfCandidates = 3;
            List<List<Token>> candidates = new List<List<Token>>();
            List<List<Token>> segregatedArticle = tokenizedArticle
                .GroupBy(token => token.Sentence)
                .Select(tokenGroup => tokenGroup.ToList())
                .ToList();

            for (int nI = 1; nI < Math.Min(maxNumberOfCandidates + 1, segregatedArticle.Count()); nI++)
            {
                candidates.Add(segregatedArticle[nI]);
            }

            return candidates;
        }

        private int getCandidateByNer(String nerTag, int i, List<Token> candidates, List<Token> tokenizedArticle)
        {
            if (tokenizedArticle[i].Sentence <= 3 && tokenizedArticle[i].NamedEntity.Equals(nerTag))
            {
                int startIndex = i;
                String strValue = tokenizedArticle[i].Value;
                int tempWs = tokenizedArticle[i].Frequency;

                while ((i + 1) < tokenizedArticle.Count && tokenizedArticle[i].NamedEntity == tokenizedArticle[i + 1].NamedEntity)
                {
                    i++;
                    strValue += " " + tokenizedArticle[i].Value;
                    if (tokenizedArticle[i].Frequency > tempWs)
                    {
                        tempWs = tokenizedArticle[i].Frequency;
                    }
                }

                int endIndex = i;

                var newToken = new Token(strValue, tokenizedArticle[startIndex].Position);
                newToken.Sentence = tokenizedArticle[i].Sentence;
                newToken.NamedEntity = tokenizedArticle[i].NamedEntity;
                newToken.PartOfSpeech = tokenizedArticle[i].PartOfSpeech;
                newToken.Frequency = tempWs;
                candidates.Add(newToken);

                //System.Console.WriteLine("CANDIDATE BY NER [{0}]: {1} (Position {2})", nerTag, newToken.Value, newToken.Position);
            }
            return i;
        }

        private void getCandidateByMarkers(String[] startMarkers, String[][] endMarkers, String[][] enderMarkers, int i, List<Token> candidates, List<Token> tokenizedArticle, Boolean isExclusive)
        {

            for (int j = 0; j < startMarkers.Length; j++)
            {
                if (tokenizedArticle[i].Value.Equals(startMarkers[j], StringComparison.OrdinalIgnoreCase))
                {
                    if (isExclusive)
                    {
                        i++;
                    }
                    int startIndex = i;
                    int sentenceNumber = tokenizedArticle[i].Sentence;
                    String strValue = null;
                    String posValue = null;
                    int tempWs = 0;
                    Boolean flag = true;
                    Boolean endMarkerFound = false;
                    while (flag)
                    {
                        foreach (String markers in endMarkers[j])
                        {
                            if (tokenizedArticle[i].Value.Equals(markers))
                            {
                                endMarkerFound = true;
                                flag = false;
                                break;
                            }
                        }
                        if (enderMarkers != null)
                        {
                            foreach (String markers in enderMarkers[j])
                            {
                                if (tokenizedArticle[i].Value.Equals(markers, StringComparison.OrdinalIgnoreCase))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (tokenizedArticle[i].Sentence != sentenceNumber)
                        {
                            flag = false;
                        }
                        i++;
                        if (i >= tokenizedArticle.Count)
                        {
                            flag = false;
                        }
                    }

                    int endIndex;
                    if (isExclusive)
                    {
                        endIndex = i - 1;
                    }
                    else
                    {
                        endIndex = i;
                    }
                    if (endMarkerFound)
                    {
                        for (int k = startIndex; k < endIndex; k++)
                        {
                            if (strValue == null)
                            {
                                strValue = tokenizedArticle[k].Value;
                                posValue = tokenizedArticle[k].PartOfSpeech;
                            }
                            else
                            {
                                strValue += " " + tokenizedArticle[k].Value;
                                posValue += " " + tokenizedArticle[k].PartOfSpeech;
                            }

                            if (tokenizedArticle[k].Frequency > tempWs)
                            {
                                tempWs = tokenizedArticle[k].Frequency;
                            }
                        }
                        var newToken = new Token(strValue, tokenizedArticle[startIndex].Position);
                        newToken.Sentence = tokenizedArticle[startIndex].Sentence;
                        newToken.NamedEntity = tokenizedArticle[endIndex].NamedEntity;
                        newToken.PartOfSpeech = tokenizedArticle[endIndex].PartOfSpeech;
                        newToken.Frequency = tempWs;
                        candidates.Add(newToken);

                        //System.Console.WriteLine("CANDIDATE BY MARKERS: {0}\n\t{1}", newToken.Value, posValue);
                    }
                    else
                    {
                        i = startIndex - 1;
                    }
                    j = startMarkers.Length;
                }
            }
        }

        private int getCandidateByPos(String posTag, int i, List<Token> candidates, List<Token> tokenizedArticle)
        {
            if (i < tokenizedArticle.Count && tokenizedArticle[i].PartOfSpeech != null && tokenizedArticle[i].PartOfSpeech.Equals(posTag) && tokenizedArticle[i].Sentence <= 3)
            {
                int startIndex = i;
                String strValue = tokenizedArticle[i].Value;
                int tempWs = tokenizedArticle[i].Frequency;

                while ((i + 1) < tokenizedArticle.Count && tokenizedArticle[i].PartOfSpeech == tokenizedArticle[i + 1].PartOfSpeech)
                {
                    i++;
                    strValue += " " + tokenizedArticle[i].Value;
                    if (tokenizedArticle[i].Frequency > tempWs)
                    {
                        tempWs = tokenizedArticle[i].Frequency;
                    }
                }

                int endIndex = i;

                var newToken = new Token(strValue, tokenizedArticle[startIndex].Position);
                newToken.Sentence = tokenizedArticle[i].Sentence;
                newToken.NamedEntity = tokenizedArticle[i].NamedEntity;
                newToken.PartOfSpeech = tokenizedArticle[i].PartOfSpeech;
                newToken.Frequency = tempWs;
                candidates.Add(newToken);

                //System.Console.WriteLine("CANDIDATE BY POS [{0}]: {1} (Position {2})", posTag, newToken.Value, newToken.Position);
            }
            return i;
        }
    }
}
