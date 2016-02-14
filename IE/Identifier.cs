using IE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IE
{
    public class Identifier
    {
        private List<Token> articleCurrent;
        private List<Token> listWhoCandidates;
        private List<Token> listWhenCandidates;
        private List<Token> listWhereCandidates;
        private List<List<Token>> listWhatCandidates;
        private List<List<Token>> listWhyCandidates;
        private List<String> listWho;
        private List<String> listWhen;
        private List<String> listWhere;
        private String strWhat;
        private String strWhy;

        public Identifier()
        {
            listWhoCandidates = new List<Token>();
            listWhenCandidates = new List<Token>();
            listWhereCandidates = new List<Token>();
            listWhatCandidates = new List<List<Token>>();
            listWhyCandidates = new List<List<Token>>();
            initializeAnnotations();
        }

        private void initializeAnnotations()
        {
            listWho = new List<String>();
            listWhen = new List<String>();
            listWhere = new List<String>();
            strWhat = "";
            strWhy = "";
        }

        #region Setters
        public void setCurrentArticle(List<Token> pArticle)
        {
            articleCurrent = pArticle;
        }

        public void setWhoCandidates(List<Token> pCandidates)
        {
            listWhoCandidates = pCandidates;
        }

        public void setWhenCandidates(List<Token> pCandidates)
        {
            listWhenCandidates = pCandidates;
        }

        public void setWhereCandidates(List<Token> pCandidates)
        {
            listWhereCandidates = pCandidates;
        }

        public void setWhatCandidates(List<List<Token>> pCandidates)
        {
            listWhatCandidates = pCandidates;
        }

        public void setWhyCandidates(List<List<Token>> pCandidates)
        {
            listWhyCandidates = pCandidates;
        }
        #endregion

        #region Getters
        public List<Token> getCurrentArticle()
        {
            return articleCurrent;
        }

        public List<String> getWho()
        {
            return listWho;
        }

        public List<String> getWhen()
        {
            return listWhen;
        }

        public List<String> getWhere()
        {
            return listWhere;
        }

        public String getWhat()
        {
            return strWhat;
        }

        public String getWhy()
        {
            return strWhy;
        }
        #endregion

        public void labelAnnotations()
        {
            initializeAnnotations();
            labelWho();
            labelWhen();
            labelWhere();
            labelWhat();
            labelWhy();
        }

        #region Labelling Functions
        private void labelWho()
        {
            if (listWhoCandidates.Count > 0)
            {
                listWho.Add(listWhoCandidates[0].Value);
            }
        }

        private void labelWhen()
        {
            if (listWhenCandidates.Count > 0)
            {
                listWhen.Add(listWhenCandidates[0].Value);
            }
        }

        private void labelWhere()
        {
            if (listWhereCandidates.Count > 0)
            {
                listWhere.Add(listWhereCandidates[0].Value);
            }
        }

        private void labelWhat()
        {
            if (listWhatCandidates.Count > 0)
            {
                strWhat = String.Join(" ", listWhatCandidates[0].Select(token => token.Value).ToArray());
                strWhat = strWhat.Replace("-LRB- ", "(");
                strWhat = strWhat.Replace(" -RRB-", ")");
                strWhat = strWhat.Replace(" . ", ".");
                strWhat = strWhat.Replace(" .", ".");
                strWhat = strWhat.Replace(" ,", ",");
                strWhat = strWhat.Replace(" !", "!");
            }
        }

        private void labelWhy()
        {
            if (listWhyCandidates.Count > 0)
            {
                strWhy = String.Join(" ", listWhyCandidates[0].Select(token => token.Value).ToArray());
                strWhy = strWhy.Replace("-LRB- ", "(");
                strWhy = strWhy.Replace(" -RRB-", ")");
                strWhy = strWhy.Replace(" . ", ".");
                strWhy = strWhy.Replace(" .", ".");
                strWhy = strWhy.Replace(" ,", ",");
                strWhy = strWhy.Replace(" !", "!");
            }
        }
        #endregion
    }
}
