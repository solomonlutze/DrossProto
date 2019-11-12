using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LayerCamera : MonoBehaviour
{
    public FloorLayer floorLayer;
    public Camera ownCamera;
    public float scrollDampTime = 0.15f;
    private Vector3 scrollVelocity = Vector3.zero;
    public float zoomDampTime = 0.5f;
    private float zoomVelocity;
    public Transform target;

    public float cameraSizeOffsetPerLayer = 1.0f;
    private float defaultCameraSize;
    private int floorOffsetFromPlayer;

    public void Init(FloorLayer fl)
    {
        floorLayer = fl;
        int firstFloorLayerIndex = LayerMask.NameToLayer("B6");
        defaultCameraSize = ownCamera.orthographicSize;
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
        target = GameMaster.Instance.GetPlayerController().transform;
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
        // }


    }
}