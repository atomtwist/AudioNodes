using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwitchGroup : Node {

	[HideInInspector]
	public int defaultSwitchStateID;
	[HideInInspector]
	public string defaultSwitchStateName;
	//[HideInInspector]
	public int currentSwitchStateID;
	[HideInInspector]
	[SerializeField]
	public List <SwitchState> switchStates;

	public override void OnEnable ()
	{
		base.OnEnable ();
		currentSwitchStateID = defaultSwitchStateID;
	}

}
