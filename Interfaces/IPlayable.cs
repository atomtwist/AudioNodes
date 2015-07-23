using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public interface IPlayable : IEventSystemHandler {
	void Play(float volume, float pitch, float pan, float delay);
	void Stop();
}
