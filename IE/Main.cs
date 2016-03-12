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
        private Boolean isAnnotated = false;
        private String sourcePath { get; set; }
        private String destinationPath { get; set; }
        private String invertedDestinationPath { get; set; }
        private String formatDateDestinationPath { get; set; }
        private FileParser fileparserFP = new FileParser();

        public Main()
        {
            InitializeComponent();
        }

        private void extract()
        {
            List<Article> listCurrentArticles = fileparserFP.parseFile(sourcePath);
            List<Annotation> listCurrentTrainingAnnotations = new List<Annotation>();
            if (isAnnotated)
            {
                listCurrentTrainingAnnotations = fileparserFP.parseAnnotations(sourcePath);
            }
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

                    if (isAnnotated)
                    {
                        preprocessor.setCurrentAnnotation(listCurrentTrainingAnnotations[nI]);
                        preprocessor.performAnnotationAssignment();
                    }

                    listTokenizedArticles.Add(preprocessor.getLatestTokenizedArticle());
                    listAllWhoCandidates.Add(preprocessor.getWhoCandidates());
                    listAllWhenCandidates.Add(preprocessor.getWhenCandidates());
                    listAllWhereCandidates.Add(preprocessor.getWhereCandidates());
                    listAllWhatCandidates.Add(preprocessor.getWhatCandidates());
                    listAllWhyCandidates.Add(preprocessor.getWhyCandidates());
                }

                if (isAnnotated)
                {
                    Trainer whoTrainer = new WhoTrainer();
                    Trainer whenTrainer = new WhenTrainer();
                    Trainer whereTrainer = new WhereTrainer();
                    whoTrainer.trainMany(listTokenizedArticles, listAllWhoCandidates);
                    whenTrainer.trainMany(listTokenizedArticles, listAllWhenCandidates);
                    whereTrainer.trainMany(listTokenizedArticles, listAllWhereCandidates);
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

        #region Extractor Methods

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
                            textBox1.Text = ofd.FileName;
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
            FileInfo fi = new FileInfo(textBox1.Text);

            if (File.Exists(fi.FullName) && fi.Extension.Equals(".xml"))
            {
                sourcePath = fi.FullName;
                groupBox1.Enabled = false;
                groupBox2.Enabled = true;
            }
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
                destinationPath = fi.FullName;
                invertedDestinationPath = fi.FullName.Insert(fi.FullName.Length - 4, "_invereted_index");
                formatDateDestinationPath = fi.FullName.Insert(fi.FullName.Length - 4 , "_format_date");
                extract();
                MessageBox.Show("Operation completed!");
                resetExtractor();
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

        #endregion

        #region Navigator Methods

        #endregion
    }
}
