using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingGun : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask grappelable;
    public Transform gunTip, camera, player;
    public float maxDistance = 50f;
    public float radius;
    private SpringJoint joint;
    public float angleBetweenAnchor;

    public bool isGrappling;
    public Vector3 grappleNormal;
    RaycastHit hit;

    [SerializeField]
    MovingSphere sphere;

    // Start is called before the first frame update
    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButtonDown(0)) {
            StartGrapple();
        }

        if (Input.GetMouseButtonUp(0)) {
            isGrappling = false;
            EndGrapple();
        }

    }

    void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {

        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, grappelable))
        {
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.enableCollision = true;
            joint.autoConfigureConnectedAnchor = false; //see what happens if this is true
            joint.connectedAnchor = grapplePoint;

            radius = Vector3.Distance(player.position, grapplePoint); 

            joint.maxDistance = radius * 0.8f; //make these editable
            joint.minDistance = radius * 0.2f;


            //TODO customize these and see what they do 
            joint.spring = 10f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
            isGrappling = true;
            grappleNormal = hit.normal;
        }

    }

    void DrawRope()
    {
        if (!joint) { return; }
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grapplePoint);
    }

    void EndGrapple()
    {
        lr.positionCount = 0;
        Destroy(joint);
    }

    public bool isUnderGrapplePoint()
    {
        float playerx = player.position.x;
        float playerz = player.position.z;
        Vector2 playerPos = new Vector2(playerx, playerz);

        float gPointx = grapplePoint.x;
        float gPointz = grapplePoint.z;
        Vector2 gPointPos = new Vector2(gPointx, gPointz);

        Vector2 diff = playerPos - gPointPos;

        if (diff.magnitude < 10f && player.position.y < grapplePoint.y)
        {
            Debug.Log(diff.magnitude);
            return true;
        }
        return false;
    }
}
    

