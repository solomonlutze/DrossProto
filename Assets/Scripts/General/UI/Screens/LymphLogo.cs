
using UnityEngine;
using UnityEngine.UI;

public class LymphLogo : MonoBehaviour
{
    public void Init(LymphType type) {
      GetComponent<Image>().sprite = GameMaster.Instance.lymphTypeToSpriteMapping[type];
    }
}
