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

    Vector2 lastPlayerInput;
    Vector3 lastVelocity;

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

        if (rb.velocity.sqrMagnitude < 0.01f || checkLateralMagnitude(rb.velocity))
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

            if (sphere.OnGround) {

                float velocityDelta = lastVelocity.magnitude - rb.velocity.magnitude;

                if (velocityDelta > .4f)
                {
                    animator.SetBool("Skrrt", true);
                } else
                {
                    animator.SetBool("Skrrt", false);
                }
            }
            /*
            Vector2 inputDiffernece = lastPlayerInput + sphere.playerInput;
            print(sphere.playerInput);
            if (sphere.OnGround && inputDiffernece.magnitude < 0.5f && sphere.playerInput != Vector2.zero && lastPlayerInput != Vector2.zero)
            { 
                animator.SetBool("Skrrt", true);  
            } else
            {
                animator.SetBool("Skrrt", false);
            }
            /*
            if (sphere.OnGround && rb.velocity.magnitude < sphere.maxSpeed-1)
            {
                animator.SetBool("Skrrt", true);
            } else
            {
                animator.SetBool("Skrrt", false);
            }
            */

        }

        if (sphere.Climbing) //we need to face towards the normal here for animations to look good
        {
            
            transform.forward = -sphere.lastContactNormal;
            speedPercent = rb.velocity.sqrMagnitude < 0.01f ? 1f : 0f;

        } else if (ggun.isGrappling)
        {
            // rotate towards grappling hook direction
            transform.localRotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
        }
        animator.SetFloat("speedPercent", speedPercent, .1f, Time.deltaTime);

        //TODO put in a ClearState 
        lastVelocity = rb.velocity;
        lastPlayerInput = sphere.playerInput;
    }

    private bool checkLateralMagnitude(Vector3 velocity)
    {
        return velocity.x < 0.01f && velocity.x > -0.01f && velocity.z < 0.01f && velocity.z > -0.01f;
    }

}
