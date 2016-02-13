using IE.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IE
{
    class Trainer
    {
        static List<Token> tokenizedArticle;
        static List<Token> candidates;

        internal static void setTokenizedArticle(List<Token> inputTokenizedArticle)
        {
            tokenizedArticle = inputTokenizedArticle;
        }

        internal static void setCandidatesList(List<Token> inputCandidates)
        {
            candidates = inputCandidates;
        }

        internal static void train()
        {
            if (tokenizedArticle == null || candidates == null)
            {
                return;
            }

            string path = @"..\..\Who.arff";

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("@relation who");
                    sw.WriteLine("@attribute word string\n@attribute sentence NUMERIC\n@attribute position NUMERIC");
                    sw.WriteLine("@attribute wordScore NUMERIC");

                    for (int c = 10; c > 0; c--)
                    {
                        sw.WriteLine("@attribute word-" + c + " string");
                    }
                    sw.WriteLine("@attribute word+1 string\n@attribute word+2 string");
                    for (int c = 10; c > 0; c--)
                    {
                        sw.WriteLine("@attribute postag-" + c + " { CCA, CCB, CCP, CCR, CCT,CDB, DTC, DTCP, DTPP, DTP, EX, FW, IN, JJD, JJN, JJR, JJS, LS, LM, MD, NNC, NNS, NNP, NNPS, PDT, PMC, PMP, POS, PRP, PRC, PRI, PRO, PRS, PRPS, PRSP, RB, RBF, RBI, RBK, RBP, RBR, RBW, RP, SYM, TO, UH, VB, VBD, VBG, VBN, VBP, VBOF, VBTS, VBZ, WDT, WP, WPS, WRB }");
                    }
                    
                    sw.WriteLine("@attribute postag+1 { CCA, CCB, CCP, CCR, CCT,CDB, DTC, DTCP, DTPP, DTP, EX, FW, IN, JJD, JJN, JJR, JJS, LS, LM, MD, NNC, NNS, NNP, NNPS, PDT, PMC, PMP, POS, PRP, PRC, PRI, PRO, PRS, PRPS, PRSP, RB, RBF, RBI, RBK, RBP, RBR, RBW, RP, SYM, TO, UH, VB, VBD, VBG, VBN, VBP, VBOF, VBTS, VBZ, WDT, WP, WPS, WRB }");
                    sw.WriteLine("@attribute postag+2 { CCA, CCB, CCP, CCR, CCT,CDB, DTC, DTCP, DTPP, DTP, EX, FW, IN, JJD, JJN, JJR, JJS, LS, LM, MD, NNC, NNS, NNP, NNPS, PDT, PMC, PMP, POS, PRP, PRC, PRI, PRO, PRS, PRPS, PRSP, RB, RBF, RBI, RBK, RBP, RBR, RBW, RP, SYM, TO, UH, VB, VBD, VBG, VBN, VBP, VBOF, VBTS, VBZ, WDT, WP, WPS, WRB }");
                    //sw.WriteLine("@attribute named-entity-class-2 {LOC, PER, ORG, DATE, TIME, O}");
                    //sw.WriteLine("@attribute named-entity-class-1 {LOC, PER, ORG, DATE, TIME, O}");
                    //sw.WriteLine("@attribute named-entity-class+1 {LOC, PER, ORG, DATE, TIME, O}");
                    //sw.WriteLine("@attribute named-entity-class+2 {LOC, PER, ORG, DATE, TIME, O}");
                    sw.WriteLine("@attribute who {yes, no}");
                    sw.WriteLine("\n@data");
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
                    
                    foreach (var candidate in candidates)
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
                            str += "\"?\",";
                            ctrBefore++;
                        }
                        while (ctrBefore < position)
                        {
                            str += "\"" + tokenizedArticle[ctrBefore-1].Value + "\",";
                            ctrBefore++;
                        }
                        for (int c=0; c<2; c++)
                        {
                            if (endIndex + c < tokenizedArticle.Count)
                            {
                                str += "\"" + tokenizedArticle[endIndex+c].Value + "\",";
                            }
                            else
                            {
                                str += "\"?\",";
                            }
                        }
                        /******** POS tags of words Before and After candidate *******/
                        ctrBefore = wordsbefore;

                        while (ctrBefore < 1)
                        {
                            str += "\"?\",";
                            ctrBefore++;
                        }
                        while (ctrBefore < position)
                        {
                            str += "\"" + tokenizedArticle[ctrBefore - 1].PartOfSpeech + "\",";
                            ctrBefore++;
                        }
                        for (int c = 0; c < 2; c++)
                        {
                            if (endIndex + c < tokenizedArticle.Count)
                            {
                                str += "\"" + tokenizedArticle[endIndex + c].PartOfSpeech + "\",";
                            }
                            else
                            {
                                str += "\"?\",";
                            }
                        }

                        if (candidate.IsWho)
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
