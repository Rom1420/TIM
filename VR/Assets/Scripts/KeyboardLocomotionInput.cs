using UnityEngine;

[RequireComponent(typeof(LocomotionMotor))]
public class KeyboardLocomotionInput : MonoBehaviour
{
    public LocomotionMotor motor;

    [Header("Keys (AZERTY)")]
    public KeyCode forwardKey = KeyCode.Z;
    public KeyCode backKey    = KeyCode.S;

    public KeyCode strafeLeftKey  = KeyCode.Q;
    public KeyCode strafeRightKey = KeyCode.D;

    public KeyCode turnLeftKey  = KeyCode.None;
    public KeyCode turnRightKey = KeyCode.None;

    public KeyCode sprintKey = KeyCode.LeftShift;

    void Reset()
    {
        motor = GetComponent<LocomotionMotor>();
    }

    void Awake()
    {
        if (motor == null) motor = GetComponent<LocomotionMotor>();
    }

    void Update()
    {
        // Move axis (comme joystick gauche)
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(strafeLeftKey))  x -= 1f;
        if (Input.GetKey(strafeRightKey)) x += 1f;

        if (Input.GetKey(forwardKey)) y += 1f;
        if (Input.GetKey(backKey))    y -= 1f;

        Vector2 moveAxis = new Vector2(x, y);
        if (moveAxis.sqrMagnitude > 1f) moveAxis.Normalize();

        // Turn (comme joystick droit X)
        float turn = 0f;
        if (Input.GetKey(turnLeftKey))  turn -= 1f;
        if (Input.GetKey(turnRightKey)) turn += 1f;

        bool sprint = Input.GetKey(sprintKey);

        motor.ApplyInput(moveAxis, turn, sprint);
    }
}
