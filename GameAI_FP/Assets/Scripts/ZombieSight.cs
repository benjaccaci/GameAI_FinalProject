using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSight : MonoBehaviour
{
    [Header("Configuration")]
    public ZombieSightConfig config;

    [Header("Wave")]
    [HideInInspector] public int waveId = -1;

    // --- Polled Properties ---
    public bool HasTarget { get; private set; }
    public Vector3 LastKnownPosition { get; private set; }
    public bool IsAlerted { get; private set; }
    public bool IsSearching { get; private set; }

    // --- Alert Chain ---
    private static int nextAlertId = 0;
    private HashSet<int> receivedAlerts = new HashSet<int>();

    // --- Damage Alert ---
    private bool damageAlertPending = false;
    private float damageAlertTimer = 0f;

    // --- Internal ---
    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        bool previousHasTarget = HasTarget;
        HasTarget = CheckVisionCone() && CheckLineOfSight();

        if (HasTarget)
        {
            LastKnownPosition = player.position;
            IsAlerted = false;

            // Start a new alert chain when we first spot the player
            if (!previousHasTarget)
            {
                int alertId = nextAlertId++;
                receivedAlerts.Add(alertId);
                StartCoroutine(PropagateAlert(LastKnownPosition, alertId, 0));
            }
        }
        else if (previousHasTarget && !HasTarget)
        {
            // Just lost sight — LastKnownPosition retains the last value
            IsAlerted = true;
        }

        // Damage alert timer
        if (damageAlertPending)
        {
            damageAlertTimer -= Time.deltaTime;
            if (damageAlertTimer <= 0f)
            {
                damageAlertPending = false;
                int alertId = nextAlertId++;
                receivedAlerts.Add(alertId);
                StartCoroutine(PropagateAlert(LastKnownPosition, alertId, 0));
            }
        }
    }

    bool CheckVisionCone()
    {
        Vector3 eyePos = transform.position + Vector3.up * config.eyeHeight;
        Vector3 dirToPlayer = player.position - eyePos;
        float distance = dirToPlayer.magnitude;

        if (distance > config.sightRange)
            return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        return angle <= config.coneHalfAngle;
    }

    bool CheckLineOfSight()
    {
        Vector3 eyePos = transform.position + Vector3.up * config.eyeHeight;
        Vector3 dirToPlayer = player.position - eyePos;
        float distance = dirToPlayer.magnitude;

        if (Physics.Raycast(eyePos, dirToPlayer.normalized, out RaycastHit hit, distance, config.obstructionMask))
        {
            // Something blocked the view before reaching the player
            return false;
        }

        return true;
    }

    /// <summary>
    /// Called by ZombieController when this zombie takes damage.
    /// Gains a one-time snapshot of the player's current position.
    /// Starts a 5-second timer to alert wave-mates if it survives.
    /// </summary>  
    public void OnDamageTaken()
    {
        if (player == null) return;

        LastKnownPosition = player.position;
        IsAlerted = true;

        if (!damageAlertPending)
        {
            damageAlertPending = true;
            damageAlertTimer = config.damageAlertDelay;
        }
    }

    /// <summary>
    /// Called by another zombie in the alert chain.
    /// </summary>
    public void ReceiveAlert(Vector3 position, int alertId, int hopCount)
    {
        if (receivedAlerts.Contains(alertId)) return;
        receivedAlerts.Add(alertId);

        // Apply noise based on hop count
        Vector3 noise = Random.insideUnitSphere * config.alertNoisePerHop * hopCount;
        noise.y = 0f;
        Vector3 noisyPosition = position + noise;

        // Snap to NavMesh
        if (NavMesh.SamplePosition(noisyPosition, out NavMeshHit hit, config.navMeshSampleRadius, NavMesh.AllAreas))
        {
            LastKnownPosition = hit.position;
        }
        else
        {
            LastKnownPosition = position; // Fallback to un-offset position
        }

        IsAlerted = true;

        // Continue the chain after delay
        StartCoroutine(PropagateAlert(position, alertId, hopCount + 1));
    }

    /// <summary>
    /// Called by ZombieController when the zombie arrives at the last known position
    /// and needs a random search point.
    /// </summary>
    public Vector3 GetSearchWanderPoint()
    {
        Vector3 randomOffset = Random.insideUnitSphere * config.searchWanderRadius;
        randomOffset.y = 0f;
        Vector3 candidatePoint = LastKnownPosition + randomOffset;

        if (NavMesh.SamplePosition(candidatePoint, out NavMeshHit hit, config.navMeshSampleRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return LastKnownPosition;
    }

    /// <summary>
    /// Gives the zombie an initial alert toward a position so it doesn't idle on spawn.
    /// </summary>
    public void AlertToPosition(Vector3 position)
    {
        LastKnownPosition = position;
        IsAlerted = true;
    }

    /// <summary>
    /// Called by ZombieController when returning to Idle so the zombie
    /// can respond to future alerts.
    /// </summary>
    public void ClearAlertState()
    {
        IsAlerted = false;
        IsSearching = false;
        receivedAlerts.Clear();
    }

    /// <summary>
    /// Called by ZombieController when transitioning to Search state.
    /// </summary>
    public void BeginSearch()
    {
        IsSearching = true;
    }

    IEnumerator PropagateAlert(Vector3 position, int alertId, int hopCount)
    {
        yield return new WaitForSeconds(config.alertHopDelay);

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy == gameObject) continue;

            ZombieSight otherSight = enemy.GetComponent<ZombieSight>();
            if (otherSight == null) continue;
            if (otherSight.waveId != waveId) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= config.alertRadius)
            {
                // Draw debug line for alert visualization
                Debug.DrawLine(
                    transform.position + Vector3.up,
                    enemy.transform.position + Vector3.up,
                    Color.magenta,
                    config.alertHopDelay + 0.5f
                );

                otherSight.ReceiveAlert(position, alertId, hopCount);
            }
        }
    }

    // --- Gizmos ---
    void OnDrawGizmosSelected()
    {
        if (config == null) return;

        Vector3 eyePos = transform.position + Vector3.up * config.eyeHeight;

        // Determine color based on state
        Color coneColor;
        if (HasTarget)
            coneColor = Color.red;
        else if (IsAlerted || IsSearching)
            coneColor = Color.yellow;
        else
            coneColor = Color.green;

        Gizmos.color = coneColor;

        // Draw cone edges
        int segments = 16;
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = Quaternion.Euler(0, 0, angle) * (Quaternion.Euler(config.coneHalfAngle, 0, 0) * Vector3.forward);
            dir = transform.rotation * dir;
            Gizmos.DrawLine(eyePos, eyePos + dir * config.sightRange);
        }

        // Draw forward direction
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(eyePos, eyePos + transform.forward * config.sightRange);

        // Draw last known position
        if (IsAlerted || IsSearching)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(LastKnownPosition, 0.5f);
        }

        // Draw alert radius
        Gizmos.color = new Color(1f, 0f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, config.alertRadius);
    }
}
