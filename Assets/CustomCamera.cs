using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCamera : MonoBehaviour
{
    public PlayerController player;
    public Constants.FloorLayer floorLayer;
    public Camera ownCamera;
    // Start is called before the first frame update
    void Start()
    {
        ownCamera = GetComponent<Camera>();
        Debug.Log("firstFloorLayerIndex: " + Constants.firstFloorLayerIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            floorLayer = player.currentFloor;
            int mask = 0;
            for (
                int i = Constants.firstFloorLayerIndex;
                i < Constants.firstFloorLayerIndex + Constants.numberOfFloorLayers;
                i++
            )
            {
                int layerValue = i <= Constants.firstFloorLayerIndex + (int)floorLayer ? 1 : 0;
                mask |= layerValue << i;
            }
            ownCamera.cullingMask = mask;
        }
    }
}
