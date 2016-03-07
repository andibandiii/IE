using IE.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IE
{
    abstract class Trainer
    {
        protected List<Token> listTokenizedArticle;
        protected List<Candidate> listCandidates;

        protected Trainer()
        {
            listTokenizedArticle = new List<Token>();
            listCandidates = new List<Candidate>();
        }

        public void setTokenizedArticle(List<Token> pTokenizedArticle)
        {
            listTokenizedArticle = pTokenizedArticle;
        }

        public void setCandidates(List<Candidate> pCandidates)
        {
            listCandidates = pCandidates;
        }

        /// <summary>
        /// Train a new model by creating an .arff file. 
        /// Set isNewFile to false if you're just adding articles to an existing training file.
        /// Otherwise, set isNewFile to true if you're creating a new training file.
        /// </summary>
        public abstract void train(bool isNewFile);

        public void trainMany(List<List<Token>> pTokenizedArticleList, List<List<Candidate>> pAllCandidateLists)
        {
            if (pTokenizedArticleList.Count != pAllCandidateLists.Count)
            {
                return;
            }

            for (int nI = 0; nI < pTokenizedArticleList.Count; nI++)
            {
                setTokenizedArticle(pTokenizedArticleList[nI]);
                setCandidates(pAllCandidateLists[nI]);
                train(nI == 0);
            }
        } 
    }
}
