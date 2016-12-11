using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    class Cpt
    {
        public Dictionary<int, double> _rowsProbs;
        
        public Node _ownerNode;

        public int[] _owner;
        public int[] _factors;
        private int[] _rowDomains;
        public int _currAddedPointer;

        public Dictionary<int, Node> _rowStructure;

        #region constructors

        public Cpt(List<Node> rowStructure)
        {
            int nVars = rowStructure.Count;
            _factors = new int[nVars];
            _rowDomains = new int[nVars];
            _rowStructure = new Dictionary<int, Node>();
            int i = 0;
            foreach (var node in rowStructure)
            {
                _rowDomains[i] = node._domain;
                _factors[i] = node._nodeID;
                _rowStructure.Add(node._nodeID, new Node(node._nodeID, node._domain));
                i++;
            }
            //int maxRowNumber = getMaxRowNumber();
            _rowsProbs = new Dictionary<int, double>();


        }

        public Cpt(CptRow row)
        {
            _rowStructure = row._vars;
            int nVars = _rowStructure.Count;
            _factors = new int[nVars];
            _rowDomains = new int[nVars];
            int i = 0;
            foreach (var node in _rowStructure.Values)
            {
                _rowDomains[i] = node._domain;
                _factors[i] = node._nodeID;
                i++;
            }
            _rowsProbs = new Dictionary<int, double>();
        }
        

        #endregion








        public double getRowProb(CptRow observedValues)
        {
            CptRow fictiveRow = buildFictiveRow(observedValues);
            int rowNumber = getRowNumber(fictiveRow);
            return _rowsProbs[rowNumber];
        }

        //----------------------------------------------------------------------------------------------------------------------

        public void sumReleventRowsFit(CptRow rowValues, ref CptRow rowStructureToSearch, Node nodeOut)
        {
            // assumption : number of row vars is smaller than curr table vars
            // assumption : row vars is the same order as this except first coulmn which is the var who goes out
            // return the CptRow paramater contains the accumalator prob fit to row values of the parameter
            foreach (var node in rowValues._vars)
            {
                rowStructureToSearch._vars[node.Key].setObservedValue(node.Value.getObservedValue());
            }

            rowValues._prob = 0;
            Node outedNode = rowStructureToSearch._vars[nodeOut._nodeID];
            for (int i = 0; i < outedNode._domain; i++)
            {
                outedNode.setObservedValue(i);
                int fitRowNumber = getRowNumber(rowStructureToSearch);
                if (_rowsProbs.ContainsKey(fitRowNumber))                
                    rowValues._prob += _rowsProbs[fitRowNumber];
            }

        }
        //----------------------------------------------------------------------------------------------------------------------
        public List<Node> getRowStructure()
        {
            return _rowStructure.Values.ToList();
        }

        public int getVarDomain(int var)
        {
            return _rowStructure[var]._domain;
            /*
            for (int i = 0; i < _factors.Length; i++ )
            {
                if (_factors[i] == var)
                    return _rowDomains[i];
            }
            return -1;*/
        }
        //----------------------------------------------------------------------------------------------------------------------
        public void addRow(CptRow cptRow)
        {
            /*
            if (_currAddedPointer >= _rowsProbs.Count)
                throw new InvalidOperationException("row max number table exception");*/
            _rowsProbs[_currAddedPointer] = cptRow._prob;
            _currAddedPointer++;
        }

        private void addRow(double prob)
        {
            /*
            if (_currAddedPointer >= _rowsProbs.Count)
                throw new InvalidOperationException("row max number table exception");*/
            _rowsProbs[_currAddedPointer] = prob;
            _currAddedPointer++;
        }

        //----------------------------------------------------------------------------------------------------------------------
        private int getMaxRowNumber()
        {
            int ans = 0;
            int accuDomain = 1;
            for (int i = _rowStructure.Count - 1; i >= 0; i--)
            {
                ans += (_rowDomains[i] - 1) * accuDomain;
                accuDomain *= _rowDomains[i];
            }
            ++ans;
            return ans;
        }
        //----------------------------------------------------------------------------------------------------------------------
        private CptRow buildFictiveRow(CptRow observedValues)
        {
            //assumption : dim of parm row is bigger than this dim
            //it might be vars in observedValues which dosent appear in this cpt
            CptRow row = new CptRow();
            foreach (var varNum in _factors)
            {
                Node foundNode = null;
                if (observedValues._vars.ContainsKey(varNum))
                    foundNode = observedValues._vars[varNum];
                if (foundNode != null)
                {
                    row.addVarToRow(foundNode);
                }
            }
            return row;

        }
        //----------------------------------------------------------------------------------------------------------------------

        public CptRow buildFictiveRowWithNewVar(CptRow observedValues)
        {
            CptRow rowStructureToSearch = new CptRow();
            //find the var who is missing
            foreach (var varNum in _factors)
            {
                Node foundNode = observedValues._vars[varNum];
                if (foundNode != null)
                {
                    rowStructureToSearch.addVarToRow(foundNode);
                }
                else
                {
                    rowStructureToSearch.addVarToRow(new Node(varNum, _rowDomains[0])); //know that the first one is missing
                }
            }

            return rowStructureToSearch;

        }
















        //----------------------------------------------------------------------------------------------------------------------
        private Dictionary<int, int> map2orders(List<int> originalOrder, List<int> currOrder)
        {
            Dictionary<int, int> ans = new Dictionary<int, int>();
            foreach (var x in originalOrder)
            {
                int place = -1;
                for (int i = 0; i < currOrder.Count; i++)
                {
                    if (currOrder[i] == x)
                    {
                        place = i;
                        break;
                    }
                }
                ans.Add(x, place);
            }
            return ans;
        }
        //----------------------------------------------------------------------------------------------------------------------
        /*
        public void setRowStructure(List<Node> nodes)
        {
            _rowDomains = new List<int>();
            foreach (var x in nodes)
            {
                _rowStructure.Add(x);
                _rowDomains.Add(x._domain);
            }
        }*/
        //----------------------------------------------------------------------------------------------------------------------

        public void fillTable(List<double> probs)
        {
            List<string> perms = buildAllPermutationInDomain();
            if (probs.Count != perms.Count)
                throw new InvalidOperationException("number of row inserted not fit to table dimension");

            foreach (double prob in probs)
            {
                addRow(prob);
            }
        }


        //--------------------------------------------------------------------------------------------------------------------


        public List<string> buildAllPermutationInDomain()
        {
            List<string> ans = new List<string>();
            List<int> domains = new List<int>();
            foreach (int domain in _rowDomains)
            {
                domains.Add(domain);
            }
            recPermutationsDomain("", domains, 0, ans);
            return ans;
        }

        private static void recPermutationsDomain(string oldPerm, List<int> domains, int place, List<string> allPerms)
        {
            if (place >= domains.Count)
            {
                allPerms.Add(oldPerm);
                return;
            }
            for (int i = 0; i < domains[place]; i++)
            {
                oldPerm += i;
                recPermutationsDomain(oldPerm, domains, place + 1, allPerms);
                oldPerm = oldPerm.Substring(0, oldPerm.Length - 1);

            }
        }
        //--------------------------------------------------------------------------------------------------------------------

        public int getRowNumber(CptRow rowValues)
        {
            int accuDomain = 1;
            int rowAns = 0;

            for (int i = rowValues._vars.Count - 1; i >= 0; i--)
            {
                Node fitNode = rowValues._vars.ElementAt(i).Value;
                rowAns += fitNode.getObservedValue() * accuDomain;
                int nextDomain = 1;
                /*
                if (i  > 0)
                {
                    nextDomain = rowValues._vars.ElementAt(i - 1).Value._domain;
                }*/
                accuDomain *= fitNode._domain;
            }
            return rowAns;
        }


        //--------------------------------------------------------------------------------------------------------------------
        public int getRowNumber(CptRow rowValues, CptRow observations)
        {
            CptRow fitRowValues = null;
            if (rowValues._vars.Count < _rowStructure.Count)
            {
                fitRowValues = new CptRow();
                foreach (var node in _rowStructure)
                {
                    if (observations._vars.ContainsKey(node.Key))
                    {
                        fitRowValues.addVarToRow(observations._vars[node.Key]);
                    }
                    else
                    {
                        fitRowValues.addVarToRow(rowValues._vars[node.Key]);
                    }
                }
            }
            if (fitRowValues != null)
                rowValues = fitRowValues;
            int rowNum = getRowNumber(rowValues);
            return rowNum;

        }
        //--------------------------------------------------------------------------------------------------------------------
        public void changeProbs(Dictionary<int,double> newRows)
        {
            if (_rowsProbs.Count != newRows.Count)
                throw new Exception();
            _rowsProbs = newRows;
        }





        //--------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            string ans = string.Empty;
            List<string> probs = new List<string>();
            foreach (var prob in _rowsProbs)
            {
                probs.Add(prob.Value.ToString());
            }
            ans = string.Join(", ", probs);
            return ans;
        }

        //--------------------------------------------------------------------------------------------------------------------

        //V2
        public double getRowProb(CptRow observedValues, CptRow queryObservations)
        {
            CptRow fictiveRow = buildFictiveRow(observedValues, queryObservations);
            int rowNumber = getRowNumber(fictiveRow);
            return _rowsProbs[rowNumber];
        }

        //----------------------------------------------------------------------------------------------------------------------
        private CptRow buildFictiveRow(CptRow observedValues, CptRow queryObservations)
        {
            //assumption : dim of parm row is bigger than this dim
            //it might be vars in observedValues which dosent appear in this cpt
            CptRow row = new CptRow();
            foreach (var varNum in _factors)
            {
                Node foundNode = null;
                if (observedValues._vars.ContainsKey(varNum))
                    foundNode = observedValues._vars[varNum];
                if (queryObservations._vars.ContainsKey(varNum))
                    foundNode = queryObservations._vars[varNum];
                if (foundNode != null)
                {
                    row.addVarToRow(foundNode);
                }
            }
            return row;

        }



        public void changePriorProb(double healthProb)
        {
            double totalNewProbs = 0;
            double priorComulativeFaultProb = 1 - _rowsProbs[0];

            //priorComulativeFaultProb = 1;

            _rowsProbs[0] = healthProb;
            totalNewProbs += healthProb;
            int notherValues = _rowsProbs.Count - 1;
            for (int i = 1; i < _rowsProbs.Count; i++)
            {
                _rowsProbs[i] = (1 - healthProb) / notherValues;
                //_rowsProbs[i] = (_rowsProbs[i] * (1 - healthProb)) / priorComulativeFaultProb;
                totalNewProbs += _rowsProbs[i];
            }
            if (totalNewProbs < 1 - 0.0001)
                throw new InvalidOperationException();

        }



        public bool verifyCpt()
        {
            double totalProbs = 0;
            foreach (var row in _rowsProbs)
            {
                totalProbs += row.Value;
            }
            if ((totalProbs % 1) != 0)
            {
                double roundNum = Math.Round(totalProbs);
                if (roundNum - totalProbs > 0.001)
                    return false;
            }
            return true;
        }



    }
}
