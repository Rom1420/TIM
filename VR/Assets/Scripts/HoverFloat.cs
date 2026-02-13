using UnityEngine;

public class HoverFloat : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.03f;
    [SerializeField] private float frequency = 0.8f;
    [SerializeField] private float rotateDegrees = 1.5f;

    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;

    private void OnEnable()
    {
        baseLocalPosition = transform.localPosition;
        baseLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        float t = Time.time * frequency * Mathf.PI * 2f;

        float yOffset = Mathf.Sin(t) * amplitude;
        float angle = Mathf.Sin(t) * rotateDegrees;

        transform.localPosition = baseLocalPosition + new Vector3(0f, yOffset, 0f);
        transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, 0f, angle);
    }
}
