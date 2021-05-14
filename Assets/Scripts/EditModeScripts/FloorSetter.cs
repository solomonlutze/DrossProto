// #if UNITY_EDITOR
// GameObject selectedObject = Selection.objects.Length > 0 ? Selection.objects[0] as GameObject : null;
// if (selectedObject && WorldObject.GetFloorLayerOfGameObject(selectedObject) != lastTargetedFloorLayer)
// {
//   lastTargetedFloorLayer = WorldObject.GetFloorLayerOfGameObject(selectedObject);
// }
using UnityEngine;
using UnityEditor;
using ScriptableObjectArchitecture;

[ExecuteInEditMode]
public class FloorSetter : MonoBehaviour
{
#if UNITY_EDITOR
  void Awake()
  {
    if (!Application.IsPlaying(gameObject))
    {
      GameObject selectedObject = Selection.objects.Length > 0 ? Selection.objects[0] as GameObject : null;
      if (selectedObject)
      {
        gameObject.layer = selectedObject.layer;
        WorldObject.ChangeLayersRecursively(transform, gameObject.layer);
      }
    }
  }
#endif
}