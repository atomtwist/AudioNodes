using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaae;

public class AmbienceMultiNode : AudioNode,IPlayable {

	public Transform followTarget;
	public float radius;
	[Space(12)]
	[Header("3D Settings")]
	public float maxDistance = 500;
	public AudioRolloffMode rollOffMode;
	public float yOffset;

	public override void OnValidate ()
	{
		base.OnValidate ();
		if (radius <= 0) radius = 1;
		GetChildNodes();
		foreach (var aNode in audioNodes) {
			var source = aNode.GetComponent<AudioSource>();
			source.rolloffMode = rollOffMode;
			source.maxDistance = maxDistance;
		}
	}

	Transform _listener;
	public override void OnEnable ()
	{
		base.OnEnable ();
		_listener = FindObjectOfType<AudioListener>().gameObject.transform;
		transform.position = Vector3.zero;
		GetChildNodes();
		foreach (var aNode in audioNodes) {
			var source = aNode.GetComponent<AudioSource>();
			source.rolloffMode = rollOffMode;
			source.maxDistance = maxDistance;
		}
	}

	void GetRandomSpherePosRecursive(ref Vector3 position)
	{
		var rndSpherePosition = Random.onUnitSphere * radius;
		if (rndSpherePosition.y < 0 ) GetRandomSpherePosRecursive(ref position);
		position = rndSpherePosition;

	}

	#region IPlayable implementation

	public void Play (float volume, float pitch, float pan, float delay)
	{
		isPlaying = true;
		//if (audioNodes.Count == 0 || audioNodes == null) return;
		transform.position = _listener.position;
		foreach (var an in audioNodes)
		{
			Vector3 rndPos = Vector3.zero;
			GetRandomSpherePosRecursive(ref rndPos);
			an.transform.localPosition = rndPos;
			var p = an as IPlayable;
			p.Play(volume * nodevolume, pitch * nodePitch, pan + nodePan, delay + nodeDelay);
		}
	}
	
	public void Stop ()
	{
		isPlaying = false;
		if (audioNodes.Count == 0 || audioNodes == null) return;
		foreach (var an in audioNodes)
		{
			var p = an as IPlayable;
			p.Stop();
		}
		/*transform.position = Vector3.zero;
		foreach (var an in audioNodes)
		{
			an.transform.localPosition = Vector3.zero;
		}*/
	}

	#endregion

	List<AudioNode> audioNodes = new List<AudioNode>();

	public void GetChildNodes()
	{
		audioNodes.Clear();
		audioNodes = transform.GetChildren().Select(c => c.GetComponent<AudioNode>()).ToList();
	}

	void Update()
	{
		if (!isPlaying) return;
		transform.position = _listener.position;
		transform.position = new Vector3(transform.position.x,transform.position.y + yOffset,transform.position.z);
	}


}
