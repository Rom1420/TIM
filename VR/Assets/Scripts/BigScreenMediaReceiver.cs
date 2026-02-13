using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[DisallowMultipleComponent]
public class BigScreenCanvasMediaReceiver : MonoBehaviour
{
    [Header("Trigger (assign a Trigger Collider manually)")]
    [Tooltip("Assign a collider set as IsTrigger=true (often on a child like 'DropZone').")]
    [SerializeField] private Collider triggerCollider;

    [Header("UI Target (World Space Canvas)")]
    [SerializeField] private Canvas worldCanvas;             // optional (auto-find/create)
    [SerializeField] private RawImage rawImage;              // optional (auto-find/create)
    [SerializeField] private RectTransform screenRect;       // optional (auto-found)
    [SerializeField] private AspectRatioFitter aspectFitter; // optional (auto-find/add)

    [Header("Canvas Placement (only used if canvas is auto-created)")]
    [SerializeField] private Vector2 screenSizeMeters = new Vector2(1.8f, 1.0f);
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 0f, 0.02f);
    [SerializeField] private bool faceForward = true;

    [Header("Video")]
    [SerializeField] private int fallbackVideoWidth = 1920;
    [SerializeField] private int fallbackVideoHeight = 1080;
    [SerializeField] private VideoAudioOutputMode audioOutput = VideoAudioOutputMode.None;

    [Header("Item Lifecycle")]
    [SerializeField] private bool destroyItemAfterApply = true;
    [SerializeField] private float destroyDelaySeconds = 0.05f;

    [Header("SFX")]
    [SerializeField] private UnityEngine.AudioSource sfxSource;
    [SerializeField] private AudioClip mediaAppliedClip;
    [SerializeField] private float sfxCooldown = 0.08f;

private float _lastSfxTime = -999f;


    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private readonly HashSet<int> processed = new HashSet<int>();

    private VideoPlayer videoPlayer;
    private RenderTexture videoRT;

    private void Reset()
    {
        AutoFindTriggerIfMissing();
        EnsureCanvasUi();
        EnsureVideoPlayer();
    }

    private void Awake()
    {
        AutoFindTriggerIfMissing();
        EnsureCanvasUi();
        EnsureVideoPlayer();
        SetScreenVisible(rawImage != null && rawImage.texture != null);
        if (!sfxSource) sfxSource = GetComponentInChildren<UnityEngine.AudioSource>(true);
    }

    private void PlayAppliedSfx()
    {
        if (!sfxSource || !mediaAppliedClip) return;
        if (Time.unscaledTime - _lastSfxTime < sfxCooldown) return;
        _lastSfxTime = Time.unscaledTime;

        sfxSource.PlayOneShot(mediaAppliedClip);
    }


    /// <summary>
    /// If user didn't assign a triggerCollider, we try to find one in children.
    /// We do NOT create/override anything.
    /// </summary>
    private void AutoFindTriggerIfMissing()
    {
        if (triggerCollider != null) return;

        // Try to find any trigger collider in children
        var colliders = GetComponentsInChildren<Collider>(true);
        foreach (var c in colliders)
        {
            if (c != null && c.isTrigger)
            {
                triggerCollider = c;
                break;
            }
        }

        if (triggerCollider == null && enableDebugLogs)
            Debug.LogWarning("[BigScreenCanvasMediaReceiver] No triggerCollider assigned/found. OnTriggerEnter won't fire.", this);
    }

    private bool createdCanvasByScript = false;

    private void EnsureCanvasUi()
    {
        if (worldCanvas == null)
            worldCanvas = GetComponentInChildren<Canvas>(true);

        if (worldCanvas == null)
        {
            // Create canvas
            var go = new GameObject("ScreenCanvas");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localOffset;
            if (faceForward) go.transform.localRotation = Quaternion.identity;

            worldCanvas = go.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;

            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();

            createdCanvasByScript = true;
        }

        // ✅ IMPORTANT : ne touche pas au layout si le canvas est déjà dans la scène
        if (createdCanvasByScript)
        {
            var canvasRt = worldCanvas.GetComponent<RectTransform>();

            // (recommandé) WorldSpace “pixels->mètres”
            // 1000 pixels = 1 mètre si scale = 0.001
            canvasRt.sizeDelta = screenSizeMeters * 1000f;
            worldCanvas.transform.localScale = Vector3.one * 0.001f;
            worldCanvas.transform.localPosition = localOffset;
            if (faceForward) worldCanvas.transform.localRotation = Quaternion.identity;
        }

        // RawImage
        if (rawImage == null)
            rawImage = worldCanvas.GetComponentInChildren<RawImage>(true);

        if (rawImage == null)
        {
            var imgGo = new GameObject("ScreenRawImage");
            imgGo.transform.SetParent(worldCanvas.transform, false);
            rawImage = imgGo.AddComponent<RawImage>();
            rawImage.color = Color.black;
        }

        screenRect = rawImage.GetComponent<RectTransform>();

        // ✅ anchors/offsets : OK à forcer, mais seulement si tu veux “plein écran”
        screenRect.anchorMin = Vector2.zero;
        screenRect.anchorMax = Vector2.one;
        screenRect.offsetMin = Vector2.zero;
        screenRect.offsetMax = Vector2.zero;

        if (aspectFitter == null)
            aspectFitter = rawImage.GetComponent<AspectRatioFitter>() ?? rawImage.gameObject.AddComponent<AspectRatioFitter>();

        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        if (aspectFitter.aspectRatio <= 0f)
            aspectFitter.aspectRatio = 16f / 9f;
    }


    private void EnsureVideoPlayer()
    {
        if (videoPlayer != null) return;

        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null) videoPlayer = gameObject.AddComponent<VideoPlayer>();

        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.isLooping = true;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.audioOutputMode = audioOutput;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Safety: if a specific triggerCollider is assigned, only accept events from it or its children colliders.
        // Note: Unity calls OnTriggerEnter on the object that has the Collider+Rigidbody relationship.
        // If you put this script on a parent, OnTriggerEnter can still fire if parent has Rigidbody etc.
        // We'll just process the 'other' collider as before.
        if (other == null) return;

        var mediaItem = other.GetComponentInParent<MediaSpawnedItem>();
        if (mediaItem == null) return;

        int id = mediaItem.GetInstanceID();
        if (!processed.Add(id)) return;

        bool ok = ApplyMedia(mediaItem);

        if (!ok)
        {
            processed.Remove(id);
            return;
        }

        if (destroyItemAfterApply)
        {
            if (destroyDelaySeconds > 0f)
            {
                mediaItem.gameObject.SetActive(false);
                Destroy(mediaItem.gameObject, destroyDelaySeconds);
            }
            else Destroy(mediaItem.gameObject);
        }
    }

    private bool ApplyMedia(MediaSpawnedItem item)
    {
        EnsureCanvasUi();
        EnsureVideoPlayer();

        // ✅ PRIORITY: VIDEO
        if (item.Media != null && item.Media.video != null)
        {
            PlayVideo(item.Media.video);
            PlayAppliedSfx(); 
            if (enableDebugLogs) Debug.Log("[BigScreenCanvasMediaReceiver] Applied VIDEO + started playback.", this);
            return true;
        }

        // ✅ IMAGE (or thumbnail fallback)
        Texture img = null;
        if (item.Media != null && item.Media.image != null) img = item.Media.image;
        else img = item.ThumbnailTexture;

        if (img != null)
        {
            StopVideo();
            ApplyImage(img);
            PlayAppliedSfx(); 
            if (enableDebugLogs) Debug.Log($"[BigScreenCanvasMediaReceiver] Applied IMAGE '{img.name}' ({img.width}x{img.height}).", this);
            return true;
        }

        if (enableDebugLogs) Debug.LogWarning("[BigScreenCanvasMediaReceiver] No image/video found on item.", this);
        return false;
    }

    private void ApplyImage(Texture tex)
    {
        if (tex == null || rawImage == null) return;

        rawImage.texture = tex;
        rawImage.uvRect = new Rect(0, 0, 1, 1);

        SetScreenVisible(true); 

        // 🔥 preserve aspect ratio (no crop) => letterboxing on black background
        if (aspectFitter != null)
            aspectFitter.aspectRatio = (tex.height > 0) ? (tex.width / (float)tex.height) : (16f / 9f);
    }

    private void PlayVideo(VideoClip clip)
    {
        if (clip == null || rawImage == null) return;

        int w = (clip.width > 0) ? (int)clip.width : fallbackVideoWidth;
        int h = (clip.height > 0) ? (int)clip.height : fallbackVideoHeight;

        EnsureVideoRT(w, h);

        videoPlayer.Stop();
        videoPlayer.clip = clip;
        videoPlayer.targetTexture = videoRT;

        rawImage.texture = videoRT;
        rawImage.uvRect = new Rect(0, 0, 1, 1);

        SetScreenVisible(true); 

        // 🔥 preserve aspect ratio (no crop)
        if (aspectFitter != null)
            aspectFitter.aspectRatio = (h > 0) ? (w / (float)h) : (16f / 9f);

        videoPlayer.Play();
    }

    private void EnsureVideoRT(int w, int h)
    {
        if (videoRT != null && (videoRT.width != w || videoRT.height != h))
        {
            videoRT.Release();
            videoRT = null;
        }

        if (videoRT == null)
        {
            videoRT = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
            videoRT.name = $"BigScreenCanvasVideoRT_{w}x{h}";
            videoRT.wrapMode = TextureWrapMode.Clamp;
        }
    }

    private void StopVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();
    }

    private static readonly Color OpaqueWhite = new Color(1f, 1f, 1f, 1f);
    private static readonly Color TransparentWhite = new Color(1f, 1f, 1f, 0f);

    private void SetScreenVisible(bool visible)
    {
        if (rawImage == null) return;
        rawImage.color = visible ? OpaqueWhite : TransparentWhite;
    }

}
