using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour
{

  public float dampTime = 0.15f;
  private Vector3 velocity = Vector3.zero;
  public Transform target;
  public Camera c;

  public Vector3 offset;
  public Vector3 critOffset;
  void Start()
  {
    c = GetComponent<Camera>();
  }

  void FixedUpdate()
  {
    if (GameMaster.Instance.GetPlayerController() != null)
    {
      target = GameMaster.Instance.GetPlayerController().transform;
      Vector3 calculatedOffset = (offset + (GameMaster.Instance.GetPlayerController().InCrit() ? critOffset : Vector3.zero));
      transform.position = Vector3.SmoothDamp(transform.position, target.transform.position + calculatedOffset, ref velocity, dampTime);
    }

  }
}