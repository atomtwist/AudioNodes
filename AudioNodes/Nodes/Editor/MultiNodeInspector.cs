using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MultiNode))]
public class MultiNodeInspector : Editor {

	MultiNode mn;

	void OnEnable()
	{
		mn = target as MultiNode;
		mn.GetChildNodes();
	}
	
	void AuditionButtons()
	{
		EditorGUILayout.BeginVertical("Box");
		EditorGUILayout.Space();
		if (GUILayout.Button("Audition"))
		{
			mn.Audition();
		}
		if (GUILayout.Button("StopAudition"))
		{
			mn.StopAudition ();
		}
		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();
	}
	
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		AuditionButtons();
	}
}
