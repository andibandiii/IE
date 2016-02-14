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
            int startIndex = 0;
            int endIndex = 0;

            string strValue = "";
            int tempWs = 0;
            for (int i = 0; i < tokenizedArticle.Count; i++)
            {

                if (tokenizedArticle[i].NamedEntity == "PER" || tokenizedArticle[i].NamedEntity == "ORG")
                {
                    startIndex = i;
                    strValue = tokenizedArticle[i].Value;
                    tempWs = tokenizedArticle[i].Frequency;

                    while (tokenizedArticle[i].NamedEntity == tokenizedArticle[i + 1].NamedEntity)
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

            return candidates;
        }

        public List<Token> performWhenCandidateSelection(List<Token> tokenizedArticle)
        {
            //REPLACE THIS CODE
            List<Token> candidates = new List<Token>();
            int startIndex = 0;
            int endIndex = 0;

            string strValue = "";
            int tempWs = 0;
            for (int i = 0; i < tokenizedArticle.Count; i++)
            {

                if (tokenizedArticle[i].NamedEntity == "PER" || tokenizedArticle[i].NamedEntity == "ORG")
                {
                    startIndex = i;
                    strValue = tokenizedArticle[i].Value;
                    tempWs = tokenizedArticle[i].Frequency;

                    while (tokenizedArticle[i].NamedEntity == tokenizedArticle[i + 1].NamedEntity)
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

            return candidates;
        }

        public List<Token> performWhereCandidateSelection(List<Token> tokenizedArticle)
        {
            //REPLACE THIS CODE
            List<Token> candidates = new List<Token>();
            int startIndex = 0;
            int endIndex = 0;

            string strValue = "";
            int tempWs = 0;
            for (int i = 0; i < tokenizedArticle.Count; i++)
            {

                if (tokenizedArticle[i].NamedEntity == "PER" || tokenizedArticle[i].NamedEntity == "ORG")
                {
                    startIndex = i;
                    strValue = tokenizedArticle[i].Value;
                    tempWs = tokenizedArticle[i].Frequency;

                    while (tokenizedArticle[i].NamedEntity == tokenizedArticle[i + 1].NamedEntity)
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

            return candidates;
        }
    }
}
