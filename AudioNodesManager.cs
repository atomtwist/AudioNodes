using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class AudioNodesManager : MonoBehaviour {
	
	private static AudioNodesManager _instance;

	public static AudioNodesManager  instance
	{
		get 
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<AudioNodesManager>();
			}
			return _instance;
		}
	}


	public AudioNodeEventPreview eventNodePreview;

	List<EventNode> eventNodes;
	List<SwitchState> switchStates;
	void OnEnable()
	{
		eventNodes = GameObject.FindObjectsOfType<EventNode>().ToList();
		switchStates = GameObject.FindObjectsOfType<SwitchState>().ToList();
		if (!Application.isPlaying) transform.position = FindObjectOfType<AudioListener>().transform.position;
	}
	

	//static wrappers
	//name
	public static void PostEvent(string eventName, GameObject targetObject)
	{
		foreach (var e in AudioNodesManager.instance.eventNodes)
		{
			e.PostEvent(eventName, targetObject);
		}
	}

	//ID
	public static void PostEvent(int uniqueID, GameObject targetObject)
	{
		foreach (var e in AudioNodesManager.instance.eventNodes)
		{
			e.PostEvent(uniqueID, targetObject);
		}
	}

	public static void SetSwitch(int uniqueID)
	{
		var s = AudioNodesManager.instance.switchStates;
		s.FirstOrDefault(ss => ss.uniqueID == uniqueID).transform.parent.GetComponent<SwitchGroup>().currentSwitchStateID = uniqueID;
	}
	







}

[System.Serializable]
public class AudioNodeEventPreview
{

}


//TODO: Make a custom Inspector for this one.
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AudioNodeEventPreview))]
public class AudioNodeEventPreviewProperty : PropertyDrawer
{

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		var events = GameObject.FindObjectsOfType<EventNode>().ToList();
		EditorGUILayout.LabelField("Preview Events:");

		foreach (var e in events)
		{
			if (!e.exposeForPreview)
				continue;
			if (GUILayout.Button(e.name))
			{
				e.PostEvent(e.uniqueID);
			}
		}

	}

}

#endif

