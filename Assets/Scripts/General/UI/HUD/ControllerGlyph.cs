using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class ControllerGlyph : MonoBehaviour
{
  public string actionName;
  public Image glyphImage;
  Sprite _glyphSprite;
  void Update()
  {
    Sprite glyphSprite = GameMaster.Instance.inputGlyphHelper.GetInputGlyphForAction(actionName);
    if (glyphSprite != _glyphSprite)
    {
      _glyphSprite = glyphSprite;
      glyphImage.sprite = _glyphSprite;
    }
  }
}