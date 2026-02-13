using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class LocomotionMotor : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 2.0f;
    public float sprintMultiplier = 1.5f;
    public Transform moveReference;

    [Header("Turn")]
    public bool snapTurn = false;
    public float smoothTurnSpeed = 90f;
    public float snapAngle = 45f;
    public float snapCooldown = 0.25f;

    [Header("Keyboard (AZERTY)")]
    public bool enableKeyboard = true;

    float _snapTimer;

    private void Awake()
    {
        if (moveReference == null)
        {
            var centerEye = GameObject.Find("CenterEyeAnchor");
            if (centerEye != null) moveReference = centerEye.transform;
            else if (Camera.main != null) moveReference = Camera.main.transform;
        }
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (!enableKeyboard) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        float x = 0f;
        float z = 0f;

        if (kb.aKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed) x += 1f;
        if (kb.wKey.isPressed) z += 1f;
        if (kb.sKey.isPressed) z -= 1f;

        bool sprint = kb.leftShiftKey.isPressed;

        float turn = 0f;
        if (kb.eKey.isPressed) turn += 1f;
        if (kb.qKey.isPressed) turn -= 1f;

        ApplyInput(new Vector2(x, z), turn, sprint);
#endif
    }

    public void ApplyInput(Vector2 moveAxis, float turn, bool sprint)
    {
        Transform reference = moveReference != null ? moveReference : transform;

        Vector3 right = reference.right;
        Vector3 forward = reference.forward;

        right.y = 0f;
        forward.y = 0f;

        right.Normalize();
        forward.Normalize();

        Vector3 move = right * moveAxis.x + forward * moveAxis.y;
        if (move.sqrMagnitude > 1f) move.Normalize();

        float speed = moveSpeed * (sprint ? sprintMultiplier : 1f);
        transform.position += move * speed * Time.deltaTime;

        if (!snapTurn)
        {
            transform.Rotate(0f, turn * smoothTurnSpeed * Time.deltaTime, 0f);
        }
        else
        {
            _snapTimer -= Time.deltaTime;
            if (_snapTimer <= 0f && Mathf.Abs(turn) > 0.5f)
            {
                transform.Rotate(0f, Mathf.Sign(turn) * snapAngle, 0f);
                _snapTimer = snapCooldown;
            }
        }
    }
}
