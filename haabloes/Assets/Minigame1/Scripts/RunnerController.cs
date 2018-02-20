using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerController : MonoBehaviour {


    public enum SpawnSequence { Coin, Obstacle, Random, Checkpoint, None };

    SpawnSequence SpawnState = SpawnSequence.Coin;
    public float VOTime;
    public float curTime;
    bool canSpawn;
    float modifier = 0;
    public bool isRunning;
    bool canSpawnCoin = true;
    float maxSpeed;
    private float startSpeed;
    [SerializeField]
    float speed;
    [SerializeField]
    float spawnTime;
    [SerializeField]
    GameObject coin;
    [SerializeField]
    GameObject[] obstacle;
    [SerializeField]
    GameObject clutter;
    [SerializeField]
    AudioClip[] VOs;
    int VOCounter = 0;
    AudioSource audioSource;
    DarkFogScript darkFogScript;
    bool timeIsUp;
    int score = 0;


    private void Start()
    {
        isRunning = true;
        canSpawn = true;
        startSpeed = speed;
        speed = 0;
        maxSpeed = startSpeed;
        
        audioSource = GetComponent<AudioSource>();
        darkFogScript = GameObject.Find("Dark Fog").GetComponent<DarkFogScript>();
        PlayVO();
        curTime = 0;
        StartCoroutine(Spawner());
        StartCoroutine(Timer());
    }

    private void PlayVO()
    {
        VOTime = VOs[VOCounter].length;
        audioSource.clip = VOs[VOCounter];
        audioSource.Play();
        curTime = -1; 
    }

    IEnumerator Timer()
    {
        while (isRunning)
        {

            curTime += Time.deltaTime;
            if (curTime >= VOTime)
            {
                timeIsUp = true;
                darkFogScript.SetPlayerReach(true);
            }
            else timeIsUp = false;
            yield return new WaitForEndOfFrame();
        }
    }

    void Update()
    {
        if(speed < maxSpeed)
        {
            speed = Mathf.Lerp(speed, maxSpeed, 0.1f);
        }
    }

    IEnumerator Spawner()
    {
        while (isRunning)
        {
            if (canSpawn)
            {
                SpawnLogic();
                yield return new WaitForSeconds(spawnTime);
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public void AdjustSpawnTimer(float f)
    {
        spawnTime += f;
    }

    //Sets the SpawnState based on which zone the dark fog is in.
    SpawnSequence SetSpawnState()
    {
        if (timeIsUp && darkFogScript.GetState() != DarkFogScript.FogZones.CheckpointZone)
        {
            spawnTime = 0.5f;
            canSpawnCoin = false;
            darkFogScript.SetSpeedGrowth(0.5f);
            return SpawnSequence.None;
            
        }
        switch (darkFogScript.GetState())
        {
            case DarkFogScript.FogZones.CheckpointZone:
                if (timeIsUp)
                {
                    return SpawnSequence.Checkpoint;
                }
                else
                {
                    return SpawnSequence.None;
                }
            case DarkFogScript.FogZones.DangerZone:
                return SpawnSequence.Coin;
                
            case DarkFogScript.FogZones.playField:
                int r = 0;
                r = Random.Range(0, 2);
                return GetRandomState(r);
                
            case DarkFogScript.FogZones.SafeZone:
                return SpawnSequence.Obstacle;
        }
        return SpawnState;
    }

    //Gets a random SpawnState
    private SpawnSequence GetRandomState(int r) { 
        
            switch (r)
            {
                case 0:
                    return SpawnSequence.Coin;
  
                case 1:
                    return SpawnSequence.Obstacle;

                default:
                    return SpawnSequence.Coin;
            }
    }

    //The logic, which decides which object to spawn.
    public void SpawnLogic()
    {
        //A switch deciding which does an action, based on the SpawnState
        switch (SetSpawnState())
        {
            case SpawnSequence.Coin:
                SpawnObject(coin, Random.Range(1, 3), Random.Range(0, 4) + 0.5f);
                break;

            case SpawnSequence.Obstacle:
                SpawnObject(obstacle[Random.Range(0,obstacle.Length)], 1);
                if (Random.Range(0, 2) == 1 && canSpawnCoin)
                    SpawnObject(coin, 1, 2.5f);
                break;

            case SpawnSequence.Checkpoint:
            if (timeIsUp)
            {
                SpawnObject(clutter,1,0);
                canSpawn = false;
            }
            else
            {
                SpawnObject(coin, 3);
            }
                break;

            default:
                break;
        }
        
    }

    private void SpawnObject(GameObject obj, int quantity, float height)
    {
        for (int i = 0; i < quantity; i++)
        {
            //the position should be added with the witdth of the object
            Vector3 spawnPos = transform.position;
            spawnPos.x = transform.position.x + (i);
            spawnPos.y = height;
            Instantiate(obj, spawnPos, obj.transform.rotation);
        }
    }

    private void SpawnObject(GameObject obj, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            //the position should be added with the witdth of the object
            Vector3 spawnPos = transform.position;
            spawnPos.x = transform.position.x + (i);
            Instantiate(obj, spawnPos, obj.transform.rotation);
        }
    }

    public void CheckPoint(float time)
    {

        StartCoroutine(Pause(time));
    }

    IEnumerator Pause(float time)
    {
        canSpawnCoin = true;
        curTime = 0 - time;
        SpawnState = SpawnSequence.Obstacle;
        darkFogScript.SetPlayerReach(false);
        darkFogScript.SetSpeedGrowth(0);
        modifier = 0;
        startSpeed += 0.5f;
        SpawnObject(coin, 10, 1.5f);
        //Start new Voiceover here
        //VOTime = Voiceover.length
        while (time > 0)
        {
            speed = Mathf.Lerp(speed, startSpeed, 0.1f);
          //  darkFogScript.AdjustSpeed(-0.1f);
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;
        }
        if(VOCounter < VOs.Length-1)
        {
            VOCounter++;
            PlayVO();
        }
        else
        {
            Debug.LogWarning("No more sounds files! DO SOMETHING!");
        }
        canSpawn = true;
        darkFogScript.SetSpeedGrowth(0.05f);
        spawnTime = 1f;
        StopCoroutine(Pause(time));
    }

    public void Delay(float value)
    {
        speed -= value;
        if (speed < 0) speed = 0;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }

    public float GetSpawnTime()
    {
        return spawnTime;
    }

    public SpawnSequence GetSpawnState()
    {
        return SpawnState;
    }

    public void SetScore(int i)
    {
        score += i;
        modifier += 0.1f;

        if (maxSpeed < 2.5 + startSpeed) maxSpeed = startSpeed + modifier;
        if (spawnTime > 1f) spawnTime -= 0.01f;
    }

    public float GetTimePercentage()
    {
        return curTime / VOTime;
    }
}
