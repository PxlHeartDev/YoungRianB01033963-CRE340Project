using UnityEngine;

public interface IDamageable
{
    // Methods
    // Should instantly kill the object
    public abstract void Kill(GameObject source);
    // Should damage the object
    public abstract void Damage(int dmg, GameObject source);
}
