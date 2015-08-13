using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SwitchGroup))]
public class SwitchGroupInspector : Editor {

	SwitchGroup g;

	void OnEnable()
	{
		g = target as SwitchGroup;
	}

	List<SwitchState> switchStates = new List<SwitchState>();
	void DrawDefaultSwitchStatePopup()
	{
		EditorGUI.BeginChangeCheck();

		//make a list of switchstates in 1st Level children
		switchStates.Clear();

		foreach (Transform t in g.transform)
		{
			var switchState = t.GetComponent<SwitchState>() ;
			if (switchState != null)
				switchStates.Add(switchState);
		}
		//return if no audioNodes
		if (switchStates.Count == 0 || switchStates == null)
			return;
		//var audioNodeID = property.FindPropertyRelative ("uniqueAudioNodeID").intValue;
		var switchStateNames = switchStates.Select(n => n.name).ToArray();
		var switchStateIDList = switchStates.Select (n => n.uniqueID).ToList();
		var selectedIndex = EditorGUILayout.Popup( "Default Switch State ",switchStateIDList.IndexOf(g.defaultSwitchStateID), switchStateNames);
		selectedIndex = Mathf.Clamp(selectedIndex,0,int.MaxValue);
		var selectedNode = switchStates[selectedIndex];
		g.defaultSwitchStateName = switchStateNames[selectedIndex];
		//property.FindPropertyRelative("switchStateObject").objectReferenceValue = selectedNode.gameObject;
		if( EditorGUI.EndChangeCheck()) 
		{
			g.defaultSwitchStateID = selectedNode.uniqueID;
		}
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		DrawDefaultSwitchStatePopup();
		EditorUtility.SetDirty(target);
	}

}
