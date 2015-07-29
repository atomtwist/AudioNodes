using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SFXNode))]
[CanEditMultipleObjects]
public class SFXNodeInspector : Editor {

	SFXNode sfxNode;

	void OnEnable()
	{
		sfxNode = target as SFXNode;
	}

	void AuditionButtons()
	{
		EditorGUILayout.BeginVertical("Box");
		EditorGUILayout.Space();
		if (GUILayout.Button("Audition Parent"))
		{
			var parentNode = sfxNode.transform.parent.GetComponent<AudioNode>();
			if (parentNode != null)
				parentNode.Audition();
			else
				sfxNode.Audition();
		}
		if (GUILayout.Button("Audition"))
		{
			sfxNode.Audition();
		}
		if (GUILayout.Button("StopAudition"))
		{
			sfxNode.StopAudition ();
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
