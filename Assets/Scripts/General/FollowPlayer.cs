using UnityEngine;
public class FollowPlayer : MonoBehaviour
{
  public Rigidbody2D rb;
  void Update()
  {
    if (GameMaster.Instance.GetPlayerController() != null)
    {
      rb.MovePosition(GameMaster.Instance.GetPlayerController().transform.position);
    }
  }
}