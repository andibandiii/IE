using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using IE.POSTaggerModule;

namespace IE
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            parseFile();

            //Initialize the POS Tagger
            POSTagger post = new POSTagger();
            //Test a sample sentence
            post.tagText("Sinabi ni Pangulong Arroyo kahapon na inatasan niya si Vice President Noli de Castro na pumuntang Libya para tingnan ang posibilidad kung may mga oportunidad ng trabaho ang mga Pilipinong manggagawa sa bansa.");

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }

        public static void parseFile()
        {
            try
            {
                XmlTextReader reader = new XmlTextReader("C:\\Users\\Andrea\\Dropbox\\IE\\IE\\news.xml");
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
            }
            catch (DirectoryNotFoundException dirEx)
            {
                Console.WriteLine("Directory not found: " + dirEx.Message);
            }


            //title
            //author
            //date
            //link
            //body
        }
    }
}
