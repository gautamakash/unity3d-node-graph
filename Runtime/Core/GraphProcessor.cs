using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine;
namespace UnityNodeGraph
{
    [System.Serializable]
    public class GraphProcessEvent : UnityEvent<ProcessReceiveData>
    {
    }
    public class GraphProcessor : MonoBehaviour
    {
        private static GraphProcessor graphProcessor;

        public static GraphProcessor instance
        {
            get
            {
                if (!graphProcessor)
                {
                    graphProcessor = FindObjectOfType(typeof(GraphProcessor)) as GraphProcessor;

                    if (!graphProcessor)
                    {
                        Debug.LogError("There needs to be one active GraphProcessor script on a GameObject in your scene.");
                    }
                    else
                    {
                        //graphProcessor.Init();
                    }
                }

                return graphProcessor;
            }
        }

        private Dictionary<string, GraphData> graphs = new Dictionary<string, GraphData>();
        private Dictionary<string, GraphProcessEvent> eventDictionary = new Dictionary<string, GraphProcessEvent>();
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public string RegisterProcessor(out string _graphId, GraphData data, UnityAction<ProcessReceiveData> listener, string jsonData = "{}")
        {
            _graphId = System.Guid.NewGuid().ToString();
            if (data == null)
            {
                Debug.Log("GraphData passed to GraphProcessor.RegisterProcessor is empty");
                return null;
            }
            graphs.Add(_graphId, data);

            GraphProcessEvent thisEvent = null;
            if (instance.eventDictionary.TryGetValue(_graphId, out thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new GraphProcessEvent();
                thisEvent.AddListener(listener);
                instance.eventDictionary.Add(_graphId, thisEvent);
            }
            Process(new ProcessPassData
            {
                graphId = _graphId,
                guid = data.rootGuid,
                flow = "Action",
                jsonData = jsonData
            });
            Process(new ProcessPassData {
                graphId = _graphId,
                guid = data.rootGuid,
                flow = "Flow",
                jsonData = jsonData
            });
            return _graphId;
        }

        public void Kill(string graphId)
        {
            if (graphs.ContainsKey(graphId))
            {
                graphs.Remove(graphId);
            }
            if (eventDictionary.ContainsKey(graphId))
            {
                eventDictionary.Remove(graphId);
            }
        }

        public void StopListening(string eventName, UnityAction<ProcessReceiveData> listener)
        {
            if (graphProcessor == null) return;
            GraphProcessEvent thisEvent = null;
            if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        public void Process(ProcessPassData passData)
        {
            //Debug.Log($"Start Processing {JsonUtility.ToJson(passData)}");
            if (graphs.ContainsKey(passData.graphId) && eventDictionary.ContainsKey(passData.graphId))
            {
                GraphData _data = graphs[passData.graphId];
                List<LinkData> links = _data.links.Where(l =>
                {
                    return (l.sourceGuid == passData.guid && l.sourcePortName == passData.flow);
                }).ToList();
                GraphProcessEvent listners = eventDictionary[passData.graphId];
                if (links != null && links.Count>0)
                {
                    links.ForEach(_linkData => {
                        NodeData _nodeData = _data.nodes.First(n => n.guid == _linkData.targetGuid);
                        JSONGraphData jsonData = JsonUtility.FromJson(_nodeData.dataJSON, typeof(JSONGraphData)) as JSONGraphData;
                        ProcessReceiveData processReceiveData = new ProcessReceiveData
                        {
                            guid = _nodeData.guid,
                            customDataJson = jsonData.data,
                            type = _nodeData.type
                        };
                        jsonData.outputPorts.ForEach(portData => {
                            ProcessOption processOption = new ProcessOption
                            {
                                name = portData.name,
                                type = portData.portType
                            };
                            _data.links.ForEach(l => {
                                if (l.sourceGuid == _linkData.targetGuid && l.sourcePortName == portData.name)
                                {
                                    processOption.targetConnection.Add(new TargetPort
                                    {
                                        name = l.targetPortName,
                                        guid = l.targetGuid,
                                        type = portData.portType
                                    });
                                }
                            });
                            if (portData.portType == "UnityNodeGraph.PortTypeFlow")
                            {
                                processReceiveData.flows.Add(processOption);
                            }
                            else if (portData.portType == "UnityNodeGraph.PortTypeAction")
                            {
                                // search for port connection
                                processReceiveData.actions.Add(processOption);
                            }
                            else
                            {
                                processReceiveData.customOptions.Add(processOption);
                            }
                        });
                        listners.Invoke(processReceiveData);
                    });
                }
                else
                {
                    NodeData _nodeData = _data.nodes.First(n => n.guid == passData.guid);
                    JSONGraphData jsonData = JsonUtility.FromJson(_nodeData.dataJSON, typeof(JSONGraphData)) as JSONGraphData;
                    ProcessReceiveData processReceiveData = new ProcessReceiveData
                    {
                        guid = _nodeData.guid,
                        customDataJson = jsonData.data,
                        type = _nodeData.type
                    };
                    listners.Invoke(processReceiveData);
                }
            }
            else
            {
                Debug.Log($"Didn't find GraphId{passData.graphId}");
            }
        }
    }
    [System.Serializable]
    public class ProcessPassData
    {
        public string graphId;
        public string guid;
        public string flow;
        public string jsonData;
    }
    [System.Serializable]
    public class ProcessReceiveData
    {
        public string guid;
        public List<ProcessOption> flows = new List<ProcessOption>();
        public List<ProcessOption> actions = new List<ProcessOption>();
        public List<ProcessOption> customOptions = new List<ProcessOption>();
        public string customDataJson;
        public string type;
        public System.Object GetCustomData(System.Type clazz)
        {
            if (customDataJson != null && customDataJson != "")
            {
                return JsonUtility.FromJson(customDataJson, clazz);
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
    [System.Serializable]
    public class ProcessOption
    {
        public string name;
        public string type;
        public List<TargetPort> targetConnection = new List<TargetPort>();
    }
    [System.Serializable]
    public class TargetPort
    {
        public string name;
        public string type;
        public string guid;
    }
}
