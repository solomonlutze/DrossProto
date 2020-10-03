using UnityEngine;

public class TEST_FallingObj : MonoBehaviour
{

  public float fallTime;
  void Start()
  {
    fallTime = transform.position.z;
  }

  void FixedUpdate()
  {
    fallTime += Time.deltaTime;
    Debug.Log("TESTFALL transform.position.z before:" + transform.position.z);
    transform.position += new Vector3(0, 0, Time.deltaTime);
    Debug.Log("TESTFALL transform.position.z after:" + transform.position.z);
    Debug.Log("TESTFALL fallTime: " + fallTime);
  }
}