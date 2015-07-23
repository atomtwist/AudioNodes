using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public interface ISwitchable : IEventSystemHandler {
	void SetSwitch(string switchStateName);
}

