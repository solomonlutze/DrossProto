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
public class LayerRendererController : MonoBehaviour
{
  public IntVariable currentTargetedFloorLayer;
  public GameEvent floorLayerChanged;
#if UNITY_EDITOR
  void Update()
  {
    if (!Application.IsPlaying(gameObject))
    {

      GameObject selectedObject = Selection.objects.Length > 0 ? Selection.objects[0] as GameObject : null;
      if (selectedObject && (int)WorldObject.GetFloorLayerOfGameObject(selectedObject) != currentTargetedFloorLayer)
      {
        currentTargetedFloorLayer.Value = (int)WorldObject.GetFloorLayerOfGameObject(selectedObject);
        floorLayerChanged.Raise();
      }
    }
  }
#endif
}