using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SFXNodeVR))]
[CanEditMultipleObjects]
public class SFXNodeVRInspector : Editor {
	
	SFXNodeVR sfxNode;
	
	void OnEnable()
	{
		sfxNode = target as SFXNodeVR;
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
