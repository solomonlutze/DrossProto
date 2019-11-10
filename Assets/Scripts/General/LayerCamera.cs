using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LayerCamera : MonoBehaviour
{
    public FloorLayer floorLayer;
    public Camera ownCamera;
    public float dampTime = 0.15f;
    private Vector3 velocity = Vector3.zero;
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
    }

    void Update()
    {
        PlayerController player = GameMaster.Instance.GetPlayerController();
        if (player != null)
        {
            EnableBasedOnPlayerFloor(player);
            HandleSmoothFollowAndParallax(player);
        }
    }

    void EnableBasedOnPlayerFloor(PlayerController player)
    {
        FloorLayer playerFloorLayer = player.currentFloor;
        floorOffsetFromPlayer = (int)(floorLayer - playerFloorLayer);
        if (playerFloorLayer < floorLayer)
        {
            ownCamera.enabled = false;
        }
        else
        {
            ownCamera.enabled = true;
        }
    }
    void HandleSmoothFollowAndParallax(PlayerController player)
    {
        target = GameMaster.Instance.GetPlayerController().transform;
        Vector3 point = ownCamera.WorldToViewportPoint(target.position);
        Vector3 delta = target.position - ownCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
        Vector3 destination = transform.position + delta;
        transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        ownCamera.orthographicSize = defaultCameraSize - (floorOffsetFromPlayer * cameraSizeOffsetPerLayer);
    }
}