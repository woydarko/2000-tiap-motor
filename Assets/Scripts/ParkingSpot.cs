using UnityEngine;

public class ParkingSpot : MonoBehaviour
{
    public bool IsOccupied { get; private set; }
    public GameObject OccupyingMotor { get; private set; }
    public NpcController OccupyingNpc { get; private set; }

    public void Occupy(GameObject motor, NpcController npc)
    {
        IsOccupied = true;
        OccupyingMotor = motor;
        OccupyingNpc = npc;
    }

    public void Vacate()
    {
        IsOccupied = false;
        OccupyingMotor = null;
        OccupyingNpc = null;
    }
}
