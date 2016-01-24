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
            ////Initialize the POS Tagger
            //POSTagger post = new POSTagger();
            ////Test a sample sentence
            //post.tagText("Sinabi ni Pangulong Arroyo kahapon na inatasan niya si Vice President Noli de Castro na pumuntang Libya para tingnan ang posibilidad kung may mga oportunidad ng trabaho ang mga Pilipinong manggagawa sa bansa.");

            List<Article> currentArticles = parseFile(@"..\..\news.xml");
            List<Token> tokenizedArticle;

            if (currentArticles != null && currentArticles.Count > 0)
            {
                Preprocessor.setArticle(currentArticles[0]);
                Preprocessor.preprocess();
                tokenizedArticle = Preprocessor.getTokenizedArticle();
            }

            //Trainer.setTokenizedArticle(tokenizedArticle);
            //Trainer.train();

            //Identifier.setTokenizedArticle(tokenizedArticle);
            //Identifier.setModels(Trainer.getModels());
            //Identifier.label5Ws();
            //tokenizedArticle = Identifier.getTokenizedArticle();
            
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }

        static List<Article> parseFile(String path)
        {
            List<Article> articleList = new List<Article>();

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNodeList articleNodes = doc.DocumentElement.SelectNodes("/data/article");

            foreach (XmlNode articleNode in articleNodes)
            {
                Article article = new Article();

                article.Author = articleNode.SelectSingleNode("author").InnerText;
                article.Body = articleNode.SelectSingleNode("body").InnerText;
                article.Link = articleNode.SelectSingleNode("link").InnerText;
                article.Title = articleNode.SelectSingleNode("title").InnerText;

                String date = articleNode.SelectSingleNode("date").SelectSingleNode("month").InnerText + "/" +
                    articleNode.SelectSingleNode("date").SelectSingleNode("day").InnerText + "/" +
                    articleNode.SelectSingleNode("date").SelectSingleNode("year").InnerText;

                DateTime tempDate = new DateTime(2000, 01, 01);
                DateTime.TryParse(date, out tempDate);
                article.Date = tempDate;

                articleList.Add(article);
            }

            //Print Results
            foreach (Article article in articleList)
            {
                Console.WriteLine("Author: " + article.Author);
                Console.WriteLine("Title: " + article.Title);
                Console.WriteLine("Date: " + article.Date);
                Console.WriteLine("Body: " + article.Body);
                Console.WriteLine("Link: " + article.Link);
            }

            return articleList;
        }
    }
}
