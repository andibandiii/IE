using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using IE.Models;

namespace IE
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            FileParser fileparserFP = new FileParser();

            List<Article> listCurrentArticles = fileparserFP.parseFile(@"..\..\aprileditorial1.xml");
            List<List<Token>> listTokenizedArticles = new List<List<Token>>();
            List<List<Token>> listAllWhoCandidates = new List<List<Token>>();

            if (listCurrentArticles != null && listCurrentArticles.Count > 0)
            {
                Preprocessor preprocessor = new Preprocessor();
                Trainer whoTrainer = new WhoTrainer();

                //Temporarily set to 2 because getting all articles takes longer run time
                for (int nI = 0; nI < 2; nI++)
                {
                    preprocessor.setCurrent(listCurrentArticles[nI]);
                    preprocessor.preprocess();
                    listTokenizedArticles.Add(preprocessor.getLatestTokenizedArticle());
                    listAllWhoCandidates.Add(preprocessor.getWhoCandidates());
                }

                whoTrainer.trainMany(listTokenizedArticles, listAllWhoCandidates);
            }

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }
    }
}
