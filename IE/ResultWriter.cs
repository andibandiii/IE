using IE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IE
{
    public class ResultWriter
    {
        private String strPath;
        List<Article> listAllArticles;
        List<List<String>> listAllWhoAnnotations;
        List<List<String>> listAllWhenAnnotations;
        List<List<String>> listAllWhereAnnotations;
        List<String> listAllWhatAnnotations;
        List<String> listAllWhyAnnotations;

        public ResultWriter(String pPath, 
            List<Article> pAllArticles,
            List<List<String>> pAllWhoAnnotations,
            List<List<String>> pAllWhenAnnotations,
            List<List<String>> pAllWhereAnnotations,
            List<String> pAllWhatAnnotations,
            List<String> pAllWhyAnnotations)
        {
            strPath = pPath;
            listAllArticles = pAllArticles;
            listAllWhoAnnotations = pAllWhoAnnotations;
            listAllWhenAnnotations = pAllWhenAnnotations;
            listAllWhereAnnotations = pAllWhereAnnotations;
            listAllWhatAnnotations = pAllWhatAnnotations;
            listAllWhyAnnotations = pAllWhyAnnotations;
        }

        public void generateOutput()
        {
            if (listAllWhoAnnotations.Count > listAllArticles.Count() ||
                listAllWhenAnnotations.Count() != listAllWhoAnnotations.Count() ||
                listAllWhereAnnotations.Count() != listAllWhoAnnotations.Count() ||
                listAllWhatAnnotations.Count() != listAllWhoAnnotations.Count() ||
                listAllWhyAnnotations.Count() != listAllWhoAnnotations.Count())
            {
                return;
            }

            XmlTextWriter writer = new XmlTextWriter(strPath, System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 4;
            writer.WriteStartElement("data");
            for (int nI = 0; nI < listAllWhoAnnotations.Count(); nI++)
            {
                addArticleToXML(writer, listAllArticles[nI].Title, listAllArticles[nI].Author, listAllArticles[nI].Date, listAllArticles[nI].Body, listAllArticles[nI].Link,
                    String.Join("; ", listAllWhoAnnotations[nI].ToArray()),
                    String.Join("; ", listAllWhenAnnotations[nI].ToArray()),
                    String.Join("; ", listAllWhereAnnotations[nI].ToArray()),
                    listAllWhatAnnotations[nI], listAllWhyAnnotations[nI]);
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        public void addArticleToXML(XmlTextWriter writer, String title, String author, DateTime date, String body, String link,
            String who, String when, String where, String what, String why)
        {
            writer.WriteStartElement("article");
            writer.WriteStartElement("title");
            writer.WriteString(title);
            writer.WriteEndElement();
            writer.WriteStartElement("author");
            writer.WriteString(author);
            writer.WriteEndElement();
            writer.WriteStartElement("date");
            writer.WriteStartElement("month");
            writer.WriteString(date.Month.ToString());
            writer.WriteEndElement();
            writer.WriteStartElement("day");
            writer.WriteString(date.Day.ToString());
            writer.WriteEndElement();
            writer.WriteStartElement("year");
            writer.WriteString(date.Year.ToString());
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteStartElement("body");
            writer.WriteString(body);
            writer.WriteEndElement();
            writer.WriteStartElement("link");
            writer.WriteString(link);
            writer.WriteEndElement();
            writer.WriteStartElement("who");
            writer.WriteString(who);
            writer.WriteEndElement();
            writer.WriteStartElement("when");
            writer.WriteString(when);
            writer.WriteEndElement();
            writer.WriteStartElement("where");
            writer.WriteString(where);
            writer.WriteEndElement();
            writer.WriteStartElement("what");
            writer.WriteString(what);
            writer.WriteEndElement();
            writer.WriteStartElement("why");
            writer.WriteString(why);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
}
