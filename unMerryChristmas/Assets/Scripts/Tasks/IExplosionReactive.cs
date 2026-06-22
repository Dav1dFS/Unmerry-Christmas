using UnityEngine;

/// <summary>
/// Implemented by scene objects that should react when an <see cref="ExplosivePresent"/>
/// detonates near them — e.g. a wreath/candle that lights, or the oven pot.
///
/// <see cref="ExplosivePresent.Explode"/> notifies every IExplosionReactive whose
/// collider lies within the blast radius exactly once per explosion. Mirrors the
/// existing <see cref="IBreakable"/> hook used by thrown objects.
/// </summary>
public interface IExplosionReactive
{
    /// <param name="origin">World position of the explosion.</param>
    /// <param name="radius">Blast radius the explosion used.</param>
    void OnExplosion(Vector3 origin, float radius);
}
