using UnityEngine;

[CreateAssetMenu(fileName = "NewZombieSightConfig", menuName = "Zombie/Sight Config")]
public class ZombieSightConfig : ScriptableObject
{
    [Header("Vision Cone")]
    [Tooltip("Maximum distance the zombie can see")]
    public float sightRange = 15f;

    [Tooltip("Half-angle of the vision cone in degrees (110 total FOV at default)")]
    public float coneHalfAngle = 55f;

    [Header("Line of Sight")]
    [Tooltip("Layers that block line of sight (set in inspector)")]
    public LayerMask obstructionMask;

    [Tooltip("Height offset from transform for the raycast origin (eye level)")]
    public float eyeHeight = 1.5f;

    [Header("Alert")]
    [Tooltip("Radius within which this zombie can alert wave-mates")]
    public float alertRadius = 10f;

    [Tooltip("Delay in seconds between each hop in the alert chain")]
    public float alertHopDelay = 0.5f;

    [Tooltip("Random position offset multiplier per hop (meters per hop count)")]
    public float alertNoisePerHop = 1.5f;

    [Tooltip("Max distance to sample NavMesh when applying alert noise")]
    public float navMeshSampleRadius = 5f;

    [Header("Search Behavior")]
    [Tooltip("How long the zombie wanders searching after reaching a stale position")]
    public float searchDuration = 6f;

    [Tooltip("Radius around last known position for random search wander points")]
    public float searchWanderRadius = 5f;

    [Header("Damage Awareness")]
    [Tooltip("Seconds after taking damage before alerting wave-mates")]
    public float damageAlertDelay = 5f;
}
