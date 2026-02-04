using UnityEngine;
using UnityEngine.EventSystems;
using Lean.Touch;

public class LeanCubeInteractor : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera arCamera;

    [Header("Raycast")]
    [SerializeField] private float maxDistance = 100f;
    [Tooltip("Layers cliquables (cubes). Optionnel mais recommandé.")]
    [SerializeField] private LayerMask cubeLayers = ~0;

    [Header("Hold to open details")]
    [Tooltip("Durée (sec) à maintenir pour ouvrir les détails.")]
    [SerializeField] private float holdSeconds = 0.6f;
    [Tooltip("Tolérance en pixels avant de considérer que le doigt bouge (annule le hold).")]
    [SerializeField] private float holdMoveThresholdPx = 20f;

    [Header("Distance selection (tap)")]
    [SerializeField] private BuildingSelectionDistanceManager selectionManager;

    // état hold
    private LeanFinger activeFinger;
    private Vector2 fingerStartPos;
    private float fingerStartTime;
    private BuildingCube holdCandidate;
    private bool holdTriggered;

    private void OnEnable()
    {
        LeanTouch.OnFingerDown += OnFingerDown;
        LeanTouch.OnFingerUpdate += OnFingerUpdate;
        LeanTouch.OnFingerUp += OnFingerUp;
        LeanTouch.OnFingerTap += OnFingerTap;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerDown -= OnFingerDown;
        LeanTouch.OnFingerUpdate -= OnFingerUpdate;
        LeanTouch.OnFingerUp -= OnFingerUp;
        LeanTouch.OnFingerTap -= OnFingerTap;
    }

    private void OnFingerDown(LeanFinger finger)
    {
        if (IsOverUI(finger)) return;

        activeFinger = finger;
        fingerStartPos = finger.ScreenPosition;
        fingerStartTime = Time.time;
        holdTriggered = false;

        holdCandidate = RaycastCube(finger.ScreenPosition);
        // si pas sur un cube, pas de hold
    }

    private void OnFingerUpdate(LeanFinger finger)
    {
        if (finger != activeFinger) return;
        if (holdTriggered) return;
        if (holdCandidate == null) return;

        // si le doigt bouge trop -> annule le hold
        float move = Vector2.Distance(finger.ScreenPosition, fingerStartPos);
        if (move > holdMoveThresholdPx)
        {
            holdCandidate = null;
            return;
        }

        // si on atteint la durée -> déclenche les détails
        float held = Time.time - fingerStartTime;
        if (held >= holdSeconds)
        {
            holdTriggered = true;

            // Ouvre les détails
            holdCandidate.HandleTap();

            // Important: on neutralise la sélection par tap
            // LeanTouch va souvent encore envoyer un Tap si le doigt est relâché :
            // on s’en protège via holdTriggered dans OnFingerTap.
        }
    }

    private void OnFingerUp(LeanFinger finger)
    {
        if (finger == activeFinger)
        {
            activeFinger = null;
            holdCandidate = null;
            holdTriggered = false;
        }
    }

    private void OnFingerTap(LeanFinger finger)
    {
        if (IsOverUI(finger)) return;

        // Si on a déjà déclenché un hold, on ignore le tap de fin
        if (finger == activeFinger && holdTriggered) return;

        BuildingCube cube = RaycastCube(finger.ScreenPosition);
        if (cube == null) return;

        // Tap court = sélection distance
        if (selectionManager != null)
            selectionManager.Select(cube);
    }

    private bool IsOverUI(LeanFinger finger)
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject(finger.Index);
    }

    private BuildingCube RaycastCube(Vector2 screenPos)
    {
        if (arCamera == null) return null;

        Ray ray = arCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, cubeLayers))
            return hit.collider.GetComponentInParent<BuildingCube>();

        return null;
    }
}
