﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine.Audio;
using System.Collections.Generic;


public enum EventActionType
{
	Play, 
	Stop,
	StopAll,
	StopMixerGroup,
	TriggerMixerSnapshot,
	SetVolume,
	SetPitch,
	SetPan,
	TriggerEvent,
	SetSwitch,
}

public enum EventActionScope
{
	Gameobject,
	Global,
}

[System.Serializable]
public class EventAction
{
	public int uniqueAudioNodeID;
	public EventActionType actionType;
	public GameObject selectedNodeGameObject;
	public EventActionScope actionScope;
	public AudioMixerGroup mixerGroup;
	public AudioMixerSnapshot mixerSnapshot;
	public float mixerSnapshotTransitionTime;
	public GameObject eventTarget;
    public string switchStateName;
	public int switchStateID;
	public List<GameObject> playingNodes;
	public float voicePitch = 1;
	public float voiceVolume = 1;
	public float voicePan = 0;
	public bool bypassEvent ;


	//TODO: Need an applySettings Method here to apply settings as eventactions before play
	
	public void ExecuteEventAction(GameObject targetGameObject)
	{
		playingNodes.Clear();
		GameObject nodeObject;
		switch (actionType)
		{
		case EventActionType.Play : 
			if (bypassEvent) return;
			//create instance of audioEvent
			nodeObject = GameObject.Instantiate<GameObject>(selectedNodeGameObject);
			//TODO: make eventInterface to update the settings
			nodeObject.GetComponent<AudioNode>().originAudioNodeID = uniqueAudioNodeID;
			nodeObject.AddComponent<DestroyAfterPlay>();
			nodeObject.transform.SetParent(targetGameObject.transform);
			nodeObject.transform.localPosition = Vector3.zero;
			ExecuteEvents.Execute<IPlayable>(nodeObject,null, (x,y) => x.Play(1, 1,0, 0) );
			//keep track of what gets played
			if (nodeObject != null)
				playingNodes.Add (nodeObject);
			break;
		case EventActionType.Stop  :
			if (bypassEvent) return;
			if (actionScope == EventActionScope.Gameobject)
			{
				var nodeObjects = targetGameObject.GetComponentsInChildren<AudioNode>();
				foreach (var n in nodeObjects)
				{
					if (n.originAudioNodeID == uniqueAudioNodeID)
					{
						ExecuteEvents.Execute<IPlayable>(n.gameObject,null, (x,y) => x.Stop() );
					}
					
				}
			}
			if (actionScope == EventActionScope.Global)
			{
				var nodeObjects = GameObject.FindObjectsOfType<AudioNode>();
				foreach (var n in nodeObjects)
				{
					if (n.originAudioNodeID == uniqueAudioNodeID)
						ExecuteEvents.Execute<IPlayable>(n.gameObject,null, (x,y) => x.Stop() );
				}
			}
			break;
		case EventActionType.StopAll  :
			if (bypassEvent) return;
			if (actionScope == EventActionScope.Gameobject)
			{
				var nodeObjects = targetGameObject.GetComponentsInChildren<AudioNode>();
				foreach (var n in nodeObjects)
				{
					ExecuteEvents.Execute<IPlayable>(n.gameObject,null, (x,y) => x.Stop() );
				}
			}
			if (actionScope == EventActionScope.Global)
			{
				var nodeObjects = GameObject.FindObjectsOfType<AudioNode>();
				foreach (var n in nodeObjects)
				{
					ExecuteEvents.Execute<IPlayable>(n.gameObject,null, (x,y) => x.Stop() );
				}
			}
			break;
		case EventActionType.StopMixerGroup  :
			if (bypassEvent) return;
			if (actionScope == EventActionScope.Gameobject)
			{
				//find all childgroups
				var mixerGroups = mixerGroup.audioMixer.FindMatchingGroups(mixerGroup.name).ToList();
				mixerGroups.Add(mixerGroup);
				var nodeObjects = targetGameObject.GetComponentsInChildren<AudioNode>();
				//find all audionodes that play on this mixergroup
				var relevant = nodeObjects.Where(node => mixerGroups.Contains(node.mixerGroupProperties.mixerGroup)).ToList();


				foreach (var n in relevant)
				{
					ExecuteEvents.Execute<IPlayable>(n.gameObject,null, (x,y) => x.Stop() );
				}
			}
			if (actionScope == EventActionScope.Global)
			{
				//find all childgroups
				var mixerGroups = mixerGroup.audioMixer.FindMatchingGroups(mixerGroup.name).ToList();
				mixerGroups.Add(mixerGroup);
				var nodeObjects = GameObject.FindObjectsOfType<AudioNode>();
				 //find all audionodes that play on this mixergroup
				var relevant = nodeObjects.Where(node => mixerGroups.Contains(node.mixerGroupProperties.mixerGroup)).ToList();
				foreach (var n in relevant)
				{
					ExecuteEvents.Execute<IPlayable>(n.gameObject,null, (x,y) => x.Stop() );
				}
			}
			break;
		case EventActionType.TriggerMixerSnapshot :
			if (bypassEvent) return;
			mixerSnapshot.TransitionTo(mixerSnapshotTransitionTime);
			break;
		case EventActionType.TriggerEvent :
			if (bypassEvent) return;
			var eventNode = selectedNodeGameObject.GetComponent<EventNode>();
			eventNode.PostEvent(uniqueAudioNodeID,eventTarget);
			//keep track of playing nodes
			foreach (var e in eventNode.eventAction)
			{
				foreach (var n in e.playingNodes)
				{
					if (n!=null)
						playingNodes.Add (n);
				}
			}
		break;
		case EventActionType.SetSwitch  :
			if (bypassEvent) return;
			var switchNodes = GameObject.FindObjectsOfType<SwitchNode>();
			foreach (var n in switchNodes)
			{
				ExecuteEvents.Execute<ISwitchable>(n.gameObject,null, (x,y) => x.SetSwitch(switchStateName) );
			}
			break;
		case EventActionType.SetVolume :
			if (bypassEvent) return;
			var nodeObjectss = targetGameObject.GetComponentsInChildren<AudioNode>();
			foreach (var n in nodeObjectss)
			{
				if (n.originAudioNodeID == uniqueAudioNodeID)
				{
					// TODO: each node needs to implement an IUpdateSettings interface
					//TODO: implement fade to vol/pitch & make an absolut/relative toggle
					ExecuteEvents.Execute<IPlaybackSettings>(n.gameObject,null, (x,y) => x.ApplyPlaybackSettings(voiceVolume,1,0,0) );
					
				}
				
			}
			break;
		case EventActionType.SetPitch :
			if (bypassEvent) return;
			var nodeObjects = targetGameObject.GetComponentsInChildren<AudioNode>();
			foreach (var n in nodeObjects)
			{
				if (n.originAudioNodeID == uniqueAudioNodeID)
				{
					// TODO: each node needs to implement an IUpdateSettings interface
					ExecuteEvents.Execute<IPlaybackSettings>(n.gameObject,null, (x,y) => x.ApplyPlaybackSettings(1,voicePitch,0,0) );

				}
				
			}
			break;
		case EventActionType.SetPan :
			if (bypassEvent) return;
			var nodeObjectsss = targetGameObject.GetComponentsInChildren<AudioNode>();
			foreach (var n in nodeObjectsss)
			{
				if (n.originAudioNodeID == uniqueAudioNodeID)
				{
					// TODO: each node needs to implement an IUpdateSettings interface
					ExecuteEvents.Execute<IPlaybackSettings>(n.gameObject,null, (x,y) => x.ApplyPlaybackSettings(1,1,voicePan,0) );
					
				}
				
			}
			break;
		}
	}
}



