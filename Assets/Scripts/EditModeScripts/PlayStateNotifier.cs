using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
[InitializeOnLoad]
public class PlayStateNotifier
{
  public static bool inEditMode = false;
  static PlayStateNotifier()
  {
    EditorApplication.playModeStateChanged += ModeChanged;
  }

  static void ModeChanged(PlayModeStateChange playModeState)
  {
    if (playModeState == PlayModeStateChange.EnteredEditMode)
    {
      inEditMode = true;
      Shader.EnableKeyword("INEDITMODE");
    }
    else if (playModeState == PlayModeStateChange.EnteredPlayMode)
    {
      inEditMode = false;
      Shader.DisableKeyword("INEDITMODE");
    }
    //else if (playModeState == PlayStateModeChange.)
  }
}

#endif