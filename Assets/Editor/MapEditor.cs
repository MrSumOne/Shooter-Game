using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(MapGenerator))]
public class MapEditor : Editor {

    //Here we allow our map to be edited in the inspector
    public override void OnInspectorGUI()
    {
        MapGenerator map = target as MapGenerator;
        //if there's a change in the inspector
        if (DrawDefaultInspector())
        {
            //generate the map
            map.GenerateMap();
        }

        if (GUILayout.Button("Generate Map"))
        {
            map.GenerateMap();
        }
    }
}
