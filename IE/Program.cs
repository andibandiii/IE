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
            String sourcePath = @"..\..\aprileditorial1.xml";
            String destinationPath = @"..\..\result.xml";

            List<Article> listCurrentArticles = fileparserFP.parseFile(sourcePath);
            List<Annotation> listCurrentTrainingAnnotations = fileparserFP.parseAnnotations(sourcePath);
            List<List<Token>> listTokenizedArticles = new List<List<Token>>();
            List<List<Token>> listAllWhoCandidates = new List<List<Token>>();
            List<List<Token>> listAllWhenCandidates = new List<List<Token>>();
            List<List<Token>> listAllWhereCandidates = new List<List<Token>>();
            List<List<List<Token>>> listAllWhatCandidates = new List<List<List<Token>>>();
            List<List<List<Token>>> listAllWhyCandidates = new List<List<List<Token>>>();
            List<List<String>> listAllWhoAnnotations = new List<List<String>>();
            List<List<String>> listAllWhenAnnotations = new List<List<String>>();
            List<List<String>> listAllWhereAnnotations = new List<List<String>>();
            List<String> listAllWhatAnnotations = new List<String>();
            List<String> listAllWhyAnnotations = new List<String>();


            if (listCurrentArticles != null && listCurrentArticles.Count > 0 &&
                listCurrentTrainingAnnotations != null && listCurrentTrainingAnnotations.Count > 0 &&
                listCurrentArticles.Count == listCurrentTrainingAnnotations.Count)
            {
                Preprocessor preprocessor = new Preprocessor();
                Trainer whoTrainer = new WhoTrainer();
                Trainer whenTrainer = new WhenTrainer();
                Trainer whereTrainer = new WhereTrainer();

                //Temporarily set to 2 because getting all articles takes longer run time
                for (int nI = 0; nI < 2; nI++)
                {
                    preprocessor.setCurrentArticle(listCurrentArticles[nI]);
                    preprocessor.preprocess();

                    preprocessor.setCurrentAnnotation(listCurrentTrainingAnnotations[nI]);
                    preprocessor.performAnnotationAssignment();

                    listTokenizedArticles.Add(preprocessor.getLatestTokenizedArticle());
                    listAllWhoCandidates.Add(preprocessor.getWhoCandidates());
                    listAllWhenCandidates.Add(preprocessor.getWhenCandidates());
                    listAllWhereCandidates.Add(preprocessor.getWhereCandidates());
                    listAllWhatCandidates.Add(preprocessor.getWhatCandidates());
                    listAllWhyCandidates.Add(preprocessor.getWhyCandidates());
                }

                whoTrainer.trainMany(listTokenizedArticles, listAllWhoCandidates);
                whenTrainer.trainMany(listTokenizedArticles, listAllWhenCandidates);
                whereTrainer.trainMany(listTokenizedArticles, listAllWhereCandidates);
            }

            Identifier annotationIdentifier = new Identifier();
            for (int nI = 0; nI < listTokenizedArticles.Count(); nI++)
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

            ResultWriter rw = new ResultWriter(destinationPath, listCurrentArticles, listAllWhoAnnotations, listAllWhenAnnotations, listAllWhereAnnotations, listAllWhatAnnotations, listAllWhyAnnotations);
            rw.generateOutput();
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }
    }
}
