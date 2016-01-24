using edu.stanford.nlp.ie.crf;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.process;
using IE.Models;
using java.io;
using System.Collections.Generic;

namespace IE
{
    public static class Preprocessor
    {
        static Article article;
        static List<Token> tokenizedArticle;

        internal static void setArticle(Article inputArticle)
        {
            article = inputArticle;
        }

        internal static List<Token> getTokenizedArticle()
        {
            return tokenizedArticle;
        }

        internal static void preprocess()
        {
            if(article == null)
            {
                return;
            }

            tokenize();
            performSS();
            performNER();
            performPOST();
            performWS();
        }

        static void tokenize()
        {
            tokenizedArticle = new List<Token>();
            int ctr = 0;

            PTBTokenizer ptbt = new PTBTokenizer(new StringReader(article.Body), new CoreLabelTokenFactory(), "");

            while (ptbt.hasNext())
            {
                ctr++;
                CoreLabel label = ((CoreLabel)ptbt.next());
                tokenizedArticle.Add(new Token(label.toString(), ctr));
                
                System.Console.WriteLine(label);
            }
        }

        static void performSS()
        {
            int ctr = 1;
            bool isPreceded = false;

            foreach(Token token in tokenizedArticle)
            {
                // Simple sentence segmentation
                if (token.Value.Equals('.'))
                {
                    isPreceded = true;
                    continue;
                }

                if(char.IsUpper(token.Value[0]) && isPreceded)
                { 
                    ctr++;
                }

                token.Sentence = ctr;
                isPreceded = false;
            }
        }

        static void performNER()
        {
            java.util.List tokens;
            List<string> values = new List<string>();
            object[] nerValues;
            var classifier = CRFClassifier.getClassifierNoExceptions(@"..\..\NERModel\filipino.all.4class.distsim.crf.ser.gz");

            foreach (Token token in tokenizedArticle)
            {
                values.Add(token.Value);
            }

            tokens = Sentence.toCoreLabelList(values.ToArray());

            nerValues = classifier.classifySentence(tokens).toArray();

            for(int i = 0; i < tokenizedArticle.Count; i++)
            {
                NamedEntity nerValue;
                System.Enum.TryParse(((CoreLabel)nerValues[i]).ner(), out nerValue);

                tokenizedArticle[i].NamedEntity = nerValue;
            }
        }

        static void performPOST()
        {

        }

        static void performWS()
        {
            Dictionary<string, int> frequencies = new Dictionary<string, int>();

            foreach(Token token in tokenizedArticle)
            {
                if(frequencies.ContainsKey(token.Value))
                {
                    frequencies[token.Value]++;
                }
                else
                {
                    frequencies[token.Value] = 1;
                }
            }

            foreach(Token token in tokenizedArticle)
            {
                token.Frequency = frequencies[token.Value];
            }
        }
    }
}
