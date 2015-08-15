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
		EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;
	}

	void OnDisable()
	{
		EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;
	}

	void OnHierarchyChanged ()
	{
		UpdateSwitchStates();
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
		//it doesnt like if items have the same name
		var switchStateNames = switchStates.Select(n => n.name.ToString()).ToArray();
		var switchStateIDList = switchStates.Select (n => n.uniqueID).ToList();
		var selectedIndex = EditorGUILayout.Popup( "Default Switch State ",switchStateIDList.IndexOf(g.defaultSwitchStateID), switchStateNames);
		selectedIndex = Mathf.Clamp(selectedIndex,0,int.MaxValue);
		var selectedNode = switchStates[selectedIndex];
		g.defaultSwitchStateName = switchStateNames[selectedIndex];
		if( EditorGUI.EndChangeCheck()) 
		{
			g.defaultSwitchStateID = selectedNode.uniqueID;
		}
	}

	void UpdateSwitchStates()
	{
		if (switchStates != null || switchStates.Count > 0)
		{
			g.switchStates = switchStates;
		}
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		DrawDefaultSwitchStatePopup();
		UpdateSwitchStates();
		EditorUtility.SetDirty(target);
	}

}
