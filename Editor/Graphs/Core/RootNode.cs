using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
namespace UnityNodeGraph
{
    public class RootNode : CoreNode
    {
        public override void OnInitialize()
        {
            if (jsonData.outputPorts.Count == 0)
            {
                jsonData.outputPorts.Add(new PortData
                {
                    name = "Action",
                    portType = typeof(PortTypeAction).ToString(),
                    portCapacity = 1
                });
                jsonData.outputPorts.Add(new PortData
                {
                    name = "Flow",
                    portType = typeof(PortTypeFlow).ToString()
                });
            }
            jsonData.inputPorts.ForEach(_portData => {
                AddInputPortFromPortData(_portData);
            });
            jsonData.outputPorts.ForEach(_portData => {
                AddOutputPortFromPortData(_portData);
            });
            this.AddToClassList("rootNode");
        }
        public override GraphCustomData getCustomData()
        {
            return null;
        }
    }
}
