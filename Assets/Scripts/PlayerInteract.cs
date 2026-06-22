using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 3f;

    [Header("UI")]
    public MoneyUI moneyUI;

    const int PARKING_FEE = 2000;

    void Update()
    {
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        NpcController nearest = FindNearestInteractableNpc();
        if (nearest == null) return;

        MoneySystem.Instance.AddMoney(PARKING_FEE);
        nearest.PayAndLeave();

        if (moneyUI != null)
            moneyUI.ShowCollect(PARKING_FEE);
    }

    NpcController FindNearestInteractableNpc()
    {
        NpcController best = null;
        float bestDist = interactRange;

        foreach (var npc in FindObjectsByType<NpcController>(FindObjectsSortMode.None))
        {
            if (!npc.CanInteract()) continue;
            float d = Vector3.Distance(transform.position, npc.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = npc;
            }
        }
        return best;
    }
}
