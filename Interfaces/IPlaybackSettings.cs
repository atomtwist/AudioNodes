using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public interface IPlaybackSettings : IEventSystemHandler {

	void ApplyPlaybackSettings(float volume, float pitch, float pan, float delay);

}
