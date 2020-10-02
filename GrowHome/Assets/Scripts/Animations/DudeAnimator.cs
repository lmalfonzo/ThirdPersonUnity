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

    GrapplingGun ggun;


    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        animator = GetComponent<Animator>();
        sphere = GetComponentInParent<MovingSphere>();
        sight = cameraSight.GetComponent<BowScript>();
        ggun = GetComponentInParent<GrapplingGun>();
    }

    void UpdateState()
    {
        animator.SetBool("OnGround", sphere.OnGround);
        animator.SetBool("Climbing", sphere.Climbing);
        animator.SetBool("Grappling", ggun.isGrappling);
    }

    void LateUpdate()
    {
        UpdateState();

        transform.up = sphere.lastContactNormal; //set normal to current normal

        if (rb.velocity.sqrMagnitude < 0.01f)
        {
            speedPercent = 0f;
            transform.localRotation = previousLookRotation;
        }
        else
        {
            speedPercent = 1f;
            previousLookRotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
            previousLookRotation.x = 0;
            previousLookRotation.z = 0;
            transform.localRotation = previousLookRotation;
        }

        if (sphere.Climbing) //we need to face towards the normal here for animations to look good
        {
            speedPercent = 0f;
            transform.forward = -sphere.lastContactNormal;
            if (rb.velocity.sqrMagnitude < 0.01f)
            {
                speedPercent = 1f;
            }
        }
        animator.SetFloat("speedPercent", speedPercent, .1f, Time.deltaTime);
    }

}
