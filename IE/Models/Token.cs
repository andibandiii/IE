using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IE.Models
{
    enum NamedEntity
    {
        PER,
        LOC,
        DATE,
        ORG
    }

    public class Token
    {
        public static readonly String[] NamedEntityTags = {
            "PER", "LOC", "DATE", "ORG"
        };

        public static readonly String[] PartOfSpeechTags = {
            "NN", "NNC", "NNP", "NNPA", "PR", "PRS", "PRSP", "PRO", "PROP", "PRQ", "PRL", "PRN", "PRC", "PRF", "DT", "DTC", "DTCP", "DTP", "DTPP", "CC", "CCA", "CCD", "CCC", "CCP", "VB", "VBW", "VBS", "VBH", "VBL", "VBTS", "VBTR", "VBTF", "JJ", "JJD", "JJC", "JJCC", "JJCS", "JJCN", "JJN", "RB", "RBD", "RBN", "RBC", "RBQ", "RBT", "RBF", "RBW", "RBI", "RBM", "PP", "PPA", "PPIN", "PPF", "PPM", "PPU", "PPR", "PPD", "PPBY", "PPTS", "PPL", "PPO", "CD", "CDB", "PM", "PMP", "PME", "PMQ", "PMC", "PMS"
        };

        public String Value { get; set; }

        public int Sentence { get; set; }

        public int Position { get; set; }

        public String PartOfSpeech { get; set; }

        public String NamedEntity { get; set; }

        public int Frequency { get; set; }

        public Boolean IsWho { get; set; }

        public Boolean IsWhen { get; set; }

        public Boolean IsWhere { get; set; }

        public Boolean IsWhat { get; set; }

        public Boolean IsWhy { get; set; }

        public Token(String value, int position)
        {
            Value = value;
            Position = position;
        }
    }
}
