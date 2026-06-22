using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NpcController : MonoBehaviour
{
    public enum State { RidingToSpot, WalkingToDoor, AtDoor, WalkingToMotor, Leaving }

    [Header("Settings")]
    public float rideSpeed = 4f;
    public float walkSpeed = 2f;
    public float doorWaitTime = 7f;

    [Header("Offsets")]
    public Vector3 seatOffset = new Vector3(0f, 0.6f, 0f);

    public State CurrentState { get; private set; }
    public bool IsPaid { get; private set; }

    GameObject _motor;
    NavMeshAgent _motorAgent;
    ParkingSpot _targetSpot;
    Vector3 _exitPoint;
    Transform _doorPoint;
    Vector3 _originalWorldScale;

    public void Init(GameObject motor, ParkingSpot spot, Vector3 exitPoint, Transform doorPoint)
    {
        _motor = motor;
        _targetSpot = spot;
        _exitPoint = exitPoint;
        _doorPoint = doorPoint;
        _originalWorldScale = transform.lossyScale;

        _motorAgent = motor.GetComponent<NavMeshAgent>();
        if (_motorAgent != null)
        {
            _motorAgent.speed = rideSpeed;
            _motorAgent.angularSpeed = 300f;
            _motorAgent.acceleration = 8f;
            _motorAgent.stoppingDistance = 0.15f;
        }

        SitOnMotor();
        CurrentState = State.RidingToSpot;
        StartCoroutine(RideToSpot());
    }

    void SitOnMotor()
    {
        transform.SetParent(_motor.transform);
        transform.localPosition = seatOffset;
        transform.localRotation = Quaternion.identity;
        Vector3 ms = _motor.transform.lossyScale;
        transform.localScale = new Vector3(
            _originalWorldScale.x / ms.x,
            _originalWorldScale.y / ms.y,
            _originalWorldScale.z / ms.z);
    }

    IEnumerator RideToSpot()
    {
        if (_motorAgent == null) yield break;

        _motorAgent.SetDestination(_targetSpot.transform.position);
        yield return null;

        while (!IsAgentArrived(_motorAgent))
            yield return null;

        Vector3 snapPos = _targetSpot.transform.position;
        snapPos.y = _motor.transform.position.y;
        _motor.transform.position = snapPos;
        _motorAgent.enabled = false;
        _targetSpot.Occupy(_motor, this);

        // NPC turun — pertahankan Y world agar tidak lompat
        transform.SetParent(null);
        CurrentState = State.WalkingToDoor;
        StartCoroutine(WalkToDoor());
    }

    IEnumerator WalkToDoor()
    {
        Vector3 target = _doorPoint.position;
        target.y = transform.position.y; // pertahankan Y NPC

        while (Vector3.Distance(transform.position, target) > 0.3f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target, walkSpeed * Time.deltaTime);
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized);
            yield return null;
        }

        CurrentState = State.AtDoor;
        yield return new WaitForSeconds(doorWaitTime);

        CurrentState = State.WalkingToMotor;
        StartCoroutine(WalkToMotor());
    }

    IEnumerator WalkToMotor()
    {
        Vector3 target = _motor.transform.position;
        target.y = transform.position.y;

        while (Vector3.Distance(transform.position, target) > 0.5f)
        {
            target = _motor.transform.position;
            target.y = transform.position.y;
            transform.position = Vector3.MoveTowards(
                transform.position, target, walkSpeed * Time.deltaTime);
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized);
            yield return null;
        }

        if (!IsPaid)
            StartCoroutine(LeaveRoutine());
    }

    public void PayAndLeave()
    {
        if (!CanInteract()) return;
        IsPaid = true;
        StopAllCoroutines();
        StartCoroutine(LeaveRoutine());
    }

    IEnumerator LeaveRoutine()
    {
        CurrentState = State.Leaving;

        // Jalan manual ke motor
        Vector3 motorPos = _motor.transform.position;
        motorPos.y = transform.position.y;
        while (Vector3.Distance(transform.position, motorPos) > 0.5f)
        {
            motorPos = _motor.transform.position;
            motorPos.y = transform.position.y;
            transform.position = Vector3.MoveTowards(
                transform.position, motorPos, walkSpeed * Time.deltaTime);
            Vector3 dir = motorPos - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized);
            yield return null;
        }

        SitOnMotor();
        _targetSpot.Vacate();

        if (_motorAgent != null)
        {
            _motorAgent.enabled = true;
            _motorAgent.speed = rideSpeed;
            _motorAgent.stoppingDistance = 0.2f;
            _motorAgent.SetDestination(_exitPoint);
            yield return null;
            while (!IsAgentArrived(_motorAgent))
                yield return null;
        }

        Destroy(_motor);
        Destroy(gameObject);
    }

    bool IsAgentArrived(NavMeshAgent agent)
    {
        if (!agent.enabled || !agent.isOnNavMesh) return true;
        if (agent.pathPending) return false;
        return agent.remainingDistance <= agent.stoppingDistance;
    }

    public bool CanInteract() => CurrentState == State.WalkingToMotor && !IsPaid;
}
