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
        PlayerController player = GameMaster.Instance.GetPlayerController();
        if (player != null)
        {
            floorLayer = player.currentFloor;
            Debug.Log("camera - player current floor: "+player.currentFloor);
            int mask = 0;
            for (
                int i = Constants.firstFloorLayerIndex;
                i < Constants.firstFloorLayerIndex + Constants.numberOfFloorLayers;
                i++
            )
            {
                int layerValue =
                //   i <= Constants.firstFloorLayerIndex + (int)floorLayer + 1 // NOTE: This +1 makes the layer above you visible!!
                i <= Constants.firstFloorLayerIndex + (int)floorLayer
                  ? 1 : 0;
                mask |= layerValue << i;
            }
            ownCamera.cullingMask = mask;
        }
    }
}
