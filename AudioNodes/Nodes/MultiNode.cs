using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaae;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class MultiNode : AudioNode, IPlayable, IPlaybackSettings {

	#region IPlaybackSettings implementation

	public void ApplyPlaybackSettings (float volume, float pitch, float pan, float delay)
	{
		if (playableNodes.Count == 0 || playableNodes == null) return;
		foreach (var i in playableNodes)
		{
			var s = i as IPlaybackSettings;
			s.ApplyPlaybackSettings(volume * nodevolume, pitch * nodePitch, pan + nodePan, delay + nodeDelay);
		}
	}

	#endregion

	
	#region IPlayable implementation
	public virtual void Play (float volume, float pitch, float pan, float delay)
	{
		if (playableNodes.Count == 0 || playableNodes == null) return;
		foreach (var i in playableNodes)
		{
			i.Play(volume * nodevolume, pitch * nodePitch, pan + nodePan, delay + nodeDelay);
		}
	}

	[DebugButton]
	public virtual void Stop ()
	{
		if (playableNodes.Count == 0 || playableNodes == null) return;
		foreach (var i in playableNodes)
		{
			i.Stop();
		}
	}
	#endregion
	
	public List<IPlayable> playableNodes = new List<IPlayable>();

	// Use this for initialization
	public override void OnEnable () {
		base.OnEnable();
		//find all playable child nodes and add them to the list
		GetChildNodes();
	}

	public void GetChildNodes()
	{
		playableNodes.Clear();
		playableNodes = transform.GetChildren().Select(c => c.GetComponent<IPlayable>()).ToList();
	}

}
