using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    [SerializeField] private float chaseSpeed = 5f;

    private void FixedUpdate() 
    { 
        transform.position = Vector3.Lerp(transform.position, target.position + offset, chaseSpeed * Time.deltaTime);   
    }
    
}
