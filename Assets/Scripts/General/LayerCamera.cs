using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
public class LayerCamera : MonoBehaviour
{
  public FloorLayer floorLayer;
  public PostProcessLayer ppl;
  public PostProcessVolume processVolume;
  public DepthOfField depthOfField;
  public Camera ownCamera;
  public float scrollDampTime = 0.15f;
  private Vector3 scrollVelocity = Vector3.zero;
  public float zoomDampTime = 0.5f;
  private float zoomVelocity;
  public float focusDampTime = 0.5f;
  private float focusDampVelocity;
  public Transform target;

  public float cameraSizeOffsetPerLayer = 1.0f;
  private float defaultCameraSize;
  private int floorOffsetFromPlayer;

  public void Init(FloorLayer fl)
  {
    floorLayer = fl;
    gameObject.name = "LayerCamera_" + fl.ToString();
    gameObject.layer = WorldObject.GetGameObjectLayerFromFloorLayer(fl);
    ppl.volumeLayer = LayerMask.GetMask(floorLayer.ToString());
    depthOfField = ScriptableObject.CreateInstance<DepthOfField>();
    depthOfField.enabled.Override(true);
    depthOfField.focusDistance.Override(10); // default focusDistance
    depthOfField.aperture.Override(3.6f); // default aperture
    depthOfField.focalLength.Override(50); // default focalLength - ~70 is blurry
    processVolume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, depthOfField);

    int firstFloorLayerIndex = LayerMask.NameToLayer("B6");
    defaultCameraSize = ownCamera.orthographicSize;
    // ownCamera.usePhysicalProperties = true;
    int mask = 0;
    for (
        int i = firstFloorLayerIndex;
        i < firstFloorLayerIndex + Constants.numberOfFloorLayers;
        i++
    )
    {
      int layerValue =
      //   i <= Constants.firstFloorLayerIndex + (int)floorLayer + 1 // NOTE: This +1 makes the layer above you visible!!
      i == firstFloorLayerIndex + (int)floorLayer
          ? 1 : 0;
      mask |= layerValue << i;
    }
    ownCamera.cullingMask = mask;
    ownCamera.depth = (int)floorLayer;
  }

  void Update()
  {
    PlayerController player = GameMaster.Instance.GetPlayerController();
    if (player != null)
    {
      floorOffsetFromPlayer = (int)(floorLayer - player.currentFloor); // positive means we are above player; negative means we are below
      HandleScrollAndEnabling(player);
      HandleSmoothFollow(player);
      HandleParallax();
      HandleFocus();
    }
  }

  void HandleScrollAndEnabling(PlayerController player)
  {
    if (floorOffsetFromPlayer > 0)
    {
      // ownCamera.enabled = false;
    }
    else
    {
      ownCamera.enabled = true;
    }
  }
  void HandleSmoothFollow(PlayerController player)
  {
    target = GameMaster.Instance.GetPlayerController().cameraFollowTarget;
    // target = GameMaster.Instance.GetPlayerController().transform;
    Vector3 point = ownCamera.WorldToViewportPoint(target.position);
    Vector3 delta = target.position - ownCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
    Vector3 destination = transform.position + delta;
    transform.position = Vector3.SmoothDamp(transform.position, destination, ref scrollVelocity, scrollDampTime);
  }
  void HandleParallax()
  {
    float targetSize = defaultCameraSize - (floorOffsetFromPlayer * cameraSizeOffsetPerLayer);
    if (floorOffsetFromPlayer > 0) { targetSize = 0.1f; }
    // if (floorOffsetFromPlayer < 0)
    // {
    float actualDampTime = floorOffsetFromPlayer > 0 ? zoomDampTime / floorOffsetFromPlayer : zoomDampTime;
    ownCamera.orthographicSize = Mathf.SmoothDamp(ownCamera.orthographicSize, targetSize, ref zoomVelocity, actualDampTime); // ;defaultCameraSize - (floorOffsetFromPlayer * cameraSizeOffsetPerLayer);
  }

  void HandleFocus()
  {
    float targetFocalLength = 60;
    if (floorOffsetFromPlayer < 0)
    {
      targetFocalLength = 65 - (floorOffsetFromPlayer * 5);
    }
    float actualFocusTime = floorOffsetFromPlayer > 0 ? focusDampTime / floorOffsetFromPlayer : focusDampTime;
    depthOfField.focalLength.value = Mathf.SmoothDamp(depthOfField.focalLength, targetFocalLength, ref focusDampVelocity, actualFocusTime); // ;defaultCameraSize - (floorOffsetFromPlayer * cameraSizeOffsetPerLayer);
  }
}