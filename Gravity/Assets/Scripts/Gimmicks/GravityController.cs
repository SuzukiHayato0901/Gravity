using UnityEngine;
using System.Collections;

public class GravityController : MonoBehaviour
{
    public Vector3 localGravity;

    [SerializeField] private float rbSpeed;
    private Rigidbody rb;

    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        setLocalGravity();
    }

    void setLocalGravity()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            rb.AddForce(localGravity, ForceMode.Acceleration);

            if(rb.useGravity == false)
                rb.useGravity = true;
             else
                rb.useGravity = false;
        }
    }
}
