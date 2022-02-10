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
        // if (GridManager.Instance.loadedChunks != null)
        // {
        //   Debug.Log("number of currently-loaded chunks: " + GridManager.Instance.loadedChunks.Count);
        // }
        GridManager.Instance.StartLoadAndUnloadChunks(cameraLocation);
      }
    }
  }

}