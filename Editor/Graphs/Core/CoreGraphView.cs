using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace UnityNodeGraph
{
    public class CoreGraphView : GraphView
    {
        #region Variables
        public GraphData graphData;
        public string rootGuid;
        private NodeSearchWindow _searchWindow;
        private EditorWindow _editorWindow;
        #endregion
        #region cunstructor
        public CoreGraphView(EditorWindow editorWindow)
        {
            // Add Stylesheet to Graph
            styleSheets.Add(Resources.Load<StyleSheet>("stylesheet_dialogue_graph"));

            // Implement Visual changes
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            // Add Grid to Graph
            GridBackground grid = new GridBackground();
            Insert(0, grid);
            // Stretch Grid
            grid.StretchToParentSize();
            _editorWindow = editorWindow;
        }
        #endregion
        #region Node Operations
        public CoreNode AddNode(CoreNode _node, bool root = false)
        {
            if (root)
            {
                //_node.AddOutputPort("Next", typeof(PortTypeFlow));
                //_node.SetPosition(new Rect(100, 200, 100, 150));
            }
            _node.Refresh();
            AddElement(_node);
            // Implement custom Nodes Values
            return _node;
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(_port=> {
                if (startPort!=_port && startPort.node!=_port.node && startPort.portType == _port.portType && startPort.direction!=_port.direction)
                {
                    compatiblePorts.Add(_port);
                }
            });
            return compatiblePorts;
        }
        #endregion
        #region Data Operation
        public void LoadData(GraphData graphData)
        {
            this.graphData = graphData;
            if (graphData!=null && graphData.rootGuid!=null && graphData.rootGuid!="")
            {
                rootGuid = graphData.rootGuid;
                graphData.nodes.ForEach(_nodeData=> {
                    if (_nodeData.guid == graphData.rootGuid)
                    {
                        _nodeData.name = graphData.name;
                        AddNode(new RootNode().Init(_nodeData), true);
                    }
                });
            }
            else if(graphData!=null)
            {
                rootGuid = AddNode(new RootNode().Init(new NodeData() { name = graphData.name, position = new Vector2(100, 200) }), true).guid;
            }
            GenerateNodes();
            ConnectNodes();
            // Add Search Window
            AddSearchWindow(_editorWindow, graphData);
        }
        void GenerateNodes()
        {
            if(graphData!=null && graphData.nodes!=null && graphData.nodes.Count > 0)
            {
                graphData.nodes.ForEach(_nodeData=> {
                    if (_nodeData.guid != rootGuid)
                    {
                        CoreNode _node = GetInstance(_nodeData.type) as CoreNode;
                        _node.Init(_nodeData);
                        AddNode(_node);
                    }
                });
            }
        }
        void ConnectNodes()
        {
            if (graphData != null && graphData.links != null && graphData.links.Count > 0 && graphData.nodes != null && graphData.nodes.Count > 0)
            {
                this.nodes.ToList().Cast<CoreNode>().ToList().ForEach(_node=>{
                //this.nodes.Cast<CoreNode>().toList().ForEach(_node => {
                    graphData.links.Where(x=>x.sourceGuid == _node.guid).ToList().ForEach(_link => {
                        Port sourcePort = _node.outputPorts.First(p => p.portName == _link.sourcePortName);
                        CoreNode targetNode = this.nodes.ToList().Cast<CoreNode>().ToList().First(x => x.guid == _link.targetGuid);
                        Port targetPort = targetNode.inputPorts.First(p => p.portName == _link.targetPortName);
                        // Connect Ports
                        if(sourcePort!=null && targetPort != null)
                        {
                            Edge _edge = new Edge
                            {
                                output=sourcePort,
                                input=targetPort
                            };
                            _edge.input.Connect(_edge);
                            _edge.output.Connect(_edge);
                            this.Add(_edge);
                        }
                    });
                });
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
        #endregion
        #region UI Operation
        private void AddSearchWindow(EditorWindow editorWindow, GraphData graphData)
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Init(this, editorWindow, graphData);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }
        private void GenerateMiniMap()
        {
            var miniMap = new MiniMap { anchored = true };
            //Vector2 cords = this.contentViewContainer.WorldToLocal(new Vector2(window.maxSize.x - 10, 30));
            miniMap.SetPosition(new Rect(10, 30, 200, 140));
            //miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
            this.Add(miniMap);
        }
        public void GenerateBlackBoard()
        {
            Blackboard blackboard = new Blackboard(this);
            blackboard.Add(new BlackboardSection { title="Exposed Properties" });
            blackboard.addItemRequested = _blackboard => { };
            blackboard.SetPosition(new Rect(10, 30, 200, 200));
            this.Add(blackboard);
        }
        #endregion
    }
}
