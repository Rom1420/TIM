using TMPro;
using UnityEngine;

public class DistanceDataHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        Clear();
    }

    public void Show(string aId, string bId, float distMeters, float timeMin, float speedMps)
    {
        if (text == null) return;

        text.gameObject.SetActive(true);
        text.text =
            $"A: {aId}\n" +
            $"B: {bId}\n" +
            $"Distance: {distMeters:0.00} m\n" +
            $"Temps (marche): {timeMin:0.0} min\n" +
            $"Vitesse: {speedMps:0.00} m/s";
    }

    public void Clear()
    {
        if (text == null) return;
        text.text = "";
        text.gameObject.SetActive(false);
    }
}
