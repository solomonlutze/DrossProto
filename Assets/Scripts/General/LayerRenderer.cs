using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class LayerRenderer : MonoBehaviour
{
    public float fadeDampTime = 0.05f;

    void Update()
    {
        PlayerController player = GameMaster.Instance.GetPlayerController();
        {
            HandleOpacity(player);
        }
    }

    void HandleOpacity(PlayerController player)
    {
        ChangeOpacityRecursively(transform, player.currentFloor, fadeDampTime);
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