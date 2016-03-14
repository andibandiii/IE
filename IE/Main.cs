using IE.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IE
{
    public partial class Main : Form
    {
        private String[] sourcePaths = new String[3];
        private FileParser fileparserFP = new FileParser();

        private List<TextBox> textBoxes = new List<TextBox>();
        private List<GroupBox> firstBoxes = new List<GroupBox>();
        private List<GroupBox> secondBoxes = new List<GroupBox>();
        private List<ComboBox> comboBoxes = new List<ComboBox>();

        private List<Article> listViewerArticles = new List<Article>();
        private List<Article> listNavigatorArticles = new List<Article>();

        private List<Annotation> listViewerAnnotations = new List<Annotation>();
        private List<Annotation> listNavigatorAnnotations = new List<Annotation>();

        public Main()
        {
            InitializeComponent();
            
            textBoxes.Add(textBox1);
            textBoxes.Add(textBox3);
            textBoxes.Add(textBox4);

            firstBoxes.Add(groupBox1);
            firstBoxes.Add(groupBox3);
            firstBoxes.Add(groupBox4);

            secondBoxes.Add(groupBox2);
            secondBoxes.Add(groupBox6);
            secondBoxes.Add(groupBox5);

            comboBoxes.Add(null);
            comboBoxes.Add(comboBox4);
            comboBoxes.Add(comboBox5);
        }

        private void loadArticles()
        {
            comboBoxes[tabControl1.SelectedIndex].Items.Clear();

            foreach (Article a in tabControl1.SelectedIndex == 1 ?
                listViewerArticles :
                listNavigatorArticles)
            {
                comboBoxes[tabControl1.SelectedIndex].Items.Add(a.Title);
            }

            comboBoxes[tabControl1.SelectedIndex].SelectedIndex = 0;
        }

        public void saveChanges(int[] i, Annotation a)
        {

        }

        private void btnBrowseImport_Click(object sender, EventArgs e)
        {
            Stream s = null;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Import news articles (*.xml)";
            ofd.Filter = "XML files|*.xml";
            ofd.InitialDirectory = @"C:\";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((s = ofd.OpenFile()) != null)
                    {
                        using (s)
                        {
                            textBoxes[tabControl1.SelectedIndex].Text = ofd.FileName;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(textBoxes[tabControl1.SelectedIndex].Text);

            if (File.Exists(fi.FullName) && fi.Extension.Equals(".xml"))
            {
                sourcePaths[tabControl1.SelectedIndex] = fi.FullName;
                
                if (tabControl1.SelectedIndex == 1)
                {
                    listViewerArticles = fileparserFP.parseFile(sourcePaths[tabControl1.SelectedIndex]);
                    listViewerAnnotations = fileparserFP.parseAnnotations(sourcePaths[tabControl1.SelectedIndex]);

                    if (listViewerArticles.Count <= 0)
                    {
                        MessageBox.Show("No articles found!");
                        return;
                    }

                    loadArticles();
                }

                //firstBoxes[tabControl1.SelectedIndex].Enabled = false;
                secondBoxes[tabControl1.SelectedIndex].Enabled = true;
            }
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            ArticleView view = new ArticleView(this,
                new int[] { tabControl1.SelectedIndex, comboBoxes[tabControl1.SelectedIndex].SelectedIndex },
                (tabControl1.SelectedIndex == 1 ?
                listViewerArticles :
                listNavigatorArticles)[comboBoxes[tabControl1.SelectedIndex].SelectedIndex],
                (tabControl1.SelectedIndex == 1 ?
                listViewerAnnotations :
                listNavigatorAnnotations)[comboBoxes[tabControl1.SelectedIndex].SelectedIndex]);
            view.ShowDialog();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                resetViewer();
            }
            else if (tabControl1.SelectedIndex == 2)
            {
                resetNavigator();
            }
        }

        #region Extractor Methods

        private void extract(String destinationPath, String invertedDestinationPath, String formatDateDestinationPath)
        {
            Boolean isAnnotated = false;
            List<Article> listCurrentArticles = fileparserFP.parseFile(sourcePaths[tabControl1.SelectedIndex]);
            List<Annotation> listCurrentTrainingAnnotations = new List<Annotation>();
            List<List<Token>> listTokenizedArticles = new List<List<Token>>();
            List<List<Candidate>> listAllWhoCandidates = new List<List<Candidate>>();
            List<List<Candidate>> listAllWhenCandidates = new List<List<Candidate>>();
            List<List<Candidate>> listAllWhereCandidates = new List<List<Candidate>>();
            List<List<List<Token>>> listAllWhatCandidates = new List<List<List<Token>>>();
            List<List<List<Token>>> listAllWhyCandidates = new List<List<List<Token>>>();
            List<List<String>> listAllWhoAnnotations = new List<List<String>>();
            List<List<String>> listAllWhenAnnotations = new List<List<String>>();
            List<List<String>> listAllWhereAnnotations = new List<List<String>>();
            List<String> listAllWhatAnnotations = new List<String>();
            List<String> listAllWhyAnnotations = new List<String>();


            if (listCurrentArticles != null && listCurrentArticles.Count > 0 &&
                (!isAnnotated || (listCurrentTrainingAnnotations != null && listCurrentTrainingAnnotations.Count > 0 &&
                listCurrentArticles.Count == listCurrentTrainingAnnotations.Count)))
            {
                Preprocessor preprocessor = new Preprocessor();

                //Temporarily set to 2 because getting all articles takes longer run time
                for (int nI = 0; nI < listCurrentArticles.Count; nI++)
                {
                    preprocessor.setCurrentArticle(listCurrentArticles[nI]);
                    preprocessor.preprocess();

                    listTokenizedArticles.Add(preprocessor.getLatestTokenizedArticle());
                    listAllWhoCandidates.Add(preprocessor.getWhoCandidates());
                    listAllWhenCandidates.Add(preprocessor.getWhenCandidates());
                    listAllWhereCandidates.Add(preprocessor.getWhereCandidates());
                    listAllWhatCandidates.Add(preprocessor.getWhatCandidates());
                    listAllWhyCandidates.Add(preprocessor.getWhyCandidates());
                }
            }

            Identifier annotationIdentifier = new Identifier();
            for (int nI = 0; nI < listCurrentArticles.Count; nI++)
            {
                annotationIdentifier.setCurrentArticle(listTokenizedArticles[nI]);
                annotationIdentifier.setWhoCandidates(listAllWhoCandidates[nI]);
                annotationIdentifier.setWhenCandidates(listAllWhenCandidates[nI]);
                annotationIdentifier.setWhereCandidates(listAllWhereCandidates[nI]);
                annotationIdentifier.setWhatCandidates(listAllWhatCandidates[nI]);
                annotationIdentifier.setWhyCandidates(listAllWhyCandidates[nI]);
                annotationIdentifier.labelAnnotations();
                listAllWhoAnnotations.Add(annotationIdentifier.getWho());
                listAllWhenAnnotations.Add(annotationIdentifier.getWhen());
                listAllWhereAnnotations.Add(annotationIdentifier.getWhere());
                listAllWhatAnnotations.Add(annotationIdentifier.getWhat());
                listAllWhyAnnotations.Add(annotationIdentifier.getWhy());
            }

            ResultWriter rw = new ResultWriter(destinationPath, invertedDestinationPath, formatDateDestinationPath, listCurrentArticles, listAllWhoAnnotations, listAllWhenAnnotations, listAllWhereAnnotations, listAllWhatAnnotations, listAllWhyAnnotations);
            rw.generateOutput();
            rw.generateOutputFormatDate();
            rw.generateInvertedIndexOutput();
        }

        private void btnBrowseExtract_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Extract 5W's to file (*.xml)";
            sfd.Filter = "XML files|*.xml";
            sfd.InitialDirectory = @"C:\";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = sfd.FileName;
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(textBox2.Text);

            if (fi.Extension.Equals(".xml"))
            {
                String destinationPath = fi.FullName;
                String invertedDestinationPath = fi.FullName.Insert(fi.FullName.Length - 4, "_invereted_index");
                String formatDateDestinationPath = fi.FullName.Insert(fi.FullName.Length - 4, "_format_date");

                extract(destinationPath, invertedDestinationPath, formatDateDestinationPath);
                MessageBox.Show("Operation completed!");
                resetExtractor();

                textBox3.Text = destinationPath;
                textBox4.Text = invertedDestinationPath;
            }
        }

        private void resetExtractor()
        {
            groupBox1.Enabled = true;
            groupBox2.Enabled = false;
            textBox1.Text = "";
            textBox2.Text = "";
        }

        #endregion

        #region Viewer Methods

        private void resetViewer()
        {
            groupBox3.Enabled = true;
            groupBox6.Enabled = false;
            textBox3.Text = "";
            comboBox4.Items.Clear();
            listViewerArticles = null;
            listViewerAnnotations = null;
        }

        #endregion

        #region Navigator Methods

        private void resetNavigator()
        {
            //groupBox3.Enabled = true;
            //groupBox6.Enabled = false;
            //textBox3.Text = "";
            //comboBox4.Items.Clear();
            //listViewerArticles = null;
            //listViewerAnnotations = null;
        }

        #endregion
    }
}
