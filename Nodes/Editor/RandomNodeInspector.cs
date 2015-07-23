using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RandomNode))]
public class RandomNodeInspector : Editor {

	RandomNode rn;

	void OnEnable()
	{
		rn = target as RandomNode;
		rn.GetChildNodes();
	}

	void AuditionButtons()
	{
		EditorGUILayout.BeginVertical("Box");
		EditorGUILayout.Space();
		if (GUILayout.Button("Audition"))
		{
			rn.Audition();
		}
		if (GUILayout.Button("StopAudition"))
		{
			rn.StopAudition ();
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
