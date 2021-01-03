using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCamera : MonoBehaviour
{
    public FloorLayer floorLayer;
    public Camera ownCamera;
    // Start is called before the first frame update
    void Start()
    {
        ownCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        int firstFloorLayerIndex = LayerMask.NameToLayer("B6");
        PlayerController player = GameMaster.Instance.GetPlayerController();
        if (player != null)
        {
            floorLayer = player.currentFloor;
            int mask = 0;
            for (
                int i = firstFloorLayerIndex;
                i < firstFloorLayerIndex + Constants.numberOfFloorLayers;
                i++
            )
            {
                int layerValue =
                  i <= firstFloorLayerIndex + (int)floorLayer + 1 // NOTE: This +1 makes the layer above you visible!!
                                                                  // i <= firstFloorLayerIndex + (int)floorLayer
                  ? 1 : 0;
                mask |= layerValue << i;
            }
            ownCamera.cullingMask = mask;
        }
    }
}
