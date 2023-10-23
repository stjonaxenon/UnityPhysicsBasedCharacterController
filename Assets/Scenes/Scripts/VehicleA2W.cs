using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VehicleA2W : MonoBehaviour
{
    [field: SerializeField, Header("DEPENDENCIES")] 
    public InputReader InputReader { get; private set; }
    [field: SerializeField] public GameObject WheelF { get; private set; }
    [field: SerializeField] public GameObject WheelB { get; private set; }

    [field: SerializeField] public Vector3 centerOfMaceOffset;

    
    private GameObject[] _wheels;
    public Rigidbody RB { get; set; }
    void Start()
    {
        InputReader.JumpEvent += OnJump;

        RB = GetComponent<Rigidbody>();
        
        _wheels = new GameObject[2];
        _wheels[0] = WheelF;
        _wheels[1] = WheelB;

        RB.centerOfMass += centerOfMaceOffset;
        
        for (int i = 0; i < _wheels.Length; i++)
        {
            if (RB && _wheels[i])
            {
                _wheels[i].GetComponent<Wheel>().VehicleRB = RB;
            }
            
        }
    }

    private void OnJump()
    {
    }

    void Update()
    {
        Move();
    }

    private void FixedUpdate()
    {
       
    }

    void Move()
    {
        Vector2 inputMoveDir = InputReader.MovementValue;
        
        for (int i = 0; i < _wheels.Length; i++)
        {
            if (RB && _wheels[i])
            {
                _wheels[i].GetComponent<Wheel>().InputMoveDir = inputMoveDir;
            }
            
        }
    }

    private void OnDestroy()
    {
        InputReader.JumpEvent -= OnJump;
    }
}
