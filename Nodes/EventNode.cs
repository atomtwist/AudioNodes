using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;
using UnityEngine.EventSystems;


public class EventNode : Node {

	//tagging system for previewing
	[HideInInspector]
	public string tag;

	public override void OnEnable ()
	{
		base.OnEnable ();
		if (transform.parent != null)
			tag = transform.parent.name;
	}

	public override void OnValidate ()
	{
		base.OnValidate ();
		tag = transform.parent.name;
	}

	[SerializeField]
	[HideInInspector]
	public List<EventAction> eventAction;
	public bool exposeForPreview = true;
		
	public void PostEvent(int uniqueEventID)
	{
		if (uniqueID != uniqueEventID) return;
		foreach (var e in eventAction)
		{
			e.ExecuteEventAction(FindObjectOfType<AudioNodesManager>().gameObject);
		}
	}
	

	public void PostEvent(string eventName)
	{
		if (eventName != name) return;
		foreach (var e in eventAction)
		{
			e.ExecuteEventAction(FindObjectOfType<AudioNodesManager>().gameObject);
		}
	}
	
	public void PostEvent(int uniqueEventID, GameObject targetGameObject)
	{
		if (uniqueID != uniqueEventID) return;
		//this might not be very fast TODO: Find a static gameobject to play from
		if (targetGameObject == null)
		{
			targetGameObject = AudioNodesManager.instance.gameObject;
		}
		foreach (var e in eventAction)
		{
			e.ExecuteEventAction(targetGameObject);
		}
	}
	

	public void PostEvent(string eventName, GameObject targetGameObject)
	{
		if (eventName != name) return;
		//this might not be very fast TODO: Find a static gameobject to play from
		if (targetGameObject == null)
			targetGameObject = AudioNodesManager.instance.gameObject;
		foreach (var e in eventAction)
		{
			e.ExecuteEventAction(targetGameObject);
		}
	}
	
	public void AuditionEvent()
	{
		if (eventAction == null) return;
		foreach (var e in eventAction)
		{
			e.ExecuteEventAction(gameObject);
/*			foreach (var n in e.playingNodes) {
			}*/
		}
	}
	
	public void StopAudition()
	{
		foreach (var e in eventAction) {
			foreach (var p in e.playingNodes) {
				if (p == null)
					continue;
				var d = p.GetComponent<AudioNode>() as IPlayable;
				d.Stop();
			}
		}
	}

}
