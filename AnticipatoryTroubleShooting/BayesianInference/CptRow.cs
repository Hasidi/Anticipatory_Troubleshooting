using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    class CptRow
    {
        public double _prob;

        public Dictionary<int, Node> _vars;

        #region constructors


        public CptRow()
        {
            _vars = new Dictionary<int, Node>();
        }

        public CptRow(List<Node> structure)
        {
            _vars = new Dictionary<int, Node>();
            foreach (var node in structure)
            {
                _vars.Add(node._nodeID, new Node(node));
            }
        }
        public CptRow(CptRow cptRow)
        {
            _vars = new Dictionary<int, Node>();
            foreach (var node in cptRow._vars.Values)
            {
                Node nNode = new Node(node);
                _vars.Add(node._nodeID, nNode);

            }
        }
        #endregion

        public double getProb()
        {
            return _prob;
        }
        /*
        public int[] getOrder()
        {

            foreach (var node in _vars)
            {

            }
        }*/

        public void insertValues(string values)
        {
            if (_vars.Count == 0)
                throw new InvalidProgramException("No nodes in the current row");
            for (int i = 0; i < values.Length; i++)
            {
                Node currNode = _vars.ElementAt(i).Value;
                currNode.setObservedValue((int.Parse(values[i].ToString())));
            }
        }

        public override string ToString()
        {
            string ans = "{";
            foreach (var node in _vars)
            {
                ans += node.Value._nodeID + ":" + node.Value.getObservedValue() + ", ";
            }
            ans = ans.Substring(0, ans.Length - 2);
            ans += "}" + "  (" + _prob + ")";
            return ans;
        }
        /*
        // compare only values not prob
        public override bool Equals(object obj)
        {
            CptRow toCompare = (CptRow)obj;
            foreach (var x in _vars)
            {
                if (x.Value.getObservedValue() != toCompare._vars[x.Key].getObservedValue())
                    return false;
            }
            return true;
        }
        */

        public int getVarPlace(int varNumber)
        {
            int place = -1;
            int i = 0;
            foreach (var node in _vars)
            {
                if (node.Value._nodeID == varNumber)
                {
                    place = i;
                    break;
                }
            }
            return place;
        }

        public void addVarToRow(Node node)
        {
            Node newNode = new Node(node);
            _vars.Add(node._nodeID, newNode);
        }


    }
}
