using UnityEngine;

public class PatrolState : BaseState
{
    private bool _isMoving;
    private bool _isWaiting;

    private float _waitTime = 2f; // Lama idle
    private float _waitTimer;

    private Vector3 _destination;

    // Radius untuk mengecek apakah waypoint sedang ditempati agent lain
    private float _agentCheckRadius = 0.7f;

    public void EnterState(Enemy enemy)
    {
        _isMoving = false;
        _isWaiting = false;
        _waitTimer = 0f;
    }

    public void UpdateState(Enemy enemy)
    {
        // Switch ke chase
        if (Vector3.Distance(enemy.transform.position, enemy.Player.transform.position) < enemy.ChaseDistance)
        {
            enemy.SwitchState(enemy.ChaseState);
            return;
        }

        // Sedang wait
        if (_isWaiting)
        {
            _waitTimer += Time.deltaTime;

            if (_waitTimer >= _waitTime)
            {
                _isWaiting = false;
                _isMoving = false;
            }

            return;
        }

        // Mulai cari waypoint jika belum bergerak
        if (!_isMoving)
        {
            _isMoving = true;

            Transform nextWaypoint = GetSafeWaypoint(enemy);

            if (nextWaypoint == null)
            {
                // Tidak ada waypoint aman → idle sebentar
                _isWaiting = true;
                _waitTimer = 0f;
                enemy.NavMeshAgent.ResetPath();
                return;
            }

            _destination = nextWaypoint.position;
            enemy.NavMeshAgent.SetDestination(_destination);
        }
        else
        {
            // Jika sudah sampai → idle
            if (!enemy.NavMeshAgent.pathPending &&
                enemy.NavMeshAgent.remainingDistance <= enemy.NavMeshAgent.stoppingDistance)
            {
                enemy.NavMeshAgent.ResetPath();
                _isWaiting = true;
                _waitTimer = 0f;
            }
        }
    }

    public void ExitState(Enemy enemy)
    {
        Debug.Log("Stop Patrol");
    }

    // -------------------------------------------------------
    //           FUNGSI CEK WAYPOINT AMAN
    // -------------------------------------------------------
    private Transform GetSafeWaypoint(Enemy enemy)
    {
        int attempts = 0;
        int maxAttempts = enemy.Waypoints.Count;

        while (attempts < maxAttempts)
        {
            int index = Random.Range(0, enemy.Waypoints.Count);
            Transform wp = enemy.Waypoints[index];

            // Cek apakah ada agent lain di waypoint ini
            if (!IsWaypointOccupied(enemy, wp.position))
            {
                return wp;
            }

            attempts++;
        }

        // Semua waypoint penuh
        return null;
    }

    // -------------------------------------------------------
    //       DETECTION: APAKAH ADA AGENT DI WAYPOINT?
    // -------------------------------------------------------
    private bool IsWaypointOccupied(Enemy self, Vector3 waypointPos)
    {
        Collider[] hits = Physics.OverlapSphere(waypointPos, _agentCheckRadius);

        foreach (var h in hits)
        {
            // Cegah mendeteksi dirinya sendiri
            if (h.transform == self.transform)
                continue;

            // Cek apakah objek ini punya Enemy component (agent lain)
            if (h.GetComponent<Enemy>() != null)
            {
                return true; // waypoint sedang ditempati agent lain
            }
        }

        return false;
    }
}
