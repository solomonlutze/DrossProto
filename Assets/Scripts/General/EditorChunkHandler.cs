using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class EditorChunkHandler
{
  static Vector2Int cameraCurrentChunk;
  // void Update()
  // {
  //   Debug.Log("camera position: " + SceneView.camera == null ? "no camera" : ImageEffectAllowedInSceneView.camera.transform.position);
  // }
  static EditorChunkHandler()
  {
    SceneView.duringSceneGui += OnSceneGUI;
  }

  private static void OnSceneGUI(SceneView sceneView)
  {
    if (sceneView != null && sceneView.camera != null)
    {
      TileLocation cameraLocation = new TileLocation(sceneView.camera.transform.position);
      if (cameraCurrentChunk != cameraLocation.chunkCoordinates)
      {
        cameraCurrentChunk = cameraLocation.chunkCoordinates;
        GridManager.Instance.LoadAndUnloadChunks(cameraLocation);
      }
    }
  }
}