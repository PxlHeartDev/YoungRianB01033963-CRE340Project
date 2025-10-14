using Mono.Cecil;
using UnityEngine;

public interface IDamageable
{
    // Should instantly kill the object
    public abstract void Kill(MonoBehaviour source);
    // Should damage the object
    public abstract void Damage(int dmg, MonoBehaviour source);
}
