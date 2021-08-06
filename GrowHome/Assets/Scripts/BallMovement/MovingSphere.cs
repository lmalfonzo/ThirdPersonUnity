using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Add Cooldown to Launcher
//TODO: Make Dash move in user input
//TODO: Highlighter for places to launch up to 
//TODO: Plane of balance/point of center (this is the launch position)
//TODO: offset the point of center relative to the player

/*
 * Movement of a Ball
 * 
 * Includes running, climbing, wall jumping, air jumping and flinging from one point to another
 * 
 */

public class MovingSphere: MonoBehaviour
{
    [SerializeField, Range (0f, 100f)]
    public float maxSpeed = 10f, maxClimbSpeed = 2f;

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10, maxAirAcceleration = 1f, maxClimbAcceleration = 20f;

    [SerializeField]
    Rect allowedArea = new Rect(-5f, -5f, 10f, 10f);

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxExtraAirJumps = 0;

    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;

    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;

    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [SerializeField]
    LayerMask probeMask = -1, stairsMask = -1, climbMask = -1;

    [SerializeField]
    Transform playerInputSpace = default;

    [SerializeField, Range(90, 180)]
    float maxClimbAngle = 140;

    [SerializeField]
    Material normalMaterial = default, climbingMaterial = default;

    [SerializeField, Min(1.1f)]
    float bunnyHopFactor;

    [SerializeField, Min(15f)]
    float dashDistance;

    [SerializeField]
    KeyCode dashButton;

    [SerializeField, Min(1)]
    int dashCooldown = 1;

    [SerializeField, Min(5f)]
    float launchOffset;

    [SerializeField]
    GrapplingGun gg;

    Vector3 velocity, connectionVelocity;

    Rigidbody body, connectedBody, previousConnectedBody;

    bool desiredJump, desiresClimbing;

    int groundContactCount, steepContactCount, climbContactCount;

    public bool OnGround => groundContactCount > 0;

    bool OnSteep => steepContactCount > 0;

    public bool Climbing => climbContactCount > 0 && stepsSinceLastJump > 2;

    public bool stoppedPlayerInput;

    bool desiresDash, inDash, desiresLaunch;

    int jumpPhase;

    float minGroundDotProduct, minStairsDotProduct, minClimbDotProduct;

    Vector3 contactNormal, steepNormal, climbNormal, lastClimbNormal;

    public Vector3 lastContactNormal;

    int stepsSinceLastGrounded, stepsSinceLastJump, stepsSinceTouchedGround, stepsSinceLastGrapple;

    float secsSinceLastDash, secsSinceLastLaunch, launchTime;

    Vector3 upAxis, rightAxis, forwardAxis;

    Vector3 connectionWorldPosition, connectionLocalPosition;

    MeshRenderer meshRenderer;

    public Vector2 playerInput;

    int bunnyHopString;

    public Vector3 launchTarget;

    

    /*
     * Set minimum angles for walking, stairs and climbing
     */
    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
    }

    /*
     * Get the components to use
     */
    void Awake()
    {
        Application.targetFrameRate = 145;
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        meshRenderer = GetComponent<MeshRenderer>();
        OnValidate();
    }

    /*
     * Player Input management
     * 
     */ 
    void Update()
    {
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        if (playerInputSpace)
        {
            rightAxis = ProjectOnDirectionPlane(playerInputSpace.right, upAxis);
            forwardAxis =
                ProjectOnDirectionPlane(playerInputSpace.forward, upAxis);
        } else
        {
            rightAxis = ProjectOnDirectionPlane(Vector3.right, upAxis);
            forwardAxis =
                ProjectOnDirectionPlane(Vector3.forward, upAxis);
        }


        desiredJump |= Input.GetButtonDown("Jump");
        desiresClimbing = Input.GetButton("Climb");
        desiresDash |= Input.GetKeyDown(dashButton);
        desiresLaunch |= Input.GetKeyDown(KeyCode.P);

        meshRenderer.material = Climbing ? climbingMaterial : normalMaterial;
    }

    /*
     * Dash in the current velocities direction (along xz)
     */
    void DashInDirection()
    {
        if (secsSinceLastDash > dashCooldown)
        {
            secsSinceLastDash = 0f;
            inDash = true;
            //disable gravity
            body.useGravity = false;

            Vector3 DashDirection = Vector3.zero;

            if (body.velocity.sqrMagnitude < 0.01f || ((body.velocity.x < 0.01f && body.velocity.x > -0.01f) && (body.velocity.z < 0.01f && body.velocity.z > -0.01f))){
                DashDirection = playerInputSpace.forward;
            } else {
                
                DashDirection.x = body.velocity.x;
                DashDirection.z = body.velocity.z;
                DashDirection.y = 0;
            }
            
            body.velocity = Vector3.zero;

            //apply the velocity
            velocity = DashDirection.normalized * dashDistance;

            Debug.Log("Dashed at " + Time.time);
        }        
    }

    /*
     * Physics Step management (jumping, dashing, climbing)
     * 
     * After checking user input, will apply correct velocites to rigidbody
     */
    void FixedUpdate()
    {
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis); // can move this if sticking to one gravity
        UpdateState();
        AdjustVelocity();

        if (desiredJump)
        {
            desiredJump = false;
            Jump(gravity);
        }
      

        if (Climbing)
        {
            velocity -= contactNormal * (maxClimbAcceleration * 0.9f * Time.deltaTime);
        }
        else if (OnGround && velocity.sqrMagnitude < 0.01f)
        {
            if (desiresDash)
            {
                desiresDash = false;
                DashInDirection();
            } else
            {
                velocity +=
                contactNormal *
                (Vector3.Dot(gravity, contactNormal) * Time.deltaTime);
                stoppedPlayerInput = true;
            } 
        }
        else if (desiresClimbing && OnGround)
        {
            velocity += (gravity - contactNormal * (maxClimbAcceleration * 0.9f)) *
                Time.deltaTime;
        } else if (desiresDash)
        {
            desiresDash = false;
            DashInDirection();
        } 
        else
        {
            velocity += gravity * Time.deltaTime;
        }

        if (desiresLaunch && secsSinceLastLaunch > launchTime) //TODO: make a cooldown and less ugly
        {
            LaunchToTarget();
            desiresLaunch = false;
        }

        if (secsSinceLastLaunch > launchTime) // we are curently in the launch, set no velocity until it finishes
        {
            if (secsSinceLastLaunch - launchTime < .1f) 
            {
                body.velocity = Vector3.zero;
            } else
            {
                body.velocity = velocity;
            } 
        }
        
        if (secsSinceLastDash > 0.2f && inDash)
        {
            body.useGravity = true;
            if (!Input.GetMouseButton(0)) {
                body.velocity /= 2;
            } 
            inDash = false;
        }

        if (gg.isGrappling)
        {
            stepsSinceLastGrapple += 1;
            if (stepsSinceLastGrapple < 2 && !gg.isUnderGrapplePoint())
            {
                maxAirAcceleration *= 4f * (gg.radius/gg.maxDistance);
                body.velocity = Vector3.zero;

            }
            Debug.Log("MS: " + gg.isUnderGrapplePoint());
            
        } else if (!gg.isGrappling && stepsSinceLastGrapple > 1)
        {
            stepsSinceLastGrapple = 0;
            maxAirAcceleration *= .25f / (gg.radius / gg.maxDistance);

        }

        ClearState();
    }

    /*
     * Launches the player from their current position to the specified launch position
     */
    void LaunchToTarget()
    {
        Vector3? launchVelocity = CalculateLaunchVelocity();
        //Vector3 launchVelocity = CalculateLaunchVelocity();
        Debug.Log("Velocity = " + launchVelocity);
        if (!float.IsNaN(launchVelocity.Value.x))
        {
            body.velocity = Vector3.zero;
            body.useGravity = false;
            body.velocity = (Vector3) launchVelocity;
            body.useGravity = true;
        } else
        {
            launchTime = 0f;
        }
    }

    /*
     *  Calculates the launch velocity used in LaunchToTarget()
     */
    Vector3 CalculateLaunchVelocity()
    {
        secsSinceLastLaunch = 0;

        float displacementY = launchTarget.y - transform.position.y;
        float h = displacementY + launchOffset;
        Vector3 displacementXZ = new Vector3(launchTarget.x - transform.position.x, 0, launchTarget.z - transform.position.z);
        launchTime = (Mathf.Sqrt(-2 * h / Physics.gravity.y) + Mathf.Sqrt(2 * (displacementY - h) / Physics.gravity.y));
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * Physics.gravity.y * h);
        Vector3 velocityXZ = displacementXZ / launchTime;

        return velocityXZ + velocityY;
    }

    /*
     * Clears everything for the next physics step to utilize
     */
    void ClearState()
    {
        lastContactNormal = contactNormal;
        groundContactCount = steepContactCount = climbContactCount = 0 ;
        contactNormal = steepNormal = climbNormal = connectionVelocity =  Vector3.zero;
        previousConnectedBody = connectedBody;
        connectedBody = null;
        stoppedPlayerInput = false;
    }

    /*
     * Jump aligned to the wall/ground
     */
    void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;
        if (OnGround)
        {
            jumpDirection = contactNormal;
        } else if (OnSteep)
        {
            jumpDirection = steepNormal;
            //jumpPhase = 0; //Re-Enable if you want extra jumps to reset on wall jumps
        } else if (maxExtraAirJumps > 0 && jumpPhase <= maxExtraAirJumps)
        {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
            jumpPhase++;
        } else
        {
            return;
        }

        stepsSinceLastJump = 0;
        
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
        jumpDirection = (jumpDirection + upAxis).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        if (stepsSinceTouchedGround < 31  && bunnyHopString <= 10 && !stoppedPlayerInput)
        {
            maxSpeed += bunnyHopFactor;
            maxAirAcceleration += bunnyHopFactor;
            bunnyHopString++;

        }

        velocity += jumpDirection * jumpSpeed;

    }

    /*
     * Tracks condititions and cooldowns of time since grounded, last jumped, last dash, last launch
     */
    void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        secsSinceLastDash += Time.deltaTime;
        secsSinceLastLaunch += Time.deltaTime;
        velocity = body.velocity;
        if (CheckClimbing() || OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceTouchedGround += 1;
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJump > 1)
            {
                jumpPhase = 0;
            }
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
            if (stepsSinceTouchedGround > 30)
            {
                maxSpeed -= bunnyHopFactor * bunnyHopString;
                maxAirAcceleration -= bunnyHopFactor * bunnyHopString;
                bunnyHopString = 0;
            }
        } else
        {
            stepsSinceTouchedGround = 0;
            contactNormal = upAxis;
        }

        if (connectedBody)
        {
            if (connectedBody.isKinematic || connectedBody.mass >= body.mass)
            {
                UpdateConnectionState();
            }
        }

    }

    /*
     * Connects the player to a moving platform
     */
    void UpdateConnectionState()
    {
        if (connectedBody == previousConnectedBody)
        {
            Vector3 connectionMovement =
                connectedBody.transform.TransformPoint(connectionLocalPosition) - 
                connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;
        }
        connectionWorldPosition = body.position;
        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(
            connectionWorldPosition);
    }

    /*
     * Checks if the player is wedged in a crevase
     */ 
    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    /*
     * Checks the minimum angle allowed for running, climbing, stairs and steeps
     */
    void EvaluateCollision(Collision collision) {
        int layer = collision.gameObject.layer;
        float minDot = GetMinDot(layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            if (upDot >= minDot)
            {
                groundContactCount += 1;
                contactNormal += normal;
                connectedBody = collision.rigidbody;
            }
            else
            {
                if (upDot > -0.01f)
                {
                    steepContactCount++;
                    steepNormal += normal;
                    if (groundContactCount == 0)
                    {
                        connectedBody = collision.rigidbody;
                    }
                }
                if (desiresClimbing && upDot >= minClimbDotProduct &&
                    (climbMask & (1 << layer)) != 0)
                {
                    climbContactCount += 1;
                    climbNormal += normal;
                    lastClimbNormal = normal;
                    connectedBody = collision.rigidbody;
                }
            }

            
        }
    }

    //Vector3 ProjectOnContactPlane(Vector3 vector)
    //{
    //    return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    //}

    Vector3 ProjectOnDirectionPlane (Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    /*
     * Adjusts the velocity to a desired velocity relative to camera //Not sure if right
     */
    void AdjustVelocity()
    {
        float acceleration, speed;
        Vector3 xAxis, zAxis;
        if (Climbing)
        {
            acceleration = maxClimbAcceleration;
            speed = maxClimbSpeed;
            xAxis = Vector3.Cross(contactNormal, upAxis);
            zAxis = upAxis;
        } else
        {
            acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            speed = OnGround && desiresClimbing ? maxClimbSpeed : maxSpeed;
            speed = maxSpeed;
            xAxis = rightAxis;
            zAxis = forwardAxis;
        }

        xAxis = ProjectOnDirectionPlane(xAxis, contactNormal);
        zAxis = ProjectOnDirectionPlane(zAxis, contactNormal);

        Vector3 relativeVelocity = velocity - connectionVelocity;
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);

        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX =
            Mathf.MoveTowards(currentX, playerInput.x * speed, maxSpeedChange);
        float newZ =
            Mathf.MoveTowards(currentZ, playerInput.y * speed, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    /*
     * With a set minimum angle, snap to the ground when approaching a decline and walk along it instead
     * of flying off
     */
    bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }

        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }

        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;
        
        float dot = Vector3.Dot(velocity, hit.normal);
        velocity = (velocity - hit.normal * dot).normalized * speed;
        connectedBody = hit.rigidbody;
        return true;
    }

    float GetMinDot (int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
    }

    /*
     * When climbing, check if the angle you are climbing exceeds the allowed angle, return false if >=
     */
    bool CheckClimbing ()
    {
        if (Climbing)
        {
            if (climbContactCount > 1)
            {
                climbNormal.Normalize();
                float upDot = Vector3.Dot(upAxis, climbNormal);
                if (upDot >= minGroundDotProduct)
                {
                    climbNormal = lastClimbNormal;
                }
            }
            groundContactCount = climbContactCount;
            contactNormal = climbNormal;
            return true;
        }
        return false;
    }
}
