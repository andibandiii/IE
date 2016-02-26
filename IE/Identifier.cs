using IE.Models;
using java.io;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using weka.classifiers;
using weka.classifiers.meta;
using weka.core;
using weka.filters;
using weka.filters.unsupervised.attribute;

namespace IE
{
    public class Identifier
    {
        private List<Token> articleCurrent;
        private List<List<Token>> segregatedArticleCurrent;
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
        private FastVector fvPOS;
        Classifier whoClassifier;
        Classifier whenClassifier;
        Classifier whereClassifier;

        public Identifier()
        {
            listWhoCandidates = new List<Token>();
            listWhenCandidates = new List<Token>();
            listWhereCandidates = new List<Token>();
            listWhatCandidates = new List<List<Token>>();
            listWhyCandidates = new List<List<Token>>();

            fvPOS = new FastVector(Token.PartOfSpeechTags.Length);
            foreach (String POS in Token.PartOfSpeechTags)
            {
                fvPOS.addElement(POS);
            }

            whoClassifier = (Classifier)SerializationHelper.read(@"..\..\IdentifierModels\who.model");
            whenClassifier = (Classifier)SerializationHelper.read(@"..\..\IdentifierModels\when.model");
            whereClassifier = (Classifier)SerializationHelper.read(@"..\..\IdentifierModels\where.model");

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
            segregatedArticleCurrent = articleCurrent
                        .GroupBy(token => token.Sentence)
                        .Select(tokenGroup => tokenGroup.ToList())
                        .ToList();
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
            Instances whoInstances = createWhoInstances();

            foreach (Instance instance in whoInstances)
            {
                double[] classProbability = whoClassifier.distributionForInstance(instance);
                if (classProbability[0] >= classProbability[1])
                {
                    listWho.Add(instance.stringValue(0));
                }
            }
        }

        private void labelWhen()
        {
            Instances whenInstances = createWhenInstances();

            foreach (Instance instance in whenInstances)
            {
                double[] classProbability = whenClassifier.distributionForInstance(instance);
                if (classProbability[0] >= classProbability[1])
                {
                    listWhen.Add(instance.stringValue(0));
                }
            }
        }

        private void labelWhere()
        {
            Instances whereInstances = createWhereInstances();

            foreach (Instance instance in whereInstances)
            {
                double[] classProbability = whereClassifier.distributionForInstance(instance);
                if (classProbability[0] >= classProbability[1])
                {
                    listWhere.Add(instance.stringValue(0));
                }
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

        #region Instances Creation
        #region Instance Group Creation
        private Instances createWhoInstances()
        {
            FastVector fvWho = createWhoFastVector();
            Instances whoInstances = new Instances("WhoInstances", fvWho, listWhoCandidates.Count);
            foreach (Token candidate in listWhoCandidates)
            {
                if (candidate.Value == null) continue;
                Instance whoInstance = createSingleWhoInstance(fvWho, candidate);
                whoInstance.setDataset(whoInstances);
                whoInstances.add(whoInstance);
            }
            whoInstances.setClassIndex(fvWho.size() - 1);
            return whoInstances;
        }

        private Instances createWhenInstances()
        {
            FastVector fvWhen = createWhenFastVector();
            Instances whenInstances = new Instances("WhenInstances", fvWhen, listWhenCandidates.Count);
            foreach (Token candidate in listWhenCandidates)
            {
                if (candidate.Value == null) continue;
                Instance whenInstance = createSingleWhenInstance(fvWhen, candidate);
                whenInstance.setDataset(whenInstances);
                whenInstances.add(whenInstance);
            }
            whenInstances.setClassIndex(fvWhen.size() - 1);
            return whenInstances;
        }

        private Instances createWhereInstances()
        {
            FastVector fvWhere = createWhereFastVector();
            Instances whereInstances = new Instances("WhereInstances", fvWhere, listWhereCandidates.Count);
            foreach (Token candidate in listWhereCandidates)
            {
                if (candidate.Value == null) continue;
                Instance whereInstance = createSingleWhereInstance(fvWhere, candidate);
                whereInstance.setDataset(whereInstances);
                whereInstances.add(whereInstance);
            }
            whereInstances.setClassIndex(fvWhere.size() - 1);
            return whereInstances;
        }
        #endregion

        #region Single Instance Creation
        private Instance createSingleWhoInstance(FastVector fvWho, Token candidate)
        {
            Instance whoCandidate = new DenseInstance(31);
            whoCandidate.setValue((weka.core.Attribute)fvWho.elementAt(0), candidate.Value);
            whoCandidate.setValue((weka.core.Attribute)fvWho.elementAt(1), candidate.Value.Split(' ').Count());
            whoCandidate.setValue((weka.core.Attribute)fvWho.elementAt(2), candidate.Sentence);
            whoCandidate.setValue((weka.core.Attribute)fvWho.elementAt(3), candidate.Position);
            double sentenceStartProximity = -1;
            foreach (List<Token> tokenList in segregatedArticleCurrent)
            {
                if (tokenList.Count > 0 && tokenList[0].Sentence == candidate.Sentence)
                {
                    sentenceStartProximity = (double)(candidate.Position - tokenList[0].Position) / (double)tokenList.Count;
                    break;
                }
            }
            if (sentenceStartProximity > -1)
            {
                whoCandidate.setValue((weka.core.Attribute)fvWho.elementAt(4), sentenceStartProximity);
            }
            whoCandidate.setValue((weka.core.Attribute)fvWho.elementAt(5), candidate.Frequency);
            for (int i = 10; i > 0; i--)
            {
                if (candidate.Position - i - 1 >= 0)
                {
                    whoCandidate.setValue((weka.core.Attribute)fvWho.elementAt(10 - i + 6), articleCurrent[candidate.Position - i - 1].Value);
                    if (articleCurrent[candidate.Position - i - 1].PartOfSpeech != null)
                    {
                        whoCandidate.setValue((weka.core.Attribute)fvWho.elementAt(10 - i + 18), articleCurrent[candidate.Position - i - 1].PartOfSpeech);
                    }
                }
            }
            if (candidate.Position < articleCurrent.Count)
            {
                whoCandidate.setValue((weka.core.Attribute)fvWho.elementAt(16), articleCurrent[candidate.Position].Value);
                whoCandidate.setValue((weka.core.Attribute)fvWho.elementAt(28), articleCurrent[candidate.Position].PartOfSpeech);
            }
            if (candidate.Position + 1 < articleCurrent.Count)
            {
                whoCandidate.setValue((weka.core.Attribute)fvWho.elementAt(17), articleCurrent[candidate.Position + 1].Value);
                whoCandidate.setValue((weka.core.Attribute)fvWho.elementAt(29), articleCurrent[candidate.Position + 1].PartOfSpeech);
            }
            return whoCandidate;
        }

        private Instance createSingleWhenInstance(FastVector fvWhen, Token candidate)
        {
            Instance whenCandidate = new DenseInstance(31);
            whenCandidate.setValue((weka.core.Attribute)fvWhen.elementAt(0), candidate.Value);
            whenCandidate.setValue((weka.core.Attribute)fvWhen.elementAt(1), candidate.Value.Split(' ').Count());
            whenCandidate.setValue((weka.core.Attribute)fvWhen.elementAt(2), candidate.Sentence);
            whenCandidate.setValue((weka.core.Attribute)fvWhen.elementAt(3), candidate.Position);
            double sentenceStartProximity = -1;
            foreach (List<Token> tokenList in segregatedArticleCurrent)
            {
                if (tokenList.Count > 0 && tokenList[0].Sentence == candidate.Sentence)
                {
                    sentenceStartProximity = (double)(candidate.Position - tokenList[0].Position) / (double)tokenList.Count;
                    break;
                }
            }
            if (sentenceStartProximity > -1)
            {
                whenCandidate.setValue((weka.core.Attribute)fvWhen.elementAt(4), sentenceStartProximity);
            }
            whenCandidate.setValue((weka.core.Attribute)fvWhen.elementAt(5), candidate.Frequency);
            for (int i = 10; i > 0; i--)
            {
                if (candidate.Position - i - 1 >= 0)
                {
                    whenCandidate.setValue((weka.core.Attribute)fvWhen.elementAt(10 - i + 6), articleCurrent[candidate.Position - i - 1].Value);
                    if (articleCurrent[candidate.Position - i - 1].PartOfSpeech != null)
                    {
                        whenCandidate.setValue((weka.core.Attribute)fvWhen.elementAt(10 - i + 18), articleCurrent[candidate.Position - i - 1].PartOfSpeech);
                    }
                }
            }
            if (candidate.Position < articleCurrent.Count)
            {
                whenCandidate.setValue((weka.core.Attribute)fvWhen.elementAt(16), articleCurrent[candidate.Position].Value);
                whenCandidate.setValue((weka.core.Attribute)fvWhen.elementAt(28), articleCurrent[candidate.Position].PartOfSpeech);
            }
            if (candidate.Position + 1 < articleCurrent.Count)
            {
                whenCandidate.setValue((weka.core.Attribute)fvWhen.elementAt(17), articleCurrent[candidate.Position + 1].Value);
                whenCandidate.setValue((weka.core.Attribute)fvWhen.elementAt(29), articleCurrent[candidate.Position + 1].PartOfSpeech);
            }
            return whenCandidate;
        }

        private Instance createSingleWhereInstance(FastVector fvWhere, Token candidate)
        {
            Instance whereCandidate = new DenseInstance(31);
            whereCandidate.setValue((weka.core.Attribute)fvWhere.elementAt(0), candidate.Value);
            whereCandidate.setValue((weka.core.Attribute)fvWhere.elementAt(1), candidate.Value.Split(' ').Count());
            whereCandidate.setValue((weka.core.Attribute)fvWhere.elementAt(2), candidate.Sentence);
            whereCandidate.setValue((weka.core.Attribute)fvWhere.elementAt(3), candidate.Position);
            double sentenceStartProximity = -1;
            foreach (List<Token> tokenList in segregatedArticleCurrent)
            {
                if (tokenList.Count > 0 && tokenList[0].Sentence == candidate.Sentence)
                {
                    sentenceStartProximity = (double)(candidate.Position - tokenList[0].Position) / (double)tokenList.Count;
                    break;
                }
            }
            if (sentenceStartProximity > -1)
            {
                whereCandidate.setValue((weka.core.Attribute)fvWhere.elementAt(4), sentenceStartProximity);
            }
            whereCandidate.setValue((weka.core.Attribute)fvWhere.elementAt(5), candidate.Frequency);
            for (int i = 10; i > 0; i--)
            {
                if (candidate.Position - i - 1 >= 0)
                {
                    whereCandidate.setValue((weka.core.Attribute)fvWhere.elementAt(10 - i + 6), articleCurrent[candidate.Position - i - 1].Value);
                    if (articleCurrent[candidate.Position - i - 1].PartOfSpeech != null)
                    {
                        whereCandidate.setValue((weka.core.Attribute)fvWhere.elementAt(10 - i + 18), articleCurrent[candidate.Position - i - 1].PartOfSpeech);
                    }
                }
            }
            if (candidate.Position < articleCurrent.Count)
            {
                whereCandidate.setValue((weka.core.Attribute)fvWhere.elementAt(16), articleCurrent[candidate.Position].Value);
                whereCandidate.setValue((weka.core.Attribute)fvWhere.elementAt(28), articleCurrent[candidate.Position].PartOfSpeech);
            }
            if (candidate.Position + 1 < articleCurrent.Count)
            {
                whereCandidate.setValue((weka.core.Attribute)fvWhere.elementAt(17), articleCurrent[candidate.Position + 1].Value);
                whereCandidate.setValue((weka.core.Attribute)fvWhere.elementAt(29), articleCurrent[candidate.Position + 1].PartOfSpeech);
            }
            return whereCandidate;
        }
        #endregion
        #endregion

        #region Fast Vector Creation
        private FastVector createWhoFastVector()
        {
            FastVector fvWho = new FastVector(31);
            fvWho.addElement(new weka.core.Attribute("word", (FastVector)null));
            fvWho.addElement(new weka.core.Attribute("wordCount"));
            fvWho.addElement(new weka.core.Attribute("sentence"));
            fvWho.addElement(new weka.core.Attribute("position"));
            fvWho.addElement(new weka.core.Attribute("sentenceStartProximity"));
            fvWho.addElement(new weka.core.Attribute("wordScore"));
            for (int i = 10; i > 0; i--)
            {
                fvWho.addElement(new weka.core.Attribute("word-" + i, (FastVector)null));
            }
            fvWho.addElement(new weka.core.Attribute("word+1", (FastVector)null));
            fvWho.addElement(new weka.core.Attribute("word+2", (FastVector)null));
            for (int i = 10; i > 0; i--)
            {
                fvWho.addElement(new weka.core.Attribute("postag-" + i, fvPOS));
            }
            fvWho.addElement(new weka.core.Attribute("postag+1", fvPOS));
            fvWho.addElement(new weka.core.Attribute("postag+2", fvPOS));
            FastVector fvClass = new FastVector(2);
            fvClass.addElement("yes");
            fvClass.addElement("no");
            fvWho.addElement(new weka.core.Attribute("who", fvClass));
            return fvWho;
        }

        private FastVector createWhenFastVector()
        {
            FastVector fvWhen = new FastVector(31);
            fvWhen.addElement(new weka.core.Attribute("word", (FastVector)null));
            fvWhen.addElement(new weka.core.Attribute("wordCount"));
            fvWhen.addElement(new weka.core.Attribute("sentence"));
            fvWhen.addElement(new weka.core.Attribute("position"));
            fvWhen.addElement(new weka.core.Attribute("sentenceStartProximity"));
            fvWhen.addElement(new weka.core.Attribute("wordScore"));
            for (int i = 10; i > 0; i--)
            {
                fvWhen.addElement(new weka.core.Attribute("word-" + i, (FastVector)null));
            }
            fvWhen.addElement(new weka.core.Attribute("word+1", (FastVector)null));
            fvWhen.addElement(new weka.core.Attribute("word+2", (FastVector)null));
            for (int i = 10; i > 0; i--)
            {
                fvWhen.addElement(new weka.core.Attribute("postag-" + i, fvPOS));
            }
            fvWhen.addElement(new weka.core.Attribute("postag+1", fvPOS));
            fvWhen.addElement(new weka.core.Attribute("postag+2", fvPOS));
            FastVector fvClass = new FastVector(2);
            fvClass.addElement("yes");
            fvClass.addElement("no");
            fvWhen.addElement(new weka.core.Attribute("when", fvClass));
            return fvWhen;
        }

        private FastVector createWhereFastVector()
        {
            FastVector fvWhere = new FastVector(31);
            fvWhere.addElement(new weka.core.Attribute("word", (FastVector)null));
            fvWhere.addElement(new weka.core.Attribute("wordCount"));
            fvWhere.addElement(new weka.core.Attribute("sentence"));
            fvWhere.addElement(new weka.core.Attribute("position"));
            fvWhere.addElement(new weka.core.Attribute("sentenceStartProximity"));
            fvWhere.addElement(new weka.core.Attribute("wordScore"));
            for (int i = 10; i > 0; i--)
            {
                fvWhere.addElement(new weka.core.Attribute("word-" + i, (FastVector)null));
            }
            fvWhere.addElement(new weka.core.Attribute("word+1", (FastVector)null));
            fvWhere.addElement(new weka.core.Attribute("word+2", (FastVector)null));
            for (int i = 10; i > 0; i--)
            {
                fvWhere.addElement(new weka.core.Attribute("postag-" + i, fvPOS));
            }
            fvWhere.addElement(new weka.core.Attribute("postag+1", fvPOS));
            fvWhere.addElement(new weka.core.Attribute("postag+2", fvPOS));
            FastVector fvClass = new FastVector(2);
            fvClass.addElement("yes");
            fvClass.addElement("no");
            fvWhere.addElement(new weka.core.Attribute("where", fvClass));
            return fvWhere;
        }
        #endregion
    }
}
