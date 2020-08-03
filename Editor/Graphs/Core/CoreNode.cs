using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace UnityNodeGraph
{
    public abstract class CoreNode : Node
    {
        #region Variables
        public string guid; // Unique guid for node
        public List<Port> inputPorts = new List<Port>();
        public List<Port> outputPorts = new List<Port>();
        public JSONGraphData jsonData = new JSONGraphData();
        #endregion
        #region constructure
        // Cunstructor
        public CoreNode()
        {
            // Add Stylesheet for nodes
            styleSheets.Add(Resources.Load<StyleSheet>("stylesheet_core_node"));
            AddToClassList("node");
        }
        #endregion
        #region node functions
        public void Refresh()
        {
            this.RefreshExpandedState();
            this.RefreshPorts();
        }
        public CoreNode Init(NodeData nodeData)
        {
            title = nodeData.name;
            guid = (nodeData.guid != null) ? nodeData.guid : System.Guid.NewGuid().ToString();
            Rect _placement = new Rect();
            _placement.position = nodeData.position;
            _placement.size = nodeData.size;
            this.SetPosition(_placement);
            jsonData = JsonUtility.FromJson(nodeData.dataJSON, typeof(JSONGraphData)) as JSONGraphData;
            if (jsonData == null) jsonData = new JSONGraphData();
            OnInitialize();
            return this;
        }
        #endregion
        #region Field functions
        public TextField AddField(string name, bool multiline = false)
        {
            TextField field = new TextField(name) { multiline=multiline };
            this.extensionContainer.Add(field);
            return field;
        }
        #endregion
        #region port functions
        // Add Input port to current Node
        public Port AddInputPort(string name, System.Type clazz, Port.Capacity portCapacity = Port.Capacity.Single)
        {
            Port _port = InstantiatePort(Orientation.Horizontal, Direction.Input, portCapacity, clazz);
            _port.portName = name;
            _port.portColor = FindPortColor(clazz.ToString());
            inputContainer.Add(_port);
            inputPorts.Add(_port);
            return _port;
        }
        // Add Output port to current Node
        public Port AddOutputPort(string name, System.Type clazz, Port.Capacity portCapacity = Port.Capacity.Single)
        {
            Port _port = InstantiatePort(Orientation.Horizontal, Direction.Output, portCapacity, clazz);
            _port.portName = name;
            _port.portColor = FindPortColor(clazz.ToString());
            outputContainer.Add(_port);
            outputPorts.Add(_port);
            return _port;
        }
        Color FindPortColor(string type)
        {
            switch (type)
            {
                case "UnityNodeGraph.PortTypeFlow":
                    return new Color32(63, 127, 191, 232);
                case "UnityNodeGraph.PortTypeAction":
                    return new Color32(191, 63, 63, 232);
                default:
                    return Color.gray;
            }
        }
        public PortData PortToPortData(Port _port)
        {
            int portCapacity = (_port.capacity == Port.Capacity.Single) ? 0 : 1;
            return new PortData {
                name = _port.portName,
                portType = _port.portType.ToString(),
                portCapacity = portCapacity
            };
        }
        public Port AddInputPortFromPortData(PortData _portData)
        {
            Port.Capacity capacity = (_portData.portCapacity == 0) ? Port.Capacity.Single : Port.Capacity.Multi;
            return AddInputPort(_portData.name, GetType(_portData.portType), capacity);
        }
        public Port AddOutputPortFromPortData(PortData _portData)
        {
            Port.Capacity capacity = (_portData.portCapacity == 0) ? Port.Capacity.Single : Port.Capacity.Multi;
            return AddOutputPort(_portData.name, GetType(_portData.portType), capacity);
        }
        private System.Type GetType(string strFullyQualifiedName)
        {
            System.Type type = System.Type.GetType(strFullyQualifiedName);
            if (type != null)
                return type;
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return type;
            }
            return null;
        }
        #endregion
        #region Virtual Function
        public abstract void OnInitialize();
        public NodeData GenerateData()
        {
            NodeData data = new NodeData()
            {
                guid = guid,
                name = name,
                type = this.GetType().ToString(),
                dataJSON = GenerateJson()
            };
            return data;
        }
        public abstract GraphCustomData getCustomData();
        public string GenerateJson()
        {
            GraphCustomData customData = getCustomData();
            if (customData != null)
            {
                jsonData.data = JsonUtility.ToJson(customData);
            }
            return JsonUtility.ToJson(jsonData);
        }
        #endregion
    }
    public class PortTypeFlow
    {

    }
    public class PortTypeAction
    {

    }
}
