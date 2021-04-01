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
      return carapaceBarContentsFill == null ? null : carapaceBarContentsFill.GetComponent<Image>();
    }
  }

  public RectTransform carapaceBarEmpty;
  public RectTransform carapaceBarBackground;
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

  float maxCarapaceBarWidth;
  private Coroutine carapaceBrokenFlashCoroutine;

  // END CARAPACE
  public Character character;
  void Start()
  {
    Init();
  }
  // Update is called once per frame
  void Update()
  {
    if (character != null)
    {
      HandleCarapaceBar();
    }
    else
    {
      Init();
    }
  }

  void Init()
  {
    carapaceBarContentsSprite.color = defaultCarapaceColor;
    carapaceBarContainer.alpha = 1;
    carapaceBarBackground.transform.localScale = new Vector3(1, 1, 0);
  }

  void HandleCarapaceBar()
  {
    float currentCarapace = Mathf.Max(character.GetCharacterVital(CharacterVital.CurrentCarapace), 0);
    float maxCarapace = character.GetCurrentMaxCarapace();
    if (!character.blocking && !character.carapaceBroken)
    {
      carapaceBarContainer.alpha -= Time.deltaTime / carapaceBarFadeTime;
      carapaceBarContainer.alpha = Mathf.Max(.75f, carapaceBarContainer.alpha);
    }
    else
    {
      carapaceBarContainer.alpha += Time.deltaTime / carapaceBarFadeTime;
      carapaceBarContainer.alpha = Mathf.Min(1, carapaceBarContainer.alpha);
    }
    carapaceBarBackground.transform.localScale = new Vector3(maxCarapace / 100, 1, 0);
    carapaceBarContentsFill.localScale = new Vector3(currentCarapace / 100, carapaceBarContentsFill.localScale.y, 0);
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
