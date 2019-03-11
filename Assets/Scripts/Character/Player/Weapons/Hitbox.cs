using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType {
    Physical,
    Heat,
    Cold

}

public enum HitboxShape { Box, Circle, Spawn };

// TODO: Currently only used by trait-spawned attacks. Should also be used by weapon-spawned attacks.
public enum HitboxSpawnPoint { CenteredOnPlayer, CenteredOnCrosshair }

[System.Serializable]
public class DamageObject {

    // amount of damage we do
    public float damage;
    // amount of impulse force applied on hit

    public float knockback;
    // Duration in seconds of stun. TODO: should this value represent something else?
    public float stun;

    public Transform hitboxTransform;

    public Transform attackerTransform;
    // amount of invulnerability this attack imparts. TODO: separate windows per damage source?
    public float invulnerabilityWindow;
    // whether this damage object should respect invulnerability from OTHER targets
    public bool ignoreInvulnerability;

    public TileDurability durabilityDamageLevel = TileDurability.Delicate;

    public DamageType damageType;

    // public DamageObject() {}
    public DamageObject(float d, float k, float s, float i, DamageType dt, TileDurability durabilityDamage, Transform hitboxT, Transform attackerT) {
        damage = d;
        knockback = k;
        stun = s;
        invulnerabilityWindow = i;
        durabilityDamageLevel = durabilityDamage;
        hitboxTransform = hitboxT;
        damageType = DamageType.Physical;
        attackerTransform = attackerT;
    }
}

// Damage-dealing hitbox object.
[System.Serializable]
public class HitboxInfo
{
    public string id;
    public string hitboxDescription;
    // should this hitbox follow our initializingTransform; if false, it just hangs out in worldspace
    public bool followInitializingTransform;
    public float duration;
    public HitboxShape hitboxShape;
    // used only for circle shape.
    public float radius;

    // Used only for box shape.
    public Vector2 size;
    public DamageObject damageMultipliers;
}
public class Hitbox : MonoBehaviour {

    //TODO: Size things
    // - should be possible to set the shape: square, circle, or hitboxSpawn
    // - should be possible to set radius, xy, or neither (for hitboxSpawn)

    //how long this hitbox should last for
    public float duration;

    // what weapon does this hitbox belong to
    public Transform owningTransform;
    // damage we will apply on a hit
    public DamageObject damageObj;
    public Constants.FloorLayer floor;

	// Use this for initialization
	void Start () {
	}

    // Characteristics of hitbox initialized by weapon.
    public void Init(Transform initializingTransform, Character owner, HitboxInfo info) {
        duration = info.duration;
        // WARNING: duration of 0 means a hitbox needs to be cleaned up manually
        if (duration > 0) {
		    StartCoroutine(CleanUpSelf());
        }
        damageObj = new DamageObject(
            info.damageMultipliers.damage,
            info.damageMultipliers.knockback,
            info.damageMultipliers.stun,
            info.damageMultipliers.invulnerabilityWindow,
            DamageType.Physical,
            info.damageMultipliers.durabilityDamageLevel,
            transform,
            owner.transform
        );
        owningTransform = initializingTransform;
        floor = owner.currentFloor;
        switch (info.hitboxShape) {
            case HitboxShape.Box:
                transform.localScale = info.size;
                break;
            case HitboxShape.Circle:
                Destroy(GetComponent<BoxCollider2D>());
                CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
                circleCol.radius = info.radius;
                break;
            case HitboxShape.Spawn:
                break;
        }
        if (info.followInitializingTransform && initializingTransform != null) {
            transform.parent = initializingTransform;
        }
        if (info.hitboxShape == HitboxShape.Spawn) {
            transform.localScale = Vector2.one;
        }
    }

    // Destroys this hitbox. Waits until end of frame so it doesn't get cleaned up same frame; not sure
    // if that's necessary or not
    IEnumerator CleanUpSelf() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    void Update() {
		HandleTile();
    }

	void HandleTile() {
        // TODO: this should work for other collider shapes, not just boxes.
        // It should also work for bigger box colliders, which this doesn't.
        BoxCollider2D b = GetComponent<BoxCollider2D>();
        Vector3[] corners = {
            transform.TransformPoint(b.offset + new Vector2(b.size.x, b.size.y)*0.5f),
            transform.TransformPoint(b.offset + new Vector2(-b.size.x, b.size.y)*0.5f),
            transform.TransformPoint(b.offset + new Vector2(b.size.x, -b.size.y)*0.5f),
            transform.TransformPoint(b.offset + new Vector2(-b.size.x, -b.size.y)*0.5f)
        };
        foreach (Vector3 cornerPosition in corners) {
            EnvironmentTile tile = GameMaster.Instance.GetTileAtLocation(cornerPosition, floor);
            tile.TakeDamage(cornerPosition, floor, damageObj);
        }

    }

    // Anything we hit, try to damage! This can be applied to destructible environments later too.
    void OnTriggerEnter2D(Collider2D col) {
        col.SendMessage("TakeDamage", damageObj, SendMessageOptions.DontRequireReceiver);
    }
}
