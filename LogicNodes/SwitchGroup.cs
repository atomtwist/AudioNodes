using UnityEngine;
using System.Collections;

public class SwitchGroup : Node {

	[HideInInspector]
	public int defaultSwitchStateID;
	[HideInInspector]
	public string defaultSwitchStateName;
	[HideInInspector]
	public int currentSwitchStateID;

	public override void OnEnable ()
	{
		base.OnEnable ();

	}

}
