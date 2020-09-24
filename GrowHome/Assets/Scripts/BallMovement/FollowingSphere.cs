using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingSphere : MonoBehaviour
{
    [SerializeField]
    MovingSphere followingSphere;
    Rigidbody body;

    //Queue<Vector3> positions = new Queue<Vector3>();
    Queue<Vector3> velocities = new Queue<Vector3>();

    [SerializeField, Min(20)]
    int delayTimestep = 20;

    
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    //Using positions makes incredibly jittery movement but accurate following
    //Using velocities makes smooth movement but inconsistent following
    //To make the homing shots, use the positions for accurate placement (every 1 second, record one)
    //Paired with the velocity at that current moment
    //make a projectile 
    void FixedUpdate()
    {
        if (velocities.Count < delayTimestep - 1)
        {
            velocities.Enqueue(followingSphere.GetComponent<Rigidbody>().velocity);
        }

        if (velocities.Count > delayTimestep - 2)
        {
            body.velocity = velocities.Dequeue();
        }

        if (body.velocity.sqrMagnitude < 0.01f)
        {
            AdjustBack();
        }
        
    }
    void AdjustBack()
    {
        body.MovePosition(followingSphere.transform.position);
    }
}
