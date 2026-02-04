using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class StudentFilterUI : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject filterPanel;
    [SerializeField] private Button openFilterButton;
    [SerializeField] private Button closeButton;

    [Header("Filter Controls")]
    [SerializeField] private TMP_Dropdown yearDropdown;
    [SerializeField] private TMP_Dropdown specializationDropdown;
    [SerializeField] private TMP_Dropdown roomDropdown;
    [SerializeField] private TMP_Dropdown hourDropdown;
    [SerializeField] private TMP_Dropdown transportDropdown;
    [SerializeField] private TMP_InputField objectSearchInput;
    [SerializeField] private Button applyFiltersButton;
    [SerializeField] private Button clearFiltersButton;

    [Header("Results")]
    [SerializeField] private GameObject resultsScrollView;
    [SerializeField] private Transform resultsContent;
    [SerializeField] private GameObject studentItemPrefab;
    [SerializeField] private TMP_Text resultsCountText;

    [Header("Database")]
    [SerializeField] private StudentDatabase studentDatabase;

    private List<GameObject> spawnedResultItems = new List<GameObject>();

    private void Awake()
    {
        // Auto-find database if not assigned
        if (studentDatabase == null)
        {
            studentDatabase = FindObjectOfType<StudentDatabase>();
        }

        // Button listeners - Remove existing first to avoid duplicates
        if (openFilterButton != null)
        {
            openFilterButton.onClick.RemoveAllListeners();
            openFilterButton.onClick.AddListener(OpenFilterMenu);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseFilterMenu);
        }
        
        if (applyFiltersButton != null)
        {
            applyFiltersButton.onClick.RemoveAllListeners();
            applyFiltersButton.onClick.AddListener(ApplyFilters);
        }
        
        if (clearFiltersButton != null)
        {
            clearFiltersButton.onClick.RemoveAllListeners();
            clearFiltersButton.onClick.AddListener(ClearFilters);
        }

        CloseFilterMenu();
    }

    private void Start()
    {
        InitializeDropdowns();
    }
    
    private void HapticLight()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    try
    {
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        int sdkInt = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
        AndroidJavaObject vibrator = null;

        // Android 12+ : VibratorManager
        if (sdkInt >= 31)
        {
            using var contextClass = new AndroidJavaClass("android.content.Context");
            string VIBRATOR_MANAGER_SERVICE = contextClass.GetStatic<string>("VIBRATOR_MANAGER_SERVICE");
            var vibratorManager = activity.Call<AndroidJavaObject>("getSystemService", VIBRATOR_MANAGER_SERVICE);
            if (vibratorManager != null)
                vibrator = vibratorManager.Call<AndroidJavaObject>("getDefaultVibrator");
        }

        // Fallback older vibrator
        if (vibrator == null)
        {
            using var contextClass = new AndroidJavaClass("android.content.Context");
            string VIBRATOR_SERVICE = contextClass.GetStatic<string>("VIBRATOR_SERVICE");
            vibrator = activity.Call<AndroidJavaObject>("getSystemService", VIBRATOR_SERVICE);
        }

        if (vibrator == null) return;
        if (!vibrator.Call<bool>("hasVibrator")) return;

        const long durationMs = 20;   // micro
        const int amplitude = 30;     // léger (1..255)

        if (sdkInt >= 26)
        {
            using var vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
            var effect = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", durationMs, amplitude);
            vibrator.Call("vibrate", effect);
        }
        else
        {
            vibrator.Call("vibrate", durationMs);
        }
    }
    catch
    {
        // fail silencieux
    }
#endif
    }


    private void InitializeDropdowns()
    {
        if (studentDatabase == null)
        {
            Debug.LogError("[StudentFilterUI] StudentDatabase non trouvée!");
            return;
        }

        // Year Dropdown
        if (yearDropdown != null)
        {
            yearDropdown.ClearOptions();
            List<string> yearOptions = new List<string> { "All" };
            yearOptions.AddRange(studentDatabase.GetAllYears().Select(y => y.ToString()));
            yearDropdown.AddOptions(yearOptions);
        }

        // Specialization Dropdown
        if (specializationDropdown != null)
        {
            specializationDropdown.ClearOptions();
            List<string> specOptions = new List<string> { "All" };
            specOptions.AddRange(studentDatabase.GetAllSpecializations());
            specializationDropdown.AddOptions(specOptions);
        }

        // Room Dropdown
        if (roomDropdown != null)
        {
            roomDropdown.ClearOptions();
            List<string> roomOptions = new List<string> { "All" };
            roomOptions.AddRange(studentDatabase.GetAllRooms());
            roomDropdown.AddOptions(roomOptions);
        }

        // Hour Dropdown
        if (hourDropdown != null)
        {
            hourDropdown.ClearOptions();
            hourDropdown.AddOptions(new List<string> { "All", "13h", "14h", "15h", "16h", "17h" });
        }

        // Transport Dropdown
        if (transportDropdown != null)
        {
            transportDropdown.ClearOptions();
            List<string> transportOptions = new List<string> { "All" };
            transportOptions.AddRange(studentDatabase.GetAllTransportModes());
            transportDropdown.AddOptions(transportOptions);
        }
    }

    public void OpenFilterMenu()
    {
        HapticLight();
        
        filterPanel?.SetActive(true);
    }

    public void CloseFilterMenu()
    {
        filterPanel?.SetActive(false);
    }

    public void ApplyFilters()
    {
        HapticLight();
        
        if (studentDatabase == null) return;

        // Récupérer les valeurs des filtres
        int? selectedYear = null;
        if (yearDropdown != null && yearDropdown.value > 0)
        {
            string yearText = yearDropdown.options[yearDropdown.value].text;
            if (int.TryParse(yearText, out int year))
            {
                selectedYear = year;
            }
        }

        string selectedSpec = specializationDropdown != null && specializationDropdown.value > 0
            ? specializationDropdown.options[specializationDropdown.value].text
            : null;

        string selectedRoom = roomDropdown != null && roomDropdown.value > 0
            ? roomDropdown.options[roomDropdown.value].text
            : null;

        int? selectedHour = null;
        if (hourDropdown != null && hourDropdown.value > 0)
        {
            selectedHour = 12 + hourDropdown.value; // 13, 14, 15, 16, 17
        }

        string selectedTransport = transportDropdown != null && transportDropdown.value > 0
            ? transportDropdown.options[transportDropdown.value].text
            : null;

        string objectSearch = objectSearchInput != null ? objectSearchInput.text : null;

        // Appliquer les filtres
        List<StudentData> filteredStudents = studentDatabase.FilterStudents(
            year: selectedYear,
            specialization: selectedSpec,
            roomProximity: selectedRoom,
            hour: selectedHour,
            transport: selectedTransport,
            objectFilter: objectSearch
        );

        // Afficher les résultats
        DisplayResults(filteredStudents);
    }

    public void ClearFilters()
    {
        HapticLight();
        
        // Réinitialiser tous les dropdowns à "All"
        yearDropdown?.SetValueWithoutNotify(0);
        specializationDropdown?.SetValueWithoutNotify(0);
        roomDropdown?.SetValueWithoutNotify(0);
        hourDropdown?.SetValueWithoutNotify(0);
        transportDropdown?.SetValueWithoutNotify(0);

        if (objectSearchInput != null)
        {
            objectSearchInput.text = "";
        }

        // Afficher tous les étudiants
        if (studentDatabase != null)
        {
            DisplayResults(studentDatabase.GetAllStudents());
        }
    }

    private void DisplayResults(List<StudentData> students)
    {
        // Nettoyer les résultats précédents
        foreach (var item in spawnedResultItems)
        {
            Destroy(item);
        }
        spawnedResultItems.Clear();

        // Mettre à jour le compteur
        if (resultsCountText != null)
        {
            resultsCountText.text = $"Students found: {students.Count}";
        }

        // Vérifier si on a un prefab
        if (studentItemPrefab == null || resultsContent == null)
        {
            Debug.LogWarning("[StudentFilterUI] studentItemPrefab ou resultsContent non assigné!");
            return;
        }

        // Créer les éléments de résultat
        foreach (var student in students)
        {
            GameObject item = Instantiate(studentItemPrefab, resultsContent);
            spawnedResultItems.Add(item);

            // Remplir les informations
            StudentResultItem resultItem = item.GetComponent<StudentResultItem>();
            if (resultItem != null)
            {
                resultItem.SetStudentData(student);
            }
            else
            {
                // Fallback: chercher des TMP_Text dans l'ordre
                TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();
                if (texts.Length > 0)
                {
                    texts[0].text = FormatStudentInfo(student);
                }
            }
        }

        // Afficher le scroll view des résultats
        if (resultsScrollView != null)
        {
            resultsScrollView.SetActive(true);
        }
    }

    private string FormatStudentInfo(StudentData student)
    {
        return $"<b>{student.name}</b>\n" +
               $"Year: {student.year} | Spec: {student.specialization} | Transport: {student.transport}\n" +
               $"13h: {student.room_13h} | 14h: {student.room_14h} | 15h: {student.room_15h} | 16h: {student.room_16h} | 17h: {student.room_17h}";
    }

    /// <summary>
    /// Permet d'ouvrir directement le menu avec un filtre de salle (depuis RoomCube par exemple).
    /// </summary>
    public void OpenWithRoomFilter(string roomId)
    {
        OpenFilterMenu();

        // Sélectionner la salle dans le dropdown
        if (roomDropdown != null)
        {
            int index = roomDropdown.options.FindIndex(opt => opt.text == roomId);
            if (index >= 0)
            {
                roomDropdown.value = index;
            }
        }

        // Appliquer automatiquement
        ApplyFilters();
    }
}
