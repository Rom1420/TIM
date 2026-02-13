using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using Oculus.Interaction;
using UnityEngine.Video;
using System.Collections;

public class MediaTileView : MonoBehaviour
{
    [SerializeField] private RawImage thumbnail;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private GameObject selectedVisual;
    [SerializeField] private InteractableUnityEventWrapper pokeEvents;
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private Texture2D videoPlaceholder;
    [SerializeField] private bool generateVideoThumbnails = true;
    [SerializeField] private Vector2Int videoThumbnailSize = new Vector2Int(256, 256);

    [Header("Sound")]
    [SerializeField] private UnityEngine.AudioSource audioSource;
    [SerializeField] private AudioClip pokeClip;
    [SerializeField] private float cooldown = 0.12f;

    private float _lastPlay = -999f;

    private MediaRef _media;
    private UnityAction _pokeAction;
    private VideoPlayer _videoPlayer;
    private RenderTexture _videoTexture;
    private Coroutine _thumbnailRoutine;

    public event Action<MediaTileView> Selected;

    public MediaRef Media => _media;
    public Texture ThumbnailTexture => thumbnail != null ? thumbnail.texture : null;

    private void Awake()
    {
        if (pokeEvents == null)
        {
            pokeEvents = GetComponentInChildren<InteractableUnityEventWrapper>();
        }

        _pokeAction = HandlePoke;
    }

    private void OnEnable()
    {
        if (pokeEvents != null)
        {
            pokeEvents.WhenSelect.AddListener(_pokeAction);
            if (enableDebugLogs)
            {
                Debug.Log($"[MediaTileView] Poke listener added on {name}.", this);
            }
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning($"[MediaTileView] No pokeEvents found on {name}.", this);
        }
    }

    private void OnDisable()
    {
        if (pokeEvents != null)
        {
            pokeEvents.WhenSelect.RemoveListener(_pokeAction);
            if (enableDebugLogs)
            {
                Debug.Log($"[MediaTileView] Poke listener removed on {name}.", this);
            }
        }
    }

    public void Initialize(MediaRef media)
    {
        _media = media;

        if (_thumbnailRoutine != null)
        {
            StopCoroutine(_thumbnailRoutine);
            _thumbnailRoutine = null;
        }

        if (thumbnail != null)
        {
            Texture2D texture = null;
            if (media != null)
            {
                texture = media.image;
                if (texture == null && media.type == MediaType.Video)
                {
                    texture = videoPlaceholder;
                }
            }

            thumbnail.texture = texture;
            thumbnail.enabled = thumbnail.texture != null;
        }

        if (generateVideoThumbnails && media != null && media.type == MediaType.Video && media.video != null)
        {
            _thumbnailRoutine = StartCoroutine(LoadVideoThumbnail(media.video));
        }

        if (nameLabel != null)
        {
            nameLabel.text = media != null ? media.displayName : string.Empty;
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedVisual != null)
        {
            selectedVisual.SetActive(selected);
        }
    }

    public void HandlePoke()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[MediaTileView] HandlePoke on {name}. Media set: {_media != null}.", this);
        }

        if (_media == null)
        {
            return;
        }
        // AJoute un log
        PlayPokeSound();
        Selected?.Invoke(this);
    }

    private void PlayPokeSound()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[MediaTileView] PlayPokeSound on {name}. Audio source set: {audioSource != null}. Clip set: {pokeClip != null}.", this);
        }

        if (!audioSource) return;
        if (Time.time - _lastPlay < cooldown) return;
        _lastPlay = Time.time;

        if (pokeClip) audioSource.PlayOneShot(pokeClip);
        else audioSource.Play();
    }

    private IEnumerator LoadVideoThumbnail(VideoClip clip)
    {
        EnsureVideoPlayer();
        SetupVideoTexture();

        if (_videoPlayer == null || _videoTexture == null)
        {
            yield break;
        }

        bool frameReady = false;
        void OnFrameReady(VideoPlayer player, long frame)
        {
            frameReady = true;
        }

        _videoPlayer.frameReady += OnFrameReady;
        _videoPlayer.clip = clip;
        _videoPlayer.Prepare();

        while (!_videoPlayer.isPrepared)
        {
            yield return null;
        }

        _videoPlayer.Play();

        float timeout = 1.5f;
        float elapsed = 0f;
        while (!frameReady && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        _videoPlayer.Pause();
        _videoPlayer.frameReady -= OnFrameReady;

        if (thumbnail != null && _videoTexture != null)
        {
            thumbnail.texture = _videoTexture;
            thumbnail.enabled = true;
        }
    }

    private void EnsureVideoPlayer()
    {
        if (_videoPlayer != null)
        {
            return;
        }

        _videoPlayer = gameObject.AddComponent<VideoPlayer>();
        _videoPlayer.playOnAwake = false;
        _videoPlayer.isLooping = false;
        _videoPlayer.waitForFirstFrame = true;
        _videoPlayer.sendFrameReadyEvents = true;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
    }

    private void SetupVideoTexture()
    {
        int width = Mathf.Max(1, videoThumbnailSize.x);
        int height = Mathf.Max(1, videoThumbnailSize.y);

        if (_videoTexture != null && (_videoTexture.width != width || _videoTexture.height != height))
        {
            _videoPlayer.targetTexture = null;
            _videoTexture.Release();
            Destroy(_videoTexture);
            _videoTexture = null;
        }

        if (_videoTexture == null)
        {
            _videoTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            _videoTexture.Create();
        }

        if (_videoPlayer != null)
        {
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _videoPlayer.targetTexture = _videoTexture;
        }
    }

    private void OnDestroy()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.targetTexture = null;
        }

        if (_videoTexture != null)
        {
            _videoTexture.Release();
            Destroy(_videoTexture);
            _videoTexture = null;
        }
    }
}
