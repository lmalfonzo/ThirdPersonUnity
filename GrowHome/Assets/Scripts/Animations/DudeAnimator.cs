using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DudeAnimator : MonoBehaviour
{
    Animator animator;
    Rigidbody rb;
    MovingSphere sphere;
    [SerializeField]
    OrbitCamera cameraSight;


    float speedPercent;
    BowScript sight;

    Vector3 movement;
    float distance;

    Quaternion previousLookRotation;

    MeshRenderer meshRenderer;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        animator = GetComponent<Animator>();
        sphere = GetComponentInParent<MovingSphere>();
        sight = cameraSight.GetComponent<BowScript>();
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

        }
        else if (sphere.Climbing)
        {
            speedPercent = .5f;
        }
        else
        {
            speedPercent = .75f;
        }
        animator.SetFloat("speedPercent", speedPercent, .1f, Time.deltaTime);
        if (rb.velocity.sqrMagnitude < 0.001f)
        {
            transform.localRotation = previousLookRotation;
        } else
        {
            previousLookRotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
            transform.localRotation = previousLookRotation;
        }
        //transform.LookAt(sight.firingPosition);

    }

}
