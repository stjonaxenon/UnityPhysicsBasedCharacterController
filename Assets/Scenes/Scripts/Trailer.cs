using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trailer : MonoBehaviour
{
    [field: SerializeField, Header("DEPENDENCIES")] 
    public GameObject WheelFR { get; private set; }
    [field: SerializeField] public GameObject WheelFL { get; private set; }
    [field: SerializeField] public GameObject WheelBR { get; private set; }
    [field: SerializeField] public GameObject WheelBL { get; private set; }

    [field: SerializeField] public Vector3 centerOfMaceOffset;

    
    private GameObject[] _wheels;
    
    public Rigidbody RB { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        _wheels = new GameObject[4];
        _wheels[0] = WheelFR;
        _wheels[1] = WheelFL;
        _wheels[2] = WheelBR;
        _wheels[3] = WheelBL;
        RB.centerOfMass += centerOfMaceOffset;
        for (int i = 0; i < _wheels.Length; i++)
        {
            if (RB && _wheels[i])
            {
                _wheels[i].GetComponent<Wheel>().VehicleRB = RB;
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
