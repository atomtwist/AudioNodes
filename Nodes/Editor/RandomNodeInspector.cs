using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RandomNode))]
[CanEditMultipleObjects]
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
		if (GUILayout.Button("Audition Parent"))
		{
			var parentNode = rn.transform.parent.GetComponent<AudioNode>();
			if (parentNode != null)
				parentNode.Audition();
			else
				rn.Audition();
		}
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
