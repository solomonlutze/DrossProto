using UnityEditor;
using UnityEngine;
public class CameraSettings
{
  [MenuItem("Edit/SceneView Settings/Update Camera Settings")]
  static void UpdateCameraSettings()
  {
    SceneView.CameraSettings settings = new SceneView.CameraSettings();
    // settings.accelerationEnabled = false;
    // settings.speedMin = 1f;
    // settings.speedMax = 10f;
    // settings.speed = 5f;
    // settings.easingEnabled = true;
    // settings.easingDuration = 0.6f;
    // settings.dynamicClip = false;
    settings.fieldOfView = 130f;
    // settings.nearClip = 0.01f;
    // settings.farClip = 1000f;
    // settings.occlusionCulling = true;
    SceneView sceneView = SceneView.lastActiveSceneView;
    Debug.Log("updating camera settings");
    sceneView.cameraSettings = settings;
  }
}