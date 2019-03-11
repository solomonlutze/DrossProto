using System;
using UnityEngine;
public static class Constants
{
    public enum GameState { Play, Dead, Dialogue, Menu }
    public const float DEFAULT_DETECTION_RANGE = 20f;
    public enum FloorLayer { B6, B5, B4, B3, B2, B1, F1, F2, F3, F4, F5, F6 }
    public static int firstFloorLayerIndex = LayerMask.NameToLayer("B6");
    public static int numberOfFloorLayers = Enum.GetNames(typeof(Constants.FloorLayer)).Length;
}