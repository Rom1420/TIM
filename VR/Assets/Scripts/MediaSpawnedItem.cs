using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.UI;

public class MediaSpawnedItem : MonoBehaviour
{
    public MediaRef Media;
    public Texture ThumbnailTexture;

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private RawImage targetImage;
    [SerializeField] private bool createQuadIfMissing = true;
    [SerializeField] private bool enableDebugLogs = true;

    private void Awake()
    {
        EnsureTargets();
        ValidateGrabbable();
    }

    public void Init(MediaRef media, Texture thumbnail)
    {
        Media = media;
        ThumbnailTexture = thumbnail;
        EnsureTargets();
        ApplyThumbnail();
    }

    private void EnsureTargets()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetImage == null)
        {
            targetImage = GetComponentInChildren<RawImage>();
        }

        if (targetRenderer == null && targetImage == null && createQuadIfMissing)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "ThumbnailQuad";
            quad.transform.SetParent(transform, false);

            if (quad.TryGetComponent(out MeshCollider meshCollider))
            {
                Destroy(meshCollider);
                BoxCollider boxCollider = quad.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(1f, 1f, 0.02f);
            }

            targetRenderer = quad.GetComponent<Renderer>();
        }
    }

    private void ValidateGrabbable()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[MediaSpawnedItem] Missing Rigidbody; grab interactions will not work.", this);
            }
            return;
        }

        Grabbable grabbable = GetComponent<Grabbable>();
        GrabInteractable grabInteractable = GetComponent<GrabInteractable>();
        HandGrabInteractable handGrabInteractable = GetComponent<HandGrabInteractable>();

        if (grabbable == null && enableDebugLogs)
        {
            Debug.LogWarning("[MediaSpawnedItem] Missing Grabbable component.", this);
        }

        if (grabInteractable == null && enableDebugLogs)
        {
            Debug.LogWarning("[MediaSpawnedItem] Missing GrabInteractable component.", this);
        }

        if (handGrabInteractable == null && enableDebugLogs)
        {
            Debug.LogWarning("[MediaSpawnedItem] Missing HandGrabInteractable component.", this);
        }

        if (grabbable != null)
        {
            grabbable.InjectOptionalRigidbody(rb);
        }

        if (grabInteractable != null)
        {
            if (grabInteractable.Rigidbody == null)
            {
                grabInteractable.InjectRigidbody(rb);
            }

            if (grabbable != null && grabInteractable.PointableElement == null)
            {
                grabInteractable.InjectOptionalPointableElement(grabbable);
            }
        }

        if (handGrabInteractable != null)
        {
            if (handGrabInteractable.Rigidbody == null)
            {
                handGrabInteractable.InjectRigidbody(rb);
            }

            if (grabbable != null && handGrabInteractable.PointableElement == null)
            {
                handGrabInteractable.InjectOptionalPointableElement(grabbable);
            }
        }
    }

    private void ApplyThumbnail()
    {
        if (ThumbnailTexture == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[MediaSpawnedItem] No thumbnail texture to apply.", this);
            }
            return;
        }

        if (targetImage != null)
        {
            targetImage.texture = ThumbnailTexture;
            targetImage.enabled = true;
            return;
        }

        if (targetRenderer != null)
        {
            targetRenderer.material.mainTexture = ThumbnailTexture;
            return;
        }

        if (enableDebugLogs)
        {
            Debug.LogWarning("[MediaSpawnedItem] No renderer or RawImage found to apply thumbnail.", this);
        }
    }
}
