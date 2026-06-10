using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BossShine))]
public class BossShineEditor : Editor
{
    private BossShine bossShine;
    private SerializedProperty shineLocalOffsetProperty;

    private void OnEnable()
    {
        bossShine = (BossShine)target;
        shineLocalOffsetProperty = serializedObject.FindProperty("shineLocalOffset");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
        
        if (GUILayout.Button("Reset Shine Position"))
        {
            Undo.RecordObject(bossShine, "Reset Shine Position");
            shineLocalOffsetProperty.vector3Value = Vector3.zero;
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void OnSceneGUI()
    {
        if (!bossShine.DrawGizmo) return;

        Transform transform = bossShine.transform;
        Vector3 shineWorldPos = transform.TransformPoint(shineLocalOffsetProperty.vector3Value);
        
        Handles.color = bossShine.GizmoColor;
        EditorGUI.BeginChangeCheck();
        
        Vector3 newWorldPos = Handles.PositionHandle(shineWorldPos, Quaternion.identity);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(bossShine, "Move Shine Position");
            shineLocalOffsetProperty.vector3Value = transform.InverseTransformPoint(newWorldPos);
            serializedObject.ApplyModifiedProperties();
        }
        
        Handles.DrawLine(transform.position, shineWorldPos);
        Handles.SphereHandleCap(0, shineWorldPos, Quaternion.identity, 0.2f, EventType.Repaint);
    }
}