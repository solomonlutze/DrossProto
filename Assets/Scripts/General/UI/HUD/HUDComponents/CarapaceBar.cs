using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarapaceBar : MonoBehaviour
{

  // CARAPACE
  public CanvasGroup carapaceBarContainer;
  public RectTransform carapaceBarController;
  public RectTransform carapaceBarContentsFill;
  public Image carapaceBarContentsSprite
  {
    get
    {
      Debug.Log("carapaceBarContentsFill " + carapaceBarContentsFill);
      return carapaceBarContentsFill == null ? null : carapaceBarContentsFill.GetComponent<Image>();
    }
  }

  public RectTransform carapaceBarEmpty;
  public Image carapaceBarEmptySprite
  {
    get
    {
      return carapaceBarEmpty == null ? null : carapaceBarEmpty.GetComponent<Image>();
    }
  }

  public float carapaceBarFadeTime = .5f;
  public Color lowCarapaceColor;
  public Color brokenCarapaceColor;
  public Color defaultCarapaceColor;
  private Coroutine carapaceBrokenFlashCoroutine;

  // END CARAPACE
  public Character character;
  void Start()
  {
    // character = GetComponent<PlayerController>();
    carapaceBarContentsSprite.color = defaultCarapaceColor;
    carapaceBarContainer.alpha = 1;
  }
  // Update is called once per frame
  void Update()
  {
    if (character != null)
    {
      HandleCarapaceBar();
    }
  }

  void HandleCarapaceBar()
  {
    float currentCarapace = Mathf.Max(character.GetCharacterVital(CharacterVital.CurrentCarapace), 0);
    float maxCarapace = character.GetCurrentMaxCarapace();
    if (!character.blocking && !character.carapaceBroken)
    {
      carapaceBarContainer.alpha -= Time.deltaTime / carapaceBarFadeTime;
      carapaceBarContainer.alpha = Mathf.Max(.75f, carapaceBarContainer.alpha);
      return;
    }
    carapaceBarContainer.alpha += Time.deltaTime / carapaceBarFadeTime;
    carapaceBarContainer.alpha = Mathf.Min(1, carapaceBarContainer.alpha);
    carapaceBarContentsFill.localScale = new Vector3(currentCarapace / maxCarapace, carapaceBarContentsFill.localScale.y, 0);
    if (character.carapaceBroken)
    {
      if (carapaceBrokenFlashCoroutine == null)
      {
        carapaceBrokenFlashCoroutine = StartCoroutine(CarapaceBrokenFlash());
      }
    }
  }

  public IEnumerator CarapaceBrokenFlash()
  {
    while (character.carapaceBroken)
    {
      carapaceBarEmptySprite.color = carapaceBarEmptySprite.color == brokenCarapaceColor ? lowCarapaceColor : brokenCarapaceColor;
      yield return new WaitForSeconds(.2f);
    }
    carapaceBarEmptySprite.color = Color.clear;
    carapaceBrokenFlashCoroutine = null;
  }


}
