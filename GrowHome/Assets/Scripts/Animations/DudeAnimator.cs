using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DudeAnimator : MonoBehaviour
{
    Animator animator;
    Rigidbody rb;
    MovingSphere sphere;
    BowScript sight;
    float speedPercent;

    void Start()
    {
        rb = GetComponent<Rigidbody>();   
        animator = GetComponentInChildren<Animator>();
        sphere = GetComponent<MovingSphere>();
        sight = GetComponentInChildren<BowScript>();
    }

    // Update is called once per frame
    void Update()
    {
        speedPercent = 0f;
        if (sphere.OnGround)
        {
            speedPercent = .25f;
            if (rb.velocity.sqrMagnitude < 0.01f)
            {
                speedPercent = 0f;
            }
            
        } else if (sphere.Climbing)
        {
            speedPercent = .5f;
        } else
        {
            speedPercent = .75f;
        }
        animator.SetFloat("speedPercent", speedPercent, .1f, Time.deltaTime);
        //sight.firingPosition;
    }
}
