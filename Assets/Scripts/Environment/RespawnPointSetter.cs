using UnityEngine;

public class RespawnPointSetter : MonoBehaviour
{

  public SpawnPoint spawnPointToSet;
  public void OnTriggerEnter2D(Collider2D col)
  {
    if (col.gameObject.tag == "Player")
    {
      GameMaster.Instance.nextSpawnPoint = spawnPointToSet.gameObject;
    }
  }
}