using IE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IE
{
    class FileParser
    {
        public List<Article> parseFile(String path)
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

        public List<Annotation> parseAnnotations(String path)
        {
            List<Annotation> annotationList = new List<Annotation>();

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

                annotationList.Add(annotation);
            }

            return annotationList;
        }
    }
}
