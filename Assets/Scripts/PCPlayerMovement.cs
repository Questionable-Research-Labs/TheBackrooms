using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCPlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    [SerializeField] public float speed = 12f; 
    
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        var transform1 = transform;
        Vector3 move = transform1.right * x + transform1.forward * z;

        controller.Move(move * speed * Time.deltaTime);
    }
}
