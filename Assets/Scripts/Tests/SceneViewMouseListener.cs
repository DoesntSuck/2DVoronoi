using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets
{
    [CustomEditor(typeof(SceneViewMouseMoveListener))]
    public class SceneViewMouseListener : Editor
    {
        SceneViewMouseMoveListener listener;

        void OnEnable()
        {
            // Get the selected Delaunay Test script
            if (Selection.activeGameObject != null)
                listener = Selection.activeGameObject.GetComponent<SceneViewMouseMoveListener>();
        }

        void OnSceneGUI()
        {
            // Get mouse position on GUI and convert to a world space ray
            Vector2 mouseGUIPosition = Event.current.mousePosition;
            Ray worldSpaceRay = HandleUtility.GUIPointToWorldRay(mouseGUIPosition);

            // Pass ray to delaunay test
            listener.MouseMoved(worldSpaceRay);
        }
    }
}
