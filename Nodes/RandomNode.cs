using UnityEngine;
using System.Collections;
using Kaae;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;

public class RandomNode : AudioNode, IPlayable, IPlaybackSettings {

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
	public void Play (float volume, float pitch, float pan, float delay)
	{
		if (playableNodes.Count == 0 || playableNodes == null) return;
		var r = Random.Range(0,playableNodes.Count);
		//apply mixeroverride
		var aNodes = playableNodes.Cast<AudioNode>().ToList();
		foreach (var n in aNodes)
		{
			if (!n.mixerGroupProperties.overrideParentMixerGroup)
				n.mixerGroupProperties.mixerGroup = mixerGroupProperties.mixerGroup;
		}
		playableNodes[r].Play(volume * nodevolume, pitch * nodePitch, pan + nodePan, delay + nodeDelay);
	}
	

	public void Stop ()
	{
		if (playableNodes.Count == 0 || playableNodes == null) return;
		foreach (var i in playableNodes)
		{
			i.Stop();
			var a = i as AudioNode;
			a.isPlaying = false;
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
