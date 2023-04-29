using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

[CustomEditor(typeof(ComponentVariableLister))]
public class ComponentVariableListerEditor : Editor
{
    private ComponentVariableLister lister;
    private Component targetComponent;

    private void OnEnable()
    {
        lister = target as ComponentVariableLister;
        targetComponent = lister.GetComponent<Component>();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        if (targetComponent == null)
        {
            targetComponent = (target as ComponentVariableLister).gameObject.GetComponent<Component>();
        }

        targetComponent = EditorGUILayout.ObjectField("Target Component", targetComponent, typeof(Component), true) as Component;

        if (GUILayout.Button("List Variables"))
        {
            string filePath = EditorUtility.SaveFilePanel("Save List Variables", "", "component_variables.txt", "txt");
            if (!string.IsNullOrEmpty(filePath))
            {
                lister.ListVariables(targetComponent, filePath);
                AssetDatabase.Refresh();
            }
        }
    }

    // Add this section to draw the UI elements for the target component selection and the "List Variables" button
    private void OnSceneGUI()
    {
        Handles.BeginGUI();

        var rect = new Rect(Screen.width - 200, Screen.height - 50, 190, 40);
        GUI.Box(rect, "");

        GUILayout.BeginArea(rect);
        GUILayout.BeginHorizontal();

        GUILayout.Label("Target Component: ", GUILayout.Width(100));
        targetComponent = EditorGUILayout.ObjectField(targetComponent, typeof(Component), true) as Component;

        if (GUILayout.Button("List Variables"))
        {
            string filePath = EditorUtility.SaveFilePanel("Save List Variables", "", "component_variables.txt", "txt");
            if (!string.IsNullOrEmpty(filePath))
            {
                lister.ListVariables(targetComponent, filePath);
                AssetDatabase.Refresh();
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        Handles.EndGUI();
    }
}

public class ComponentVariableLister : MonoBehaviour
{
    public void ListVariables(Component targetComponent, string filePath)
    {
        if (targetComponent == null)
        {
            Debug.LogWarning("Target component is null.");
            return;
        }

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            ListFields(targetComponent, writer, 0);
        }
    }

    private static void ListFields(object obj, StreamWriter writer, int depth)
    {
        if (obj == null) return;

        var type = obj.GetType();

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (field.IsDefined(typeof(HideInInspector), false)) continue;

            var fieldType = field.FieldType;
            var value = field.GetValue(obj);

            WriteIndent(writer, depth);
            writer.Write("{0}: ", field.Name);
            if (value == null)
            {
                writer.Write("null");
            }
            else if (fieldType.IsValueType || fieldType == typeof(string))
            {
                writer.Write(value);
            }
            else if (fieldType.IsArray)
            {
                writer.WriteLine();
                ListCollection((ICollection)value, writer, depth + 1);
            }
            else
            {
                writer.WriteLine();
                ListFields(value, writer, depth + 1);
            }
        }
    }

    private static void WriteIndent(StreamWriter writer, int depth)
    {
        for (int i = 0; i < depth; i++)
        {
            writer.Write("  ");
        }
        writer.WriteLine();
    }

    private static void ListCollection(ICollection collection, StreamWriter writer, int depth)
    {
        foreach (var element in collection)
        {
            WriteIndent(writer, depth);
            if (element != null && (element.GetType().IsValueType || element.GetType() == typeof(string)))
            {
                writer.WriteLine(element);
            }
            else
            {
                writer.WriteLine(element != null ? element.GetType().ToString() : "null");
                ListFields(element, writer, depth + 1);
            }
        }
    }
}
