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
	void OnEnable()
	{
		eventNodes = GameObject.FindObjectsOfType<EventNode>().ToList();
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
	

	public static void SetSwitch(string switchStateName)
	{
		var switchNodes = GameObject.FindObjectsOfType<SwitchNode>();
		foreach (var n in switchNodes)
		{
			ExecuteEvents.Execute<ISwitchable>(n.gameObject,null, (x,y) => x.SetSwitch(switchStateName) );
		}
	}



  //contextMenu
#if UNITY_EDITOR 
	//TODO: Make Convert to Context Menu actions

	[MenuItem("GameObject/AudioNodes/AUDIO/Create SFX Node", false, 0)]
	public static void CreateSFXNode()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		newNode.AddComponent<SFXNode>();
		newNode.GetComponent<AudioSource>().playOnAwake = false;
		newNode.transform.SetParent(selectedObject.transform);
	}

	[MenuItem("GameObject/AudioNodes/VR/Create VR SFX Node", false, 0)]
	public static void CreateSFXNodeVR()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		newNode.AddComponent<SFXNodeVR>();
		newNode.GetComponent<AudioSource>().playOnAwake = false;
		newNode.transform.SetParent(selectedObject.transform);
	}

	[MenuItem("GameObject/AudioNodes/VR/Create Ambience Rig Node", false, 0)]
	public static void CreateAmbienceRigVR()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		newNode.AddComponent<AmbienceMultiNode>();
		newNode.transform.SetParent(selectedObject.transform);
	}

	[MenuItem("GameObject/AudioNodes/AUDIO/Create Multi Node", false, 0)]
	public static void CreateMultiNode()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		newNode.AddComponent<MultiNode>();
		newNode.transform.SetParent(selectedObject.transform);
	}

	//parent
	static GameObject newNode;
	[MenuItem("GameObject/AudioNodes/PARENT/Create Multi Node", false, 0)]
	public static void CreateParentMultiNode(MenuCommand c)
	{
		var selectedObject = Selection.activeObject as GameObject;
		var selectedTransforms = Selection.GetTransforms(SelectionMode.TopLevel) ;
		Debug.Log (selectedTransforms[0]);
		if (selectedTransforms[0] != null && selectedTransforms[0].name == c.context.name)
		{
			Debug.Log ("doing");
			newNode = new GameObject();
			newNode.AddComponent<MultiNode>();
			newNode.transform.SetParent(selectedObject.transform.parent);
			//maybe remove this
			var audioNode = newNode.GetComponent<AudioNode>();
			audioNode.exposeToEventnodes = true;
		}
		foreach (var s in selectedTransforms) {
			if (newNode == null)
				continue;
			s.parent = newNode.transform;
			s.GetComponent<AudioNode>().exposeToEventnodes = false;
		}
	}

	[MenuItem("GameObject/AudioNodes/AUDIO/Create Random Node", false, 0)]
	public static void CreateRandomNode()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		newNode.AddComponent<RandomNode>();
		newNode.transform.SetParent(selectedObject.transform);
	}

	[MenuItem("GameObject/AudioNodes/AUDIO/Create Switch Node", false , 0)]
	public static void CreateSwitchNode()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		newNode.AddComponent<SwitchNode>();
		newNode.transform.SetParent(selectedObject.transform);
	}

	[MenuItem("GameObject/AudioNodes/EVENT/Create Event Node", false, 0)]
	public static void CreateEventNode()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		newNode.AddComponent<EventNode>();
		newNode.transform.SetParent(selectedObject.transform);
	}

	[MenuItem("GameObject/AudioNodes/LOGIC/Create Switch State", false, 5)]
	public static void CreateSwitchState()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		newNode.AddComponent<SwitchState>();
		newNode.transform.SetParent(selectedObject.transform);
	}

	//TODO: make this create event from AudioNodes & put it into the right place in hierarchy
	[MenuItem("GameObject/AudioNodes/AUDIO/Create Emitter Node", false, 0)]
	public static void CreateEmitterNode()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		newNode.AddComponent<EmitterNode>();
		newNode.transform.SetParent(selectedObject.transform);
	}

#endif
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

