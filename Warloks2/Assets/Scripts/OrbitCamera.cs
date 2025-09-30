using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Orbit Settings")]
    public float orbitSpeed = 30f;
    public float distance = 5f;
    public float height = 2f;
    public Vector3 orbitAxis = Vector3.up;

    [Header("Orbit Path")]
    public bool ellipticalOrbit = false;
    public float ellipseWidth = 1f;

    private float currentAngle = 0f;

    void Update()
    {
        if (target == null) return;

        // Увеличиваем угол вращения
        currentAngle += orbitSpeed * Time.deltaTime;
        if (currentAngle > 360f) currentAngle -= 360f;

        // Вычисляем позицию на орбите
        Vector3 orbitPosition = CalculateOrbitPosition(currentAngle);

        // Устанавливаем позицию и направление камеры
        transform.position = orbitPosition;
        transform.LookAt(target.position + Vector3.up * height * 0.5f);
    }

    private Vector3 CalculateOrbitPosition(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;

        float x = Mathf.Cos(rad) * distance;
        float z = Mathf.Sin(rad) * distance;

        if (ellipticalOrbit)
        {
            x *= ellipseWidth;
        }

        Vector3 orbitPos = new Vector3(x, height, z);
        return target.position + orbitPos;
    }

    // Визуализация орбиты в редакторе
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, distance);

            if (ellipticalOrbit)
            {
                Gizmos.color = Color.red;
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(target.position, Quaternion.identity, new Vector3(ellipseWidth, 1f, 1f));
                Gizmos.DrawWireSphere(Vector3.zero, distance);
                Gizmos.matrix = oldMatrix;
            }
        }
    }
}