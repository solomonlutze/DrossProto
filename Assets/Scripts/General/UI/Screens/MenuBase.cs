using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuBase : MonoBehaviour
{

  public Transform menuButtonsContainer;

  public void OnEnable()
  {
    StartCoroutine(SelectFirstButton());
  }

  // Required for menus that create buttons, bc the button cannot be selected the same frame it's created!!
  // Do not ask me about it!!
  // Do not delete it!!
  private IEnumerator SelectFirstButton()
  {
    yield return null;
    EventSystem.current.SetSelectedGameObject(menuButtonsContainer.GetChild(0).gameObject);
  }

}