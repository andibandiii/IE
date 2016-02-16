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
                i = getCandidateByNer(NamedEntity.PER, i, candidates, tokenizedArticle);
                i = getCandidateByNer(NamedEntity.ORG, i, candidates, tokenizedArticle);
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

            return candidates;
        }

        public List<Token> performWhenCandidateSelection(List<Token> tokenizedArticle)
        {
            List<Token> candidates = new List<Token>();
            String[] startMarkers = new String[5] { "ang", "nasa", "noong", "nuong", "sa" };
            String[][] endMarkers = new String[5][] { new String[] { "ay", "."}, new String[] { "para"}, new String[] { "."}, new String[] { "."},
                                                      new String[] { "na", "noong", "nuong","sa","."} };
            for (int i = 0; i < tokenizedArticle.Count; i++)
            {
                i = getCandidateByNer(NamedEntity.DATE, i, candidates, tokenizedArticle);
                //i = getCandidateByMarkers(startMarkers, endMarkers, i, candidates, tokenizedArticle);
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

            return candidates;
        }

        public List<Token> performWhereCandidateSelection(List<Token> tokenizedArticle)
        {
            List<Token> candidates = new List<Token>();
            for (int i = 0; i < tokenizedArticle.Count; i++)
            {
                i = getCandidateByNer(NamedEntity.LOC, i, candidates, tokenizedArticle);
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

        private int getCandidateByNer(NamedEntity nerTag, int i, List<Token> candidates, List<Token> tokenizedArticle)
        {
            if (tokenizedArticle[i].NamedEntity.Equals(nerTag))
            {
                int startIndex = i;
                String strValue = tokenizedArticle[i].Value;
                int tempWs = tokenizedArticle[i].Frequency;

                while (tokenizedArticle[i].NamedEntity == tokenizedArticle[i + 1].NamedEntity)
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
            }
            return i;
        }

        private int getCandidateByMarkers(String[] startMarkers, String[][] endMarkers, int i, List<Token> candidates, List<Token> tokenizedArticle)
        {
            for (int j = 0; j < startMarkers.Length; j++)
                if (tokenizedArticle[i].Value.Equals(startMarkers[j]))
                {
                    i++;
                    int startIndex = i;
                    int sentenceNumber = tokenizedArticle[i].Sentence;
                    String strValue = null;
                    int tempWs = 0;
                    Boolean flag = true;
                    foreach (String markers in endMarkers[j])
                    {
                        if (tokenizedArticle[i].Value.Equals(markers))
                        {
                            flag = false;
                            break;
                        }
                    }
                    while (flag)
                    {
                        if (strValue == null)
                            strValue = tokenizedArticle[i].Value;
                        else
                            strValue += " " + tokenizedArticle[i].Value;
                        if (tokenizedArticle[i].Frequency > tempWs)
                        {
                            tempWs = tokenizedArticle[i].Frequency;
                        }
                        i++;
                        foreach (String markers in endMarkers[j])
                        {
                            if (tokenizedArticle[i].Value.Equals(markers) || tokenizedArticle[i].Sentence != sentenceNumber)
                            {
                                flag = false;
                                break;
                            }
                        }
                    }

                    int endIndex = i;

                    var newToken = new Token(strValue, tokenizedArticle[startIndex].Position);
                    newToken.Sentence = tokenizedArticle[i].Sentence;
                    newToken.NamedEntity = tokenizedArticle[i].NamedEntity;
                    newToken.PartOfSpeech = tokenizedArticle[i].PartOfSpeech;
                    newToken.Frequency = tempWs;
                    candidates.Add(newToken);
                }
            return i;
        }
    }
}
