using UnityEngine;

[System.Serializable]
public class AiTransition
{
  public AiDecision decision;
  public AiState trueState;
  public AiState falseState;
}