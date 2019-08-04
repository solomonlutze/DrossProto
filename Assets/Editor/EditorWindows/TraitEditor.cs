using UnityEditor;

[CustomEditor(typeof(PassiveTrait))]
public class PassiveTraitEditor : Editor
{
  // possibly necessary to support property drawer?
}


[CustomEditor(typeof(ActiveTrait))]
public class ActiveTraitEditor : Editor
{
  // possibly necessary to support property drawer?
}

[CustomEditor(typeof(ActiveTraitEffect))]
public class ActiveTraitEffectEditor : Editor
{
  // possibly necessary to support property drawer?
}
