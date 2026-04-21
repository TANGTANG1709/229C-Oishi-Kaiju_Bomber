using UnityEngine;

/// <summary>
/// ProjectileBase — shared component on all enemy projectiles.
/// Lets KaijuController read damage value from OnTriggerEnter2D without knowing the type.
/// </summary>
public class ProjectileBase : MonoBehaviour
{
    public int damage = 10;
}
