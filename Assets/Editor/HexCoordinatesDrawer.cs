using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HexCoordinates))]
public class HexCoordinatesDrawer : PropertyDrawer
{
    //Property drawer render their contents via an OnGUI method

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Get the x and z values from the property 
        //create a new set of coordinates 

        HexCoordinates coordinates = new HexCoordinates(property.FindPropertyRelative("x").intValue, property.FindPropertyRelative("z").intValue);

        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, coordinates.ToString());
    }
}
