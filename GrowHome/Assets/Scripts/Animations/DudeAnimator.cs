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
    Quaternion previousClimbRotation;

    MeshRenderer meshRenderer;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        animator = GetComponent<Animator>();
        sphere = GetComponentInParent<MovingSphere>();
        sight = cameraSight.GetComponent<BowScript>();
    }

    void LateUpdate()
    {
        transform.up = sphere.lastContactNormal; //set normal to current normal
        speedPercent = 0f;

        speedPercent = sphere.OnGround ? .25f : .75f;

        if (rb.velocity.sqrMagnitude < 0.01f)
        {
            speedPercent = 0f;
            transform.localRotation = previousLookRotation;
        }
        else
        {
            previousLookRotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
            previousLookRotation.x = 0;
            previousLookRotation.z = 0;
            transform.localRotation = previousLookRotation;
        }

        if (sphere.Climbing) //we need to face towards the normal here for animations to look good
        {
            transform.forward = -sphere.lastContactNormal;
            speedPercent = .5f;
        }
        animator.SetFloat("speedPercent", speedPercent, .1f, Time.deltaTime);
    }

}
