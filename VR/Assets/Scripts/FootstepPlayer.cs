using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform movementSource;   // <-- TrackingSpace / Locomotor / Character root
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] metalFootsteps;

    [Header("Tuning")]
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private float moveThreshold = 0.01f;

    private float stepTimer;
    private Vector3 lastPosition;

    void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        // Si pas assigné, on tente de trouver TrackingSpace automatiquement
        if (!movementSource)
        {
            var t = transform.Find("TrackingSpace");
            if (t) movementSource = t;
            else movementSource = transform; // fallback
        }
    }

    void Start()
    {
        lastPosition = movementSource.position;
    }

    void Update()
    {
        Vector3 delta = movementSource.position - lastPosition;
        delta.y = 0f;

        float distanceThisFrame = delta.magnitude;
        bool isMoving = distanceThisFrame > moveThreshold;

        if (isMoving)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            // Option: remettre à stepInterval plutôt que 0 si tu veux éviter le "pas instant" au redémarrage
            stepTimer = 0f;
        }

        lastPosition = movementSource.position;
    }

    void PlayFootstep()
    {
        if (metalFootsteps == null || metalFootsteps.Length == 0) return;
        if (!audioSource) return;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.volume = Random.Range(0.8f, 1.0f);

        var clip = metalFootsteps[Random.Range(0, metalFootsteps.Length)];
        audioSource.PlayOneShot(clip);
    }
}
