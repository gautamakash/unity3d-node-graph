using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Text.RegularExpressions;

namespace UnityNodeGraph
{
    public class CoreGraphView : GraphView
    {
        #region Variables
        public GraphData graphData;
        public string rootGuid;
        private NodeSearchWindow _searchWindow;
        private EditorWindow _editorWindow;
        private List<string> hiddenFields = new List<string>() {
            "__rootGuid",
            "__nodes",
            "__links"
        };
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
        public string SplitCamelCase(string str)
        {
            if (str==null || str=="")
            {
                return str;
            }
            str = Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
            return str.First().ToString().ToUpper() + str.Substring(1);
        }
        public void LoadData(GraphData graphData)
        {
            this.graphData = graphData;
            if (graphData!=null && graphData.__rootGuid!=null && graphData.__rootGuid!="")
            {
                rootGuid = graphData.__rootGuid;
                graphData.__nodes.ForEach(_nodeData=> {
                    if (_nodeData.guid == graphData.__rootGuid)
                    {
                        //_nodeData.name = "Entry"; // graphData.name;
                        AddNode(new RootNode().Init(_nodeData), true);
                    }
                });
            }
            else if(graphData!=null)
            {
                NodeData _newRootNode = new NodeData() { name = "Graph Entry", position = new Vector2(250, 100) };
                JSONGraphData _rootJSONGraphData = new JSONGraphData();
                _rootJSONGraphData.data = JsonUtility.ToJson(graphData.GetRootPortData());
                _newRootNode.dataJSON = JsonUtility.ToJson(_rootJSONGraphData);
                rootGuid = AddNode(new RootNode().Init(_newRootNode), true).guid;
            }
            GenerateNodes();
            ConnectNodes();
            // Add Search Window
            AddSearchWindow(_editorWindow, graphData);
        }
        void GenerateNodes()
        {
            if(graphData!=null && graphData.__nodes!=null && graphData.__nodes.Count > 0)
            {
                graphData.__nodes.ForEach(_nodeData=> {
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
            if (graphData != null && graphData.__links != null && graphData.__links.Count > 0 && graphData.__nodes != null && graphData.__nodes.Count > 0)
            {
                this.nodes.ToList().Cast<CoreNode>().ToList().ForEach(_node=>{
                //this.nodes.Cast<CoreNode>().toList().ForEach(_node => {
                    graphData.__links.Where(x=>x.sourceGuid == _node.guid).ToList().ForEach(_link => {
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
        public void GenerateBlackBoard(GraphData currentData)
        {
            Blackboard blackboard = new Blackboard(this) {
                scrollable=true,
            };
            blackboard.AddToClassList("grayboard");
            //blackboard.Add(new BlackboardSection { title="Exposed Properties" });
            blackboard.addItemRequested = _blackboard => { };
            blackboard.SetPosition(new Rect(10, 30, 200, 200));
            this.Add(blackboard);

            /* Start: how to add to blackboard */
            /*var container = new VisualElement();
            var blackboardField = new BlackboardField {
                text = "Sample_Property",
                typeText = "String Property"
            };
            var blackboardFieldValue = new TextField("Value") {
                value = "Value"
            };
            blackboardFieldValue.RegisterValueChangedCallback(evt => {
                Debug.Log(evt.newValue);
            });
            var blackBoardRow = new BlackboardRow(blackboardField, blackboardFieldValue);
            container.Add(blackboardField);
            container.Add(blackBoardRow);
            blackboard.Add(container);*/
            /* End: how to add to blackboard */
            if (currentData != null)
            {
                blackboard.title = currentData.name;
                blackboard.subTitle = "Properties";
                FieldInfo[] fields = currentData.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                //Debug.Log(currentData.GetType());
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo fieldInfo = fields[i];
                    if (!hiddenFields.Contains(fieldInfo.Name))
                    {
                        //Debug.Log(fields[i].ToString());
                        string fName = SplitCamelCase(fieldInfo.Name);
                        var container = new VisualElement();
                        BlackboardSection bbField;
                        BlackboardRow bbRow;
                        switch (fieldInfo.FieldType.ToString())
                        {
                            case "System.String":
                                bbField = new BlackboardSection { title = fName };
                                container.Add(bbField);
                                TextField bbfValue = new TextField()
                                {
                                    value = fieldInfo.GetValue(currentData) as string,
                                    multiline = true
                                };
                                bbfValue.RegisterValueChangedCallback(evt => {
                                    fieldInfo.SetValue(currentData, evt.newValue);
                                    //Debug.Log(evt.newValue);
                                });
                                bbRow = new BlackboardRow(bbField, bbfValue);
                                container.Add(bbRow);
                                break;
                            case "System.Int32":
                                bbField = new BlackboardSection { title = fName };
                                container.Add(bbField);
                                IntegerField bbfIntValue = new IntegerField()
                                {
                                    value = (int) fieldInfo.GetValue(currentData)
                                };
                                bbfIntValue.RegisterValueChangedCallback(evt => {
                                    fieldInfo.SetValue(currentData, evt.newValue);
                                    //Debug.Log(evt.newValue);
                                });
                                bbRow = new BlackboardRow(bbField, bbfIntValue);
                                container.Add(bbRow);
                                break;
                            case "System.Single":
                                bbField = new BlackboardSection { title = fName };
                                container.Add(bbField);
                                FloatField bbfFloatValue = new FloatField()
                                {
                                    value = (float)fieldInfo.GetValue(currentData)
                                };
                                bbfFloatValue.RegisterValueChangedCallback(evt => {
                                    fieldInfo.SetValue(currentData, evt.newValue);
                                    //Debug.Log(evt.newValue);
                                });
                                bbRow = new BlackboardRow(bbField, bbfFloatValue);
                                container.Add(bbRow);
                                break;
                            case "UnityEngine.Vector2":
                                bbField = new BlackboardSection { title = fName };
                                container.Add(bbField);
                                Vector2Field bbfVector2Value = new Vector2Field()
                                {
                                    value = (Vector2)fieldInfo.GetValue(currentData)
                                };
                                bbfVector2Value.RegisterValueChangedCallback(evt => {
                                    fieldInfo.SetValue(currentData, evt.newValue);
                                    //Debug.Log(evt.newValue);
                                });
                                bbRow = new BlackboardRow(bbField, bbfVector2Value);
                                container.Add(bbRow);
                                break;
                            case "UnityEngine.Vector3":
                                bbField = new BlackboardSection { title = fName };
                                container.Add(bbField);
                                Vector3Field bbfVector3Value = new Vector3Field()
                                {
                                    value = (Vector3)fieldInfo.GetValue(currentData)
                                };
                                bbfVector3Value.RegisterValueChangedCallback(evt => {
                                    fieldInfo.SetValue(currentData, evt.newValue);
                                    //Debug.Log(evt.newValue);
                                });
                                bbRow = new BlackboardRow(bbField, bbfVector3Value);
                                container.Add(bbRow);
                                break;
                            case "UnityEngine.AnimationCurve":
                                bbField = new BlackboardSection { title = fName };
                                container.Add(bbField);
                                CurveField bbfAnimationCurveValue = new CurveField()
                                {
                                    value = (UnityEngine.AnimationCurve)fieldInfo.GetValue(currentData)
                                };
                                bbfAnimationCurveValue.RegisterValueChangedCallback(evt => {
                                    fieldInfo.SetValue(currentData, evt.newValue);
                                    //Debug.Log(evt.newValue);
                                });
                                bbRow = new BlackboardRow(bbField, bbfAnimationCurveValue);
                                container.Add(bbRow);
                                break;
                            default:
                                if (fieldInfo.FieldType.ToString().IndexOf("UnityEngine.")==0)
                                {
                                    bbField = new BlackboardSection { title = fName };
                                    container.Add(bbField);
                                    ObjectField bbfObjectValue = new ObjectField()
                                    {
                                        value = (UnityEngine.Object)fieldInfo.GetValue(currentData),
                                        allowSceneObjects = false,
                                        objectType = fieldInfo.FieldType
                                    };
                                    bbfObjectValue.RegisterValueChangedCallback(evt => {
                                        fieldInfo.SetValue(currentData, evt.newValue);
                                        //Debug.Log(evt.newValue);
                                    });
                                    bbRow = new BlackboardRow(bbField, bbfObjectValue);
                                    container.Add(bbRow);
                                }
                                else
                                {
                                    bbField = new BlackboardSection { title = fName };
                                    container.Add(bbField);
                                    EnumField bbfEnumValue = new EnumField((System.Enum)fieldInfo.GetValue(currentData))
                                    {
                                        value = (System.Enum)fieldInfo.GetValue(currentData)                                        
                                    };
                                    bbfEnumValue.RegisterValueChangedCallback(evt => {
                                        fieldInfo.SetValue(currentData, evt.newValue);
                                        //Debug.Log(evt.newValue);
                                    });
                                    bbRow = new BlackboardRow(bbField, bbfEnumValue);
                                    container.Add(bbRow);
                                }
                                break;
                        }                   

                        blackboard.Add(container);
                    }
                }

            }
            // Get the properties of 'Type' class object.
        }
        #endregion
    }
}
