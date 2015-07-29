using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// makes sure that the static constructor is always called in the editor.
[InitializeOnLoad]
public class EditorMenus : Editor
{
	#region DragAndDropAudioClips
	static EditorMenus ()
	{
		// Adds a callback for when the hierarchy window processes GUI events
		// for every GameObject in the heirarchy.
		EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemCallback;
	}

	static int _hoverID;

	static void HierarchyWindowItemCallback(int pID, Rect pRect)
	{
		if (Event.current.type == EventType.DragUpdated)
		{
			if (pRect.Contains(Event.current.mousePosition)) 
			{
				_hoverID = pID;

			}
		}


		// happens when an acceptable item is released over the GUI window
		if (Event.current.type == EventType.DragPerform)
		{

			// get all the drag and drop information ready for processing.
			DragAndDrop.AcceptDrag();

			
			// used to emulate selection of new objects.
			var selectedObjects = new List<GameObject>();
			
			// run through each object that was dragged in.
			foreach (var objectRef in DragAndDrop.objectReferences)
			{
				// if the object is the particular asset type...
				if (objectRef is AudioClip)
				{
					// we create a new GameObject using the asset's name.
					var gameObject = new GameObject(objectRef.name);
					var dragTarget = EditorUtility.InstanceIDToObject(_hoverID) as GameObject;
					gameObject.transform.SetParent(dragTarget.transform);
					
					// we attach component X, associated with asset X.
					var sfxNode = gameObject.AddComponent<SFXNode>();
					
					// we place asset X within component X.
					sfxNode.clip = objectRef as AudioClip;

					sfxNode.name = objectRef.name;
					
					// add to the list of selected objects.
					selectedObjects.Add(gameObject);
				}
			}
			
			// we didn't drag any assets of type AssetX, so do nothing.
			if (selectedObjects.Count == 0) return;
			
			// emulate selection of newly created objects.
			Selection.objects = selectedObjects.ToArray();
			
			// make sure this call is the only one that processes the event.
			Event.current.Use();
		}
		#endregion
	}

	#region contextMenu
	//contextMenu
	//TODO: Make Convert to Context Menu actions
	
	[MenuItem("GameObject/AudioNodes/AUDIO/Create SFX Node %#s", false, 0)]
	public static void CreateSFXNode()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		var audioNode = newNode.AddComponent<SFXNode>();
		newNode.GetComponent<AudioSource>().playOnAwake = false;
		newNode.transform.SetParent(selectedObject.transform);
		newNode.name = "SFX Node";
		audioNode.name = "SFX Node";
		Selection.activeObject = newNode;
	}
	
	[MenuItem("GameObject/AudioNodes/VR/Create VR SFX Node %#v", false, 0)]
	public static void CreateSFXNodeVR()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		var audioNode = newNode.AddComponent<SFXNodeVR>();
		newNode.GetComponent<AudioSource>().playOnAwake = false;
		newNode.transform.SetParent(selectedObject.transform);
		newNode.name = "VR SFX Node";
		audioNode.name = "VR SFX Node";
		Selection.activeObject = newNode;
	}
	
	[MenuItem("GameObject/AudioNodes/VR/Create Ambience Rig Node %#a", false, 0)]
	public static void CreateAmbienceRigVR()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		var audioNode = newNode.AddComponent<AmbienceMultiNode>();
		newNode.transform.SetParent(selectedObject.transform);
		newNode.name = "Ambience Rig";
		audioNode.name = "Ambience Rig";
		Selection.activeObject = newNode;
	}
	
	[MenuItem("GameObject/AudioNodes/AUDIO/Create Multi Node %#m", false, 0)]
	public static void CreateMultiNode()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		var audioNode = newNode.AddComponent<MultiNode>();
		newNode.transform.SetParent(selectedObject.transform);
		newNode.name = "Multi Node";
		audioNode.name = "Multi Node";
		Selection.activeObject = newNode;
	}
	
	[MenuItem("GameObject/AudioNodes/AUDIO/Create Random Node %#r", false, 0)]
	public static void CreateRandomNode()
	{
		var selectedObject = Selection.activeObject as GameObject;
		var newNode = new GameObject();
		newNode.AddComponent<RandomNode>();
		newNode.transform.SetParent(selectedObject.transform);
		newNode.name = "Random Node";
		newNode.gameObject.name = "Random Node";
		Selection.activeObject = newNode;
	}
	
	
	[MenuItem("GameObject/AudioNodes/EVENT/Create Event Node %#e", false, 0)]
	public static void CreateEventNode()
	{
		var selectedObjects = Selection.gameObjects;
		foreach (var selectedObject in selectedObjects )
		{
			if (selectedObject.GetComponent<AudioNode>() != null)
			{
				var eventParent = GameObject.Find("EVENTNODES");
				var newNode = new GameObject();
				var eventNode = newNode.AddComponent<EventNode>();
				EventAction eventAction = new EventAction();
				eventAction.actionType = EventActionType.Play;
				eventAction.uniqueAudioNodeID = selectedObject.GetComponent<AudioNode>().uniqueID;
				eventAction.followGameObject = true;
				eventNode.eventAction = new List<EventAction>();
				eventNode.eventAction.Add(eventAction);
				newNode.transform.SetParent(eventParent.transform);
				eventNode.tag = eventParent.transform.name;
				newNode.transform.localPosition = Vector3.zero;
				newNode.name = selectedObject.name;
				eventNode.name = selectedObject.name;
				Selection.activeObject = newNode;
			}
			else
			{
				var newNode = new GameObject();
				var audioNode = newNode.AddComponent<EventNode>();
				newNode.transform.SetParent(selectedObject.transform);
				newNode.name = "Event Node";
				audioNode.name = "Event Node";
				Selection.activeObject = newNode;
			}
		}
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

	[MenuItem("GameObject/AudioNodes/PARENT/Create Random Node", false, 0)]
	public static void CreateParentRandomNode(MenuCommand c)
	{
		var selectedObject = Selection.activeObject as GameObject;
		var selectedTransforms = Selection.GetTransforms(SelectionMode.TopLevel) ;
		Debug.Log (selectedTransforms[0]);
		if (selectedTransforms[0] != null && selectedTransforms[0].name == c.context.name)
		{
			newNode = new GameObject();
			newNode.AddComponent<RandomNode>();
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



	
	//project view
	[MenuItem("Assets/AudioNodes/Create SFX Node")]
	public static void AudioClipToSFXNode()
	{
		var file = Selection.activeObject;
		if (file is AudioClip)
		{
			var audioNodeParent = GameObject.Find ("AUDIONODES");
		}
	}
	#endregion

}
