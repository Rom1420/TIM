using UnityEngine;

[DisallowMultipleComponent]
public class SelectionOverlay : MonoBehaviour
{
    [Header("Overlay Material (shader overlay)")]
    [SerializeField] private Material overlayMaterial; // un material qui utilise ton shader overlay

    [Header("Tuning")]
    [SerializeField] private float overlayScale = 1.01f; // léger scale pour éviter z-fighting
    [SerializeField] private string colorProperty = "_OverlayColor"; // nom dans ton shader

    private Renderer baseRenderer;
    private GameObject overlayGo;
    private Renderer overlayRenderer;
    private Material overlayMatInstance;
    private MaterialPropertyBlock mpb;

    private void Awake()
    {
        baseRenderer = GetComponentInChildren<Renderer>();
        if (baseRenderer == null)
        {
            Debug.LogWarning("[SelectionOverlay] No Renderer found.");
            return;
        }

        if (overlayMaterial == null)
        {
            Debug.LogWarning("[SelectionOverlay] overlayMaterial not set.");
            return;
        }

        // Crée un enfant overlay qui duplique le mesh
        var mf = baseRenderer.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning("[SelectionOverlay] No MeshFilter/sharedMesh found on base renderer.");
            return;
        }

        overlayGo = new GameObject("HighlightOverlay");
        overlayGo.transform.SetParent(baseRenderer.transform, false);
        overlayGo.transform.localPosition = Vector3.zero;
        overlayGo.transform.localRotation = Quaternion.identity;
        overlayGo.transform.localScale = Vector3.one * overlayScale;

        var overlayMf = overlayGo.AddComponent<MeshFilter>();
        overlayMf.sharedMesh = mf.sharedMesh;

        overlayRenderer = overlayGo.AddComponent<MeshRenderer>();

        // Instance pour ne pas modifier le material original (et permettre des couleurs différentes par cube)
        overlayMatInstance = new Material(overlayMaterial);
        overlayRenderer.sharedMaterial = overlayMatInstance;

        mpb = new MaterialPropertyBlock();

        SetActive(false);
    }

    public void SetActive(bool on)
    {
        if (overlayGo != null)
            overlayGo.SetActive(on);
    }

    public void SetColor(Color c)
    {
        if (overlayRenderer == null) return;

        overlayRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(colorProperty, c);
        overlayRenderer.SetPropertyBlock(mpb);
    }

    public void Set(bool on, Color c)
    {
        SetActive(on);
        if (on) SetColor(c);
    }
}
