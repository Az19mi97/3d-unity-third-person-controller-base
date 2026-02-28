using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Hvem kameraet følger
    public Vector3 offset;   // Hvor kameraet skal være i forhold til target
    public float smoothSpeed = 0.125f; // Hvor hurtigt kameraet følger

    void LateUpdate()
    {
        if(target == null)
            return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Optional: kamera kigger altid på spilleren
        transform.LookAt(target);
    }
}