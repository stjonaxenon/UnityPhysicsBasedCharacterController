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
    [field: SerializeField] public bool isTurnWheel;
    [SerializeField] private float frictionFactor;

    [field: SerializeField, Header("WHEEL MESH")]
    private GameObject wheelMash;
    [SerializeField] private float wheelMashRadius;
    [SerializeField] private float wheelMashOffset;
    
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
        
        if (Physics.SphereCast(transform.position,0.15f,
                 -transform.up,out RaycastHit hit, rayCastMaxDistance) && VehicleRB)
        {


            finalForce = GetSuspentionForce(hit) + GetForwardForce() + GetFriction();
            //画合力
           Debug.DrawLine(transform.position,transform.position + finalForce.normalized * 5, Color.blue);
           //画摩擦力
           /*Debug.DrawLine(transform.position,
               transform.position + GetFriction().normalized * GetFriction().magnitude/finalForce.magnitude , 
               Color.red);*/
           // 画推进力
           Debug.DrawLine(transform.position,
               transform.position + GetForwardForce().normalized * GetForwardForce().magnitude/finalForce.magnitude , 
               Color.magenta);


            VehicleRB.AddForceAtPosition( finalForce , transform.position  );
            
            UpdateWheelMeshPosition(hit);
        }
        else
        {
            UpdateWheelMeshPosition();
        }
        
        SetTurnWheelRotation();
        
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
        Vector3 forwardForce = transform.forward * Mathf.Round(InputMoveDir.y)  * tractionForce -
                               transform.forward * VehicleRB.velocity.magnitude * tractionForceDamper;
        return forwardForce;
    }

    private Vector3 GetFriction()
    {
        float slipSpeed = Vector3.Dot(VehicleRB.GetPointVelocity(transform.position), transform.right);
        Vector3 slipVel = transform.right * slipSpeed;
        //if (slipVel.magnitude < 0.001f) {return Vector3.zero;}
        Vector3 antiSlipVel = -slipVel;
        Debug.DrawLine(transform.position, transform.position + antiSlipVel, Color.red);

        Vector3 antiSlipAccl = antiSlipVel / Time.fixedDeltaTime;
        VehicleRB.AddForceAtPosition(antiSlipAccl * frictionFactor, transform.position,ForceMode.Acceleration);
        //Vector3 antiSlipForce = VehicleRB.mass * antiSlipAccl;
        //return  antiSlipForce;
        return Vector3.zero;
    }

    private void SetTurnWheelRotation()
    {
        if (!isTurnWheel) { return; }

        //Debug.Log(transform.rotation);
        Quaternion targetRotation = VehicleRB.rotation * Quaternion.Euler(0, 45 * Mathf.Round(InputMoveDir.x), 0);
        transform.rotation = targetRotation;
    }

    private void UpdateWheelMeshPosition()
    {
        if (!wheelMash) {return;}

        wheelMash.transform.position = transform.position - transform.up * rayCastMaxDistance + transform.up * (wheelMashRadius + wheelMashOffset);
    }
    
    private void UpdateWheelMeshPosition(RaycastHit hit)
    {
        if (!wheelMash) {return;}

        wheelMash.transform.position = transform.position - transform.up * hit.distance + transform.up * (wheelMashRadius + wheelMashOffset);
    }
}
