using UnityEngine;

public class BuildingSelectionDistanceManager : MonoBehaviour
{
    [Header("Optional visuals")]
    [SerializeField] private LineRenderer line;
    [SerializeField] private bool showLine = true;

    [Header("Walking")]
    [SerializeField] private float walkingSpeedMps = 1.4f;

    [Header("Behavior")]
    [Tooltip("Si true: retaper sur le même cube le désélectionne.")]
    [SerializeField] private bool tapSameToDeselect = true;

    [Header("UI")]
    [SerializeField] private DistanceDataHUD hud;

    [Header("Selection overlay")]
    [SerializeField] private Color colorA = new Color(0.2f, 0.6f, 1f, 0.45f);
    [SerializeField] private Color colorB = new Color(1f, 0.35f, 0.35f, 0.45f);

    private BuildingCube selectedA;
    private BuildingCube selectedB;

    [Header("Scale")]
    [SerializeField] private float metersPerUnityUnit = 100f; // Échelle réelle : conversion Unity → mètres basée sur une calibration du plan du bâtiment définis par 500m entre sortie nord et (≈ 1 unit = 98 m)


    private void Awake()
    {
        if (line != null)
        {
            line.positionCount = 2;
            line.enabled = false;
        }
    }

    // -------- Overlay helpers --------
    private static SelectionOverlay Overlay(BuildingCube cube)
    {
        return cube != null ? cube.GetComponent<SelectionOverlay>() : null;
    }

    private void SetSelected(BuildingCube cube, bool on, Color c)
    {
        var ov = Overlay(cube);
        if (ov != null) ov.Set(on, c);
    }

    private void ClearA()
    {
        SetSelected(selectedA, false, colorA);
        selectedA = null;
        if (hud != null) hud.Clear();
        UpdateLine();
        Debug.Log("[Distance] A cleared");
    }

    private void ClearB()
    {
        SetSelected(selectedB, false, colorB);
        selectedB = null;
        if (hud != null) hud.Clear();
        UpdateLine();
        Debug.Log("[Distance] B cleared");
    }
    // --------------------------------

    public void Select(BuildingCube cube)
    {
        if (cube == null) return;

        // Tap sur le même cube
        if (tapSameToDeselect)
        {
            if (cube == selectedA)
            {
                ClearA();
                return;
            }
            if (cube == selectedB)
            {
                ClearB();
                return;
            }
        }

        // Choix A puis B
        if (selectedA == null)
        {
            selectedA = cube;
            SetSelected(selectedA, true, colorA);

            if (hud != null) hud.Clear(); // 1 seul point => cacher
            Debug.Log($"[Distance] A = {cube.roomId} ({cube.gameObject.name})");
            UpdateLine();
            return;
        }

        if (selectedB == null && cube != selectedA)
        {
            selectedB = cube;
            SetSelected(selectedB, true, colorB);

            Debug.Log($"[Distance] B = {cube.roomId} ({cube.gameObject.name})");
            ComputeAndLog();   // ici Show() est appelé
            UpdateLine();
            return;
        }

        // Si A et B existent déjà, on reset -> A=cube, B cleared
        // IMPORTANT : on enlève l’overlay des anciens A/B
        SetSelected(selectedA, false, colorA);
        SetSelected(selectedB, false, colorB);

        selectedA = cube;
        selectedB = null;

        SetSelected(selectedA, true, colorA);

        if (hud != null) hud.Clear(); // on revient à 1 point => cacher
        Debug.Log($"[Distance] Reset -> A = {cube.roomId}, B cleared");
        UpdateLine();
    }

    private void ComputeAndLog()
    {
        if (selectedA == null || selectedB == null) return;
        if (hud != null) hud.Clear();

        Vector3 a = selectedA.transform.position;
        Vector3 b = selectedB.transform.position;
        
        float distUnits = Vector3.Distance(a, b);
        float distMeters = distUnits * metersPerUnityUnit;

        float timeSec = distMeters / Mathf.Max(0.01f, walkingSpeedMps);
        float timeMin = timeSec / 60f;

        if (hud != null)
            hud.Show(selectedA.roomId, selectedB.roomId, distMeters, timeMin, walkingSpeedMps);

        Debug.Log($"[Distance] {selectedA.roomId} -> {selectedB.roomId} = {distMeters:0.00} m | walk ~ {timeMin:0.00} min");
    }

    private void UpdateLine()
    {
        if (line == null) return;

        bool canShow = showLine && selectedA != null && selectedB != null;
        line.enabled = canShow;

        if (!canShow) return;

        line.SetPosition(0, selectedA.transform.position);
        line.SetPosition(1, selectedB.transform.position);
    }

    private void LateUpdate()
    {
        // En AR, les objets peuvent bouger légèrement (tracking update) -> on suit la ligne en temps réel
        if (line != null && line.enabled)
            UpdateLine();
    }
}