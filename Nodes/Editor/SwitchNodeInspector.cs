using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SwitchNode))]
public class SwitchNodeInspector : Editor {

	SwitchNode sNode;

	void OnEnable()
	{
		sNode = target as SwitchNode;
	}



	public override void OnInspectorGUI ()
	{
		//base.OnInspectorGUI ();
		DrawDefaultInspector();
		serializedObject.Update();

		EditorGUILayout.Space();EditorGUILayout.Space();
		DrawSwitchStatePopup();
		DrawPlayModePopup();

		AuditionButtons();

		serializedObject.ApplyModifiedProperties();
	}

	void DrawPlayModePopup()
	{
		EditorGUILayout.PropertyField(serializedObject.FindProperty("switchMode"));
	}

	List<SwitchState> switchStates = new List<SwitchState>();
	void DrawSwitchStatePopup()
	{
		EditorGUI.BeginChangeCheck();
		switchStates.Clear();
		foreach (Transform t in sNode.transform)
		{
			var switchState = t.GetComponent<SwitchState>() ;
			if (switchState != null)
				switchStates.Add(switchState);
		}
		//return if no switchStates
		if (switchStates.Count == 0 || switchStates == null)
			return;
		
		var switchStateNames = switchStates.Select(n => n.name).ToArray();
		var switchStateIDList = switchStates.Select (n => n.uniqueID).ToList();
		var selectedIndex = EditorGUILayout.Popup("Switch State", switchStateIDList.IndexOf(sNode.selectedSwitchStateID), switchStateNames ) ;
		selectedIndex = Mathf.Clamp(selectedIndex,0,int.MaxValue);
		var selectedState = switchStates[selectedIndex];
		sNode.selectedSwitchStateGameObject = selectedState.gameObject;
		if( EditorGUI.EndChangeCheck()) 
		{
			sNode.selectedSwitchStateID = selectedState.uniqueID;
		}
		sNode.unselectedStateObjects = switchStates.Select (s => s.gameObject).Where (g => g != selectedState.gameObject).ToList();
	}

	void AuditionButtons()
	{
		EditorGUILayout.BeginVertical("Box");
		EditorGUILayout.Space();
		if (GUILayout.Button("Audition"))
		{
			sNode.Audition();
		}
		if (GUILayout.Button("StopAudition"))
		{
			sNode.StopAudition ();
		}
/*		if (GUILayout.Button("Switch"))
		{
			sNode.SetSwitch ();
		}*/
		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();
	}

}
