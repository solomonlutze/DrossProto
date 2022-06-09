using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MenuBase
{

  public GameObject[] spawners;
  public void Unpause()
  {
    GameMaster.Instance.SetGameUnpaused();
  }

  public void Die()
  {
    GameMaster.Instance.Die();
  }

  public void Restart()
  {
    GameMaster.Instance.Restart();
  }

  public void SpawnEnemy(int idx)
  {
    bool wasActive = spawners[idx].activeSelf;
    if (!wasActive)
    {
      spawners[idx].SetActive(true);
    }
    spawners[idx].SendMessage("Activate", SendMessageOptions.RequireReceiver);
    if (!wasActive)
    {
      spawners[idx].SetActive(false);
    }
  }
  public void Quit()
  {
    Application.Quit();
  }
}