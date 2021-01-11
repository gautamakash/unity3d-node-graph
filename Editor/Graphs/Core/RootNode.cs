using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
namespace UnityNodeGraph
{
    public class RootNode : CoreNode
    {
        RootNodeData rootData;
        public override void OnInitialize()
        {
            AddToClassList("root");
            rootData = jsonData.GetCustomData(typeof(RootNodeData)) as RootNodeData;
            if (jsonData.outputPorts.Count == 0)
            {
                rootData.ports.ForEach(_rootPort=> {
                    jsonData.outputPorts.Add(new PortData
                    {
                        name = _rootPort.name,
                        portType = _rootPort.portType,
                        portCapacity = _rootPort.portCapacity
                    });
                });
            }
            jsonData.inputPorts.ForEach(_portData => {
                AddInputPortFromPortData(_portData);
            });
            jsonData.outputPorts.ForEach(_portData => {
                AddOutputPortFromPortData(_portData);
            });
            this.AddToClassList("entryNode");
        }
        public override GraphCustomData getCustomData()
        {
            return null;
        }
    }
}
