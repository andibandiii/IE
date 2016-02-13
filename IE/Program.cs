﻿using System;
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
            List<Article> currentArticles = parseFile(@"..\..\aprileditorial1.xml");
            List<Token> tokenizedArticle;

            List<Annotation> currentAnnotations = parseAnnotations(@"..\..\aprileditorial1.xml");
            List<Token> candidates;

            if (currentArticles != null && currentArticles.Count > 0)
            {
                //Temporarily set to 2 because getting all articles takes longer run time
                for (int nI = 0; nI < 2; nI++)
                {
                    Preprocessor.setArticle(currentArticles[nI]);
                    Preprocessor.setAnnotations(currentAnnotations[nI]);
                    Preprocessor.preprocess();
                    tokenizedArticle = Preprocessor.getTokenizedArticle();
                    candidates = Preprocessor.getCandidates();
                    Trainer.setTokenizedArticle(tokenizedArticle);
                    Trainer.setCandidatesList(candidates);
                    Trainer.train(nI == 0 ? true : false);
                }
            }

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

            return articleList;
        }

        static List<Annotation> parseAnnotations(String path)
        {
            List<Annotation> annotationsList = new List<Annotation>();

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNodeList articleNodes = doc.DocumentElement.SelectNodes("/data/article");

            foreach (XmlNode articleNode in articleNodes)
            {
                Annotation annotation = new Annotation();

                annotation.Who = articleNode.SelectSingleNode("who").InnerText;
                annotation.Where = articleNode.SelectSingleNode("where").InnerText;
                annotation.When = articleNode.SelectSingleNode("when").InnerText;
                annotation.What = articleNode.SelectSingleNode("what").InnerText;
                annotation.Why = articleNode.SelectSingleNode("why").InnerText;

                annotationsList.Add(annotation);
            }

            return annotationsList;
        }
    }
}
