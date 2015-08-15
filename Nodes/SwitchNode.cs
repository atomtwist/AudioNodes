using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SwitchNode : EventNode {



	public override void OnEnable ()
	{
		base.OnEnable ();
		GetChildEvents();
	}

	public override void PostEvent (int uniqueEventID)
	{
		if (uniqueEventID != this.uniqueID) 
			return;
		foreach (var eventNode in childEvents) {
			if (eventNode.eventAction == null) continue;
			foreach (var e in eventNode.eventAction)
			{
				if (eventNode.switchGroupGameObject == null) return;
				if(eventNode.defaultSwitchStateID == eventNode.switchGroupGameObject.GetComponent<SwitchGroup>().currentSwitchStateID)
				{
					e.ExecuteEventAction(AudioNodesManager.instance.gameObject);
				}
			}
		}
	}

	public override void PostEvent (int uniqueEventID, GameObject targetGameObject)
	{
		if (uniqueEventID != this.uniqueID) 
			return;
		foreach (var eventNode in childEvents) {
			if (eventNode.eventAction == null) continue;
			foreach (var e in eventNode.eventAction)
			{
				if (eventNode.switchGroupGameObject == null) return;
				if(eventNode.defaultSwitchStateID == eventNode.switchGroupGameObject.GetComponent<SwitchGroup>().currentSwitchStateID)
				{
					e.ExecuteEventAction(targetGameObject);
				}
			}
		}
	}

	public override void AuditionEvent ()
	{
		foreach (var eventNode in childEvents) {
			if (eventNode.eventAction == null) continue;
			foreach (var e in eventNode.eventAction)
			{
				if (eventNode.switchGroupGameObject == null) return;
				if(eventNode.defaultSwitchStateID == eventNode.switchGroupGameObject.GetComponent<SwitchGroup>().currentSwitchStateID)
				{
					e.ExecuteEventAction(eventNode.gameObject);
				}
			}
		}
	}

	public override void StopAudition ()
	{
		foreach (var eventNode in childEvents) {
			foreach (var e in eventNode.eventAction) {
				foreach (var p in e.playingNodes) {
					if (p == null)
						continue;
					var d = p.GetComponent<AudioNode>() as IPlayable;
					d.Stop();
				}
			}
		}
	}

	List<EventNode> childEvents = new List<EventNode>();
	public void GetChildEvents()
	{
		childEvents.Clear();
		foreach (Transform t in transform)
		{
			var eventNode = t.GetComponent<EventNode>();
			childEvents.Add(eventNode);
		}
	}

}
