using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using Kaae;
using System.Linq;

[ExecuteInEditMode]
public abstract class Node : MonoBehaviour {

	public string name;
	public int uniqueID;

	//TODO: Move this into a custom Inspector !!
	public virtual void OnValidate()
	{
		/*var nodes = FindObjectsOfType<Node>().ToList();
		nodes.Remove(this);
		if (nodes.FirstOrDefault(n => n.name == name) || name == "")
		{
			gameObject.name = "GameObject";
			if (name != "")
				Debug.LogError("AUDIONODES: Name already exists. Please Enter a unique Name.");
			name = "";
		} else
			gameObject.name = name;*/
		if (name == "") return;
		if (name != "") gameObject.name = name;
	}

	#region HandleID

	public virtual void OnEnable()
	{
		var nodes = FindObjectsOfType<Node>().ToList();
		nodes.Remove(this);
		if (nodes.FirstOrDefault(n => n.uniqueID == uniqueID))
			CreateUniqueID();
		transform.localPosition = Vector3.zero;
	}

	public virtual void Reset()
	{
		CreateUniqueID();
	}

	System.Guid guid;
	void CreateUniqueID()
	{
		byte[] gb = System.Guid.NewGuid().ToByteArray();
		uniqueID = System.BitConverter.ToInt32(gb,0);
	}

	#endregion



}
