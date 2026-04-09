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
    public virtual bool OverridesInvestigate() => false;
    public virtual bool OverridesSearch() => false;

    // idle zombie
    public virtual void OnIdle() { }
    // chasing player
    public virtual void OnChasing() { }
    // attacking player
    public virtual void OnAttacking() { }
    // investigating last known position
    public virtual void OnInvestigating() { }
    // searching area after arriving at last known position
    public virtual void OnSearching() { }
    // zombie dies
    public virtual void OnDeath() { }
    // zombie takes damage
    public virtual void OnDamaged() { }
}