using UnityEngine;
using System.Collections;
using System.Linq;

public class SwitchState : Node {



	public override void Reset ()
	{
		base.Reset ();
		if (transform.parent == null)
			return;
		var parentSwitchGroup = transform.parent.GetComponent<SwitchGroup>();
		if (parentSwitchGroup == null)
			return;
		parentSwitchGroup.switchStates.Clear();
		parentSwitchGroup.switchStates = parentSwitchGroup.GetComponentsInChildren<SwitchState>().ToList();
	}	
}
