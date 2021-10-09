using UnityEngine;
using ScriptableObjectArchitecture;
public class ClearScriptableObjectValue : MonoBehaviour
{

  void Awake()
  {
    gameObject.SetActive(false);
  }
}