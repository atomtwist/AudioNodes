using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(EventNode),true)]
[CanEditMultipleObjects]
public class EventNodeInspector : Editor {

	ReorderableList list;
	EventNode eNode;

	public float gap = 16;
	float numberOfItems;
	private void OnEnable()
	{
		numberOfItems = 3;
		list = new ReorderableList(serializedObject, serializedObject.FindProperty("eventAction"),true,true,true,true);
		list.drawHeaderCallback =  
		(Rect rect) => {
			GUI.Label(rect, "Event Actions");
		};
		list.drawElementCallback =  
		(Rect rect, int index, bool isActive, bool isFocused) => {
			//get property
			var element = list.serializedProperty.GetArrayElementAtIndex(index);
			rect = new Rect(rect.x,rect.y, rect.width,EditorGUIUtility.singleLineHeight);
			//layout shit
			DrawEventActionPopup(element, rect);
			rect.y = rect.y + gap;
			DrawPlaybackActions(element, rect);
			DrawMixerActions(element, rect);
		};
		eNode = target as EventNode;
		EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;

	}

	void OnDisable()
	{
		EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;
	}

	void OnHierarchyChanged ()
	{
		GetSwitchGroupFromParent();
		Debug.Log ("ChangedbRo");
	}

	void DrawEventActionPopup(SerializedProperty property, Rect rect)
	{
		property.FindPropertyRelative("actionType").enumValueIndex = EditorGUI.Popup(rect, "Event Action", property.FindPropertyRelative("actionType").enumValueIndex,property.FindPropertyRelative("actionType").enumDisplayNames);
		property.FindPropertyRelative("bypassEvent").boolValue = EditorGUI.Toggle(new Rect(new Vector2(rect.xMin + 75,rect.y),rect.size),property.FindPropertyRelative("bypassEvent").boolValue);
	}
	
	void DrawPlaybackActions(SerializedProperty property, Rect rect)
	{
		//play
		if ( property.FindPropertyRelative("actionType").enumValueIndex == (int)EventActionType.Play)
		{
			DrawAudioNodePopup(property, rect);
			rect.y = rect.y + gap;
			property.FindPropertyRelative("positionBehaviour").enumValueIndex = EditorGUI.Popup(rect, "Position Behaviour", property.FindPropertyRelative("positionBehaviour").enumValueIndex,property.FindPropertyRelative("positionBehaviour").enumDisplayNames);
			property.FindPropertyRelative("followGameObject").boolValue = !Convert.ToBoolean( property.FindPropertyRelative("positionBehaviour").enumValueIndex);
		}
		//stop
		if ( property.FindPropertyRelative("actionType").enumValueIndex == (int)EventActionType.Stop)
		{
			DrawAudioNodePopup(property, rect);
			rect.y += gap;
			property.FindPropertyRelative("actionScope").enumValueIndex = EditorGUI.Popup(rect, "Action Scope", property.FindPropertyRelative("actionScope").enumValueIndex,property.FindPropertyRelative("actionScope").enumDisplayNames);
		}
		if ( property.FindPropertyRelative("actionType").enumValueIndex == (int)EventActionType.StopAll)
		{
			property.FindPropertyRelative("actionScope").enumValueIndex = EditorGUI.Popup(rect, "Action Scope", property.FindPropertyRelative("actionScope").enumValueIndex,property.FindPropertyRelative("actionScope").enumDisplayNames);
		}
		//stop mixer group
		if ( property.FindPropertyRelative("actionType").enumValueIndex == (int)EventActionType.StopMixerGroup)
		{
			EditorGUI.PropertyField(rect, property.FindPropertyRelative("mixerGroup"));
			rect.y += gap;
			property.FindPropertyRelative("actionScope").enumValueIndex = EditorGUI.Popup(rect, "Action Scope", property.FindPropertyRelative("actionScope").enumValueIndex,property.FindPropertyRelative("actionScope").enumDisplayNames);
		}
		//trigger event
		if ( property.FindPropertyRelative("actionType").enumValueIndex == (int)EventActionType.TriggerEvent)
		{
			DrawEventNodePopup(property, rect);
			rect.y += gap;
			EditorGUI.PropertyField(rect, property.FindPropertyRelative("eventTarget"));
		}

		//set Volume
		if ( property.FindPropertyRelative("actionType").enumValueIndex == (int)EventActionType.SetVolume)
		{
			DrawAudioNodePopup(property, rect);
			rect.y += gap;
			DrawVolumeSettings(property,rect);
		}
		//set Pitch
		if ( property.FindPropertyRelative("actionType").enumValueIndex == (int)EventActionType.SetPitch)
		{
			DrawAudioNodePopup(property, rect);
			rect.y += gap;
			DrawPitchSettings(property,rect);
		}
		//set Pan
		if ( property.FindPropertyRelative("actionType").enumValueIndex == (int)EventActionType.SetPan)
		{
			DrawAudioNodePopup(property, rect);
			rect.y += gap;
			DrawPanSettings(property,rect);
		}
		//switch
		if ( property.FindPropertyRelative("actionType").enumValueIndex == (int)EventActionType.SwitchSelect)
		{
			//DrawSwitchGroupPopup(property, rect);
			//rect.y += gap;
			DrawSwitchStatePopup(property, rect);
			rect.y += gap;
			DrawEventNodePopup(property, rect);
		}
	}

	void DrawVolumeSettings(SerializedProperty property, Rect rect)
	{
		EditorGUI.Slider(rect, property.FindPropertyRelative ("voiceVolume"),0,1);
	}

	void DrawPitchSettings(SerializedProperty property, Rect rect)
	{
		EditorGUI.Slider(rect, property.FindPropertyRelative ("voicePitch"),-3,3);
	}

	void DrawPanSettings(SerializedProperty property, Rect rect)
	{
		EditorGUI.Slider(rect, property.FindPropertyRelative ("voicePan"),-1,1);
	}


	void DrawAudioNodePopup(SerializedProperty property, Rect rect)
	{
		EditorGUI.BeginChangeCheck();
		var audioNodes = GameObject.FindObjectsOfType<AudioNode>().Where(n => n.exposeToEventnodes).ToList();
		//return if no audioNodes
		if (audioNodes.Count == 0 || audioNodes == null)
			return;
		//var audioNodeID = property.FindPropertyRelative ("uniqueAudioNodeID").intValue;
		var audioNodeNames = audioNodes.Select(n => n.name).ToArray();
		var audioNodeIDList = audioNodes.Select (n => n.uniqueID).ToList();
		var selectedIndex = EditorGUI.Popup( rect, "Audio Node ",audioNodeIDList.IndexOf(property.FindPropertyRelative ("uniqueAudioNodeID").intValue), audioNodeNames);
		selectedIndex = Mathf.Clamp(selectedIndex,0,int.MaxValue);
		var selectedNode = audioNodes[selectedIndex];
		property.FindPropertyRelative("selectedNodeGameObject").objectReferenceValue = selectedNode.gameObject;
		if( EditorGUI.EndChangeCheck()) 
		{
			property.FindPropertyRelative ("uniqueAudioNodeID").intValue = selectedNode.uniqueID;
		}
	}

	//switch
/*	void DrawSwitchGroupPopup(SerializedProperty property, Rect rect)
	{
		EditorGUI.BeginChangeCheck();
		var switchGroups = GameObject.FindObjectsOfType<SwitchGroup>().ToList();
		//return if no audioNodes
		if (switchGroups.Count == 0 || switchGroups == null)
			return;
		//var audioNodeID = property.FindPropertyRelative ("uniqueAudioNodeID").intValue;
		var switchGroupNames = switchGroups.Select(n => n.name).ToArray();
		var switchGroupIDList = switchGroups.Select (n => n.uniqueID).ToList();
		var selectedIndex = EditorGUI.Popup( rect, "Switch Group ",switchGroupIDList.IndexOf(property.FindPropertyRelative ("uniqueSwitchGroupID").intValue), switchGroupNames);
		selectedIndex = Mathf.Clamp(selectedIndex,0,int.MaxValue);
		var selectedNode = switchGroups[selectedIndex];
		property.FindPropertyRelative("switchGroupGameObject").objectReferenceValue = selectedNode.gameObject;
		if( EditorGUI.EndChangeCheck()) 
		{
			property.FindPropertyRelative ("uniqueSwitchGroupID").intValue = selectedNode.uniqueID;
		}
	}*/

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
		var selectedIndex = EditorGUILayout.Popup("Switch Group ",switchGroupIDList.IndexOf(eNode.switchGroupID), switchGroupNames);
		selectedIndex = Mathf.Clamp(selectedIndex,0,int.MaxValue);
		if (selectedIndex < switchGroups.Count)
		{
			var selectedNode = switchGroups[selectedIndex];
			eNode.switchGroupGameObject = selectedNode.gameObject;
			if( EditorGUI.EndChangeCheck()) 
			{
				eNode.switchGroupID = selectedNode.uniqueID;
			}
		}
		else
		{
			eNode.switchGroupGameObject = null;
			if( EditorGUI.EndChangeCheck()) 
			{
				eNode.switchGroupID = 0;
			}
		}
	}
	
	List<SwitchState> switchStates = new List<SwitchState>();
	void DrawSwitchStatePopup(SerializedProperty property, Rect rect)
	{
		EditorGUI.BeginChangeCheck();
		//check fro switchNodes
		if (eNode.switchGroupGameObject == null)
			return;
		//make a list of switchstates in 1st Level children
		switchStates.Clear();
		var switchNodeGameObject = eNode.switchGroupGameObject;
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
		var selectedIndex = EditorGUI.Popup( rect, "Switch State ",switchStateIDList.IndexOf(property.FindPropertyRelative ("switchStateID").intValue), switchStateNames);
		selectedIndex = Mathf.Clamp(selectedIndex,0,int.MaxValue);
		var selectedNode = switchStates[selectedIndex];
		property.FindPropertyRelative("switchStateName").stringValue = switchStateNames[selectedIndex];
		//property.FindPropertyRelative("switchStateObject").objectReferenceValue = selectedNode.gameObject;
		if( EditorGUI.EndChangeCheck()) 
		{
			property.FindPropertyRelative ("switchStateID").intValue = selectedNode.uniqueID;
		}
	}


	void DrawDefaultSwitchStatePopup()
	{
		EditorGUI.BeginChangeCheck();
		//check fro switchNodes
		if (eNode.switchGroupGameObject == null)
			return;
		//make a list of switchstates in 1st Level children
		switchStates.Clear();
		var switchNodeGameObject = eNode.switchGroupGameObject;
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
		var selectedIndex = EditorGUILayout.Popup( "Default Switch State ",switchStateIDList.IndexOf(eNode.defaultSwitchStateID), switchStateNames);
		selectedIndex = Mathf.Clamp(selectedIndex,0,int.MaxValue);
		var selectedNode = switchStates[selectedIndex];
		eNode.defaultSwitchStateName = switchStateNames[selectedIndex];
		//property.FindPropertyRelative("switchStateObject").objectReferenceValue = selectedNode.gameObject;
		if( EditorGUI.EndChangeCheck()) 
		{
			eNode.defaultSwitchStateID = selectedNode.uniqueID;
		}
	}
	
	void DrawEventNodePopup(SerializedProperty property, Rect rect)
	{
		EditorGUI.BeginChangeCheck();
		var eventNodes = GameObject.FindObjectsOfType<EventNode>().Where (e => e.uniqueID != eNode.uniqueID).ToList();
		//return if no eventNodes
		if (eventNodes.Count == 0 || eventNodes == null)
			return;
		//filter EventNodeList to prevent recursive triggering
		var loopList = eventNodes.ToList();
		foreach (var item in loopList) {
			foreach (var n in item.eventAction) {
				if (n.uniqueAudioNodeID == eNode.uniqueID)
					eventNodes.Remove(item);
				//prevent parent switchNode to get triggered
			 	var parentSwitchNode = eNode.transform.parent.GetComponent<SwitchNode>();
				if (parentSwitchNode != null)
				{
					eventNodes.Remove(parentSwitchNode);
				}
			}
		}
		var eventNodeNames = eventNodes.Select(n => n.name).ToArray();
		var eventNodeIDList = eventNodes.Select (n => n.uniqueID).ToList();
		var selectedIndex = EditorGUI.Popup( rect, "Event Node ",eventNodeIDList.IndexOf(property.FindPropertyRelative ("uniqueAudioNodeID").intValue), eventNodeNames);
		selectedIndex = Mathf.Clamp(selectedIndex,0,int.MaxValue);
		var selectedNode = eventNodes[selectedIndex];
		property.FindPropertyRelative("selectedNodeGameObject").objectReferenceValue = selectedNode.gameObject;
		if( EditorGUI.EndChangeCheck()) 
		{
			property.FindPropertyRelative ("uniqueAudioNodeID").intValue = selectedNode.uniqueID;
		}
	}

	void DrawMixerActions(SerializedProperty property, Rect rect)
	{
		if ( property.FindPropertyRelative("actionType").enumValueIndex == (int)EventActionType.TriggerMixerSnapshot)
		{
			EditorGUI.PropertyField(rect, property.FindPropertyRelative("mixerSnapshot"));
			rect.y += gap;
			property.FindPropertyRelative("mixerSnapshotTransitionTime").floatValue = EditorGUI.FloatField(rect, "Transition Time", property.FindPropertyRelative("mixerSnapshotTransitionTime").floatValue);
		}
	}

	void GetSwitchGroupFromParent()
	{
		if (eNode == null) return;
		var parentSwitchNode = eNode.transform.parent.GetComponent<SwitchNode>();
		if(parentSwitchNode != null)
		{
			eNode.switchGroupID = parentSwitchNode.switchGroupID;
			eNode.switchGroupGameObject = parentSwitchNode.switchGroupGameObject;

			//if group has states, pick the first state...maybe
		} 
		else
		{
			eNode.switchGroupID = 0;
			eNode.switchGroupGameObject = null;
		}
	}




	void AuditionButtons()
	{
		EditorGUILayout.BeginVertical("Box");
		EditorGUILayout.Space();
		if (GUILayout.Button("Audition Event"))
		{
			eNode.AuditionEvent();
		}
		if (GUILayout.Button("StopAudition"))
		{
			eNode.StopAudition();
		}
		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		//DrawSwitchGroupPopup();
		if (eNode.switchGroupGameObject != null)
			DrawDefaultSwitchStatePopup();
		EditorGUILayout.Space();
		serializedObject.Update();
		list.elementHeight = 23.333333f * numberOfItems;
		list.DoLayoutList();
		serializedObject.ApplyModifiedProperties();
		AuditionButtons();

	}
}
