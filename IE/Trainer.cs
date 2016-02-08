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
            //if (tokenizedArticle == null || candidates == null)
            if (tokenizedArticle == null)
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
                    sw.WriteLine("@attribute word-2 string\n@attribute word-1 string\n@attribute word+1 string\n@attribute word+2 string");
                    sw.WriteLine("@attribute postag-2 { CCA, CCB, CCP, CCR, CCT,CDB, DTC, DTCP, DTPP, DTP, EX, FW, IN, JJD, JJN, JJR, JJS, LS, LM, MD, NNC, NNS, NNP, NNPS, PDT, PMC, PMP, POS, PRP, PRC, PRI, PRO, PRS, PRPS, PRSP, RB, RBF, RBI, RBK, RBP, RBR, RBW, RP, SYM, TO, UH, VB, VBD, VBG, VBN, VBP, VBOF, VBTS, VBZ, WDT, WP, WPS, WRB }");
                    sw.WriteLine("@attribute postag-1 { CCA, CCB, CCP, CCR, CCT,CDB, DTC, DTCP, DTPP, DTP, EX, FW, IN, JJD, JJN, JJR, JJS, LS, LM, MD, NNC, NNS, NNP, NNPS, PDT, PMC, PMP, POS, PRP, PRC, PRI, PRO, PRS, PRPS, PRSP, RB, RBF, RBI, RBK, RBP, RBR, RBW, RP, SYM, TO, UH, VB, VBD, VBG, VBN, VBP, VBOF, VBTS, VBZ, WDT, WP, WPS, WRB }");
                    sw.WriteLine("@attribute postag+1 { CCA, CCB, CCP, CCR, CCT,CDB, DTC, DTCP, DTPP, DTP, EX, FW, IN, JJD, JJN, JJR, JJS, LS, LM, MD, NNC, NNS, NNP, NNPS, PDT, PMC, PMP, POS, PRP, PRC, PRI, PRO, PRS, PRPS, PRSP, RB, RBF, RBI, RBK, RBP, RBR, RBW, RP, SYM, TO, UH, VB, VBD, VBG, VBN, VBP, VBOF, VBTS, VBZ, WDT, WP, WPS, WRB }");
                    sw.WriteLine("@attribute postag+2 { CCA, CCB, CCP, CCR, CCT,CDB, DTC, DTCP, DTPP, DTP, EX, FW, IN, JJD, JJN, JJR, JJS, LS, LM, MD, NNC, NNS, NNP, NNPS, PDT, PMC, PMP, POS, PRP, PRC, PRI, PRO, PRS, PRPS, PRSP, RB, RBF, RBI, RBK, RBP, RBR, RBW, RP, SYM, TO, UH, VB, VBD, VBG, VBN, VBP, VBOF, VBTS, VBZ, WDT, WP, WPS, WRB }");
                    sw.WriteLine("@attribute named-entity-class-2 {LOC, PER, ORG, DATE, TIME, O}");
                    sw.WriteLine("@attribute named-entity-class-1 {LOC, PER, ORG, DATE, TIME, O}");
                    sw.WriteLine("@attribute named-entity-class+1 {LOC, PER, ORG, DATE, TIME, O}");
                    sw.WriteLine("@attribute named-entity-class+2 {LOC, PER, ORG, DATE, TIME, O}");
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
                    int ctr = 0;

                    //foreach (var candidate in candidates)
                    foreach (var candidate in tokenizedArticle)
                    {
                        value = candidate.Value;
                        sentence = candidate.Sentence;
                        position = candidate.Position;
                        frequency = candidate.Frequency;
                        str = "\"" + value + "\"," + sentence + "," + position + "," + frequency + ",";

                        while (ctr < tokenizedArticle.Count && tokenizedArticle[ctr].Value.Equals(value) == false && tokenizedArticle[ctr].Sentence.Equals(sentence) == false && tokenizedArticle[ctr].Position.Equals(position) == false)
                        {
                            ctr++;
                        }

                        if (ctr - 2 >= 0 && ctr + 2 <= tokenizedArticle.Count - 1)
                        {
                            str += "\"" + tokenizedArticle[ctr - 2].Value + "\",\"" + tokenizedArticle[ctr - 1].Value + "\",\"" + tokenizedArticle[ctr + 1].Value + "\",\"" + tokenizedArticle[ctr + 2].Value + "\", " + tokenizedArticle[ctr - 2].PartOfSpeech + ", " + tokenizedArticle[ctr - 1].PartOfSpeech + ", " + tokenizedArticle[ctr + 1].PartOfSpeech + ", " + tokenizedArticle[ctr + 2].PartOfSpeech + ", " + tokenizedArticle[ctr - 2].NamedEntity + ", " + tokenizedArticle[ctr - 1].NamedEntity + ", " + tokenizedArticle[ctr + 1].NamedEntity + ", " + tokenizedArticle[ctr + 2].NamedEntity;
                        }
                        else if (ctr == 0)
                        {
                            str += "\"?\",\"?\",\"" + tokenizedArticle[ctr + 1].Value + "\",\"" + tokenizedArticle[ctr + 2].Value + "\", ?, ?, " + tokenizedArticle[ctr + 1].PartOfSpeech + ", " + tokenizedArticle[ctr + 2].PartOfSpeech + ", ?, ?, " + tokenizedArticle[ctr + 1].NamedEntity + ", " + tokenizedArticle[ctr + 2].NamedEntity;
                        }
                        else if (ctr == 1)
                        {
                            str += "\"?\",\"" + tokenizedArticle[ctr - 1].Value + "\",\"" + tokenizedArticle[ctr + 1].Value + "\",\"" + tokenizedArticle[ctr + 2].Value + "\", ?, " + tokenizedArticle[ctr - 1].PartOfSpeech + ", " + tokenizedArticle[ctr + 1].PartOfSpeech + ", " + tokenizedArticle[ctr + 2].PartOfSpeech + ", ?, " + tokenizedArticle[ctr - 1].NamedEntity + ", " + tokenizedArticle[ctr + 1].NamedEntity + ", " + tokenizedArticle[ctr + 2].NamedEntity;
                        }
                        else if (ctr == tokenizedArticle.Count - 1)
                        {
                            str += "\"" + tokenizedArticle[ctr - 2].Value + "\",\"" + tokenizedArticle[ctr - 1].Value + "\",\"?\",\"?\", " + tokenizedArticle[ctr - 2].PartOfSpeech + ", " + tokenizedArticle[ctr - 1].PartOfSpeech + ", ?, ?" + ", " + tokenizedArticle[ctr - 2].NamedEntity + ", " + tokenizedArticle[ctr - 1].NamedEntity + ", ?, ?";
                        }
                        else if (ctr == tokenizedArticle.Count - 2)
                        {
                            str += "\"" + tokenizedArticle[ctr - 2].Value + "\",\"" + tokenizedArticle[ctr - 1].Value + "\",\"" + tokenizedArticle[ctr + 1].Value + "\",\"?\", " + tokenizedArticle[ctr - 2].PartOfSpeech + ", " + tokenizedArticle[ctr - 1].PartOfSpeech + ", " + tokenizedArticle[ctr + 1].PartOfSpeech + ", ?" + ", " + tokenizedArticle[ctr - 2].NamedEntity + ", " + tokenizedArticle[ctr - 1].NamedEntity + ", " + tokenizedArticle[ctr + 1].NamedEntity + ", ?";
                        }
                        
                        if (candidate.IsWho)
                        {
                            str += ", yes";
                        }
                        else
                        {
                            str += ", no";
                        }
                        ctr++;
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
