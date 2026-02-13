using Oculus.Interaction;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoverAroundPlayer : MonoBehaviour
{
    [SerializeField] private Grabbable grabbable;

    [Header("Anchor (auto camera)")]
    [SerializeField] private Transform cameraTransform; // auto-resolved at runtime
    [SerializeField] private Vector3 anchorOffset = new Vector3(0f, -0.15f, 0.7f);

    [Header("Follow (spring)")]
    [SerializeField] private float followSpring = 60f;
    [SerializeField] private float followDamping = 18f;
    [SerializeField] private float maxSpeed = 1.8f;

    [Header("Float feel")]
    [SerializeField] private float bobAmplitude = 0.03f;
    [SerializeField] private float bobFrequency = 0.8f;

    [Header("Physics stabilization when idle")]
    [SerializeField] private float idleDrag = 6f;
    [SerializeField] private float idleAngularDrag = 10f;

    [Header("Billboard")]
    [SerializeField] private bool faceCameraWhenIdle = true;
    [SerializeField] private bool lockPitch = false; // FALSE = suit pitch+yaw (rotation horizontale + verticale)
    [SerializeField] private float rotateSpeed = 12f;

    private Rigidbody rb;
    private bool wasGrabbedLastFrame;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (grabbable == null) grabbable = GetComponent<Grabbable>();
        ResolveCamera();
    }

    private void ResolveCamera()
    {
        if (cameraTransform != null) return;

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            return;
        }

        var rig = FindFirstObjectByType<OVRCameraRig>();
        if (rig != null && rig.centerEyeAnchor != null)
        {
            cameraTransform = rig.centerEyeAnchor;
            return;
        }

        Debug.LogWarning("[HoverAroundPlayer] No camera found in scene.");
    }

    private void FixedUpdate()
    {
        bool isGrabbed = grabbable != null && grabbable.SelectingPointsCount > 0;

        if (isGrabbed)
        {
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
            rb.constraints = RigidbodyConstraints.None;

            wasGrabbedLastFrame = true;
            return;
        }

        if (wasGrabbedLastFrame)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            wasGrabbedLastFrame = false;
        }

        rb.linearDamping = idleDrag;
        rb.angularDamping = idleAngularDrag;

        // ✅ Important: on freeze la physique, mais on laisse le script orienter l'objet
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (cameraTransform == null) return;

        Vector3 bob = Vector3.up * (Mathf.Sin(Time.time * (Mathf.PI * 2f) * bobFrequency) * bobAmplitude);
        Vector3 targetPos = cameraTransform.TransformPoint(anchorOffset) + bob;

        Vector3 error = targetPos - rb.position;
        Vector3 vel = rb.linearVelocity;

        Vector3 force = (error * followSpring) - (vel * followDamping);
        rb.AddForce(force, ForceMode.Acceleration);

        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        if (faceCameraWhenIdle)
        {
            Vector3 lookDir = (transform.position - cameraTransform.position); // direction caméra -> objet
            if (lockPitch) lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime);
            }
        }
    }
}
