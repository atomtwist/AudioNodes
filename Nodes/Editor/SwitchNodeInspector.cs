using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SwitchNode))]
public class SwitchNodeInspector : Editor {

	SwitchNode s;

	void OnEnable()
	{
		s = target as SwitchNode;
		s.GetChildEvents();
	}

	void AuditionButtons()
	{
		EditorGUILayout.BeginVertical("Box");
		EditorGUILayout.Space();
		if (GUILayout.Button("Audition Event"))
		{
			s.AuditionEvent();
		}
		if (GUILayout.Button("StopAudition"))
		{
			s.StopAudition();
		}
		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();
		AuditionButtons();
	}

}
