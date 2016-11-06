using UnityEngine;
using System.Collections;

public class SimpleRaycaster : MonoBehaviour {

    public LayerMask mask;
    public Vector3 rayVector;
    public float length;
    //public float lastDistance;

    void OnDrawGizmos() {
        Gizmos.color = new Color((GetDistance() / length), 1.0f - (GetDistance() / length), 0.0f); 
        Gizmos.DrawRay(transform.position, transform.TransformDirection(rayVector).normalized * length);
    }

    public float GetDistance() {
        Ray ray = new Ray(transform.position, transform.TransformDirection(rayVector).normalized);
        RaycastHit hit;
        float distance = float.PositiveInfinity;
        if (Physics.Raycast(ray, out hit, length, mask)) {
            distance = hit.distance;
        }
        return distance;
    }
}
