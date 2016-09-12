using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets
{
    [CustomEditor(typeof(DelaunayTest))]
    public class SceneViewMouseListener : Editor
    {
        DelaunayTest delaunayTest;

        void OnEnable()
        {
            // Get the selected Delaunay Test script
            if (Selection.activeGameObject != null)
                delaunayTest = Selection.activeGameObject.GetComponent<DelaunayTest>();
        }

        void OnSceneGUI()
        {
            // Get mouse position on GUI and convert to a world space ray
            Vector2 mouseGUIPosition = Event.current.mousePosition;
            Ray worldSpaceRay = HandleUtility.GUIPointToWorldRay(mouseGUIPosition);

            // Pass ray to delaunay test
            delaunayTest.MouseMoved(worldSpaceRay);
        }
    }
}
