using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaae;
using Atomtwist.AudioNodes;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Atomtwist;

public class EmitterNode : AudioNode, IPlayable {

	[System.Serializable]
	public class RepeatSettings
	{
		[Range(0,1)]
		public float repeatRate;
		public float startValue = 0;
		public float endValue = 5;
		public float repeatTime;
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(RepeatSettings))]
	public class RepeatSettingsDrawer : PropertyDrawer
	{
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUILayout.PropertyField(property.FindPropertyRelative("startValue"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("endValue"));
			EditorGUILayout.PropertyField (property.FindPropertyRelative("repeatRate"));
			property.FindPropertyRelative("repeatTime").floatValue = property.FindPropertyRelative("repeatRate").floatValue.ATScaleRange(0,1,property.FindPropertyRelative("startValue").floatValue,property.FindPropertyRelative("endValue").floatValue);
			EditorGUILayout.PropertyField (property.FindPropertyRelative("repeatTime"));
		}
	}
#endif

	[SerializeField]
	public RepeatSettings repeatSettings;

	//bool isPlaying;
	#region IPlayable implementation
	public void Play (float volume, float pitch, float pan, float delay)
	{
		if (childNode == null) return;
		//var f = childNode as AudioNode;
		isPlaying = true;
		finishedRun = false;
		startTime = AudioSettings.dspTime;
		childNode.Play(volume * nodevolume, pitch * nodePitch, pan + nodePan, delay + nodeDelay);
	}
	
	
	public void Stop ()
	{
		if (childNode == null) return;
		isPlaying = false;
		childNode.Stop();
	}
	#endregion

	double startTime;
	bool finishedRun;
	void InvokeRepeater()
	{
		if (AudioSettings.dspTime > startTime + repeatSettings.repeatTime)
		{
			if (isPlaying) finishedRun = false; else return;
			if (!finishedRun) 
			{
				//Debug.Log ("Finished");
				Play(1,1,0,0);
			}
			finishedRun = true;
		}
		if (!finishedRun)
		{
			//do stuff in here
		}
	}
	

	public IPlayable childNode;
	
	// Use this for initialization
	public override void OnEnable () {
		base.OnEnable();
		//find all playable child nodes and add them to the list
		if (transform.childCount > 0)
			childNode = transform.GetChild(0).GetComponent<IPlayable>();
#if UNITY_EDITOR
		if (!Application.isPlaying) 
		{
			isPlaying = false;
			UnityEditor.EditorApplication.update += InvokeRepeater;
		}
#endif
	}

	void Update()
	{
		InvokeRepeater();
	}
	

}
