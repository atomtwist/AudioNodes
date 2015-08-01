using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class DestroyAfterPlay : MonoBehaviour {

	public bool followGameObject = true;
	List<AudioSource> audioSources;
	List<AudioNode> audioNodes;

	void OnEnable()
	{
		if (Application.isPlaying)
		{
			DontDestroyOnLoad(gameObject);
		}
		audioSources = GetComponentsInChildren<AudioSource>().ToList();
		audioNodes = GetComponentsInChildren<AudioNode>().ToList();
#if UNITY_EDITOR
		if (!Application.isPlaying)
			EditorApplication.update += EditorUpdate;
#endif
	}

	void EditorUpdate ()
	{
		DestroyIfNotPlaying();
	}

	IEnumerator WaitBeforeDestroy()
	{
		yield return new WaitForSeconds(2);
		Destroy (gameObject);
	}


//TODO: Make this an Audiosettings coroutine so itll work in editor
	void DestroyIfNotPlaying()
	{
		if (Application.isPlaying) 
		{
			if (AnySourcePlaying()) return;
			StartCoroutine(WaitBeforeDestroy());
		}
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			if (audioSources[0] == null) return;
			if (AnySourcePlaying()) return;
			EditorApplication.update -= EditorUpdate;
			DestroyImmediate(gameObject);
		}
#endif

	}

	void OnDestroy()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			EditorApplication.update -= EditorUpdate;
#endif
	}

	bool AnySourcePlaying()
	{
		if (audioSources.FirstOrDefault(s => s.isPlaying) || audioNodes.FirstOrDefault(n => n.isPlaying)) return true;
		else
			return false;
	}

	// Update is called once per frame
	void Update () {
		DestroyIfNotPlaying();

	}
}
