using UnityEngine;
using System.Collections;

public class SimpleRaycaster : MonoBehaviour {

    public LayerMask mask;
    public Vector3 rayVector;
    public float length;
    //public float lastDistance;

    void OnDrawGizmos() {
        float dist = GetDistance();
        Gizmos.color = new Color(1.0f - (dist / (length/3.0f)), (dist / (length/3.0f)), 0.0f); 
        Gizmos.DrawRay(transform.position, transform.TransformDirection(rayVector).normalized * dist);
        Gizmos.DrawRay(transform.position + transform.right * 0.05f, transform.TransformDirection(rayVector).normalized * dist + transform.right * 0.05f);
        Gizmos.DrawRay(transform.position +- transform.right * 0.05f, transform.TransformDirection(rayVector).normalized * dist - transform.right * 0.05f);
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
