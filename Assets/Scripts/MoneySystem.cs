using UnityEngine;

public class MoneySystem : MonoBehaviour
{
    public static MoneySystem Instance { get; private set; }

    public int TotalMoney { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddMoney(int amount)
    {
        TotalMoney += amount;
        Debug.Log($"[Money] +{amount} | Total: {TotalMoney}");
    }
}
