using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkFogScript : MonoBehaviour {
    //                     Far away, Okay dist, Close     , Super close!
    public enum FogZones { SafeZone, playField, DangerZone, CheckpointZone };

    FogZones FogState;

    [SerializeField]
    float speed;
    [SerializeField]
    float maxSpeed;
    [SerializeField]
    float speedGrowth;
    [SerializeField]
    float offset;
    [SerializeField]
    float dangerZone;
    [SerializeField]
    float safeZone;
    [SerializeField]
    Material overlay;
    [SerializeField]
    bool canReachPlayer;
    [SerializeField]
    bool canMove;
    Transform player;
    float dist;
    float distanceToPlayer;
    RunnerController controller;
    Vector3 target;

    void Start()
    {
        controller = GameObject.Find("Controller").GetComponent<RunnerController>();
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        target = player.position;
    }


    void Update()
    {
        if (controller.isRunning)
        {
            dist = Mathf.Clamp01((distanceToPlayer - 6) / (1 - 6));
            //The dark fog can't get closer than to the player than percentage of the distance
            safeZone = 7 - controller.GetTimePercentage() * 4;
            
            if (controller.GetTimePercentage() <= dist)
            {

                canMove = false;
            }
            else
            {

                canMove = true;
            }
            //Refactor maxSpeed
            if (canReachPlayer) maxSpeed += speedGrowth;
            else if (canMove)

                maxSpeed = controller.GetMaxSpeed() / 10;

            else maxSpeed = 0;
            Move();
            SetAlpha();
            FogLogic();
        }
    }

    void Move()
    {
        speed = Mathf.Clamp(Mathf.Lerp(speed, maxSpeed, speedGrowth), -10, 3);
        target.y = 0;
        transform.position = Vector3. MoveTowards(transform.position, target, speed * Time.deltaTime);
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, target.x - 9, target.x - 0.2f), 0, 0);
        distanceToPlayer = GetDistanceToPlayer();
        
    }

    public FogZones GetState()
    {
        return FogState;
    }

    void FogLogic()
    {
        //if the fog is closer than the dangerzone
        if (distanceToPlayer < dangerZone && distanceToPlayer > player.position.x - 1)
        {
            if (canReachPlayer)
            {   //If the fog can reach the player (Time is up), spawn a checkpoint..
                FogState = FogZones.CheckpointZone;
            }
            else
            {
                //Otherwise it shall count as a danger zone, so coins will spawn.
                FogState = FogZones.DangerZone;
            }

           
            
        }   //If the fog is within between the safezone and dangerzone, the 
        else if (distanceToPlayer < safeZone && distanceToPlayer > dangerZone)
        {
            speedGrowth = 0.05f;  
            FogState = FogZones.playField;
        }
        else if (distanceToPlayer >= safeZone)
        {

            FogState = FogZones.SafeZone;
            if (speed < 0) speed += 0.1f;
            speedGrowth = 0.5f + (distanceToPlayer / 100) ;

        }
    }

    float GetDistanceToPlayer()
    {
        float dist = player.transform.position.x - (transform.position.x + offset);
        //Debug.Log(dist);
        return dist;
    }

    //Adjusts the speed. This is used when a coin is gathered or an object is hit.
    public void AdjustSpeed(float f)
    {
        speed += f;
    }

    public void SetSpeedGrowth(float f)
    {
        speedGrowth = f;
    }

    public void SetPlayerReach(bool state) {
        canReachPlayer = state;
    }

    public void SetAlpha()
    {

        //dist = (distance - maxRange) / (minRange - maxRange) 
        float d = Mathf.Clamp01((distanceToPlayer - 6) / (2 - 6));
        float alpha =  d * 255;
        Color c = overlay.color;
        c.a = alpha / 255;
        overlay.color = c;
    }
}
