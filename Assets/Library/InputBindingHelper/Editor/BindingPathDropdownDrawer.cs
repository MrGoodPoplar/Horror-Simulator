using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(BindingPathDropdownAttribute))]
public class BindingPathDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        List<string> bindingPaths = InputBindingHelper.GetAllPossibleBindingPaths();

        if (bindingPaths.Count == 0)
        {
            EditorGUI.LabelField(position, label.text, "No bindings found");
            return;
        }

        int selectedIndex = Mathf.Max(0, bindingPaths.IndexOf(property.stringValue));
        selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, bindingPaths.ToArray());

        if (selectedIndex >= 0)
        {
            property.stringValue = bindingPaths[selectedIndex];
        }
    }
}
#endif