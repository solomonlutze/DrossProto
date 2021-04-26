
using UnityEngine;
using UnityEngine.UI;
public class CharacterBodyPartVisual_Old : MonoBehaviour
{
  public SpriteRenderer spriteRenderer1;
  public SpriteRenderer spriteRenderer2;
  public Image image1;
  public Image image2;

  public void SetSpritesFromTrait(Trait trait)
  {

    if (spriteRenderer1 != null)
    {
      spriteRenderer1.sprite = trait.visual1;
    }
    if (spriteRenderer2 != null)
    {
      spriteRenderer2.sprite = trait.visual2;
    }
    if (image1 != null)
    {
      image1.sprite = trait.visual1;
    }
    if (image2 != null)
    {
      image2.sprite = trait.visual2;
    }
  }

  public void ClearSprites()
  {
    if (spriteRenderer1 != null)
    {
      spriteRenderer1.sprite = null;
    }
    if (spriteRenderer2 != null)
    {
      spriteRenderer2.sprite = null;
    }
    if (image1 != null)
    {
      image1.sprite = null;
    }
    if (image2 != null)
    {
      image2.sprite = null;
    }
  }
}