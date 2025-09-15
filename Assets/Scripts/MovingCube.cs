using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCube : MonoBehaviour
{
    [SerializeField]
    public float moveSpeed = 1f;
    public static MovingCube CurrentCube { get; private set; }
    public static MovingCube LastCube { get; private set; }

    internal void Stop()
    {
        moveSpeed = 0f;
        float hangover = transform.position.z - LastCube.transform.position.z;
        SplitCubeOnZ(hangover);
    }

    private void SplitCubeOnZ(float hangover)
    {
        float newSize = -LastCube.transform.localScale.z + Math.Abs(hangover);
        float fallingBlockSize = transform.localScale.z - newSize;

        // FIX: Change LastCube.transform.position.x to LastCube.transform.position.z
        float newZPosition = LastCube.transform.position.z + (hangover / 2);

        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, newSize);
        transform.position = new Vector3(transform.position.x, transform.position.y, newZPosition);
    }

    private void OnEnable()
    {
        CurrentCube = this;
        if (LastCube == null)
        {
            LastCube = GameObject.Find("Start").GetComponent<MovingCube>();
        }
    }


    void Update()
    {
        
        Vector3 movement = -Vector3.left - Vector3.forward;

        
        movement.Normalize();

        
        transform.position += movement * moveSpeed * Time.deltaTime;

    }
}
