using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset;

    [Header("Clamp X Using Transforms")]
    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        Vector3 smoothed = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);

        // Keep camera z
        smoothed.z = transform.position.z;

        if (_cam != null && leftLimit != null && rightLimit != null)
        {
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            float minX = leftLimit.position.x + halfW;
            float maxX = rightLimit.position.x - halfW;

            smoothed.x = Mathf.Clamp(smoothed.x, minX, maxX);
        }

        transform.position = smoothed;
    }
}