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

            Article currentArticle = parseFile("C:\\Users\\Andrea\\Dropbox\\IE\\IE\\news.xml");
            List<Token> tokenizedArticle;

            Preprocessor.setArticle(currentArticle);
            Preprocessor.preprocess();
            tokenizedArticle = Preprocessor.getTokenizedArticle();

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

        static Article parseFile(String path)
        {
            try
            {
                XmlTextReader reader = new XmlTextReader(path);
                ArrayList articles = new ArrayList();
                
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "article")
                        {
                            ArrayList a = new ArrayList();
                            while (reader.Read())
                            {
                                if(reader.NodeType == XmlNodeType.Element)
                                {
                                    string name = reader.Name;
                                    if (name == "date")
                                    {
                                        string date = null;
                                        for (int i = 0; i < 12; i++)
                                        {
                                            reader.Read();
                                            if (reader.NodeType == XmlNodeType.Text)
                                            {
                                                date += reader.Value;
                                            }
                                        }
                                        a.Add(date);
                                    }
                                }
                                else if (reader.NodeType == XmlNodeType.Text)
                                {
                                    string val = reader.Value;
                                    a.Add(val);
                                }
                                else if (reader.NodeType == XmlNodeType.EndElement)
                                {
                                    string name = reader.Name;
                                    if (name == "article")
                                    {
                                        articles.Add(a);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                
                Console.ReadLine();
                foreach (ArrayList value in articles)
                {
                    foreach(string s in value)
                        Console.WriteLine(s);
                }

                return null;
            }
            catch (DirectoryNotFoundException dirEx)
            {
                Console.WriteLine("Directory not found: " + dirEx.Message);
            }

            return null;
            //title
            //author
            //date
            //link
            //body
        }
    }
}
