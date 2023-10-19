using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PhyBasedCharController : MonoBehaviour
{
 
    [field: SerializeField, Header("Dependencies")] 
    public InputReader InputReader { get; private set; }
    [field: SerializeField] public GameObject CentreOfMassSphere { get; private set; }

    
    [field: SerializeField, Header("Ride Spring")] 
    public float rayCastMaxDistance = 3.0f ;
    [field: SerializeField] public float rideHeight = 1.5f ;
    [field: SerializeField] public float RideSpringStrength = 1000.0f  ;
    [field: SerializeField] public float RideSpringDamper = 100.0f ;

    [field: SerializeField, Header("Locomotion")]
    public float maxSpeed = 8.0f;
    [field: SerializeField] public float acceleration = 200.0f;
    [field: SerializeField] public AnimationCurve accelerationFactorFromDot;
    [field: SerializeField] public float maxAccelForce = 150f;
    //[field: SerializeField] public AnimationCurve maxAccelerationForceFactorFromDot;
    [field: SerializeField] public float speedFactor = 1.0f;
    [field: SerializeField] public Vector3 forceScale = new Vector3(1.0f, 0f, 1.0f);
    [field: SerializeField] public float gravityScaleDrop = 10.0f;

    
    //[field: SerializeField] public float moveForce = 3000.0f ;
    [field: SerializeField] public float centreOfMassOffsetY = -10.0f ;
    [field: SerializeField] public float uprightJointSpringStrength = 1000.0f  ;
    [field: SerializeField] public float uprightJointSpringDamper = 100.0f ;
    
    
    
    [field: SerializeField, Header("Jumping")] 
    public float JumpingForce = 200.0f ;


    private Rigidbody _RB;
    private Vector3 _COM; // Centre of Mass.
    private Vector3 m_GoalVel;
    private Vector3 groundVel; // ray cast 后当前地面的移动速度
    
    void Start()
    {
        _RB = gameObject.GetComponent<Rigidbody>();
        
        
        _COM = _RB.centerOfMass + new Vector3(0, centreOfMassOffsetY, 0);
        _RB.centerOfMass = _COM;
        CentreOfMassSphere.transform.position = transform.TransformPoint(_COM);
        
        m_GoalVel = Vector3.zero;
        
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
                groundVel = hitBody.velocity;
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
            
            _RB.useGravity = false;
        }

        else
        {
            _RB.useGravity = true;
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
        //Debug.Log(targetRotation.y);
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
        // if (InputReader.MovementValue == Vector2.zero)
        // {
        //     return;
        // }
        
        Vector2 inputMoveDir = InputReader.MovementValue.normalized;
        

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        Vector3 move = inputMoveDir.y * cameraForward + inputMoveDir.x * cameraRight;
        if (move.magnitude > 1.0f)
        {
            move.Normalize();
        }

        // WSAD键盘信息转化为在3D世界中的单位向量。
        Vector3 m_UnitGoal = move;
        
        Debug.DrawLine(transform.position, transform.position + m_UnitGoal * 10, Color.magenta, 1.0f);
        
        // 计算 新的 goal vel
        Vector3 unitVel = m_GoalVel.normalized;

        float velDot = Vector3.Dot(m_UnitGoal, unitVel);

        // 通过曲线计算加速度，当目标移动方向于实际移动方向相反时，加速度是原来的两倍。
        // 这样可以使角色转头的时候更灵活。
        float accel = acceleration * accelerationFactorFromDot.Evaluate(velDot);
        
        // goalVel 是角色最终希望达到的速度向量，这个速度不受maxAccelForce 的限制。
        Vector3 goalVel = m_UnitGoal * maxSpeed * speedFactor;

        // m_GoalVel: 在下一个物理模拟帧中，角色希望达到的速度，
        // groundVel: 脚下地面移动的速度
        m_GoalVel = Vector3.MoveTowards(m_GoalVel, (goalVel) + (groundVel), accel * Time.fixedDeltaTime);

        // 已知下一物理帧需要的速度，求要达到这个加速度所需要的力。
        Vector3 neededAccel = (m_GoalVel - _RB.velocity) / Time.fixedDeltaTime;

        //float maxAccel = maxAccelForce * maxAccelerationForceFactorFromDot(velDot) * maxAccelForceFactor;
        float maxAccel = maxAccelForce;

         neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);
        
        _RB.AddForce(Vector3.Scale(neededAccel * _RB.mass, forceScale)); 
    }
}
