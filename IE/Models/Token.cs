using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IE.Models
{
    enum NamedEntity
    {
        PERSON,
        LOCATION,
        DATE,
        ORGANIZATION,
        TIME
    }

    class Token
    {
        public String Value { get; set; }

        public int Sentence { get; set; }

        public int Position { get; set; }

        public String PartOfSpeech { get; set; }

        public NamedEntity NamedEntity { get; set; }

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
