﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    Animator anim;
    public GameObject player;
    public RuntimeAnimatorController walkAnim;
    public RuntimeAnimatorController runAnim;
    public RuntimeAnimatorController idleAnim;
    NavMeshAgent agent;
    private Sounds sound;
    public float runSpeed = 7f, normalSpeed = 1.5f;
    public float detectionRange = 2f;
    private Vector3 deltaPosition, prevPosition;
    private bool scream = false, charge = false, stop = false, isStay = false;
    public bool isPlayerDetect = false;
    public List<Transform> Spots;
    private Transform newSpot;
    private Transform spawnSpot;
    private float timeWaitAndObserve = 3f;
    private int startSpotsIndex = 2;

    public VHS.FirstPersonController playerMovement;
    public bool isPlayerOpenDoor = false;
    public GameObject actualDoor;
    [SerializeField] GameObject chest;
    [SerializeField] TypingInput chestAlarm;
    [SerializeField] GameObject computer;
    [SerializeField] Tablet computerAlarm;
    [SerializeField] GameObject terminal;
    [SerializeField] Lockpicking terminalAlarm;
    [SerializeField] SkinnedMeshRenderer monsterMesh;
    [SerializeField] Renderer monsterRenderer;
    [SerializeField] List<Material> transparentShader;
    [SerializeField] List<Material> visibleShader;
    Material[] visibleMaterials;
    Material[] transparetMaterials;


    void Start()
    {
        visibleMaterials = visibleShader.ToArray();
        transparetMaterials = transparentShader.ToArray();
        prevPosition = transform.position;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        sound = GetComponent<Sounds>();
        spawnMonster();
        setNewPointDestinationToMoster();

    }

    private void spawnMonster()
    {
        spawnSpot = Spots[6];
        transform.position = spawnSpot.position;
    }

    void Update()
    {
        checkIsDoorBlocked();
        checkPlayerDetection();
        rotateMonster();
        changeMonsterShader();
        if(!chestAlarm.isAlarm && !computerAlarm.isAlarm && !terminalAlarm.isAlarm)
        {
            if (makeNewTarget())
            {
                setNewPointDestinationToMoster();
            }
        }
        moveMonster();

    }

    private void changeMonsterShader()
    {
        if(Vector3.Distance(transform.position, player.transform.position) < 2f)
        {
            monsterMesh.materials = visibleMaterials;
        }
        else
        {
            monsterMesh.materials = transparetMaterials;
        }
    }

    private void moveMonster()
    {
        if (isPlayerOpenDoor)
        {
            prepareMonsterRunToDoor(actualDoor);
        }
        else if (chestAlarm.isAlarm)
        {
            prepareMonsterCheckAlarm(chest);
            turnOffAlarm();
        }
        else if (computerAlarm.isAlarm)
        {
            prepareMonsterCheckAlarm(computer);
            turnOffAlarm();
        }
        else if (terminalAlarm.isAlarm)
        {
            prepareMonsterCheckAlarm(terminal);
            turnOffAlarm();
        }
        else if (Vector3.Distance(transform.position, player.transform.position) < detectionRange && Vector3.Distance(transform.position, player.transform.position) > 1.5f)
        {
            prepareMonsterToRun();
        }
        else
        {
            if (isStay)
            {
                isPlayerDetect = false;
                prepareMonsterToStay();
            }
            else
            {
                prepareMonsterToWalk();
            }
        }
    }

    void turnOffAlarm()
    {
        if (Vector3.Distance(transform.position, chest.transform.position) < 1.5f)
        {
            chestAlarm.isAlarm = false;
            chestAlarm.alarm.Stop();
            StartCoroutine("stayAndObserve");
        }
        if (Vector3.Distance(transform.position, computer.transform.position) < 1.5f)
        {
            computerAlarm.isAlarm = false;
            computerAlarm.alarm.Stop();
            StartCoroutine("stayAndObserve");
        }
        if (Vector3.Distance(transform.position, terminal.transform.position) < 2f)
        {
            terminalAlarm.isAlarm = false;
            terminalAlarm.alarm.Stop();
            StartCoroutine("stayAndObserve");
        }
    }

    public void prepareMonsterCheckAlarm(GameObject alarmSource)
    {
        if (!charge)
        {
            StartCoroutine("Prepare");
        }
        else if (!scream)
        {
            sound.Sound1();
            scream = true;
        }
        else
        {
            startMonsterRunToAlarm(alarmSource);
        }
    }

    private void startMonsterRunToAlarm(GameObject alarmSource)
    {
        agent.speed = runSpeed;
        anim.runtimeAnimatorController = runAnim;
        float step = normalSpeed * 100 * Time.deltaTime; // calculate distance to move
        agent.SetDestination(alarmSource.transform.position);
        rotateMonster();

    }


    private void checkIsDoorBlocked()
    {
        if (GameData.door1)
            startSpotsIndex = 0;
        else
            startSpotsIndex = 2;

    }

    private void checkPlayerDetection()
    {
        if (!isPlayerDetect)
            checkPlayerMovementMode();
    }

    private void checkPlayerMovementMode()
    {
        if (playerMovement.movementInputData.HasInput && playerMovement.movementInputData.IsRunning && !playerMovement.movementInputData.IsCrouching)
        {
            detectionRange = 12f;
        }
        else if ((playerMovement.movementInputData.HasInput && playerMovement.movementInputData.IsCrouching) || (!playerMovement.movementInputData.HasInput && !playerMovement.isHoldingBreath))
        {
            detectionRange = 4.5f;
        }
        else if (!playerMovement.movementInputData.HasInput && playerMovement.isHoldingBreath)
        {
            detectionRange = 0f;
        }
        else
        {
            detectionRange = 6.7f;
        }
    }

    private void prepareMonsterToWalk()
    {
        agent.speed = normalSpeed;
        anim.runtimeAnimatorController = walkAnim;
        scream = false;
        charge = false;
        isStay = false;
    }

    private void prepareMonsterToStay()
    {
        agent.speed = 0f;
        anim.runtimeAnimatorController = idleAnim;
        charge = false;
        isStay = true;
    }

    private void prepareMonsterToRun()
    {
        if (!charge)
        {
            StartCoroutine("Prepare");
        }
        else if (!scream)
        {
            sound.Sound1();
            scream = true;
        }
        else
        {
            startMonsterRun();
        }
    }

    public void prepareMonsterRunToDoor(GameObject targetObject)
    {
        if (!charge)
        {
            StartCoroutine("Prepare");
        }
        else if (!scream)
        {
            sound.Sound1();
            scream = true;
        }
        else
        {
            startMonsterRunToDoor(targetObject);
        }
    }

    private void startMonsterRunToDoor(GameObject targetObject)
    {
        isPlayerDetect = false;
        agent.speed = runSpeed;
        anim.runtimeAnimatorController = runAnim;
        agent.SetDestination(targetObject.transform.position);
        rotateMonster();
    }

    private void startMonsterRun()
    {
        isPlayerDetect = true;
        agent.speed = runSpeed;
        anim.runtimeAnimatorController = runAnim;
        agent.SetDestination(player.transform.position);
        rotateMonster();
    }

    private void setNewPointDestinationToMoster()
    {
        isPlayerDetect = false;
        newSpot = Spots[UnityEngine.Random.Range(startSpotsIndex, Spots.Count)];
        agent.SetDestination(newSpot.transform.position);
    }

    private bool makeNewTarget()
    {
        float radiusAroundTargetPoint = 1.5f;
        if(agent.remainingDistance <= (agent.stoppingDistance + radiusAroundTargetPoint))
        {
            if (agent.speed != runSpeed && !chestAlarm.isAlarm && !computerAlarm.isAlarm && !terminalAlarm.isAlarm)
            {
                StartCoroutine("stayAndObserve");
            }
            isPlayerOpenDoor = false;
            return true;
        }
        if(agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            return false;
        }
        else
        {
            return false;
        }
    }

    private void rotateMonster()
    {
        deltaPosition = transform.position - prevPosition;
        if (deltaPosition != Vector3.zero && !stop)
        {
            transform.forward = deltaPosition;
        }
        prevPosition = transform.position;
    }
    IEnumerator Prepare()
    {
        agent.speed = 0f;
        anim.runtimeAnimatorController = idleAnim;
        stop = true;
        yield return new WaitForSeconds(.4f);
        stop = false;
        charge = true;
    }

    IEnumerator stayAndObserve()
    {
        agent.isStopped = true;
        prepareMonsterToStay();
        yield return new WaitForSeconds(1f);
        transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(0, 180, 0), timeWaitAndObserve);
        yield return new WaitForSeconds(timeWaitAndObserve);
        prepareMonsterToWalk();
        agent.isStopped = false;
    }

}

