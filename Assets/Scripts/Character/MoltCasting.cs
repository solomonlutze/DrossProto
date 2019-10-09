using UnityEngine;

public class MoltCasting : MonoBehaviour
{

  public SpriteRenderer moltCasingSprite;

  public void Init(Color casingColor, Character owner)
  {
    WorldObject.ChangeLayersRecursively(transform, owner.currentFloor);
    moltCasingSprite.sprite = owner.mainRenderer.sprite;
    moltCasingSprite.color = casingColor;
  }
}