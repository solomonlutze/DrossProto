using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableProjectileItem : ConsumableItemData {
    
    public Projectile projectilePrefab;
    
	// public override void Use(Character owner) {
    //     Projectile projectile = Instantiate(projectilePrefab, owner.gameObject.transform.position, owner.gameObject.transform.rotation).GetComponent<Projectile>();
    //     Physics2D.IgnoreCollision(projectile.GetComponent<Collider2D>(), owner.GetComponent<Collider2D>());
    //     Debug.Log("initing projectilePrefab with owner "+owner);
    //     projectile.Init(owner);
    // }
}
