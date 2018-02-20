using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerPlayerScript : MonoBehaviour {

    bool canJump = true;
    [SerializeField]
    float jumpHeight;
    float jumpSpeed;
    private Rigidbody rigidbody;
    RunnerController controller;
    bool isGrounded;
    Vector3 jumpDirection;

    private void Start()
    {
        controller = GameObject.Find("Controller").GetComponent<RunnerController>();
        rigidbody = GetComponent<Rigidbody>(); 
    }

    private void FixedUpdate()
    {
        if (controller.isRunning)
            if(canJump && isGrounded && Input.GetButton("Jump"))
                StartCoroutine(Jump());
    }

    float CalculateSpeed(float jumpHeight)
    {
        return Mathf.Sqrt(2 * jumpHeight * Physics.gravity.magnitude);
    }

    float CalculateTime(float jumpHeight)
    {
        float f = Mathf.Sqrt(2 * jumpHeight / 9.81f);
        return f;
    }

    //Lets the player jump, by adding a force to the rigidbody
    IEnumerator Jump()
    {
        jumpDirection = -Vector3.Normalize(Physics.gravity);
        canJump = false;
        //The time spent jumping (mulitplied by 2, since you need to get up and down
        float AirTime = CalculateTime(jumpHeight)*2;
        //The force required to reach the height
        jumpSpeed = CalculateSpeed(jumpHeight);
        //Adds the force to the rigidbody as an impulse.
        rigidbody.AddForce(jumpDirection * jumpSpeed, ForceMode.Impulse);

        //While the jump button is held down and the AirTime isn't used upthe jump is active
        while (Input.GetButton("Jump") && AirTime >= 0)
        {

            /*TODO
            * Ideally the force should be added here gradually here, so the player fully can control the jump
             * by holding down the jump trigger.
             * It works for now, since the velocity is set to 0 when the button is released.
             * This however gives the jump an abrupt endning, before falling down.
             * Eventually add some hangtime, in which the player isn't affected by forces, and just hangs in the air?
            */

            //Reduce the AirTime by the time the frame took.
            AirTime -= Time.deltaTime;
  
            yield return new WaitForEndOfFrame();
        }
        //If the rigidbody's velocity is more than 0.5, the value is slowly reversed, giving a smooth turn in the jump.
        while (rigidbody.velocity.magnitude > 0.5f)
        {
            rigidbody.velocity -= jumpDirection * Physics.gravity.magnitude * Time.deltaTime; 
            // new Vector3(0, rigidbody.velocity.y - Physics.gravity.magnitude * Time.deltaTime, 0);
            yield return new WaitForEndOfFrame();
        }
        /*Waits for the player to hit the ground again before proceeding. isGrounded is set when the player hits the ground.
          May have high risk for going infinite D: */
        while (!isGrounded)
        {
            yield return new WaitForEndOfFrame();
        }
        canJump = true;
        StopCoroutine(Jump());
        
    }


    //Checks whether the player is grounded or not
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "Ground")
        {
            canJump = true;
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "Ground")
        {
            isGrounded = false;
        }
    }
}
