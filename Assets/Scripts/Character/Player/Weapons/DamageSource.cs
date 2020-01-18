
using System;
using UnityEngine;

public class DamageSource : MonoBehaviour
{
    protected string _sourceString = "";
    public string sourceString
    {
        get
        {
            if (_sourceString == "")
            {
                _sourceString = Guid.NewGuid().ToString("N").Substring(0, 15);
            }
            return _sourceString;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.gameObject.SendMessage("TakeDamage", this, SendMessageOptions.DontRequireReceiver);
    }

    public virtual bool IsOwnedBy(Character c)
    {
        return false;
    }

    public virtual bool IsSameOwnerType(Character c)
    {
        return false;
    }
    public virtual int GetDamageAmount()
    {
        return 0;
    }

    public virtual DamageType GetDamageType()
    {
        return DamageType.Physical;
    }

    public virtual float GetStun()
    {
        return 0;
    }
    public virtual bool IgnoresInvulnerability()
    {
        return false;
    }

    public virtual float GetInvulnerabilityWindow(Character c)
    {
        return 0;
    }

    public virtual Vector3 GetKnockback()
    {
        return Vector3.zero;
    }

    public virtual bool ForcesItemDrop()
    {
        return false;
    }
}