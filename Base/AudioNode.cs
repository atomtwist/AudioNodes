using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using Kaae;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class AudioNode : Node {

	//mixer routing
	[System.Serializable]
	public struct MixerGroupProperties
	{
		public AudioMixerGroup mixerGroup;
		public bool overrideParentMixerGroup;
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(MixerGroupProperties))]
	public class MixerGroupPropertiesDrawer : PropertyDrawer
	{
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUILayout.PropertyField(property.FindPropertyRelative("mixerGroup"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("overrideParentMixerGroup"));
		}
	}
#endif

	public bool exposeToEventnodes;
	public bool mute;
	[HideInInspector]
	public int originAudioNodeID;
	[HideInInspector]
	public bool isPlaying = false;
	public MixerGroupProperties mixerGroupProperties;

	public override void OnValidate ()
	{
		base.OnValidate ();
		var audioNodes = GetComponentsInChildren<AudioNode>();
		foreach (var n in audioNodes)
		{
			if (!n.mixerGroupProperties.overrideParentMixerGroup)
				n.mixerGroupProperties.mixerGroup = mixerGroupProperties.mixerGroup;
		}
	}

	public override void OnEnable ()
	{
		base.OnEnable ();
		var audioNodes = GetComponentsInChildren<AudioNode>();
		foreach (var n in audioNodes)
		{
			if (!n.mixerGroupProperties.overrideParentMixerGroup)
				n.mixerGroupProperties.mixerGroup = mixerGroupProperties.mixerGroup;
		}
	}

	//TODO: Auditioning needs to be its owen Inteface & Push 2D Settings when triggered from Editor
	[HideInInspector]
	public List<GameObject> _previewObjects = new List<GameObject>();
	[DebugButton]
	public void Audition()
	{
		//create instance of audioEvent
		var previewObject = GameObject.Instantiate<GameObject>(gameObject);
		previewObject.AddComponent<DestroyAfterPlay>();
		previewObject.transform.SetParent(FindObjectOfType<AudioNodesManager>().transform);
		previewObject.transform.localPosition = Vector3.zero;
		var p = previewObject.GetComponent<AudioNode>() as IPlayable;
		p.Play(1, 1,0, 0);
		//keep track of what's playing
		_previewObjects.Add(previewObject);
		/*var p = this as IPlayable;
		p.Play(1, 1,0, 0);*/
	}

	[DebugButton]
	public void StopAudition()
	{
		foreach (var item in _previewObjects) {
			if (item == null)
				continue;
			var p = item.GetComponent<AudioNode>() as IPlayable;
			p.Stop();
		}
		_previewObjects.Clear();
	}




	//playback related
	[Range(0,1)]
	public float nodevolume = 1;
	[Range(-3,3)]
	public float nodePitch = 1;
	[Range(-1,1)]
	public float nodePan = 0;
	[Range(0,2)]
	public float nodeDelay;

}
