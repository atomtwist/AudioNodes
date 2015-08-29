using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;


public static class AudioNodesSettings
{
	public static bool VRMode;
}


public class EventNodesWindow : EditorWindow {

	#region window

	static EditorWindow audioNodesWindow;
	[MenuItem ("Window/AudioNodes™")]
	public static void ShowWindow() {
		audioNodesWindow = GetWindow<EventNodesWindow>("AudioNodes™");
		audioNodesWindow.Show();
	}
	#endregion

	void OnEnable(){
		Repaint();
		currentScrollViewWidth = this.position.xMin + 500;
		cursorChangeRect = new Rect(this.position.xMin + 500,toolbarHeight,1f,800);
	}

	#region toolbar
	
	static float toolbarHeight = 25;
	static float toolbarWidth = 100;
	static int toolbarFontSize = 11;
	static NodeType toolbarOptions;
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
		p = GUILayout.Toolbar(p, Enum.GetNames(typeof(NodeType)), toolbarButtonStyle);
		toolbarOptions = (NodeType)p;
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
		AudioNodesSettings.VRMode = GUILayout.Toggle(AudioNodesSettings.VRMode ,"VR Mode", toolbarButtonStyle);
		GUILayout.EndHorizontal();
	}
	#endregion

	#region ResizableScrollBar

	private Vector2 scrollPos = Vector2.zero;
	[SerializeField]
	float currentScrollViewWidth;
	bool resize = false;
	Rect cursorChangeRect;
	
	//	private Vector2 observerScrollPos = Vector2.zero;
	Rect resizableRect;
	private void ResizeScrollView(){
		GUI.color = new Color(0.15f,0.15f,0.15f,1);
		GUI.DrawTexture(cursorChangeRect,EditorGUIUtility.whiteTexture);
		GUI.color = Color.white;
		resizableRect = new Rect(cursorChangeRect.x,cursorChangeRect.y, cursorChangeRect.width + 10,cursorChangeRect.height);
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

	#endregion

	#region Create Event ID File

	public static void CreateEventAPI()
	{ 
		var prefabName = AudioNodesManager.instance.gameObject.name;
		var path = Application.dataPath + "/Audio/" + prefabName + "_IDs.cs";
		var events = GameObject.FindObjectsOfType<EventNode>();
		//adding SwitchGroups & States
		var switchGroups = GameObject.FindObjectsOfType<SwitchGroup>();
		var switchStates = GameObject.FindObjectsOfType<SwitchState>();
		
		if (switchGroups.Length == 0)
		{
			EditorPrefs.DeleteKey("currentSwitchGroupIDs");
			EditorPrefs.DeleteKey("currentSwitchGroupNames");
		}
		else
		{
			EditorPrefsX.SetIntArray("currentSwitchGroupIDs",switchGroups.Select(e => e.uniqueID).ToArray());
			EditorPrefsX.SetStringArray("currentSwitchGroupNames",switchGroups.Select(e => e.name).ToArray());
		}
		
		if (switchStates.Length == 0)
		{
			EditorPrefs.DeleteKey("currentSwitchStateIDs");
			EditorPrefs.DeleteKey("currentSwitchStateNames");
		}
		else
		{
			EditorPrefsX.SetIntArray("currentSwitchStateIDs",switchStates.Select(e => e.uniqueID).ToArray());
			EditorPrefsX.SetStringArray("currentSwitchStateNames",switchStates.Select(e => e.name).ToArray());
		}
		
		if (events.Length == 0)
		{
			EditorPrefs.DeleteKey("currentEventIDs");
			EditorPrefs.DeleteKey("currentEventNames");
		}
		else
		{
			EditorPrefsX.SetIntArray("currentEventIDs",events.Select(e => e.uniqueID).ToArray());
			EditorPrefsX.SetStringArray("currentEventNames",events.Select(e => e.name).ToArray());
		}
		
		var tags = events.Select (e => e.tag).ToList();
		tags = tags.GroupBy(t => t).Select(e => e.First()).ToList();
		
		using (StreamWriter outfile = 
		       new StreamWriter(path))
		{
			outfile.WriteLine(" public class " + prefabName + " {");
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
			
			//implement switchLogic here
			outfile.WriteLine(" public class SWITCHES {");
			foreach (var sg in switchGroups)
			{
				if (sg == null) continue;
				outfile.WriteLine(" public class " + sg.name.ToUpper() + " {");
				foreach (var ss in sg.switchStates)
				{
					if (ss == null) continue;
					var noSpaces = ss.name.Replace(" ", "_");
					noSpaces = noSpaces.Replace("-","_");
					noSpaces = noSpaces.Replace("  ", "_");
					noSpaces = noSpaces.Replace("   ", "_");
					outfile.WriteLine("public static int " + noSpaces + " = " + ss.uniqueID + ";" );
				}
				
				outfile.WriteLine("}" );
			}
			outfile.WriteLine("}" );
			
			
			outfile.WriteLine("}" );
		}//File written
		Debug.Log ("parameter file written to " + path);
		AssetDatabase.Refresh();
		
		
	}


	#endregion

	#region IDsChanged

	static bool IDsChanged;
	void OnInspectorUpdate() {
		var events = GameObject.FindObjectsOfType<EventNode>();
		var eventIDs = events.Select(e => e.uniqueID).ToArray();
		var eventNames = events.Select(e => e.name).ToArray();
		//switch groups 
		var switchGroups = GameObject.FindObjectsOfType<SwitchGroup>();
		var switchGroupIDs = switchGroups.Select(e => e.uniqueID).ToArray();
		var switchGroupNames = switchGroups.Select(e => e.name).ToArray();
		//switch states
		var switchStates = GameObject.FindObjectsOfType<SwitchState>();
		var switchStateIDs = switchStates.Select(e => e.uniqueID).ToArray();
		var switchStateNames = switchStates.Select(e => e.name).ToArray();
		
		if (EditorPrefsX.GetIntArray("currentEventIDs") != null)
		{
			var isIDEqual = new HashSet<int>(eventIDs).SetEquals(EditorPrefsX.GetIntArray("currentEventIDs"));
			var isNameEqual = eventNames.SequenceEqual(EditorPrefsX.GetStringArray("currentEventNames"));
			//switchgroups
			var switchGroupIDisEqual = new HashSet<int>(switchGroupIDs).SetEquals(EditorPrefsX.GetIntArray("currentSwitchGroupIDs"));
			var switchGroupNameisEqual = switchGroupNames.SequenceEqual(EditorPrefsX.GetStringArray("currentSwitchGroupNames"));
			//switchstates
			var switchStateIDisEqual = new HashSet<int>(switchStateIDs).SetEquals(EditorPrefsX.GetIntArray("currentSwitchStateIDs"));
			var switchStateNameIsEqual = switchStateNames.SequenceEqual(EditorPrefsX.GetStringArray("currentSwitchStateNames"));
			
			//DeBugging:
			/*Debug.Log (isIDEqual + " // " + isNameEqual + " // " + switchGroupIDisEqual + " // " + switchGroupNameisEqual + " // " + switchStateIDisEqual + " // " + switchStateNameIsEqual);
			Debug.Log (EditorPrefsX.GetIntArray("currentSwitchStateIDs").Length + " // " + EditorPrefsX.GetIntArray("currentSwitchStateIDs").Length);*/
			
			
			if ( isIDEqual && isNameEqual && eventIDs.Length == EditorPrefsX.GetIntArray("currentEventIDs").Length
			    && switchGroupIDisEqual && switchGroupNameisEqual && switchGroupIDs.Length == EditorPrefsX.GetIntArray("currentSwitchGroupIDs").Length
			    && switchStateIDisEqual && switchStateNameIsEqual && switchStateIDs.Length == EditorPrefsX.GetIntArray("currentSwitchStateIDs").Length)
				IDsChanged = false;
			else
				IDsChanged = true;
		}
		
		Repaint();	
	}


	#endregion

	#region EventPreviews
	
	void OnHierarchyChange()
	{
		Repaint ();
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



#endregion
	

	List<AudioNodeData> drawnItems = new List<AudioNodeData>();
	public AudioNodeAsset NodeAsshat;
	public List<AudioNodeData> _addChildren = new List<AudioNodeData>();
	public AudioNodeData SelectedNode;
	public List<AudioNodeData> SelectedNodes = new List<AudioNodeData>();
	public Rect selectedNodePosition;
	void OnGUI ()
	{
		NodeAsshat = (AudioNodeAsset)EditorGUILayout.ObjectField(NodeAsshat, typeof(AudioNodeAsset), false); 
		DrawToolBar();
		GUILayout.BeginHorizontal();
		scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(currentScrollViewWidth));
		EditorGUILayout.BeginVertical("Box");
		DrawTree(toolbarOptions);
		EditorGUILayout.EndVertical();

		GUILayout.EndScrollView();
		ResizeScrollView();
		
		//GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();

		if(SelectedNode != null) {
			GUILayout.Button(SelectedNode.ChildId);
		}
		DrawEventPreview();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		Repaint();
	} 

	#region Drag&Drop
	void DetectDrag()
	{
		//detect drag
		if (Event.current.type == EventType.DragExited)
		{
			Debug.Log("DragExit event, mousePos:" + Event.current.mousePosition + "window pos:" + position);
			/*foreach (Object obj in DragAndDrop.objectReferences)
					{
						if (obj.GetType() == typeof(Texture2D))
							_currentUI.CreateUIButton(((Texture2D)obj).width, ((Texture2D)obj).height, GetWorldPoint(Event.current.mousePosition));
					}*/
		}
	}
	#endregion


	List<T> MinMaxBetween<T>(List<T> list, int min, int max)
	{
		var inBetween = list.Skip(min).Take(max - min + 1);
		return inBetween.ToList ();
	}

	#region HierarKeyNavigation

	enum SelectDirection
	{
		NONE,
		UP,
		DOWN,
	}
	public bool hasKeyboardAndMouseFocus = true;
	AudioNodeData currentSelectedNode;
	SelectDirection selectDirection;
	void NavigateTreeWithKeys(List<AudioNodeData> list)
	{
		Event e = Event.current;
		//shift select stuff
		if (e.shift && hasKeyboardAndMouseFocus)
		{
			Event f = Event.current;
			if (f.type == EventType.KeyDown)
			{
				if (f.keyCode == KeyCode.DownArrow)
				{
					if (SelectedNodes.Count == 1)
					{
						selectDirection = SelectDirection.DOWN;
						currentSelectedNode = SelectedNode;
					}
					var nextItem = list.IndexOf(SelectedNode) + 1;
					if (nextItem > list.IndexOf(list.Last()))
					{
						f.Use ();
						return;
					}
					if (selectDirection == SelectDirection.DOWN)
						SelectedNodes = MinMaxBetween (list, list.IndexOf(currentSelectedNode), nextItem);
					if (selectDirection == SelectDirection.UP && SelectedNodes.Count > 1)
						SelectedNodes.Remove(SelectedNode);
					SelectedNode = list[nextItem];
				}
				if (f.keyCode == KeyCode.UpArrow)
				{
					if (SelectedNodes.Count == 1)
					{
						selectDirection = SelectDirection.UP;
						currentSelectedNode = SelectedNode;
					}
					var prevItem = list.IndexOf(SelectedNode) - 1;
					if (prevItem < 0)
					{
						f.Use();
						return;
					}
					if (selectDirection == SelectDirection.UP)
					{
						SelectedNodes = MinMaxBetween (list, prevItem, list.IndexOf(currentSelectedNode));
						SelectedNode = list[prevItem];
					}
					if (selectDirection == SelectDirection.DOWN && SelectedNodes.Count > 1)
					{
						SelectedNodes.Remove(SelectedNode);
						SelectedNode = list[prevItem];
					}
				}
			
				//SelectedNodes.Clear();
				if (selectedNodePosition.y > (resizableRect.yMin + (resizableRect.yMax/2)))
					scrollPos = new Vector2 (scrollPos.x, scrollPos.y +30);
				e.Use();
				//if  above change minimum if below change max
				//function that takes a list & two indexes - returns all inbetween 
			}
		}
			
		/*	if (f.type == EventType.KeyDown && f.keyCode == KeyCode.UpArrow)
			{
				//if(SelectedNodes.Count < 1) return;
				if (SelectedNodes.Count == 1) currentSelectedNode = SelectedNode;
				var prevItem = list.IndexOf(SelectedNode) - 1;
				if (prevItem < 0)
				{
					e.Use();
					return;
				}
				var min = prevItem;
				var max = list.IndexOf(currentSelectedNode);
				SelectedNodes = MinMaxBetween (list, min, max);
				SelectedNode = SelectedNodes.Last();
				/*if (SelectedNodes.Count > 1)
					SelectedNodes.Remove (list[prevItem]);
				if (selectedNodePosition.y < (resizableRect.yMin + (resizableRect.yMax/2))-150)
					scrollPos = new Vector2 (scrollPos.x, scrollPos.y -30);
				f.Use();
			}*/
		
		
		if (e.type == EventType.KeyDown && hasKeyboardAndMouseFocus)
		{
			if (e.keyCode == KeyCode.Return)
			{
				if (SelectedNode.nodeSubType == NodeSubType.WorkSpace) return;
				if (SelectedNode != null)
				{
					RenameNode();
				}
				e.Use();
			}
			
			
			if (e.keyCode == KeyCode.LeftArrow)
			{
				if (SelectedNode.foldOut)
					SelectedNode.foldOut = false;
				e.Use();
			}
			
			if (e.keyCode == KeyCode.RightArrow)
			{
				if (!SelectedNode.foldOut)
					SelectedNode.foldOut = true;
				e.Use();
			}
			
			if (e.keyCode == KeyCode.Tab)
			{
				if ((int)toolbarOptions == Enum.GetNames(typeof(NodeType)).Length-1)
				{
					toolbarOptions = (NodeType)0;
				}
				else
				{
					toolbarOptions ++;
				}
				e.Use();
			}
			if (e.keyCode == KeyCode.DownArrow)
			{
				var nextItem = list.IndexOf(SelectedNode) +1;
				if (nextItem == list.Count)
				{
					e.Use();
					return;
				}
				SelectedNode = list[nextItem];
				SelectedNodes.Clear();
				SelectedNodes.Add (SelectedNode);
				if (selectedNodePosition.y > (resizableRect.yMin + (resizableRect.yMax/2)))
					scrollPos = new Vector2 (scrollPos.x, scrollPos.y +30);
			}
			if (e.keyCode == KeyCode.UpArrow )
			{
				var prevItem = list.IndexOf(SelectedNode) -1;
				if (prevItem < 0)
				{
					e.Use();
					return;
				}
				SelectedNode = list[prevItem];
				SelectedNodes.Clear();
				SelectedNodes.Add (SelectedNode);
				if (selectedNodePosition.y < (resizableRect.yMin + (resizableRect.yMax/2))-150)
					scrollPos = new Vector2 (scrollPos.x, scrollPos.y -30);
			}
			if(e.keyCode == KeyCode.N) {
				if(SelectedNode != null) {
					var n = new AudioNodeData();
					n.Initialize();
					n.name = System.Guid.NewGuid().ToString();
					n.nodeSubType = SelectedNode.nodeSubType;
					SelectedNode.AddChild(n);
					NodeAsshat.AllTheNodes.Add (n);
				}
			}
			e.Use();
		}
	}

	
#endregion


	#region NodeOperations

	void CreateChildNode(AudioNodeData newNode)
	{
		SelectedNode.AddChild(newNode);
		_addChildren.Add (newNode);
		SelectedNode.foldOut = true;
		SelectedNode = newNode;
		EditorUtility.SetDirty(NodeAsshat);

	}

	void DeleteSelectedNode()
	{
		NodeAsshat.Remove(SelectedNode.ChildId);
		EditorUtility.SetDirty(NodeAsshat);
	}

	void RenameNode()
	{
		if (SelectedNode.nodeSubType == NodeSubType.WorkSpace) return;
		var renameWindow = EditorWindow.CreateInstance(typeof(Rename)) as Rename;
		renameWindow.Init(SelectedNode.name, this, PopupWindowAction.Rename);
		renameWindow.minSize = new Vector3(300,100);
		renameWindow.position = new Rect(Screen.width/2,Screen.height/2, 250, 50);
		renameWindow.ShowUtility();
	}

	#endregion
	
	#region ContextMenu

	public static string newNodeName;
	void NodeContextMenuCallback (object obj) {
		Debug.Log ("Selected: " + obj);
		if (obj == "Child: SFX Node")
		{
			var n = new AudioNodeData();
			n.Initialize();
			n.nodeType = toolbarOptions;
			n.nodeSubType = NodeSubType.SFXNode;
			n.icon = Resources.Load("sfx") as Texture2D;
			n.name = "new SFX Node";
			CreateChildNode(n);
		}
		if(obj == "Delete") {
			DeleteSelectedNode();
		}
	}


	void DrawContextMenu(AudioNodeData child, Event e)
	{
		SelectedNode = child;
		GUI.FocusControl(null);
		GenericMenu menu = new GenericMenu ();

		switch(child.nodeType)
		{
		case NodeType.AudioNodes : 
			menu.AddItem (new GUIContent ("Add Child/SFX Node"), false, NodeContextMenuCallback, "Child: SFX Node");
			menu.AddItem (new GUIContent ("Add Child/Multi Node"), false, NodeContextMenuCallback, "Child: Multi Node");
			menu.AddItem (new GUIContent ("Add Child/Random Node"), false, NodeContextMenuCallback, "Child: Random Node");
			menu.AddSeparator ("");
			if (child.nodeSubType != NodeSubType.WorkSpace)
			{
				menu.AddItem (new GUIContent ("Delete"), false, NodeContextMenuCallback, "Delete");
				menu.AddSeparator ("");
			}

			/*menu.AddItem (new GUIContent ("Add Parent/SFX Node"), false, NodeContextMenuCallback, "Parent: SFX Node");
			menu.AddItem (new GUIContent ("Add Parent/Multi Node"), false, NodeContextMenuCallback, "Parent: Multi Node");
			menu.AddItem (new GUIContent ("Add Parent/Random Node"), false, NodeContextMenuCallback, "Parent: Random Node");
			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Convert/SFX Node"), false, NodeContextMenuCallback, "Parent: SFX Node");
			menu.AddItem (new GUIContent ("Convert/Multi Node"), false, NodeContextMenuCallback, "Parent: Multi Node");
			menu.AddItem (new GUIContent ("Convert/Random Node"), false, NodeContextMenuCallback, "Parent: Random Node");*/
			menu.AddItem (new GUIContent ("Add Folder"), false, NodeContextMenuCallback, "Folder");
			break;
		case NodeType.EventNodes :
			if (child.nodeSubType == NodeSubType.WorkSpace)
			{
				menu.AddItem (new GUIContent ("Add Child/Event Node"), false, NodeContextMenuCallback, "Child: Event Node");
				menu.AddSeparator ("");
				menu.AddItem (new GUIContent ("Add Folder"), false, NodeContextMenuCallback, "Folder");
			}
			break;
		case NodeType.LogicNodes :
			if (child.nodeSubType == NodeSubType.Folder || child.nodeSubType == NodeSubType.WorkSpace)
			{
				menu.AddItem (new GUIContent ("Add Child/Switch Group"), false, NodeContextMenuCallback, "Child: Switch Group");
				menu.AddSeparator ("");
				menu.AddItem (new GUIContent ("Add Folder"), false, NodeContextMenuCallback, "Folder");
			}
			break;
			if (child.nodeSubType == NodeSubType.SwitchGroup)
			{
				menu.AddItem (new GUIContent ("Add Child/Switch State"), false, NodeContextMenuCallback, "Child: Switch State");
			}
			break;
		}
		menu.ShowAsContext ();
		e.Use();
	}

	#endregion

	#region DrawTree

	void ColorSelectedNode(AudioNodeData child)
	{
		if (child == null || SelectedNode == null)return;
		if (SelectedNodes.Contains(child))
		{
			GUI.backgroundColor = Color.blue;
		}
		else
			GUI.backgroundColor = Color.white;
		
		/*		if (child.ChildId == SelectedNode.ChildId)
		{
			GUI.backgroundColor = Color.blue;
		}
		else
		{
			GUI.backgroundColor = Color.white;
		}*/
	}

	void DrawTree(NodeType t) {
		if (NodeAsshat == null) return;
		if (drawnItems != null)
			drawnItems.Clear();
		//Create WorkSpaceRoot
		if (NodeAsshat.Roots.Where (r => r.nodeType == t).ToList().Count == 0)
		{
			var n = new AudioNodeData();
			n.Initialize();
			n.nodeType = t;
			n.nodeSubType = NodeSubType.WorkSpace;
			n.icon = Resources.Load("workSpace") as Texture2D;
			n.name = "WorkSpace";
			NodeAsshat.Roots.Add(n);
		}
		var drawnList = NodeAsshat.Roots.Where(node => node.nodeType == t).ToList();
		drawnList.ForEach(d => DrawChild(d, 2));
		_addChildren.ForEach(c => c.nodeType = t);
		NodeAsshat.AllTheNodes.AddRange(_addChildren);
		_addChildren.Clear();
		NavigateTreeWithKeys(drawnItems);
	}

	void DrawTreeElement(AudioNodeData child, int indent)
	{
		ColorSelectedNode(child);


		drawnItems.Add(child);
		EditorGUI.indentLevel = indent;

		//EditorGUILayout.BeginHorizontal("Box");
		using (var horizontalScope = new GUILayout.HorizontalScope("box")) {
		GUILayout.Space(indent);
		GUI.backgroundColor = Color.white;

		GUIStyle nodeStyle = new GUIStyle(EditorStyles.label);
		GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
		foldoutStyle.padding.right = 0;
		foldoutStyle.stretchWidth = false;
			if (child.Children.Count > 0)
			{
				child.foldOut = GUILayout.Toggle(child.foldOut,"", foldoutStyle);
			}
			else
				GUILayout.Space(18);
			//RenameNode
			Event e = Event.current;
			if (e.isMouse && e.button == 0 && e.type == EventType.MouseDown && e.clickCount == 2 && hasKeyboardAndMouseFocus && child == SelectedNode)
			{
				RenameNode();
			}
			GUIContent content = new GUIContent();
			content.image = child.icon;
			content.text = "  " + child.name;
			nodeStyle.active.textColor = Color.gray;
			if(GUILayout.Button(content, nodeStyle)) {

				//trying mouse selection
				bool shiftPressed = false;
				if (e.shift && hasKeyboardAndMouseFocus)
				{
					shiftPressed = true;
					Event f = Event.current;
					
					if (f.button == 0 )
					{
						var prevSelected = SelectedNode;
						SelectedNode = child;

						drawnItems.ForEach(Debug.Log);


						//SelectedNodes.AddRange(drawnItems.GetRange());
						SelectedNodes.Add(SelectedNode);
					}
					e.Use();
				}
				
				//var selectionBetweenClicks = drawnItems.SkipWhile(i => i != one).TakeWhile(t => t != second).ToList();

				if (e.button == 0 && hasKeyboardAndMouseFocus && !shiftPressed)
				{
					SelectedNode = child;
					SelectedNodes.Clear();
					SelectedNodes.Add (SelectedNode);
					GUI.FocusControl(null);
					e.Use ();
				}
				else if (e.button == 1 && hasKeyboardAndMouseFocus)
				{
					DrawContextMenu(child, e);
					e.Use ();
				}
			}
			//get selectionPosition
			if (child == SelectedNode)
			{
				selectedNodePosition = GUILayoutUtility.GetLastRect();
			}
		}

		//EditorGUILayout.EndHorizontal();

		if (child.foldOut) {           
			//draw children recursively
			if(child.Children.Count > 0) {
				EditorGUILayout.BeginVertical();
				foreach(var c in child.Children) {
					DrawChild(c, indent+20);
				}
				EditorGUILayout.EndVertical();
			}
		} 
	}

	//int currentIndent;
	void DrawFoldouts(AudioNodeData child, int indent)
	{
		GUIStyle foldOutStyle = new GUIStyle(EditorStyles.foldout);
		EditorGUI.indentLevel = indent;
		var currentIndent = indent;

		if (child.Children.Count > 0)
		{
			child.foldOut = EditorGUILayout.Foldout(child.foldOut, child.ChildId, foldOutStyle);
		}
		else
			EditorGUILayout.LabelField(child.ChildId);

		drawnItems.Add(child);
			
		//draw children recursively
		if(child.Children.Count > 0) {
			if (child.foldOut)
			{
				EditorGUILayout.BeginVertical();
				foreach(var c in child.Children) 
				{
					DrawChild(c, currentIndent+1);
				}
				EditorGUILayout.EndVertical();
			}
		}
	}

	void DrawChild(AudioNodeData child, int indent) {
		DrawTreeElement(child, indent);
		//DrawFoldouts(child, indent);
	}
}
	#endregion

	

public enum PopupWindowAction
{
	Rename,
	CreateSFXChild,
	CreateMultiChild,
	CreateRandomChild,
	CreateEventChild,
}

public class Rename : EditorWindow {
	public static string nodeName;
	EventNodesWindow w;
	PopupWindowAction windowAction;
	AudioNodeData newNode;
	
	public void Init(string name, AudioNodeData newNode, EventNodesWindow w, PopupWindowAction a)
	{
		nodeName = name;
		this.w = w;
		this.newNode = newNode;
		windowAction = a;
		w.hasKeyboardAndMouseFocus = false;
	}

	public void Init(string name, EventNodesWindow w, PopupWindowAction a)
	{
		nodeName = name;
		this.w = w;
		windowAction = a;
		w.hasKeyboardAndMouseFocus = false;
	}

	void OnDisable()
	{
		w.hasKeyboardAndMouseFocus = true; 
	}
	

	void Confirm()
	{
		Event e = Event.current;

		if (w.NodeAsshat.AllTheNodes.FirstOrDefault(c => c.name == nodeName) == null)
		{
			if (windowAction == PopupWindowAction.Rename)
			{
				w.SelectedNode.name = nodeName;
				e.Use ();
				w.hasKeyboardAndMouseFocus = true;
				Close();
			}
			if(windowAction == PopupWindowAction.CreateSFXChild)
			{
				newNode.name = nodeName;
				w.SelectedNode.AddChild(newNode);
				w._addChildren.Add (newNode);
				w.SelectedNode.foldOut = true;
				w.SelectedNode = newNode;
				e.Use();
				Close();
			}
		}
	}
	
	void OnGUI() {

		Event e = Event.current;

		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
		{
			Confirm();
		}
		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
		{
			Close ();
		}

		using (var vaerticalScope = new GUILayout.VerticalScope("box")) 
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Name:");
			GUI.SetNextControlName("nodeNameField");
			nodeName = EditorGUILayout.TextField(nodeName);

			EditorGUILayout.Space();
			if (GUILayout.Button("OK")) 
			{
				Confirm();
			}
			EditorGUILayout.Space();
			EditorGUILayout.Space();

		}
		var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
		textEditor.SelectAll();
		EditorGUI.FocusTextInControl("nodeNameField");	
	}
	void OnInspectorUpdate() {
		Repaint();
	}
}
		