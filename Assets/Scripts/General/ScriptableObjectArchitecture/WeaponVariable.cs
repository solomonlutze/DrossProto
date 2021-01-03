using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectArchitecture
{

  [CreateAssetMenu(
      fileName = "WeaponVariable.asset",
      menuName = SOArchitecture_Utility.VARIABLE_SUBMENU + "Weapon",
      order = SOArchitecture_Utility.ASSET_MENU_ORDER_COLLECTIONS + 0)]
  public sealed class WeaponVariable : BaseVariable<Weapon>
  {
  }
}