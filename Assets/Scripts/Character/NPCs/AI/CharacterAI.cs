﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// How detection works
// The range at which an entity enters detection, by default, is its DetectableRange.
// TODO: That range could be increased or decreased by the AI's DetectableRangeModifier (not defined yet)
// The range at which an entity LEAVES detection is its DetectableRange plus the detector's detectedRangeBuffer.

public enum AiStates { PlayerAggro, Aggro, Docile }
public class CharacterAI : Character {

	// current target
	private WorldObject objectOfInterest;
	public AiStates aiState;

	public bool debugStayDocile;
	public float minDistanceFromPathNode;
	// the distance PAST the target's DetectableRange that we will continue to pursue once we've noticed them
	public float DetectableRangeBuffer;
	public float attackAngle;
	private List<Node> path;
	private Vector3 destination;
	private BoxCollider2D col;

	public List<PickupItem> spawnsOnDeath;

	override protected void Awake () {
		base.Awake();
		base.Init();
		aiState = AiStates.Docile;
		col = GetComponent<BoxCollider2D>();
		path = new List<Node>();
		destination = Vector3.zero;
		ChooseObjectOfInterest();
		StartCoroutine(CalculateRouteTowardsTarget());
	}
	// How should routing work?
	// If we can get to our target unobstructed, we should just... move towards them.
	// Otherwise, we should ask the pathfinder to a route for it.
	// When we calculate a path, we send our current position to indicate what node we're in.
	// But if we were already moving towards a node,
	// 1) that node needs to be the actual starting point for our path
	// 2) we need to finish moving towards it.
	// Otherwise we get stuck on corners.
	IEnumerator CalculateRouteTowardsTarget() {
		yield return new WaitForSeconds(Random.Range(0, 1));
		while (true) {
			if (objectOfInterest != null) {
				// if (GetPathOpen()) {
				if (GameMaster.Instance.IsPathClearOfHazards(col, objectOfInterest.GetTileLocation(), this)) {
					path = null;
				} else {
					path = GameMaster.Instance.FindPath(transform.TransformPoint(col.offset), objectOfInterest.GetTileLocation(), this);
				}
			}
			yield return new WaitForSeconds(.5f);
		}
	}

	protected override void Update() {
		ChooseObjectOfInterest();
		ChooseAIState();
		OrientAndInputMovement();
		base.Update();
		HandleBehavior();
	}

	// for movement, we have:
	// target (transform), the place/thing we ultimately wanna get to;
	// path (Node[]), the list of intermediate destinations to get to our target
	// if our path is EMPTY we should assume it's unobstructed, and try to get to our target.
	// TODO: handle the case where no path is valid (we should probably lose interest)

	void OrientAndInputMovement() {
		orientTowards = Vector3.zero;
		if (objectOfInterest != null) {
			orientTowards = objectOfInterest.transform.position;
		}
	}

	protected override void HandleTile() {
		base.HandleTile();
		EnvironmentTileInfo tile = GridManager.Instance.GetTileAtLocation(CalculateCurrentTileLocation());
		if (tile.ChangesFloorLayer()
			&& path != null
			&& path.Count >= 1
			&& (path[0].floor == tile.GetTargetFloorLayer() || path[1] != null || path[1].floor == tile.GetTargetFloorLayer())
		) {
			UseTile();
		}
		// 	Debug.Log("targetFloorLayer: "+tile.targetFloorLayer);
		// 	Debug.Log("justCameFromFloor: "+justCameFromFloor);

		// 	SetCurrentFloor(tile.targetFloorLayer);
		// }
	}

	void InputAggroMovement() {
		if (path == null) {
			// Debug.DrawLine(objectOfInterest.position, transform.position, Color.green, .25f, true);
			movementInput = (objectOfInterest.transform.position - transform.position).normalized;
		}
		else if (path.Count > 0) {
			Vector3 nextNodeLocation = new Vector3(path[0].x+.5f, path[0].y+.5f, 0);
			Vector3 colliderCenterWorldSpace = transform.TransformPoint(col.offset);
			movementInput = (nextNodeLocation - colliderCenterWorldSpace).normalized;
			Debug.DrawLine(nextNodeLocation, colliderCenterWorldSpace, Color.red, .25f, true);
			if (Vector3.Distance(nextNodeLocation, colliderCenterWorldSpace) < minDistanceFromPathNode) {
				path.RemoveAt(0);
			}
		} else {
			movementInput = Vector2.zero;
		}
	}

	void InputIdleMovement() {
		movementInput = Vector2.zero;
	}

	// TODO: this should probably be state-driven
	void ChooseObjectOfInterest() {
		PlayerController player = GameMaster.Instance.GetPlayerController();
		if (player != null) {
			objectOfInterest = (WorldObject) player;
		}
		// objectOfInterest = (WorldObject) gameMaster.GetPlayerController().gameObject.GetComponent<Character>();
		// switch(aiState) {
		// 	case AiStates.PlayerAggro:
		// 		objectOfInterest = (WorldObject) gameMaster.GetPlayerController().gameObject.GetComponent<Character>();
		// 		Debug.Log("setting objectOfInterest: "+objectOfInterest);
		// 		break;
		// 	default:
		// 		return;
		// }
	}

	// Ideally, we have a list of objects of interest based on what AI state they drive.
	// E.G. If we have an AI state "scavenging", we should have a list of items (or components?) that drive that behavior
	//

	void ChooseAIState() {
		if (debugStayDocile) {
			aiState = AiStates.Docile;
			return;
		}
		switch(aiState) {
			case AiStates.Aggro:
			case AiStates.PlayerAggro:
				if (objectOfInterest == null
					|| (objectOfInterest.transform.position - transform.position).sqrMagnitude >
					(objectOfInterest.DetectableRange + DetectableRangeBuffer) * (objectOfInterest.DetectableRange + DetectableRangeBuffer)) { // our target is gone
					aiState = AiStates.Docile;
				}
				break;
			case AiStates.Docile:
				if (objectOfInterest != null) { // we got new target
					Vector3 distanceFromTarget = objectOfInterest.transform.position - transform.position;
					if (distanceFromTarget.sqrMagnitude < objectOfInterest.DetectableRange * objectOfInterest.DetectableRange) {
						aiState = AiStates.PlayerAggro;
					}
				}
				break;
			default:
				return;
		}
	}

	// cogito ergo sum
	// dear past sol: the above was a stupid joke. I expected better of you. sincerely, a less-past sol
	void HandleBehavior() {
		switch(aiState) {
			case AiStates.Aggro:
			case AiStates.PlayerAggro:
				HandleAggroBehavior();
				InputAggroMovement();
				break;
			default:
				InputIdleMovement();
				return;
		}
	}

	bool WithinRangeOfTarget() {
		return objectOfInterest != null
			&& Vector3.Distance(objectOfInterest.transform.position, transform.position)
		  		< characterAttack.range + Character.GetAttackValueModifier(attackModifiers, CharacterAttackValue.Range);
	}
	void HandleAggroBehavior() {
		if (characterAttack != null
		  && WithinRangeOfTarget()
		  && GetAngleToTarget() < attackAngle
		) {
			Attack();
		}
 		// if (weapon != null
		//   && objectOfInterest != null
		//   && Vector3.Distance(objectOfInterest.transform.position, transform.position) < weapon.weaponData.range
		//   && GetAngleToTarget() < weapon.weaponData.attackAngle) {
		// 	Attack();
		// }
	}

	private void SpawnDroppedItems() {
		foreach(PickupItem item in spawnsOnDeath) {
			GameObject instantiatedItem = Instantiate(item.gameObject, transform.position, transform.rotation);
			instantiatedItem.layer = gameObject.layer;
			instantiatedItem.GetComponent<SpriteRenderer>().sortingLayerName = LayerMask.LayerToName(gameObject.layer);
		}
	}

	public override void Die(){
		SpawnDroppedItems();
		base.Die();
	}
}
