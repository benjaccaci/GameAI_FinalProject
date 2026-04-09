using UnityEngine;

// abstracted base class for all of the zombie variants! the subclasses are the different variants

public abstract class ZombieVariantBehavior : MonoBehaviour
{
    protected ZombieController controller;
    protected virtual void Awake()
    {
        controller = GetComponent<ZombieController>();
        if (controller == null)
            Debug.LogWarning(gameObject.name + "ZombieVariantBehavior requires a ZombieController on the game object");
    }

    public virtual bool OverridesChase() => false;
    public virtual bool OverridesAttack() => false;
    
    // idle zombie
    public virtual void OnIdle() { }
    // chasing player
    public virtual void OnChasing() { }
    // attacking player
    public virtual void OnAttacking() { }
    // zombie dies
    public virtual void OnDeath() { }
    // zombie takes damage
    public virtual void OnDamaged() { }
}