using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    class Node
    {
        public int _nodeID;
        public int _domain;
        private int _observedValue;


        public Node(int nodeID, int domain)
        {
            _nodeID = nodeID;
            _domain = domain;
            _observedValue = -1;
        }

        public Node(Node node)
        {
            _nodeID = node._nodeID;
            _domain = node._domain;
            _observedValue = node._observedValue;
        }

        public bool isObserved()
        {
            return _observedValue != -1;
        }

        public int getObservedValue()
        {
            return _observedValue;
        }

        public bool setObservedValue(int value)
        {
            if (value >= 0)
                _observedValue = value;
            else
                return false;
            return true;
        }

        public void clearObservedValue()
        {
            _observedValue = -1;
        }

        public override string ToString()
        {
            return "node'" + _nodeID + "'";
        }

    }
}
