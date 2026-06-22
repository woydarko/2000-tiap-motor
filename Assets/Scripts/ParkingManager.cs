using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ParkingManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject motorPrefab;
    public GameObject npcPrefab;

    [Header("Spots")]
    public ParkingSpot[] spots;

    [Header("Spawn & Door Points")]
    public Transform spawnPoint1;
    public Transform spawnPoint2;
    public Transform doorPoint;

    [Header("Settings")]
    public float spawnInterval = 15f;
    public float motorGroundOffset = 0.3f;

    float _timer;

    void Start()
    {
        _timer = spawnInterval - 2f;

        // Auto-find jika belum di-assign
        if (spawnPoint1 == null) spawnPoint1 = GameObject.Find("SpawnPoint1")?.transform;
        if (spawnPoint2 == null) spawnPoint2 = GameObject.Find("SpawnPoint2")?.transform;
        if (doorPoint  == null) doorPoint  = GameObject.Find("DoorPoint")?.transform;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            TrySpawnNpc();
        }
    }

    void TrySpawnNpc()
    {
        ParkingSpot emptySpot = GetEmptySpot();
        if (emptySpot == null) return;
        SpawnNpc(emptySpot);
    }

    void SpawnNpc(ParkingSpot targetSpot)
    {
        // Pilih spawn point random
        bool fromFirst = Random.value > 0.5f;
        Transform spawnTf = fromFirst ? spawnPoint1 : spawnPoint2;
        Transform exitTf  = fromFirst ? spawnPoint2 : spawnPoint1;

        Vector3 motorPos = spawnTf.position;
        motorPos.y += motorGroundOffset;

        GameObject motor = Instantiate(motorPrefab, motorPos, Quaternion.identity);
        GameObject npcGO = Instantiate(npcPrefab, motorPos, Quaternion.identity);

        // Pastikan NavMeshAgent ada di motor dan NPC
        // Hanya motor yang pakai NavMeshAgent (navigasi hindari wall)
        NavMeshAgent motorAgent = motor.GetComponent<NavMeshAgent>();
        if (motorAgent == null) motorAgent = motor.AddComponent<NavMeshAgent>();
        motorAgent.baseOffset = motorGroundOffset;

        NpcController npc = npcGO.GetComponent<NpcController>();
        npc.Init(motor, targetSpot, exitTf.position, doorPoint);
    }

    ParkingSpot GetEmptySpot()
    {
        var empty = new List<ParkingSpot>();
        foreach (var s in spots)
            if (!s.IsOccupied) empty.Add(s);

        if (empty.Count == 0) return null;
        return empty[Random.Range(0, empty.Count)];
    }
}
