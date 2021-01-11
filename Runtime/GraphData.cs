using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace UnityNodeGraph
{
    [System.Serializable]
    public abstract class GraphData : ScriptableObject
    {
        [HideInInspector]
        public string __rootGuid;
        [HideInInspector]
        public List<LinkData> __links = new List<LinkData>();
        [HideInInspector]
        public List<NodeData> __nodes = new List<NodeData>();
        public abstract List<SearchElement> GetSearchMenu();
        public virtual RootNodeData GetRootPortData()
        {
            return new RootNodeData
            {
                ports=new List<PortData>()
                {
                    new PortData(){
                        name="Flow",
                        portType=typeof(PortTypeFlow).ToString()
                    }
                }
            };
        }
        /*public void getPortDetails(string nodeGuid = null)
        {
            List<LinkData> links = this.__links.Where(l =>
            {
                return (nodeGuid != null) ? (l.sourceGuid == nodeGuid) : (l.sourceGuid == this.__rootGuid);
            }).ToList();
            if (links != null && links.Count > 0)
            {
                links.ForEach(_linkData =>
                {
                    NodeData _nodeData = this.__nodes.First(n => n.guid == _linkData.targetGuid);
                    JSONGraphData jsonData = JsonUtility.FromJson(_nodeData.dataJSON, typeof(JSONGraphData)) as JSONGraphData;
                    Debug.Log(_nodeData.dataJSON);
                }
                );
            }
        }
        public List<NodeDetail> getNodeDetails(string nodeGuid = null)
        {
            List<NodeDetail> nodeDetails = new List<NodeDetail>();
            List<LinkData> links = this.__links.Where(l =>
            {
                return (nodeGuid != null) ? (l.sourceGuid == nodeGuid) : (l.sourceGuid == this.__rootGuid);
            }).ToList();
            if (links != null && links.Count > 0)
            {
                links.ForEach(_linkData =>
                {
                    NodeData _nodeData = this.__nodes.First(n => n.guid == _linkData.targetGuid);
                    JSONGraphData jsonData = JsonUtility.FromJson(_nodeData.dataJSON, typeof(JSONGraphData)) as JSONGraphData;
                    NodeDetail nodeDetail = new NodeDetail() {
                        guid = _nodeData.guid,
                        name = _nodeData.name,
                        type = _nodeData.type,
                        dataJSON = _nodeData.dataJSON
                    };
                    nodeDetails.Add(nodeDetail);
                }
                );
            }
            return nodeDetails;
        }*/
        public NodeDetail getNodeDetail(string nodeGuid = null)
        {
            if (nodeGuid == null)
            {
                nodeGuid = this.__rootGuid;
            }
            NodeData _nodeData = this.__nodes.First(n => n.guid == nodeGuid);
            JSONGraphData jsonData = JsonUtility.FromJson(_nodeData.dataJSON, typeof(JSONGraphData)) as JSONGraphData;
            NodeDetail nodeDetail = new NodeDetail()
            {
                guid = _nodeData.guid,
                name = _nodeData.name,
                type = _nodeData.type,
                dataJSON = jsonData.data
            };

            List<LinkData> links = this.__links.Where(l =>
            {
                return (l.sourceGuid == nodeGuid);
            }).ToList();
            if (links != null && links.Count > 0)
            {
                links.ForEach(_linkData =>
                {
                    NodeData _linkNodeData = this.__nodes.First(n => n.guid == _linkData.targetGuid);
                    JSONGraphData _linkJsonData = JsonUtility.FromJson(_nodeData.dataJSON, typeof(JSONGraphData)) as JSONGraphData;
                    NodeDetail _linkNodeDetail = new NodeDetail()
                    {
                        guid = _linkNodeData.guid,
                        name = _linkNodeData.name,
                        type = _linkNodeData.type,
                        dataJSON = _linkJsonData.data,
                        connectedSourcePortName = _linkData.sourcePortName,
                        connectedTargetPortName = _linkData.targetPortName
                    };
                    nodeDetail.connectedNodes.Add(_linkNodeDetail);
                }
                );
            }
            return nodeDetail;
        }
    }

    [System.Serializable]
    public class NodeDetail
    {
        public string guid;
        public string name;
        public string type;
        public string dataJSON;
        public string connectedSourcePortName;
        public string connectedTargetPortName;
        public List<NodeDetail> connectedNodes = new List<NodeDetail>();
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
        public Color color;
    }

    public class RootNodeData
    {
        public List<PortData> ports;
    }

    public class PortTypeFlow
    {

    }
    public class PortTypeAction
    {

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
