using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public enum TransformType { Rotation, Movement }
    public enum Axis { X, Y, Z }

    [Header("General Settings")]
    public TransformType transformType = TransformType.Rotation;
    public Axis axis = Axis.Y;
    public float amount = 90f; // angle in degrees or distance in units
    public float speed = 3f;
    public bool invert = false;

    private bool isOpen = false;
    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    void Start()
    {
        if (transformType == TransformType.Movement)
        {
            initialPosition = transform.localPosition;

            Vector3 direction = GetAxisVector();
            if (invert) direction *= -1;

            targetPosition = initialPosition + direction * amount;
        }
        else if (transformType == TransformType.Rotation)
        {
            initialRotation = transform.localRotation;

            Vector3 axisVector = GetAxisVector();
            if (invert) axisVector *= -1;

            targetRotation = initialRotation * Quaternion.AngleAxis(amount, axisVector);
        }
    }

    void Update()
    {
        if (transformType == TransformType.Movement)
        {
            Vector3 target = isOpen ? targetPosition : initialPosition;
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * speed);
        }
        else if (transformType == TransformType.Rotation)
        {
            Quaternion target = isOpen ? targetRotation : initialRotation;
            transform.localRotation = Quaternion.Lerp(transform.localRotation, target, Time.deltaTime * speed);
        }
    }

    public void Toggle()
    {
        isOpen = !isOpen;
    }

    private Vector3 GetAxisVector()
    {
        switch (axis)
        {
            case Axis.X: return Vector3.right;
            case Axis.Y: return Vector3.up;
            case Axis.Z: return Vector3.forward;
            default: return Vector3.up;
        }
    }
}
