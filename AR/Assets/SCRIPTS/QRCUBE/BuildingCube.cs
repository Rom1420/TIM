using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BuildingCube : MonoBehaviour
{
    [HideInInspector] public string roomId;

    [SerializeField] private BuildingDatabase database;
    [SerializeField] private BuildingDetailsUI detailsUI;

    private void Awake()
    {
        Debug.Log($"[BuildingCube] Awake on {gameObject.name}");

        if (database == null) database = FindFirstObjectByType<BuildingDatabase>();
        if (detailsUI == null) detailsUI = FindFirstObjectByType<BuildingDetailsUI>();

        Debug.Log($"[BuildingCube] DB={(database != null)} UI={(detailsUI != null)} roomId={roomId}");
    }

    /// <summary>
    /// Appelé par LeanTouch (via un manager) quand le cube est tapé.
    /// </summary>
    public void HandleTap()
    {
        Debug.Log($"[BuildingCube] TAP roomId={roomId} obj={gameObject.name}");

        if (detailsUI == null)
        {
            Debug.LogError("[BuildingCube] detailsUI NULL");
            return;
        }

        if (database != null && database.TryGet(roomId, out BuildingData data))
        {
            Debug.Log($"[BuildingCube] DATA FOUND for {roomId}");
            detailsUI.Show(data);
        }
        else
        {
            Debug.LogWarning($"[BuildingCube] DATA NOT FOUND for {roomId}");
            detailsUI.ShowNotFound(roomId);
        }
    }
}
