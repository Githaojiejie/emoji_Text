using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LinkImageText), true)]
/// <summary>
///   Custom Editor for the Text Component.
///   Extend this class to write a custom editor for an Text-derived component.
/// </summary>
public class LinkImageTextEditor : UnityEditor.UI.TextEditor
{
    private LinkImageText linkImageText;
    SerializedProperty m_Text;
    SerializedProperty m_Is3dText;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_Text = serializedObject.FindProperty("m_Text");
        linkImageText = this.target as LinkImageText;

        m_Is3dText = serializedObject.FindProperty("Is3dText");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        linkImageText.text = m_Text.stringValue;

        serializedObject.Update();
        EditorGUILayout.PropertyField(m_Is3dText, new GUIContent("3D场景里的文字"));
        serializedObject.ApplyModifiedProperties();
    }
}
