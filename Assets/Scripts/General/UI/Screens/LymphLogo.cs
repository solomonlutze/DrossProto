
using UnityEngine;
using UnityEngine.UI;

public class LymphLogo : MonoBehaviour
{
  public void Init(LymphType type)
  {
    if (GameMaster.Instance.lymphTypeToSpriteMapping.ContainsKey(type))
    {
      GetComponent<Image>().sprite = GameMaster.Instance.lymphTypeToSpriteMapping[type];
    }
    else
    {
      GetComponent<Image>().sprite = null;
    }
  }
}
