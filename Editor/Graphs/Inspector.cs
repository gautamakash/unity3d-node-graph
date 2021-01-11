using UnityEngine;
using UnityEditor;
using UnityNodeGraph;
[CustomEditor(typeof(GraphData), true)]
public class Inspector : Editor
{
    bool showDefaultFields = false;
    string getClassName(string name)
    {
        if (name.IndexOf(".") != -1)
        {
            var names = name.Split('.');
            return names[names.Length-1];
        }
        else
        {
            return name;
        }
    }
    public override void OnInspectorGUI()
    {
        GraphData data = target as GraphData;
        //base.OnInspectorGUI();
        GUILayout.Label(getClassName(data.GetType().ToString()), EditorStyles.boldLabel);
        GUILayout.Label("This is Node Graph, Double click the asset to open node editor.", EditorStyles.helpBox);

        if (!showDefaultFields)
        {
            var styleError = new GUIStyle(GUI.skin.button);
            styleError.hover.textColor = Color.red;
            if (
                GUILayout.Button(
                    new GUIContent("Show Properties", "Not Recomended, use node graph to update properties, this should be fallback if there is any issue with nodegraph."),
                    styleError
                )
            )
            {
                showDefaultFields = true;
            }
        }
        else
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Hide Properties"))
            {
                showDefaultFields = false;
            }
        }
    }
}
