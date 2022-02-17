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
  Vector3 truePosition;
  public float shakeMagnitude = .3f;
  float cameraShakeTime;
  void Start()
  {
    c = GetComponent<Camera>();
    truePosition = transform.position;
  }

  void FixedUpdate()
  {
    if (GameMaster.Instance.GetPlayerController() != null)
    {
      target = GameMaster.Instance.GetPlayerController().transform;
      Vector3 calculatedOffset = (offset + (GameMaster.Instance.GetPlayerController().InCrit() ? critOffset : Vector3.zero));
      transform.position = Vector3.SmoothDamp(truePosition, target.transform.position + calculatedOffset, ref velocity, dampTime);
      truePosition = transform.position;
    }
    if (cameraShakeTime > 0)
    {
      ShakeCamera();
      cameraShakeTime -= Time.fixedDeltaTime;
    }
  }

  public void DoCameraShake(float duration, float magnitude)
  {
    cameraShakeTime = duration;
    shakeMagnitude = magnitude;
  }
  void ShakeCamera()
  {
    Vector2 offset = Random.insideUnitCircle * shakeMagnitude;
    transform.position = truePosition + new Vector3(offset.x, offset.y, 0);
  }
}