using edu.stanford.nlp.tagger.maxent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IE.POSTaggerModule
{
    class POSTagger
    {
        /// <summary>
        /// File path for POS Tagger to be used.
        /// </summary>
        private const string modelPath = @"..\..\POSTaggerModule\models\wsj-0-18-bidirectional-nodistsim.tagger";

        /// <summary>
        /// MaxentTagger variable to be used.
        /// </summary>
        private MaxentTagger tagger;

        public POSTagger()
        {
            #if DEBUG
            tagger = new MaxentTagger(modelPath);
            #else
            try
            {
                tagger = new MaxentTagger(modelPath);
            }
            catch (Exception e)
            {
                tagger = null;
            }
            #endif
        }

        /// <summary>
        /// Function for tagging a string of text that contains one or more sentences.
        /// </summary>
        /// <param name="text">Text to be tagged</param>
        /// <returns>Key-Value pair where Key is the string and Value is the POS tag</returns>
        public Dictionary<String, String> tagText(String text)
        {
            #if RELEASE
            if (tagger == null) return null;
            #endif

            Dictionary<String, String> tokenToTag = new Dictionary<String, String>();

            var sentences = MaxentTagger.tokenizeText(new java.io.StringReader(text)).toArray();
            foreach (java.util.ArrayList sentence in sentences)
            {
                var taggedSentence = tagger.tagSentence(sentence).toArray();
                var convertedTaggedSentence = new List<String>();
                foreach (var word in taggedSentence)
                {
                    var splitWord = word.ToString().Split('/');
                    if (splitWord.Length >= 2)
                    {
                        tokenToTag[splitWord[0]] = splitWord[1];
                    }
                }
            }

            return tokenToTag;
        }
    }
}
