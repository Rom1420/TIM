using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Composant à attacher sur le prefab d'affichage d'un résultat étudiant.
/// </summary>
public class StudentResultItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text scheduleText;
    [SerializeField] private Image backgroundImage;

    private StudentData studentData;

    public void SetStudentData(StudentData data)
    {
        studentData = data;

        if (nameText != null)
        {
            nameText.text = $"<b>{data.name}</b>";
        }

        if (infoText != null)
        {
            infoText.text = $"{data.year} • {data.specialization} • {data.transport}\n{data.hair}, {data.clothing}";
        }

        if (scheduleText != null)
        {
            scheduleText.text = $"13h:{data.room_13h} 14h:{data.room_14h} 15h:{data.room_15h} 16h:{data.room_16h} 17h:{data.room_17h}";
        }
        
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            (RectTransform)transform
        );

        if (backgroundImage != null)
        {
            backgroundImage.color = GetSpecializationColor(data.specialization);
        }
    }

    private Color GetSpecializationColor(string spec)
    {
        return spec switch
        {
            "IHM" => new Color(0.8f, 0.9f, 1f, 1f),     // Bleu clair
            "MAM" => new Color(1f, 0.9f, 0.8f, 1f),     // Orange clair
            "AL" => new Color(0.9f, 1f, 0.8f, 1f),      // Vert clair
            "WD" => new Color(1f, 0.8f, 0.9f, 1f),      // Rose clair
            "SAID" => new Color(0.95f, 0.9f, 1f, 1f),   // Violet clair
            _ => Color.white
        };
    }

    public StudentData GetStudentData()
    {
        return studentData;
    }
}
