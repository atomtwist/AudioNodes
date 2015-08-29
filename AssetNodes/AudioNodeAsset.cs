using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEditor;
using PH.DataTree;
using Kaae;
using System.Linq;

public enum NodeType
{
	AudioNodes,
	EventNodes,
	LogicNodes,
}

public enum NodeSubType
{
	SFXNode,
	MultiNode,
	RandomNode,
	SequenceNode,
	Folder,
	WorkSpace,
	SwitchGroup,
	SwitchState,
	SwitchNode,
}

[System.Serializable]
public struct MixerGroupProperties
{
	public AudioMixerGroup mixerGroup;
	public bool overrideParentMixerGroup;
}

[System.Serializable]
public class PlaybackSettings
{
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

[System.Serializable]
public class AudioNodeData
{
	public Texture2D icon;
	public string ChildId;
	[SerializeField]
	private List<string> _childrenIds = new List<string>();
	[System.NonSerialized]
	public List<AudioNodeData> Children = new List<AudioNodeData>();

	public void Initialize() {
		ChildId =  System.Guid.NewGuid().ToString();
	}

	public void PopulateChildren(List<AudioNodeData> allnodes) {
		if(!_childrenIds.Any())
			return;
		Children = allnodes.Where (n => _childrenIds.Any (c => c == n.ChildId)).ToList();
	}

	public void AddChild(AudioNodeData d) {
		_childrenIds.Add (d.ChildId);
		Children.Add (d);
	}

	public void DeleteYourselfAndYourFuckingFamily(List<AudioNodeData> allnodes) {
		foreach(var c in Children) {
			c.DeleteYourselfAndYourFuckingFamily(allnodes);
		}
		allnodes.Remove (this);
		_childrenIds.Clear();
		Children.Clear();
	}

	public bool RemoveChild(string id, List<AudioNodeData> allnodes) {
		if(_childrenIds.Contains(id)) {
			_childrenIds.Remove(id);
			var c = Children.FirstOrDefault(cc => cc.ChildId == id);
			Children.Remove (c);
			c.DeleteYourselfAndYourFuckingFamily(allnodes);

			return true;
		}

		foreach(var c in Children) {
			if(c.RemoveChild(c.ChildId, allnodes)) {
				return true;
			}
		}

		return false;
	}

	public bool foldOut;
	//Node
	public NodeType nodeType;
	public NodeSubType nodeSubType;
	public string name;
	//[SerializeField]
	//AudioNode
	public MixerGroupProperties mixerGroupProperties;
	public PlaybackSettings playbackSettings;
	//SFXNode
	public AudioClip clip;
	public bool randomSampleOffset;
	[Range(0,15)]
	public float fadeIn;
	[Range(0,15)]
	public float fadeOut;
	public bool loop;
}

[System.Serializable]
public class AudioNodeAsset : ScriptableObject, ISerializationCallbackReceiver {
	//[SerializeField]
	//public DTreeNode root;
	[SerializeField]
	public List<AudioNodeData> AllTheNodes = new List<AudioNodeData>();
	[SerializeField]
	public List<AudioNodeData> Roots = new List<AudioNodeData>();
	

	public void AddChildNode()
	{
		var n = new AudioNodeData();
		n.Initialize();
		AllTheNodes.Add (n);
	}

	public void NewRootNode(NodeType t) {
		var n = new AudioNodeData();
		n.Initialize();
		n.nodeType = t;
		Roots.Add(n);
	}

	public void PopulateNodes() {
		AllTheNodes.ForEach(n => n.PopulateChildren(AllTheNodes));
		Roots.ForEach(n => n.PopulateChildren(AllTheNodes));
	}

	public void Remove(string id) {
		AllTheNodes.Remove(AllTheNodes.FirstOrDefault(n => n.ChildId == id));
		foreach(var root in Roots)
			if(root.RemoveChild(id, AllTheNodes))
				break;
		PopulateNodes();
	}

	//TODO: make methods that return Nodes & stuff

	#region ISerializationCallbackReceiver implementation

	public void OnBeforeSerialize ()
	{
	}

	public void OnAfterDeserialize ()
	{
		PopulateNodes();
	}

	#endregion
}

[CustomEditor(typeof(AudioNodeAsset))]
public class AudioAssetInspector : Editor
{
	AudioNodeAsset a;

	List<AudioNodeData> _addChildren = new List<AudioNodeData>();

	void OnEnable()
	{
		a = target as AudioNodeAsset;
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();
		if (GUILayout.Button("pop"))
		{
			a.AllTheNodes.ForEach(n => n.PopulateChildren(a.AllTheNodes));
			a.Roots.ForEach(n => n.PopulateChildren(a.AllTheNodes));
		}
		if(GUILayout.Button("new root")) {
			a.NewRootNode(NodeType.AudioNodes);
		}

		foreach(var child in a.Roots) {
			EditorGUI.indentLevel = 1;
			DrawChild(child, 2);
		}

		a.AllTheNodes.AddRange(_addChildren);
		_addChildren.Clear();
	}

	void DrawChild(AudioNodeData child, int indent) {
		EditorGUI.indentLevel = indent;
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button(child.ChildId, "label")) {
			Debug.Log ("Basd");
		}
		if(GUILayout.Button("Make child")) {
			var n = new AudioNodeData();
			n.Initialize();
			child.AddChild(n);
			_addChildren.Add (n);
		}
		EditorGUILayout.EndHorizontal();
		foreach(var c in child.Children) {
			DrawChild(c, indent+1);
		}
	}
}