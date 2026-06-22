using System.Collections;
using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI totalMoneyText;
    public TextMeshProUGUI collectPopupText;

    [Header("Interact Prompt")]
    public GameObject interactPrompt;        // "Tekan E untuk tagih"
    public TextMeshProUGUI interactPromptText;

    void Update()
    {
        // Update total uang
        if (totalMoneyText != null && MoneySystem.Instance != null)
            totalMoneyText.text = $"Rp {MoneySystem.Instance.TotalMoney:N0}";

        // Tampilkan prompt E jika ada NPC bisa diinteraksi
        bool showPrompt = false;
        if (interactPrompt != null)
        {
            foreach (var npc in FindObjectsByType<NpcController>(FindObjectsSortMode.None))
            {
                if (npc.CanInteract())
                {
                    showPrompt = true;
                    break;
                }
            }
            interactPrompt.SetActive(showPrompt);
        }
    }

    public void ShowCollect(int amount)
    {
        if (collectPopupText == null) return;
        StopAllCoroutines();
        StartCoroutine(PopupRoutine(amount));
    }

    IEnumerator PopupRoutine(int amount)
    {
        collectPopupText.text = $"+Rp {amount:N0}";
        collectPopupText.gameObject.SetActive(true);

        float t = 0f;
        Vector3 startPos = collectPopupText.rectTransform.anchoredPosition;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            collectPopupText.rectTransform.anchoredPosition = startPos + Vector3.up * (t * 30f);
            collectPopupText.color = new Color(1, 1, 0, 1 - t / 1.5f);
            yield return null;
        }

        collectPopupText.gameObject.SetActive(false);
        collectPopupText.rectTransform.anchoredPosition = startPos;
        collectPopupText.color = Color.yellow;
    }
}
