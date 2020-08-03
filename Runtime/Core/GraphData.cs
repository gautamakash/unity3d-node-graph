using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace UnityNodeGraph
{
    [System.Serializable]
    public abstract class GraphData : ScriptableObject
    {
        //[HideInInspector]
        public string rootGuid;
        //[HideInInspector]
        public List<LinkData> links = new List<LinkData>();
        //[HideInInspector]
        public List<NodeData> nodes = new List<NodeData>();
        public abstract List<SearchElement> GetSearchMenu();
    }
    [System.Serializable]
    public class LinkData
    {
        public string sourceGuid;
        public string sourcePortName;
        public string targetGuid;
        public string targetPortName;
    }
    [System.Serializable]
    public class NodeData
    {
        public string guid;
        public string name;
        public Vector2 position;
        public Vector2 size;
        public string type;
        public string dataJSON;
    }

    public class SearchElement
    {
        public string name;
        public Texture2D icon;
        public string nodeType;
        public int level = 0;
    }

    [System.Serializable]
    public class GraphCustomData
    {
    }

    [System.Serializable]
    public class PortData
    {
        public string name;
        public string portType;
        public int portCapacity;
    }
    [System.Serializable]
    public class JSONGraphData
    {
        public List<PortData> inputPorts = new List<PortData>();
        public List<PortData> outputPorts = new List<PortData>();
        public string data;
        public System.Object GetCustomData(System.Type clazz)
        {
            if (data != null && data != "")
            {
                return JsonUtility.FromJson(data, clazz);
            }
            else
            {
                return GetInstance(clazz.ToString());
            }
        }
        private object GetInstance(string strFullyQualifiedName)
        {
            System.Type type = System.Type.GetType(strFullyQualifiedName);
            if (type != null)
                return System.Activator.CreateInstance(type);
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return System.Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
