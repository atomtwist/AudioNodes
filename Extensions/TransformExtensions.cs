using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class TransformExtensions {

	#region Transform
	/// <summary>
	/// Returns all children of a Transform, but not their children.
	/// </summary>
	/// <returns>The children.</returns>
	/// <param name="transform">Transform.</param>
	public static List<Transform> GetChildren(this Transform transform)
	{
		List<Transform> children = new List<Transform>();
		var childCount = transform.childCount;
		for (var i = 0; i<childCount; i++)
		{
			var child = transform.GetChild(i);
			if (child.IsChildOf(transform)) children.Add (child);
		}
		return children;
	}
	#endregion
}
