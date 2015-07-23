using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaae;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

[System.Serializable]
public enum AudioNodePlayMode 
{
	Step,
	Continuous,
}
public class SwitchNode : AudioNode, IPlayable, ISwitchable {


	[HideInInspector]
	public AudioNodePlayMode switchMode;
	[HideInInspector]
	public int selectedSwitchStateID;
	[HideInInspector]
	public GameObject selectedSwitchStateGameObject;
	[HideInInspector]
	public List<GameObject> unselectedStateObjects;
	IPlayable selectedStatePlayer;


	SwitchState GetSwitchState(string switchStateName)
	{
		SwitchState switchState = null;
		//empty list of unselected SwitchStates
		unselectedStateObjects.Clear();
		foreach (Transform t in transform)
		{
			//get the chosen switchstate
			if (t.gameObject.name == switchStateName )
			{
				switchState = t.GetComponent<SwitchState>();
			}
			//put all other unchosen switchstates in a list, so we can stop them
			if (t.gameObject.name != switchStateName )
			{
				unselectedStateObjects.Add (t.gameObject);
			}
		}
		return switchState;
	}


	#region ISwitchable implementation
	public void SetSwitch (string switchStateName)
	{
		var switchState = GetSwitchState(switchStateName);
		if (switchState == null) 
		{
			#if AUDIO_LOGGING
			Debug.LogError("SwitchState " + switchStateName + " doesn't exist.");
			#endif
			return;
		}
		if (selectedSwitchStateGameObject == switchState.gameObject && selectedSwitchStateID == switchState.uniqueID)
			return;
		if (switchMode == AudioNodePlayMode.Step)
		{
			selectedSwitchStateGameObject = switchState.gameObject;
			selectedSwitchStateID = switchState.uniqueID;
		}
		if (switchMode == AudioNodePlayMode.Continuous)
		{
			selectedSwitchStateGameObject = switchState.gameObject;
			selectedSwitchStateID = switchState.uniqueID;
			if (gameObject.GetComponent<DestroyAfterPlay>() != null)
			{
				Play(1,1,1,0);
			}
		}
	}
	#endregion


	#region IPlayable implementation
	List<SwitchState> unselectedSwitchStates;
	public void Play (float volume, float pitch, float pan, float delay)
	{
		selectedStatePlayer = selectedSwitchStateGameObject.GetComponent<SwitchState>() as IPlayable;
		if (selectedSwitchStateGameObject == null)
			return;
		selectedStatePlayer.Play(volume * nodevolume, pitch * nodePitch, pan + nodePan, delay + nodeDelay);
		unselectedSwitchStates = unselectedStateObjects.Select(u => u.GetComponent<SwitchState>()).ToList();
		foreach (var s in unselectedSwitchStates)
		{
			var p = s as IPlayable;
			p.Stop();
		}
	}

	[DebugButton]
	public void Stop ()
	{
		foreach (var s in unselectedSwitchStates)
		{
			s.Stop();
		}
		selectedStatePlayer.Stop();
	}
	#endregion
}
