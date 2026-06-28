using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class NpcController : MonoBehaviour
{
    public enum State { RidingToSpot, WalkingToDoor, AtDoor, WalkingToMotor, Leaving }

    [Header("Settings")]
    public float rideSpeed = 4f;
    public float walkSpeed = 2f;
    public float doorWaitTime = 7f;

    [Header("Offsets")]
    public Vector3 seatOffset = new Vector3(0f, 0.6f, 0f);
    public float parkYaw = 0f;   // offset arah hadap kendaraan relatif rotasi spot
    public float walkY = 0f;     // ketinggian Y NPC saat jalan kaki
    public Vector3 dismountOffset = new Vector3(1.5f, 0f, 0f); // titik turun, relatif arah trolley (samping)

    [System.Serializable]
    public class CharacterVariant
    {
        public string nama;            // contoh: "kotaksusu", "rinso"
        public GameObject ridingModel; // pose berkendara
        public GameObject walkingModel;// model animasi jalan
    }

    [Header("Karakter (dipilih random tiap spawn)")]
    public CharacterVariant[] characters;

    // Varian terpilih untuk instance ini
    GameObject ridingModel;
    GameObject walkingModel;

    [Header("Icon Tagih (!)")]
    public Vector3 iconOffset = new Vector3(0f, 2.5f, 0f);       // offset dari kepala karakter
    public float iconScale = 0.5f;                              // ukuran icon
    public Color iconColor = new Color(1f, 0.9f, 0.2f, 1f);     // kuning cerah
    Transform _iconTf;

    public State CurrentState { get; private set; }
    public bool IsPaid { get; private set; }

    GameObject _motor;
    NavMeshAgent _motorAgent;
    ParkingSpot _targetSpot;
    Vector3 _exitPoint;
    Transform _doorPoint;
    Transform _preDoorPoint;
    Vector3 _originalWorldScale;

    public void Init(GameObject motor, ParkingSpot spot, Vector3 exitPoint, Transform doorPoint, Transform preDoorPoint)
    {
        _motor = motor;
        _targetSpot = spot;
        _exitPoint = exitPoint;
        _doorPoint = doorPoint;
        _preDoorPoint = preDoorPoint;
        _originalWorldScale = transform.lossyScale;

        // Kendaraan pakai NavMeshAgent + avoidance (tidak boleh nembus saat berkendara)
        _motorAgent = motor.GetComponent<NavMeshAgent>();
        if (_motorAgent != null)
        {
            _motorAgent.speed = rideSpeed;
            _motorAgent.angularSpeed = 300f;
            _motorAgent.acceleration = 8f;
            _motorAgent.stoppingDistance = 0.15f;
            _motorAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            _motorAgent.avoidancePriority = Random.Range(20, 60); // variasi biar tidak deadlock
        }

        SelectRandomCharacter();
        CreateIcon();

        SitOnMotor();
        CurrentState = State.RidingToSpot;
        StartCoroutine(RideToSpot());
    }

    void CreateIcon()
    {
        var go = new GameObject("TagihIcon");
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = "!";
        tmp.fontSize = 12;
        tmp.color = iconColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.rectTransform.sizeDelta = new Vector2(2f, 2f);
        go.transform.localScale = Vector3.one * iconScale;
        _iconTf = go.transform;
        go.SetActive(false);
    }

    void LateUpdate()
    {
        if (_iconTf == null) return;

        bool show = CanInteract();
        if (_iconTf.gameObject.activeSelf != show)
            _iconTf.gameObject.SetActive(show);
        if (!show) return;

        // Basis posisi = model yang sedang tampil (visual karakter), bukan pivot root
        Transform basis = (walkingModel != null && walkingModel.activeInHierarchy)
            ? walkingModel.transform
            : transform;

        // Posisi di atas kepala + hadap kamera (billboard)
        _iconTf.position = basis.position + iconOffset;
        var cam = Camera.main;
        if (cam != null)
            _iconTf.rotation = Quaternion.LookRotation(_iconTf.position - cam.transform.position);
    }

    void OnDestroy()
    {
        if (_iconTf != null) Destroy(_iconTf.gameObject);
    }

    void SelectRandomCharacter()
    {
        if (characters == null || characters.Length == 0) return;

        // Sembunyikan SEMUA model dari semua varian dulu
        foreach (var c in characters)
        {
            if (c.ridingModel != null)  c.ridingModel.SetActive(false);
            if (c.walkingModel != null) c.walkingModel.SetActive(false);
        }

        // Pilih satu varian random
        var pick = characters[Random.Range(0, characters.Length)];
        ridingModel  = pick.ridingModel;
        walkingModel = pick.walkingModel;
    }

    void ShowRiding()
    {
        if (ridingModel == null || walkingModel == null) return;
        ridingModel.SetActive(true);
        walkingModel.SetActive(false);
    }

    void ShowWalking()
    {
        if (ridingModel == null || walkingModel == null) return;
        ridingModel.SetActive(false);
        walkingModel.SetActive(true);
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
        ShowRiding();
    }

    IEnumerator RideToSpot()
    {
        if (_motorAgent == null) yield break;

        _motorAgent.SetDestination(_targetSpot.transform.position);
        yield return null;

        Vector3 spotXZ = _targetSpot.transform.position;
        Vector3 lastPos = _motor.transform.position;
        float stuckTimer = 0f;
        while (true)
        {
            Vector3 m = _motor.transform.position;
            float distXZ = Vector2.Distance(new Vector2(m.x, m.z), new Vector2(spotXZ.x, spotXZ.z));
            if (distXZ <= 0.5f) break;

            if ((m - lastPos).sqrMagnitude < 0.00005f && !_motorAgent.pathPending)
                stuckTimer += Time.deltaTime;
            else
                stuckTimer = 0f;
            if (stuckTimer > 0.4f) break;

            lastPos = m;
            yield return null;
        }

        _motorAgent.enabled = false;

        Vector3 snapPos = _targetSpot.transform.position;
        snapPos.y = _motor.transform.position.y;
        _motor.transform.position = snapPos;
        float spotYaw = _targetSpot.transform.eulerAngles.y;
        _motor.transform.rotation = Quaternion.Euler(0f, spotYaw + parkYaw, 0f);
        _targetSpot.Occupy(_motor, this);

        // NPC turun di samping trolley (titik tetap, konsisten semua karakter)
        transform.SetParent(null);
        Vector3 dismountPos = _motor.transform.position + _motor.transform.rotation * dismountOffset;
        dismountPos.y = walkY;
        transform.position = dismountPos;
        ShowWalking();

        CurrentState = State.WalkingToDoor;
        StartCoroutine(WalkSequence());
    }

    IEnumerator WalkSequence()
    {
        // 1. Ke PreDoorPoint (lurus) — biar masuk gedung lewat pintu, tidak tembus tembok
        if (_preDoorPoint != null)
            yield return StartCoroutine(WalkManual(_preDoorPoint.position, 0.1f));

        // 2. PreDoorPoint → DoorPoint (masuk gedung)
        yield return StartCoroutine(WalkManual(_doorPoint.position, 0.3f));

        CurrentState = State.AtDoor;
        yield return new WaitForSeconds(doorWaitTime);

        // 3. DoorPoint → PreDoorPoint (keluar gedung) — stop ketat biar pas di titik
        if (_preDoorPoint != null)
            yield return StartCoroutine(WalkManual(_preDoorPoint.position, 0.1f));

        // 4. Balik ke kendaraan — tagih [E] di fase ini
        CurrentState = State.WalkingToMotor;
        yield return StartCoroutine(WalkManual(_motor.transform.position, 0.5f));

        if (!IsPaid)
            StartCoroutine(LeaveRoutine());
    }

    // Jalan manual lurus ke target (jalan kaki — boleh tembus, langsung ke titik)
    IEnumerator WalkManual(Vector3 dest, float stop)
    {
        Vector3 target = dest;
        target.y = transform.position.y;
        while (Vector3.Distance(transform.position, target) > stop)
        {
            target = dest;
            target.y = transform.position.y;
            transform.position = Vector3.MoveTowards(
                transform.position, target, walkSpeed * Time.deltaTime);
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized);
            yield return null;
        }
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

        yield return StartCoroutine(WalkManual(_motor.transform.position, 0.5f));

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
        if (agent.remainingDistance > agent.stoppingDistance + 0.1f) return false;
        return !agent.hasPath || agent.velocity.sqrMagnitude < 0.01f;
    }

    public bool CanInteract() => CurrentState == State.WalkingToMotor && !IsPaid;
}
