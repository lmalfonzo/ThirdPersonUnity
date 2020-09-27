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
    KeyCode fireButton;

    void Update()
    {
        if (Input.GetKey(fireButton) && currentCharge < maxCharge)
        {
            currentCharge += chargeRate * Time.deltaTime; // this should mean consistent charge rates but needs testing
            Debug.Log(currentCharge.ToString());
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward * 100f, out hit, 100f))
        {
            spawn.LookAt(hit.point);
            firingPosition = hit.point;
        }
        else
        {
            firingPosition = transform.position + transform.forward.normalized * 100f;
            spawn.LookAt(firingPosition);
        }

        if (Input.GetKeyUp(fireButton))
        {
            Rigidbody arrow = Instantiate(arrowObject, spawn.position, Quaternion.identity) as Rigidbody;

            arrow.AddForce(spawn.forward * currentCharge, ForceMode.Impulse);
            currentCharge = 0;
        }
    }
}
