using UnityEngine;

[ExecuteAlways]
public class MatchSurfaceToRectTransform : MonoBehaviour
{
    [SerializeField] private RectTransform targetRect; // ThumbnailCard

    void LateUpdate()
    {
        if (!targetRect) return;

        // Récupère les coins monde du rect UI
        var corners = new Vector3[4];
        targetRect.GetWorldCorners(corners);

        // Calcul taille monde
        float width  = Vector3.Distance(corners[0], corners[3]);
        float height = Vector3.Distance(corners[0], corners[1]);

        // Centre
        Vector3 center = (corners[0] + corners[2]) * 0.5f;

        // Applique au Surface
        transform.position = center;
        transform.rotation = targetRect.rotation;

        // Le plan Meta est généralement 1x1 → on scale
        transform.localScale = new Vector3(width, height, 1f);
    }
}
