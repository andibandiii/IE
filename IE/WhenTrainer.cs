using IE.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IE
{
    class WhenTrainer : Trainer
    {
        private static int nNoDataCount = 0;

        public WhenTrainer() : base()
        {

        }

        public override void train(bool isNewFile)
        {
            string path = @"..\..\When.arff";
            string posTags = "{" + String.Join(",", Token.PartOfSpeechTags) + "}";

            try
            {
                if (isNewFile)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    /*!!
                     * RELATION NAME AND ATTRIBUTE DECLARATION
                     */
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("@relation when");
                        sw.WriteLine("@attribute word string");
                        sw.WriteLine("@attribute wordCount NUMERIC");
                        sw.WriteLine("@attribute sentence NUMERIC");
                        sw.WriteLine("@attribute position NUMERIC");
                        sw.WriteLine("@attribute sentenceStartProximity NUMERIC");
                        sw.WriteLine("@attribute wordScore NUMERIC");

                        for (int c = 10; c > 0; c--)
                        {
                            sw.WriteLine("@attribute word-" + c + " string");
                        }
                        for (int c = 1; c <= 10; c++)
                        {
                            sw.WriteLine("@attribute word+" + c + " string");
                        }
                        for (int c = 10; c > 0; c--)
                        {
                            sw.WriteLine("@attribute postag-" + c + " " + posTags);
                        }
                        for (int c = 1; c <= 10; c++)
                        {
                            sw.WriteLine("@attribute postag+" + c + " " + posTags);
                        }
                        sw.WriteLine("@attribute when {yes, no}");
                        sw.WriteLine("\n@data");
                    }
                }

                using (StreamWriter sw = File.AppendText(path))
                {
                    String str = null;
                    String value = null;
                    int wordcount = 0;
                    int sentence = 0;
                    int position = 0;
                    int frequency = 0;
                    int endIndex = 0;
                    int wordsbefore = 0;

                    /*!!
                     * NUMBER OF WORDS TO BE CONSIDERED BEFORE AND AFTER THE CANDIDATE
                     */
                    int beforeCount = 10;
                    int afterCount = 10;

                    /*!!
                     * MAXIMUM NUMBER OF 'no' DATA ALLOWED
                     */
                    int maxCountOfNo = 525;

                    double sentenceStartProximity = -1.0;

                    string[] arrCandidate = null;

                    List<List<Token>> segregatedTokenLists = listTokenizedArticle
                        .GroupBy(token => token.Sentence)
                        .Select(tokenGroup => tokenGroup.ToList())
                        .ToList();

                    foreach (var candidate in listCandidates)
                    {
                        value = candidate.Value;
                        sentence = candidate.Sentence;
                        position = candidate.Position;
                        frequency = candidate.Frequency;
                        arrCandidate = candidate.Value.Split(' ');
                        endIndex = position + arrCandidate.Length - 1;
                        wordcount = arrCandidate.Count();

                        foreach (List<Token> tokenList in segregatedTokenLists)
                        {
                            if (tokenList.Count > 0 && tokenList[0].Sentence == sentence)
                            {
                                sentenceStartProximity = (double) (position - tokenList[0].Position) / (double) tokenList.Count;
                                break;
                            }
                        }

                        wordsbefore = position - beforeCount;

                        /*!!
                         * INITIAL ATTRIBUTES
                         */
                        str = "\"" + value.Replace("\"", "'") + "\",";
                        str += wordcount + ",";
                        str += sentence + ",";
                        str += position + ",";
                        str += ((sentenceStartProximity == -1) ? "?" : "" + sentenceStartProximity) + ",";
                        str += frequency + ",";

                        /*
                         * ADDING WORD STRINGS IN DATASET FOR THE WORDS BEFORE AND AFTER
                         */
                        int ctrBefore = wordsbefore;

                        while (ctrBefore < 1)
                        {
                            str += "?,";
                            ctrBefore++;
                        }
                        while (ctrBefore < position)
                        {
                            str += "\"" + listTokenizedArticle[ctrBefore - 1].Value.Replace("\"", "'") + "\",";
                            ctrBefore++;
                        }
                        for (int c = 0; c < afterCount; c++)
                        {
                            if (endIndex + c < listTokenizedArticle.Count)
                            {
                                str += "\"" + listTokenizedArticle[endIndex + c].Value.Replace("\"", "'") + "\",";
                            }
                            else
                            {
                                str += "?,";
                            }
                        }

                        /*
                         * ADDING POS TAGS IN DATASET FOR THE WORDS BEFORE AND AFTER
                         */
                        ctrBefore = wordsbefore;

                        while (ctrBefore < 1)
                        {
                            str += "?,";
                            ctrBefore++;
                        }
                        while (ctrBefore < position)
                        {
                            str += listTokenizedArticle[ctrBefore - 1].PartOfSpeech + ",";
                            ctrBefore++;
                        }
                        for (int c = 0; c < afterCount; c++)
                        {
                            if (endIndex + c < listTokenizedArticle.Count)
                            {
                                str += listTokenizedArticle[endIndex + c].PartOfSpeech + ",";
                            }
                            else
                            {
                                str += "?,";
                            }
                        }

                        if (candidate.IsWhen)
                        {
                            str += "yes";
                        }
                        else
                        {
                            str += "no";
                            nNoDataCount++;
                        }

                        if (nNoDataCount <= maxCountOfNo || candidate.IsWhen)
                            sw.WriteLine(str);
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
            }
        }
    }
}
