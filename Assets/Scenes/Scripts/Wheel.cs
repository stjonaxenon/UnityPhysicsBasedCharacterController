using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    
    [field: SerializeField, Header("RIDE SPRING")] 
    public float rayCastMaxDistance = 1f ;
    [field: SerializeField] public float RideSpringStrength = 1000.0f  ;
    [field: SerializeField] public float RideSpringDamper = 100.0f ;

    [field: SerializeField, Header("LOCOMOTION")]
    public float tractionForce = 1000.0f; 
    [field: SerializeField] public float tractionForceDamper = 100.0f;
    [field: SerializeField] bool isTurnWheel;
    public Rigidbody VehicleRB { get; set; }
    public Vector2 InputMoveDir { get; set; }
    
    // Start is called before the first frame update
    void Start()
    {
        InputMoveDir = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        



    }

    private void FixedUpdate()
    {
        Vector3 finalForce = Vector3.zero;
        
        if (Physics.Raycast(transform.position,
                 -transform.up,out RaycastHit hit, rayCastMaxDistance) && VehicleRB)
        {


            finalForce = GetSuspentionForce(hit) + GetForwardForce() + GetFriction();
           Debug.DrawLine(transform.position,transform.position + finalForce.normalized * 5, Color.blue);

            VehicleRB.AddForceAtPosition( finalForce , transform.position  );
        }
        
        //GiveMoveForce();
    }

    private Vector3 GetSuspentionForce(RaycastHit hit)
    {
        float currentSpringLength = hit.distance;
        float springForceFactor = 1 - (currentSpringLength / rayCastMaxDistance);

        Vector3 springForce = transform.up * RideSpringStrength * springForceFactor;
        Vector3 springDamperForce =
            -transform.up * Vector3.Dot(VehicleRB.GetPointVelocity(transform.position), transform.up) * RideSpringDamper;

        Debug.DrawLine(transform.position,
            transform.position + -transform.up * springForceFactor * rayCastMaxDistance, Color.green);
        return springForce + springDamperForce;
    }

    private Vector3 GetForwardForce()
    {
        //Debug.Log(transform.forward * VehicleRB.velocity.magnitude * tractionForceDamper);
       return  transform.forward * InputMoveDir.y * tractionForce - transform.forward * VehicleRB.velocity.magnitude * tractionForceDamper;
    }

    private Vector3 GetFriction()
    {
        float slipSpeed = Vector3.Dot(VehicleRB.velocity, transform.right);
        return -transform.right * slipSpeed * VehicleRB.mass * 10;
    }
}
