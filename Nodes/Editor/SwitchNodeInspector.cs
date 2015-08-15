using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SwitchNode))]
public class SwitchNodeInspector : Editor {

	SwitchNode s;

	void OnEnable()
	{
		s = target as SwitchNode;
		s.GetChildEvents();
	}

	void DrawSwitchGroupPopup()
	{
		EditorGUI.BeginChangeCheck();
		var switchGroups = GameObject.FindObjectsOfType<SwitchGroup>().ToList();
		//return if no audioNodes
		if (switchGroups.Count == 0 || switchGroups == null)
			return;
		//var audioNodeID = property.FindPropertyRelative ("uniqueAudioNodeID").intValue;
		var switchGroupNamesList = switchGroups.Select(n => n.name).ToList();
		switchGroupNamesList.Add ("None");
		var switchGroupNames = switchGroupNamesList.ToArray();
		var switchGroupIDList = switchGroups.Select (n => n.uniqueID).ToList();
		switchGroupIDList.Add(0);
		var selectedIndex = EditorGUILayout.Popup("Switch Group ",switchGroupIDList.IndexOf(s.switchGroupID), switchGroupNames);
		selectedIndex = Mathf.Clamp(selectedIndex,0,int.MaxValue);
		if (selectedIndex < switchGroups.Count)
		{
			var selectedNode = switchGroups[selectedIndex];
			s.switchGroupGameObject = selectedNode.gameObject;
			if( EditorGUI.EndChangeCheck()) 
			{
				s.switchGroupID = selectedNode.uniqueID;
				//set childnote switch groups
				var childEventNodes = s.GetComponentsInChildren<EventNode>();
				foreach (var e in childEventNodes) {
					e.switchGroupID = s.switchGroupID;
					e.switchGroupGameObject = s.switchGroupGameObject;
				}
			}
		}
		else
		{
			s.switchGroupGameObject = null;
			if( EditorGUI.EndChangeCheck()) 
			{
				s.switchGroupID = 0;
				//set childnote switch groups
				//set childnote switch groups
				var childEventNodes = s.GetComponentsInChildren<EventNode>();
				foreach (var e in childEventNodes) {
					e.switchGroupID = s.switchGroupID;
					e.switchGroupGameObject = s.switchGroupGameObject;
				}
			}
		}
	}
	
	List<SwitchState> switchStates = new List<SwitchState>();
	void DrawPreviewSwitchStatePopup()
	{
		EditorGUI.BeginChangeCheck();
		//check fro switchNodes
		if (s.switchGroupGameObject == null)
			return;
		//make a list of switchstates in 1st Level children
		switchStates.Clear();
		var switchNodeGameObject = s.switchGroupGameObject;
		foreach (Transform t in switchNodeGameObject.transform)
		{
			var switchState = t.GetComponent<SwitchState>() ;
			if (switchState != null)
				switchStates.Add(switchState);
		}
		//return if no audioNodes
		if (switchStates.Count == 0 || switchStates == null)
			return;
		//var audioNodeID = property.FindPropertyRelative ("uniqueAudioNodeID").intValue;
		var switchStateNames = switchStates.Select(n => n.name).ToArray();
		var switchStateIDList = switchStates.Select (n => n.uniqueID).ToList();
		var selectedIndex = EditorGUILayout.Popup( "Preview Switch State ",switchStateIDList.IndexOf(s.defaultSwitchStateID), switchStateNames);
		selectedIndex = Mathf.Clamp(selectedIndex,0,int.MaxValue);
		var selectedNode = switchStates[selectedIndex];
		s.defaultSwitchStateName = switchStateNames[selectedIndex];
		//property.FindPropertyRelative("switchStateObject").objectReferenceValue = selectedNode.gameObject;
		if( EditorGUI.EndChangeCheck()) 
		{
			s.defaultSwitchStateID = selectedNode.uniqueID;
			s.switchGroupGameObject.GetComponent<SwitchGroup>().currentSwitchStateID = selectedNode.uniqueID;
		}
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
		DrawSwitchGroupPopup();
		DrawPreviewSwitchStatePopup();
		AuditionButtons();
	}

}
