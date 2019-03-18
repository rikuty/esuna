using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace UltimateTerrains
{
    [Serializable]
    public class GUIDNodeDictionary
    {
        [SerializeField] private List<string> keys = new List<string>();
        [SerializeField] private List<NodeSerializable> values = new List<NodeSerializable>();

        public ReadOnlyCollection<NodeSerializable> Values {
            get { return values.AsReadOnly(); }
        }

        public List<NodeSerializable> FinalNodes {
            get {
                var finalNodes = new List<NodeSerializable>();
                foreach (var node in values) {
                    if (node.IsFinal) {
                        finalNodes.Add(node);
                    }
                }
                return finalNodes;
            }
        }

        public NodeSerializable this[string str] {
            get {
                var index = keys.IndexOf(str);
                if (index >= 0)
                    return values[index];
                return null;
            }
        }

        public void Add(NodeSerializable node)
        {
            var guid = Guid.NewGuid().ToString();
            node.GUID = guid;
            keys.Add(guid);
            values.Add(node);
        }

        public void Remove(NodeSerializable node)
        {
            Remove(node.GUID);
        }

        public void Remove(string guid)
        {
            var index = keys.IndexOf(guid);
            if (index < 0)
                throw new Exception(string.Format("Node with GUID '{0}' is not in dictionary", guid));
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }
        
        public void RemoveSafe(string guid)
        {
            var index = keys.IndexOf(guid);
            if (index >= 0) {
                keys.RemoveAt(index);
                values.RemoveAt(index);
            }
        }
    }
}