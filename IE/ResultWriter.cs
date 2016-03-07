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
        private String formatDateDstPath;
        private String invertedIndexDstPath;
        List<Article> listAllArticles;
        List<List<String>> listAllWhoAnnotations;
        List<List<String>> listAllWhenAnnotations;
        List<List<String>> listAllWhereAnnotations;
        List<String> listAllWhatAnnotations;
        List<String> listAllWhyAnnotations;

        public ResultWriter(String pPath,
            String formatDatePath,
            String invertedPath,
            List<Article> pAllArticles,
            List<List<String>> pAllWhoAnnotations,
            List<List<String>> pAllWhenAnnotations,
            List<List<String>> pAllWhereAnnotations,
            List<String> pAllWhatAnnotations,
            List<String> pAllWhyAnnotations)
        {
            strPath = pPath;
            formatDateDstPath = formatDatePath;
            invertedIndexDstPath = invertedPath;
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
                    listAllWhatAnnotations[nI], listAllWhyAnnotations[nI], nI);
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        public void addArticleToXML(XmlTextWriter writer, String title, String author, DateTime date, String body, String link,
            String who, String when, String where, String what, String why, int index)
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
            writer.WriteStartElement("index");
            writer.WriteString(index.ToString());
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public Dictionary<string, List<string>> generateInvertedIndex (string feature)
        {
            List<Article> articleList = new List<Article>();

            XmlDocument doc = new XmlDocument();
            doc.Load(formatDateDstPath);

            XmlNodeList articleNodes = doc.DocumentElement.SelectNodes("/data/article");

            string[] anot = null;
            
            List<string> articles = new List<string>();

            Dictionary<string, List<string>> hash = new Dictionary<string, List<string>>();
            
            foreach (XmlNode articleNode in articleNodes)
            {
                anot = articleNode.SelectSingleNode(feature).InnerText.Split(new string[] {";"}, StringSplitOptions.RemoveEmptyEntries);
                
                for (int w = 0; w < anot.Length; w++)
                {
                    anot[w] = anot[w].Trim();
                    if (!hash.ContainsKey(anot[w]))
                    { 
                        List<string> temp = new List<string>();
                        temp.Add(articleNode.SelectSingleNode("index").InnerText);
                        hash.Add(anot[w], temp);
                    }
                    else
                    {
                        hash[anot[w]].Add(articleNode.SelectSingleNode("index").InnerText);
                    }

                }
            }

            return hash;
        }

        public void generateInvertedIndexOutput()
        {
            XmlTextWriter writer = new XmlTextWriter(invertedIndexDstPath, System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 4;
            writer.WriteStartElement("data");

            writer.WriteStartElement("who");
            addFeatureToXML(writer, "who");
            writer.WriteEndElement();
            writer.WriteStartElement("when");
            addFeatureToXML(writer, "when");
            writer.WriteEndElement();
            writer.WriteStartElement("where");
            addFeatureToXML(writer, "where");
            writer.WriteEndElement();
            writer.WriteStartElement("what");
            addFeatureToXML(writer, "what");
            writer.WriteEndElement();
            writer.WriteStartElement("why");
            addFeatureToXML(writer, "why");
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        public void addFeatureToXML(XmlTextWriter writer, string feature)
        {
            Dictionary<string, List<string>> hash = new Dictionary<string, List<string>>();
            hash = generateInvertedIndex(feature);
            foreach (KeyValuePair<string, List<string>> kvp in hash)
            {
                writer.WriteStartElement("entry");
                writer.WriteStartElement("text");
                writer.WriteString(kvp.Key);
                writer.WriteEndElement();
                foreach (string a in kvp.Value)
                {
                    writer.WriteStartElement("articleIndex");
                    writer.WriteString(a);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        public void processWhen(DateTime date, string annotation, XmlTextWriter writer)
        {
            Dictionary<string, int> hash = new Dictionary<string, int>();
            hash.Add("kahapon", -1);

            if (hash.ContainsKey(annotation))
            {
                writer.WriteStartElement("date");
                int translate = hash[annotation];
                date = date.AddDays(translate);
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
            }
            else
            {
                writer.WriteString(annotation);
            }
        }

        public void generateOutputFormatDate()
        {

            if (listAllWhoAnnotations.Count > listAllArticles.Count() ||
                listAllWhenAnnotations.Count() != listAllWhoAnnotations.Count() ||
                listAllWhereAnnotations.Count() != listAllWhoAnnotations.Count() ||
                listAllWhatAnnotations.Count() != listAllWhoAnnotations.Count() ||
                listAllWhyAnnotations.Count() != listAllWhoAnnotations.Count())
            {
                return;
            }

            XmlTextWriter writer = new XmlTextWriter(formatDateDstPath, System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 4;
            writer.WriteStartElement("data");
            for (int nI = 0; nI < listAllWhoAnnotations.Count(); nI++)
            {
                addArticleToXMLFormatWhen(writer, listAllArticles[nI].Title, listAllArticles[nI].Author, listAllArticles[nI].Date, listAllArticles[nI].Body, listAllArticles[nI].Link,
                    String.Join("; ", listAllWhoAnnotations[nI].ToArray()),
                    String.Join("; ", listAllWhenAnnotations[nI].ToArray()),
                    String.Join("; ", listAllWhereAnnotations[nI].ToArray()),
                    listAllWhatAnnotations[nI], listAllWhyAnnotations[nI], nI);
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        public void addArticleToXMLFormatWhen(XmlTextWriter writer, String title, String author, DateTime date, String body, String link,
            String who, String when, String where, String what, String why, int index)
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
            processWhen(date, when, writer);
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
            writer.WriteStartElement("index");
            writer.WriteString(index.ToString());
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
}
