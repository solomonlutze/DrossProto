using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMaster : MonoBehaviour
{
    public LayerCamera cameraPrefab;

    void Start()
    {
        // foreach (FloorLayer fl in (FloorLayer[])Enum.GetValues(typeof(FloorLayer)))
        // {
        //     LayerCamera c = Instantiate(cameraPrefab);
        //     c.Init(fl);
        // }
    }
}