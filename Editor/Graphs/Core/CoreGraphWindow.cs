using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace UnityNodeGraph
{
    public class CoreGraphWindow : EditorWindow
    {
        #region Variables
        public GraphData currentData { get; set; }
        CoreGraphView graphView;
        Toolbar graphToolbar;
        #endregion
        #region Static Load
        [OnOpenAsset(1)]
        public static bool ShowWindow(int instanceId, int line)
        {
            var item = EditorUtility.InstanceIDToObject(instanceId);
            if (item is GraphData)
            {
                var window = (CoreGraphWindow)GetWindow(typeof(CoreGraphWindow));
                window.currentData = item as GraphData;
                var packages = window.currentData.GetType().ToString().Split('.');
                window.titleContent = new UnityEngine.GUIContent(packages[packages.Length - 1]);
                window.UnloadGraphView();
                window.UnLoadToolbar();
                window.LoadGraphView();
                window.LoadToolbar();
                window.minSize = new UnityEngine.Vector2(500, 250);

                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
        #region EventListner Implementation
        private void OnEnable()
        {
            LoadGraphView();
            LoadToolbar();
        }
        private void OnDisable()
        {
            UnloadGraphView();
            UnLoadToolbar();
        }
        #endregion
        #region Load functions
        void LoadGraphView()
        {
            // Create GraphView
            graphView = new CoreGraphView(this) { name = "Dialogue Graph" };
            graphView.LoadData(currentData);
            // Add Visual Behaviours
            graphView.StretchToParentSize();

            // Load GraphView to root view
            rootVisualElement.Add(graphView);

            // Add Function to graphView
            //graphView.GenerateMiniMap();
            graphView.GenerateBlackBoard(currentData);
        }
        void LoadToolbar()
        {
            if (currentData!=null)
            {
                // Create Toolbar
                graphToolbar = new Toolbar();
                // Add Buttons
                graphToolbar.Add(new ToolbarButton(() => {
                    SaveData();
                }) { text = "Save Graph" });

                // Load Toolbar to root view
                rootVisualElement.Add(graphToolbar);
            }
            else
            {
                // Create Toolbar
                graphToolbar = new Toolbar();
                // Add Label
                graphToolbar.Add(new Label("No File Selected"));

                // Load Toolbar to root view
                rootVisualElement.Add(graphToolbar);
            }
        }
        #endregion
        #region Unload function
        void UnloadGraphView()
        {
            // Remove GraphView
            if (graphView!=null)
            {
                rootVisualElement.Remove(graphView);
            }
        }
        void UnLoadToolbar()
        {
            // Remove Toolbar
            if (graphToolbar != null)
            {
                rootVisualElement.Remove(graphToolbar);
            }
        }
        #endregion
        #region Data Manupulation
        void SaveData()
        {
            //currentData.rootGuid = graphView.rootGuid;
            List<Edge> edges = graphView.edges.ToList();

            if (!edges.Any()) return;

            List<CoreNode> nodes = graphView.nodes.ToList().Cast<CoreNode>().ToList();
            var connectedPorts = edges.Where(x => x.input.node != null).ToArray();

            currentData.__links = new List<LinkData>();

            for (int i = 0; i < connectedPorts.Length; i++)
            {
                CoreNode outputNode = connectedPorts[i].output.node as CoreNode;
                CoreNode inputNode = connectedPorts[i].input.node as CoreNode;
                currentData.__links.Add(new LinkData {
                    sourceGuid = outputNode.guid,
                    sourcePortName = connectedPorts[i].output.portName,
                    targetGuid = inputNode.guid,
                    targetPortName = connectedPorts[i].input.portName
                });
            }

            currentData.__nodes = new List<NodeData>();
            nodes.ForEach(_node=> {
                //Debug.Log(_node.GetType().ToString());
                currentData.__nodes.Add(new NodeData {
                    guid = _node.guid,
                    name = _node.title,
                    position = _node.GetPosition().position,
                    dataJSON = _node.GenerateJson(),
                    type = _node.GetType().ToString()
                });
            });
            currentData.__rootGuid = graphView.rootGuid;
            EditorUtility.SetDirty(currentData);
            //Debug.Log(JsonUtility.ToJson(currentData));
        }
        #endregion
    }
}
