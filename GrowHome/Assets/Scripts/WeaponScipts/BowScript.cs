using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowScript : MonoBehaviour
{
    float currentCharge;

    [SerializeField, Min(50f)] // to change
    float maxCharge;

    [SerializeField, Min(10f)] // to change
    float chargeRate;

    //TODO: make serialized
    [SerializeField]
    public Transform spawn;

    [SerializeField]
    Transform player;

    [SerializeField]
    Rigidbody arrowObject;

    public Vector3 firingPosition;

    [SerializeField]
    LayerMask BranchMask;

    [SerializeField]
    KeyCode fireButton;

    MovingSphere sphere;

    void Start()
    {
        sphere = GetComponentInParent<MovingSphere>();
    }

    void Update()
    {
        if (Input.GetKey(fireButton) && currentCharge < maxCharge)
        {
            currentCharge += chargeRate * Time.deltaTime; // this should mean consistent charge rates but needs testing
            Debug.Log(currentCharge.ToString());
        }

        //TODO move this too a different script
        //TODO make this easier to read
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 6f, transform.forward, out hit, 100f, BranchMask))
        {
            Debug.DrawLine(transform.position, hit.point, Color.green);
            if (hit.transform.name.Contains("Branch"))
            {
                MaterialChanger mat = hit.transform.GetComponent<MaterialChanger>();
                mat.SwitchMaterial(1); //TODO make enums
                sphere.launchTarget = hit.transform.GetComponentInChildren<Transform>();
            }
        }
        else
        {
            firingPosition = transform.position + transform.forward.normalized * 100f;
            spawn.LookAt(firingPosition);
        }

        if (Input.GetKeyUp(fireButton))
        {
            if (Physics.Raycast(transform.position, transform.forward * 100f, out hit, 100f))
            {
                spawn.LookAt(hit.point);
                firingPosition = hit.point;
            }
            Rigidbody arrow = Instantiate(arrowObject, spawn.position, Quaternion.identity) as Rigidbody;

            arrow.AddForce(spawn.forward * currentCharge, ForceMode.Impulse);
            currentCharge = 0;
        }


    }
}
