using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemMaster : MonoBehaviour
{

  public ParticleSystem testPS;

  void Start()
  {
    testPS.Stop();
  }
  public void EmitFootstep(Character c, EnvironmentTile tile, ParticleSystem.EmitParams emitParams, int count)
  {
    Debug.Log("emitting footstep particle at " + emitParams.position);
    testPS.Emit(emitParams, count);
    // testPS.GetParticles
  }
}
