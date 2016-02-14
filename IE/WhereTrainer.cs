using IE.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IE
{
    class WhereTrainer : Trainer
    {
        public WhereTrainer() : base()
        {

        }

        public override void train(bool isNewFile)
        {
            string path = @"..\..\Where.arff";
            string posTags = "{" + String.Join(",", Token.PartOfSpeechTags) + "}";

            try
            {
                if (isNewFile)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("@relation where");
                        sw.WriteLine("@attribute word string\n@attribute sentence NUMERIC\n@attribute position NUMERIC");
                        sw.WriteLine("@attribute wordScore NUMERIC");

                        for (int c = 10; c > 0; c--)
                        {
                            sw.WriteLine("@attribute word-" + c + " string");
                        }
                        sw.WriteLine("@attribute word+1 string\n@attribute word+2 string");
                        for (int c = 10; c > 0; c--)
                        {
                            sw.WriteLine("@attribute postag-" + c + " " + posTags);
                        }

                        sw.WriteLine("@attribute postag+1 " + posTags);
                        sw.WriteLine("@attribute postag+2 " + posTags);
                        //sw.WriteLine("@attribute named-entity-class-2 {LOC, PER, ORG, DATE, TIME, O}");
                        //sw.WriteLine("@attribute named-entity-class-1 {LOC, PER, ORG, DATE, TIME, O}");
                        //sw.WriteLine("@attribute named-entity-class+1 {LOC, PER, ORG, DATE, TIME, O}");
                        //sw.WriteLine("@attribute named-entity-class+2 {LOC, PER, ORG, DATE, TIME, O}");
                        sw.WriteLine("@attribute who {yes, no}");
                        sw.WriteLine("\n@data");
                    }
                }

                using (StreamWriter sw = File.AppendText(path))
                {
                    String str = null;
                    String value = null;
                    int sentence = 0;
                    int position = 0;
                    int frequency = 0;
                    int endIndex = 0;
                    int wordsbefore = 0;

                    string[] arrCandidate = null;

                    foreach (var candidate in listCandidates)
                    {
                        value = candidate.Value;
                        sentence = candidate.Sentence;
                        position = candidate.Position;
                        frequency = candidate.Frequency;
                        arrCandidate = candidate.Value.Split(' ');
                        endIndex = position + arrCandidate.Length - 1;

                        wordsbefore = position - 10;

                        str = "\"" + value + "\"," + sentence + "," + position + "," + frequency + ",";

                        int ctrBefore = wordsbefore;

                        while (ctrBefore < 1)
                        {
                            str += "?,";
                            ctrBefore++;
                        }
                        while (ctrBefore < position)
                        {
                            str += "\"" + listTokenizedArticle[ctrBefore - 1].Value + "\",";
                            ctrBefore++;
                        }
                        for (int c = 0; c < 2; c++)
                        {
                            if (endIndex + c < listTokenizedArticle.Count)
                            {
                                str += "\"" + listTokenizedArticle[endIndex + c].Value + "\",";
                            }
                            else
                            {
                                str += "?,";
                            }
                        }

                        // POS tags of words Before and After candidate
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
                        for (int c = 0; c < 2; c++)
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

                        if (candidate.IsWhere)
                        {
                            str += "yes";
                        }
                        else
                        {
                            str += "no";
                        }

                        sw.WriteLine(str);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
