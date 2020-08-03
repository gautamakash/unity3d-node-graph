using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace UnityNodeGraph
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private CoreGraphView _graphView;
        private EditorWindow _window;
        private GraphData _data;
        private Texture2D _indentationIcon;

        public void Init(CoreGraphView graphView, EditorWindow window, GraphData data)
        {
            _graphView = graphView;
            _window = window;
            _data = data;

            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0,0, new Color(0,0,0,0));
            _indentationIcon.Apply();
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> tree = new List<SearchTreeEntry>();
            if (_data!=null)
            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("Create Element"), 0));
                List<SearchElement> dataSearchList = _data.GetSearchMenu();
                dataSearchList.ForEach(searchElement=> {
                    if (searchElement.nodeType!=null & searchElement.nodeType!="")
                    {

                        tree.Add(new SearchTreeEntry(new GUIContent(searchElement.name, _indentationIcon))
                        {
                            userData = GetInstance(searchElement.nodeType),
                            level = searchElement.level
                        });
                    }
                    else
                    {
                        tree.Add(new SearchTreeGroupEntry(new GUIContent(searchElement.name), searchElement.level));
                    }
                });
            }
            else
            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("No Node Available"), 0));
            }
            return tree;
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

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 worldMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
                context.screenMousePosition - _window.position.position);
            Vector2 localMousePosition = _graphView.contentViewContainer.WorldToLocal(worldMousePosition);
            if (SearchTreeEntry.userData!=null)
            {
                CoreNode _newNode = GetInstance(SearchTreeEntry.userData.GetType().ToString()) as CoreNode;
                string[] _packageNames = SearchTreeEntry.userData.GetType().ToString().Split('.');
                _graphView.AddNode(_newNode.Init(new NodeData()
                {
                    name = _packageNames[_packageNames.Length -1],
                    position = localMousePosition,
                    size = new Vector2(100, 150)
                }));
                return true;
            }
            else
            {
                return false;
            }
            /*switch (SearchTreeEntry.userData)
            {
                case DialogueNode coreNode:
                    _graphView.AddNode(new DialogueNode().Init(new NodeData()
                    {
                        name = "DialogueNode",
                        position = localMousePosition,
                        size = new Vector2(100, 150)
                    }));
                    return true;
                default:
                    return false;
            }*/
        }
    }
}
