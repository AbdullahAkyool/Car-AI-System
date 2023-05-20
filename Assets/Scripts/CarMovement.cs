using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    [Header("PATH")]
    
    public Transform path;
    public List<Transform> nodes;
    public int currentNode = 0;

    [Header("WHEELS and COLLIDERS")]
    
    public float maxSteerAngle = 30f;
    private float targetSteerAngle = 0f;
    public float turnSpeed = 5f;
    
    public WheelCollider frontRightCollider;
    public WheelCollider frontLeftCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    public Transform frontRightWheel;
    public Transform frontLeftWheel;
    public Transform rearRightWheel;
    public Transform rearLeftWheel;
    
    [Header("MOTOR")]

    public float maxMotorTorque = 80f;
    public float currentSpeed;
    public float maxSpeed = 100f;
    public float maxBrakeTorque;
    public bool isBrake = false;

    [Header("SENSORS")] 
    
    public float sensorLength;
    public Vector3 frontSensorPosition;
    public float frontSideSensorPosition;
    public float frontSensorAngle = 30f;
    public bool isAvoiding = false;

    //private bool isFCsensorActive, isFRsensorActive, isFLsensorActive, isRAsensorActive, isLAsensorActive = false; 
    

    void Start()
    {
        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != path.transform)
            {
                nodes.Add(pathTransforms[i]);
            }
        }
    }
    
    void FixedUpdate()
    {
        Sensors();
        ApplySteer();
        Drive();
        UpdateWheelPose();
        CheckWayPointDistance();
        Brake();
        LerpToSteerAngle();
    }
    
    private void Sensors()
    {
        RaycastHit hit;
        Vector3 sensorStartPos = transform.position;
        
        float avoidMultiplier = 0f;
        isAvoiding = false;
        
        ////////////  FRONT CENTER SENSOR
        sensorStartPos += transform.forward * frontSensorPosition.z;
        sensorStartPos += transform.up * frontSensorPosition.y;
        
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if (hit.collider == true)
            {
                Debug.DrawLine(sensorStartPos,hit.point);
                isAvoiding = true;
                if (hit.normal.x < 0)
                {
                    avoidMultiplier = -1;
                }
                if (hit.normal.x > 0)
                {
                    avoidMultiplier = +1;
                }
                // if (hit.normal.x == 0)
                // {
                //     isBrake = true;
                // }
            }
        }
        
        ////////////  FRONT RIGHT SENSOR
        sensorStartPos += transform.right * frontSideSensorPosition;
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if (hit.collider == true)
            {
                Debug.DrawLine(sensorStartPos,hit.point);
                isAvoiding = true;
                avoidMultiplier -= 1f;
            }
        }
        
        ////////////  FRONT RIGHT ANGLE SENSOR

        else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(frontSensorAngle,transform.up) * transform.forward, out hit, sensorLength))
        {
            if (hit.collider == true)
            {
                Debug.DrawLine(sensorStartPos,hit.point);
                isAvoiding = true;
                avoidMultiplier -= .5f;
            }
        }
        
        ////////////  FRONT LEFT SENSOR
        sensorStartPos -= transform.right * frontSideSensorPosition * 2f;
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if (hit.collider == true)
            {
                Debug.DrawLine(sensorStartPos,hit.point);
                isAvoiding = true;
                avoidMultiplier += 1f;
            }
        }

        ////////////  FRONT LEFT ANGLE SENSOR

        else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(-frontSensorAngle,transform.up) * transform.forward, out hit, sensorLength))
        {
            if (hit.collider == true)
            {
                Debug.DrawLine(sensorStartPos,hit.point);
                isAvoiding = true;
                avoidMultiplier += .5f;
            }
        }
        
        if (isAvoiding)
        {
            targetSteerAngle = maxSteerAngle * avoidMultiplier;
            // frontLeftCollider.steerAngle = maxSteerAngle * avoidMultiplier;
            // frontRightCollider.steerAngle = maxSteerAngle * avoidMultiplier;
        }
    }
    
    private void ApplySteer()
    {
        if(isAvoiding) return;
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode].position);
        float wheelAngle = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;

        targetSteerAngle = wheelAngle;
        // frontLeftCollider.steerAngle = wheelAngle;
        // frontRightCollider.steerAngle = wheelAngle;
    }
    
    private void Drive()
    {
        currentSpeed = 2 * Mathf.PI * rearLeftCollider.radius * rearLeftCollider.rpm * 60 / 1000;

        if (currentSpeed < maxSpeed && !isBrake)
        {
            rearLeftCollider.motorTorque = maxMotorTorque;
            rearRightCollider.motorTorque = maxMotorTorque;
        }
        else
        {
            rearLeftCollider.motorTorque = 0;
            rearRightCollider.motorTorque = 0;
        }
    }

    private void Brake()
    {
        if (isBrake)
        {
            frontLeftCollider.brakeTorque = maxBrakeTorque;
            frontRightCollider.brakeTorque = maxBrakeTorque;
        }
        else
        {
            frontLeftCollider.brakeTorque = 0;
            frontRightCollider.brakeTorque = 0;
        }
    }
    
    private void CheckWayPointDistance()
    {
        Debug.Log(Vector3.Distance(transform.position, nodes[currentNode].position));
        if (Vector3.Distance(transform.position, nodes[currentNode].position) <= .5f)
        {
            if (currentNode == nodes.Count - 1)
            {
                currentNode = 0;
            }
            else
            {
                currentNode++;
            }
        }
    }

    private void LerpToSteerAngle()
    {
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
    }

    private void WheelPoses(WheelCollider _collider, Transform _transform)
    {
        Vector3 pos = _transform.position;
        Quaternion rot = _transform.rotation;
        
        _collider.GetWorldPose(out pos, out rot);

        _transform.position = pos;
        _transform.rotation = rot;
    }

    private void UpdateWheelPose()
    {
        WheelPoses(frontLeftCollider,frontLeftWheel);
        WheelPoses(frontRightCollider, frontRightWheel);
        WheelPoses(rearLeftCollider, rearLeftWheel);
        WheelPoses(rearRightCollider, rearRightWheel);
    }
}
