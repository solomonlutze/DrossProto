using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class LayerRenderer : MonoBehaviour
{
  public float fadeDampTime = 0.05f;

  private FloorLayer lastTargetedFloorLayer;
  void Update()
  {
    if (Application.IsPlaying(gameObject))
    {
      PlayerController player = GameMaster.Instance.GetPlayerController();
      if (player != null)
      {
        HandleOpacity(player.currentFloor);
      }
    }
    else
    {
      // GameObject selectedObject = Selection.objects.Length > 0 ? Selection.objects[0] as GameObject : null;
      // if (selectedObject && WorldObject.GetFloorLayerOfGameObject(selectedObject) != lastTargetedFloorLayer)
      // {
      //   lastTargetedFloorLayer = WorldObject.GetFloorLayerOfGameObject(selectedObject);
      // }
      HandleOpacity(lastTargetedFloorLayer);
    }
  }

  void HandleOpacity(FloorLayer currentFloor)
  {
    ChangeOpacityRecursively(transform, currentFloor, fadeDampTime);
  }


  public static void ChangeOpacityRecursively(Transform trans, FloorLayer playerFloorLayer, float fadeDampTime)
  {
    int floorOffsetFromPlayer = (int)(WorldObject.GetFloorLayerFromGameObjectLayer(trans.gameObject.layer) - playerFloorLayer); // positive means we are above player; negative means we are below
    float targetOpacity = 1;
    if (floorOffsetFromPlayer > 0) { targetOpacity = 0; }
    // if (floorOffsetFromPlayer > 2) { targetOpacity = 0; }
    // else if (floorOffsetFromPlayer == 2) { targetOpacity = .15f; }
    // else if (floorOffsetFromPlayer == 1) { targetOpacity = .35f; }
    float actualNewOpacity;
    float _ref = 0;
    SpriteRenderer r = trans.gameObject.GetComponent<SpriteRenderer>();
    // if (trans.gameObject.name != "DarknessBelow")
    // {
    if (r != null)
    {
      actualNewOpacity = Mathf.SmoothDamp(r.color.a, targetOpacity, ref _ref, fadeDampTime);
      r.color = new Color(r.color.r, r.color.g, r.color.b, actualNewOpacity);
    }
    Tilemap t = trans.gameObject.GetComponent<Tilemap>();
    if (t != null)
    {
      actualNewOpacity = Mathf.SmoothDamp(t.color.a, targetOpacity, ref _ref, fadeDampTime);
      t.color = new Color(t.color.r, t.color.g, t.color.b, actualNewOpacity);
    }
    // }
    foreach (Transform child in trans)
    {
      ChangeOpacityRecursively(child, playerFloorLayer, fadeDampTime);
    }
  }
}