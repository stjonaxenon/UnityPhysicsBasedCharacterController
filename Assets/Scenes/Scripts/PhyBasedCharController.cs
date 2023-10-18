using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PhyBasedCharController : MonoBehaviour
{
 
    [field:SerializeField] public InputReader InputReader { get; private set; }
    [field: SerializeField] public float rayCastMaxDistance = 30.0f ;
    [field: SerializeField] public float rideHeight = 1.5f ;
    [field: SerializeField] public float RideSpringStrength = 1000.0f  ;
    [field: SerializeField] public float RideSpringDamper = 100.0f ;
    [field: SerializeField] public float JumpingForce = 200.0f ;
    [field: SerializeField] public float moveForce = 3000.0f ;
    [field: SerializeField] public float centreOfMassOffsetY = 0.0f ;
    [field: SerializeField] public float uprightJointSpringStrength = 1000.0f  ;
    [field: SerializeField] public float uprightJointSpringDamper = 100.0f ;
    


    private Rigidbody _RB;
    
    void Start()
    {
        _RB = gameObject.GetComponent<Rigidbody>();
        Vector3 com = transform.position - new Vector3(0, centreOfMassOffsetY, 0);
        _RB.centerOfMass = com;
        InputReader.JumpEvent += OnJump;
    }

    void Update()
    {
        
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayCastMaxDistance))
        {
            Vector3 vel = _RB.velocity;
            Vector3 rayDir = transform.TransformDirection(-this.transform.up);
            Vector3 otherVel = Vector3.zero;
            Rigidbody hitBody = hit.rigidbody;

            if (hitBody)
            {
                otherVel = hitBody.velocity;
            }

            float rayDirVel = Vector3.Dot(rayDir, vel);
            float otherDirVel = Vector3.Dot(rayDir, otherVel);

            float relativeVel = rayDirVel - otherDirVel;

            float x = hit.distance - rideHeight;

            float springForce = (x * RideSpringStrength) - (relativeVel * RideSpringDamper);
            
            //Debug.DrawLine(transform.position, transform.position + (rayDir * springForce), Color.green);
            //Debug.Log(hit.distance);
            
            _RB.AddForce(rayDir * springForce);

            if (hitBody)
            {
                hitBody.AddForceAtPosition(rayDir * -springForce, hit.point);
            }
        }
        
        //Rotate
        UpdateUprightForce();
        UpdateFacingDirection();
        
        Move();
        
    }

    private void UpdateFacingDirection()
    {
        if (_RB.velocity.magnitude < 1.0f) {return;}
        Vector3 forwardDir = _RB.velocity.normalized;
        Debug.DrawLine(transform.position, transform.position + forwardDir * 100, Color.magenta);
        Quaternion targetRotation = Quaternion.LookRotation(forwardDir);
        transform.rotation = new Quaternion(transform.rotation.x, targetRotation.y, transform.rotation.z, transform.rotation.w);
        Debug.Log(targetRotation.y);
    }

    private void UpdateUprightForce()
    {
        Quaternion characterCurrent = transform.rotation;
        Quaternion uprightJointTargetRot = new Quaternion(0, characterCurrent.y,0, characterCurrent.w);
        Quaternion toGoal = ShortestRotation(uprightJointTargetRot, characterCurrent);

        Vector3 rotAxis;
        float rotDegrees;
        
        toGoal.ToAngleAxis(out rotDegrees, out rotAxis);
        rotAxis.Normalize();

        float rotRadians = rotDegrees * Mathf.Deg2Rad;
        
        _RB.AddTorque((rotAxis * (rotRadians * uprightJointSpringStrength)) - (_RB.angularVelocity * uprightJointSpringDamper));
    }

    private Quaternion ShortestRotation(Quaternion goalQuanternion, Quaternion currentQuanternion)
    {
        {

            if (Quaternion.Dot(goalQuanternion, currentQuanternion) < 0)

            {

                return goalQuanternion * Quaternion.Inverse(Multiply(currentQuanternion, -1));

            }

            else return goalQuanternion * Quaternion.Inverse(currentQuanternion);

        }
    }
    
    private static Quaternion Multiply(Quaternion input, float scalar)

    {

        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);

    }

    private void OnDestroy()
    {
        InputReader.JumpEvent -= OnJump;
    }
    
    private void OnJump()
    {
       _RB.AddForce(transform.up * JumpingForce);
    }

    
    //这个函数是临时的，
    //TODO: 增加阻尼，保证松开按键后角色会自动停止。修正角色运动方向，目前是前进时延x轴运动，右是延z轴运动。修改为根据相机朝向运动。
    private void Move()
    {
        if (InputReader.MovementValue == Vector2.zero)
        {
            return;
        }
        
        Vector2 inputMoveDir = InputReader.MovementValue.normalized;
        Vector3 moveDir = new Vector3(inputMoveDir.x, 0, inputMoveDir.y);
        Vector3 move = moveDir * moveForce * Time.deltaTime;
        _RB.AddForce(move); 
    }
}
