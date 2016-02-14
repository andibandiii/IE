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
            StringBuilder sb = new StringBuilder();
            foreach (Token token in articleCurrent)
            {
                if (token.Sentence <= 1)
                {
                    sb.Append(token.Value);
                    sb.Append(" ");
                }
                else
                {
                    break;
                }
            }
            strWhat = sb.ToString();
            strWhat = strWhat.Replace("-LRB- ", "(");
            strWhat = strWhat.Replace(" -RRB-", ")");
            strWhat = strWhat.Replace(" . ", ".");
            strWhat = strWhat.Replace(" .", ".");
            strWhat = strWhat.Replace(" ,", ",");
            strWhat = strWhat.Replace(" !", "!");
        }

        private void labelWhy()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Token token in articleCurrent)
            {
                if (token.Sentence == 2)
                {
                    sb.Append(token.Value);
                    sb.Append(" ");
                }
                else if (token.Sentence > 2)
                {
                    break;
                }
            }
            strWhy = sb.ToString();
            strWhy = strWhy.Replace("-LRB- ", "(");
            strWhy = strWhy.Replace(" -RRB-", ")");
            strWhy = strWhy.Replace(" . ", ".");
            strWhy = strWhy.Replace(" .", ".");
            strWhy = strWhy.Replace(" ,", ",");
            strWhy = strWhy.Replace(" !", "!");
        }
        #endregion
    }
}
