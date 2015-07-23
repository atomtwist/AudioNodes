using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Atomtwist.AudioNodes {

	public class MinMaxSlider : PropertyAttribute {
		
		public readonly float max;
		public readonly float min;
		
		public MinMaxSlider (float min, float max) {
			this.min = min;
			this.max = max;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer (typeof (MinMaxSlider))]
	class MinMaxSliderDrawer : PropertyDrawer {
		
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			
			if (property.propertyType == SerializedPropertyType.Vector2) {
				Vector2 range = property.vector2Value;
				float min = range.x;
				float max = range.y;
				MinMaxSlider attr = attribute as MinMaxSlider;
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.BeginHorizontal();
				EditorGUI.MinMaxSlider (label, position, ref min, ref max, attr.min, attr.max);
				if (EditorGUI.EndChangeCheck ()) {
					range.x = min;
					range.y = max;
					property.vector2Value = range;
				}
				EditorGUILayout.FloatField(range.x);
				EditorGUILayout.FloatField(range.y);
				EditorGUILayout.EndHorizontal();
			} else {
				EditorGUI.LabelField (position, label, "Use only with Vector2");
			}
		}
	}
#endif
}
