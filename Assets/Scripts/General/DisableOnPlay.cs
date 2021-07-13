using UnityEngine;

public class DisableOnPlay : MonoBehaviour
{
  void Awake()
  {
    gameObject.SetActive(false);
  }
}