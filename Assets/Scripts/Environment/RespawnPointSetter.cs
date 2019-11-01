using UnityEngine;

public class RespawnPointSetter : MonoBehaviour
{

  public SpawnPoint spawnPointToSet;
  public void OnTriggerEnter2D(Collider2D col)
  {
    Debug.Log("trigger enter?");
    if (col.gameObject.tag == "Player")
    {
      Debug.Log("is player?");
      GameMaster.Instance.nextSpawnPoint = spawnPointToSet.gameObject;
    }
  }
}