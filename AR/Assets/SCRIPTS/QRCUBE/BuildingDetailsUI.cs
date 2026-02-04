using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingDetailsUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;  // panel plein écran

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text seatsText;
    [SerializeField] private TMP_Text evacuationText;
    [SerializeField] private TMP_Text objectsText;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
        Hide();
    }

    public void Show(BuildingData data)
    {
        root.SetActive(true);

        titleText.text = data.room_id;
        seatsText.text = $"Seats available: {data.number_of_seats_available}";
        evacuationText.text = $"Evacuation: {data.evacuation_map}";
        objectsText.text = $"Objects: {data.objects_present.Replace("|", ", ")}";
    }

    public void ShowNotFound(string roomId)
    {
        root.SetActive(true);

        titleText.text = roomId;
        seatsText.text = "Seats available: ?";
        evacuationText.text = "Evacuation: ?";
        objectsText.text = "Objects: (not found)";
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}
