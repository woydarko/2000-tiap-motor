using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 4.5f; // diperbesar sesuai ukuran karakter

    [Header("UI")]
    public MoneyUI moneyUI;

    const int PARKING_FEE = 2000;

    void Update()
    {
        NpcController nearest = FindNearestInteractableNpc();

        // Prompt "Pencet E" muncul hanya kalau ada NPC eligible DALAM range
        if (moneyUI != null)
            moneyUI.SetPrompt(nearest != null);

        // Tagih
        if (nearest != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            MoneySystem.Instance.AddMoney(PARKING_FEE);
            nearest.PayAndLeave();
            if (moneyUI != null)
                moneyUI.ShowCollect(PARKING_FEE);
        }
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
