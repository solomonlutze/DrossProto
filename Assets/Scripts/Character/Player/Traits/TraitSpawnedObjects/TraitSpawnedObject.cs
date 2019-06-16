using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraitSpawnedObject : MonoBehaviour
{
    protected Character owner;
    // Start is called before the first frame update
    public void Init(Character c) {
        owner = c;
    }
}
