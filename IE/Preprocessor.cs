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

        }

        static void performNER()
        {
            // Path to the folder with classifiers models
            var jarRoot = @"..\..\..\..\paket-files\nlp.stanford.edu\stanford-ner-2015-12-09";
            var classifiersDirecrory = jarRoot + @"\classifiers";

            // Loading 3 class classifier model
            var classifier = CRFClassifier.getClassifierNoExceptions(
                classifiersDirecrory + @"\english.all.3class.distsim.crf.ser.gz");

            var s1 = "Good afternoon Rajat Raina, how are you today?";
            System.Console.WriteLine("{0}\n", classifier.classifyToString(s1));

            var s2 = "I go to school at Stanford University, which is located in California.";
            System.Console.WriteLine("{0}\n", classifier.classifyWithInlineXML(s2));

            System.Console.WriteLine("{0}\n", classifier.classifyToString(s2, "xml", true));
        }

        static void performPOST()
        {

        }

        static void performWS()
        {

        }
    }
}
