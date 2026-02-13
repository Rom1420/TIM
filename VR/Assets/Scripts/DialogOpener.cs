using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DialogOpener : MonoBehaviour
{
    [SerializeField] private Transform headTransform;
    [SerializeField] private GameObject dialogRoot;
    [SerializeField] private float forwardDistance = 1.0f;
    [SerializeField] private float verticalOffset = -0.15f;
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private InputActionReference toggleAction;
#endif
    [SerializeField] private bool allowKeyboardFallback = true;
    [SerializeField] private KeyCode fallbackKey = KeyCode.JoystickButton2;
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private MediaDialogController mediaDialogController;
    [Header("Spawn")]
    [SerializeField] private GameObject spawnPrefab;
    [SerializeField] private Transform spawnTransform;
    [SerializeField] private float spawnForwardDistance = 1.2f;
    [SerializeField] private float spawnVerticalOffset = -0.1f;
    [SerializeField] private Vector3 spawnScale = Vector3.one * 0.3f;

    [SerializeField] private UnityEngine.AudioSource uiAudioSource;
    [SerializeField] private AudioClip tilePokeClip;


    private MediaSpawnedItem currentSpawnedItem;

    private void Start()
    {
        if (!uiAudioSource)
            uiAudioSource = FindAnyObjectByType<UnityEngine.AudioSource>(); // ou GetComponent<AudioSource>() si sur le même GO

        if (headTransform == null && Camera.main != null)
        {
            headTransform = Camera.main.transform;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[DialogOpener] Start. HeadTransform set: {headTransform != null}, DialogRoot set: {dialogRoot != null}");
#if ENABLE_INPUT_SYSTEM
            Debug.Log($"[DialogOpener] Input System enabled. Toggle action set: {toggleAction != null && toggleAction.action != null}");
#elif ENABLE_LEGACY_INPUT_MANAGER
            Debug.Log($"[DialogOpener] Legacy Input Manager enabled. Fallback key: {fallbackKey}, AllowKeyboardFallback: {allowKeyboardFallback}");
#else
            Debug.Log("[DialogOpener] No input system enabled (ENABLE_INPUT_SYSTEM / ENABLE_LEGACY_INPUT_MANAGER not defined).");
#endif
        }

        if (dialogRoot != null)
        {
            dialogRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        if (toggleAction != null && toggleAction.action != null)
        {
            toggleAction.action.Enable();
            if (enableDebugLogs)
            {
                Debug.Log("[DialogOpener] Toggle action enabled.");
            }
        }
#endif

        if (mediaDialogController != null)
        {
            mediaDialogController.OnMediaSelected.AddListener(HandleMediaSelected);
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning("[DialogOpener] MediaDialogController is null; selection events won't spawn media.");
        }
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        if (toggleAction != null && toggleAction.action != null)
        {
            toggleAction.action.Disable();
        }
#endif

        if (mediaDialogController != null)
        {
            mediaDialogController.OnMediaSelected.RemoveListener(HandleMediaSelected);
        }
    }

    private void Update()
    {
        bool toggled = false;

#if ENABLE_INPUT_SYSTEM
        if (toggleAction != null && toggleAction.action != null)
        {
            if (toggleAction.action.WasPressedThisFrame())
            {
                toggled = true;
                if (enableDebugLogs)
                {
                    Debug.Log("[DialogOpener] Input System toggle pressed.");
                }
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (allowKeyboardFallback && Input.GetKeyDown(fallbackKey))
        {
            toggled = true;
            if (enableDebugLogs)
            {
                Debug.Log($"[DialogOpener] Legacy input key pressed: {fallbackKey}");
            }
        }
#endif

        if (toggled)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (dialogRoot == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[DialogOpener] Toggle called but DialogRoot is null.");
            }
            return;
        }

        if (dialogRoot.activeSelf)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        if (dialogRoot == null || headTransform == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[DialogOpener] Open aborted. DialogRoot set: {dialogRoot != null}, HeadTransform set: {headTransform != null}");
            }
            return;
        }

        DestroyCurrentSpawnedItem();

        Vector3 headPos = headTransform.position;
        Vector3 headForward = headTransform.forward;
        Vector3 panelPos = headPos + headForward * forwardDistance + Vector3.up * verticalOffset;

        dialogRoot.transform.position = panelPos;
        Vector3 toHead = headPos - panelPos;
        if (toHead.sqrMagnitude > 0.0001f)
        {
            dialogRoot.transform.rotation = Quaternion.LookRotation(toHead, headTransform.up);
        }

        dialogRoot.SetActive(true);
        dialogRoot.SetActive(true);
        mediaDialogController?.Populate();
        if (enableDebugLogs)
        {
            Debug.Log("[DialogOpener] Dialog opened.");
        }
    }

    public void Close()
    {
        if (dialogRoot == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[DialogOpener] Close called but DialogRoot is null.");
            }
            return;
        }

        dialogRoot.SetActive(false);
        if (enableDebugLogs)
        {
            Debug.Log("[DialogOpener] Dialog closed.");
        }
    }

    private void DestroyCurrentSpawnedItem()
    {
        if (currentSpawnedItem == null)
        {
            return;
        }

        GameObject itemObject = currentSpawnedItem.gameObject;
        currentSpawnedItem = null;

        if (itemObject == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(itemObject);
        }
        else
        {
            DestroyImmediate(itemObject);
        }

        if (enableDebugLogs)
        {
            Debug.Log("[DialogOpener] Destroyed current MediaSpawnedItem before opening dialog.");
        }
    }

    private void HandleMediaSelected(MediaRef media)
    {   
         Debug.Log($"[DialogOpener] HandleMediaSelected fired. uiAudioSource={(uiAudioSource!=null)} clip={(tilePokeClip!=null)}");
        if (uiAudioSource && tilePokeClip)
            uiAudioSource.PlayOneShot(tilePokeClip);

        Close();

        if (media == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[DialogOpener] Media selection received but MediaRef is null.");
            }
            return;
        }

        if (spawnPrefab == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[DialogOpener] Spawn prefab is not assigned.");
            }
            return;
        }

        if (!TryGetSpawnPose(out Vector3 position, out Quaternion rotation))
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[DialogOpener] Unable to resolve spawn pose (no spawnTransform or headTransform).");
            }
            return;
        }

        Texture thumbnail = mediaDialogController != null ? mediaDialogController.LastSelectedThumbnail : null;
        if (thumbnail == null && media.type == MediaType.Image)
        {
            thumbnail = media.image;
        }

        GameObject instance = Instantiate(spawnPrefab, position, rotation);
        instance.transform.localScale = spawnScale;

        MediaSpawnedItem spawnedItem = instance.GetComponent<MediaSpawnedItem>();
        if (spawnedItem == null)
        {
            spawnedItem = instance.AddComponent<MediaSpawnedItem>();
        }

        spawnedItem.Init(media, thumbnail);
        currentSpawnedItem = spawnedItem;

        if (enableDebugLogs)
        {
            Debug.Log($"[DialogOpener] Spawned media item for {media.displayName}.", instance);
        }
    }

    private bool TryGetSpawnPose(out Vector3 position, out Quaternion rotation)
    {
        if (spawnTransform != null)
        {
            position = spawnTransform.position;
            rotation = spawnTransform.rotation;
            return true;
        }

        if (headTransform == null && Camera.main != null)
        {
            headTransform = Camera.main.transform;
        }

        if (headTransform == null)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        Vector3 headPos = headTransform.position;
        Vector3 headForward = headTransform.forward;
        position = headPos + headForward * spawnForwardDistance + Vector3.up * spawnVerticalOffset;

        Vector3 toHead = headPos - position;
        if (toHead.sqrMagnitude > 0.0001f)
        {
            rotation = Quaternion.LookRotation(toHead, headTransform.up);
        }
        else
        {
            rotation = headTransform.rotation;
        }

        return true;
    }
}
