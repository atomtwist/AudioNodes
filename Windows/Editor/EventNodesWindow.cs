﻿using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public static class EventIDData
{
	[SerializeField]
	public 	static int[] currentEventIDs;
}

public class EventNodesWindow : EditorWindow {


	static EditorWindow hierarchyWindow;

	[MenuItem ("Window/AudioNodes™")]
	public static void ShowWindow() {
		hierarchyWindow = GetWindow<EventNodesWindow>("AudioNodes™");
		hierarchyWindow.Show();
	}
	
	#region toolbar
	enum ToolBaroptions
	{
		AudioNodes, 
		EventNodes, 
		LogicNodes,
	}
	
	static float toolbarHeight = 25;
	static float toolbarWidth = 100;
	static int toolbarFontSize = 11;
	static ToolBaroptions toolbarOptions;
	static GameObject audioNodeManager;
	static string status;
	
	
	void DrawToolBar()
	{
		var toolbarStyle = new GUIStyle(EditorStyles.toolbar);
		var toolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
		toolbarStyle.fixedHeight = toolbarHeight;
		
		toolbarButtonStyle.fixedHeight = toolbarHeight;
		toolbarButtonStyle.fixedWidth = toolbarWidth;
		toolbarButtonStyle.fontSize = toolbarFontSize;
		toolbarButtonStyle.fontStyle = FontStyle.Bold;
		
		GUILayout.BeginHorizontal(toolbarStyle);
		var p = (int)toolbarOptions;
		p = GUILayout.Toolbar(p,  Enum.GetNames(typeof(ToolBaroptions)), toolbarButtonStyle);
		toolbarOptions = (ToolBaroptions)p;
		if (IDsChanged)
		{
			GUI.contentColor = Color.yellow;

		}
			
		else
		{
			GUI.contentColor = Color.white;
		}

		if (GUILayout.Button("Create ID's", toolbarButtonStyle))
			CreateEventAPI();
		GUILayout.FlexibleSpace();
		GUI.contentColor = Color.white;
		GUILayout.EndHorizontal();
	}
	#endregion

	#region Event API

	public static void CreateEventAPI()
	{ 
		var path = Application.dataPath + "/Audio/Atomtwist/AudioNodes/EventNode_IDs.cs";
		var events = GameObject.FindObjectsOfType<EventNode>();
		EditorPrefsX.SetIntArray("currentEventIDs",events.Select(e => e.uniqueID).ToArray());
		EditorPrefsX.SetStringArray("currentEventNames",events.Select(e => e.name).ToArray());
		var tags = events.Select (e => e.tag).ToList();
		tags = tags.GroupBy(t => t).Select(e => e.First()).ToList();

		using (StreamWriter outfile = 
		       new StreamWriter(path))
		{
			outfile.WriteLine(" public class " + "AN" + " {");
			foreach (var tag in tags) {
				outfile.WriteLine(" public class " + tag.ToUpper() + " {");
				foreach (var e in events)
				{
					if (e.transform.parent.name != tag)
						continue;
					var noSpaces = e.name.Replace(" ", "_");
					noSpaces = noSpaces.Replace("-","_");
					noSpaces = noSpaces.Replace("  ", "_");
					noSpaces = noSpaces.Replace("   ", "_");
					outfile.WriteLine("public static int " + noSpaces + " = " + e.uniqueID + ";" );
				}
				outfile.WriteLine("}" );
			}
			outfile.WriteLine("}" );
		}//File written
		Debug.Log ("parameter file written to " + path);
		AssetDatabase.Refresh();

		
	}

	#endregion

	
/*	static bool showPosition = true;
	void DrawHierarchy()
	{
		audioNodeManager = FindObjectOfType<AudioNodesManager>().gameObject;
		var audioNodes = audioNodeManager.GetComponentsInChildren<AudioNode>();
		showPosition = EditorGUILayout.Foldout(showPosition, status);

		if (showPosition)
		if (audioNodeManager.transform) {
			EditorGUI.indentLevel = 1;
			foreach (Transform t in audioNodeManager.transform)
			{
				EditorGUILayout.PrefixLabel(t.transform.name);
				status = audioNodeManager.transform.name;
			}
		}
	}*/


	
	void DrawLAbel(string text)
	{
		EditorGUILayout.LabelField(text);
		//EditorGUILayout.SelectableLabel(text);
	}

	static bool IDsChanged;
	void OnInspectorUpdate() {
		var events = GameObject.FindObjectsOfType<EventNode>();
		var eventIDs = events.Select(e => e.uniqueID).ToArray();
		var eventNames = events.Select(e => e.name).ToArray();
		if (EditorPrefsX.GetIntArray("currentEventIDs") != null)
		{
			var isIDEqual = new HashSet<int>(eventIDs).SetEquals(EditorPrefsX.GetIntArray("currentEventIDs"));
			var isNameEqual = new HashSet<string>(eventNames).SetEquals(EditorPrefsX.GetStringArray("currentEventNames"));
			if ( !isIDEqual || !isNameEqual || eventIDs.Length != EditorPrefsX.GetIntArray("currentEventIDs").Length)
				IDsChanged = true;
			if ( isIDEqual && isNameEqual && eventIDs.Length == EditorPrefsX.GetIntArray("currentEventIDs").Length)
				IDsChanged = false;
		}

		Repaint();	
	}

	void OnGUI ()
	{
		DrawToolBar();
		GUILayout.BeginHorizontal();
		scrollPos = GUILayout.BeginScrollView(scrollPos,GUILayout.Width(currentScrollViewWidth));
		for(int i=0;i<200;i++) 
		{
			EditorGUILayout.BeginVertical("Box");
			EditorGUILayout.LabelField("lkjlkjlkjlkjlkjlkjlkjlkjlkjlkjlkjlkjlkjlkjlkjlkjlkjlkjlkj");			
			EditorGUILayout.EndVertical();
		}
		GUILayout.EndScrollView();
		
		ResizeScrollView();
		
		//GUILayout.FlexibleSpace();
		DrawEventPreview();
		
		GUILayout.EndHorizontal();
		Repaint();
	}


	private Vector2 scrollPos = Vector2.zero;
	float currentScrollViewWidth;
	bool resize = false;
	Rect cursorChangeRect;
	
	void OnEnable(){
		currentScrollViewWidth = this.position.xMin + 140;
		cursorChangeRect = new Rect(this.position.xMin + 140,toolbarHeight,1f,800);
	}
	

	
//	private Vector2 observerScrollPos = Vector2.zero;
	private void ResizeScrollView(){
		GUI.color = new Color(0.15f,0.15f,0.15f,1);
		GUI.DrawTexture(cursorChangeRect,EditorGUIUtility.whiteTexture);
		GUI.color = Color.white;
		var resizableRect = new Rect(cursorChangeRect.x,cursorChangeRect.y, cursorChangeRect.width + 10,cursorChangeRect.height);
		//GUI.DrawTexture(resizableRect,EditorGUIUtility.whiteTexture);
		EditorGUIUtility.AddCursorRect(resizableRect,MouseCursor.SplitResizeLeftRight);
		
		if( Event.current.type == EventType.mouseDown && resizableRect.Contains(Event.current.mousePosition)){
			resize = true;
		}
		if(resize){
			currentScrollViewWidth = Event.current.mousePosition.x;
			currentScrollViewWidth = Mathf.Clamp(currentScrollViewWidth, this.position.xMin + 140, this.position.xMax - 140 );
 
			cursorChangeRect.Set(currentScrollViewWidth,cursorChangeRect.y,cursorChangeRect.width,cursorChangeRect.height);
			resizableRect.Set(currentScrollViewWidth,resizableRect.y,resizableRect.width,resizableRect.height);

		}
		if(Event.current.type == EventType.MouseUp)
			resize = false;        
	}
	


	void DrawEventPreview()
	{
		var events = GameObject.FindObjectsOfType<EventNode>().ToList();
		var tags = events.Select (e => e.tag).ToList();
		tags = tags.GroupBy(t => t).Select(e => e.First()).ToList();
		GUILayout.BeginHorizontal();
		foreach (var tag in tags) {
			if (tag == null || tag == "")
				continue;
			EditorGUILayout.BeginVertical("Box");
			EditorGUILayout.LabelField(tag);
			foreach (var e in events)
			{
				if (!e.exposeForPreview)
					continue;
				if (e.transform.parent.name != tag)
					continue;
				if (GUILayout.Button(e.name))
				{
					e.PostEvent(e.uniqueID);
				}
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.LabelField("");
		GUILayout.EndHorizontal();
	}		
}