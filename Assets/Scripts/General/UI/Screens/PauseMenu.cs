using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MenuBase
{


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

  public void Quit()
  {
    Application.Quit();
  }
}