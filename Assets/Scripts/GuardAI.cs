///
/// ShadeRunner GuardAI v1.3
/// 

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class GuardAI : MonoBehaviour
{

    /// <summary>
    /// Transforms in order of priority to target if visible
    /// </summary>
    [SerializeField]
    public List<Transform> prioritizedTransforms;

    /// <summary>
    /// Set this to a layer that will hide the player, e.g. Walls
    /// </summary>
    [SerializeField]
    LayerMask visibilityLayerMask;

    /// <summary>
    /// Set this to a series of GameObject nodes for the AI to patrol along
    /// </summary>
    [SerializeField]
    public List<GameObject> patrolPath;

    [SerializeField]
    float minVisibilityDistance = 3f;
    [SerializeField]
    float maxVisiblityDistance = 10f;
    [SerializeField]
    float fieldOfView = 120f;



    [SerializeField]
    public float sprintSpeed = 1f;
    [SerializeField]
    public float jogSpeed = 0.6f;
    [SerializeField]
    public float walkSpeed = 0.2f;

    [SerializeField]
    public float reactionTimeMin = 0.2f;
    [SerializeField]
    public float reactionTimeMax = 0.3f;

    [SerializeField]
    public float planningTimeMin = 0.4f;
    [SerializeField]
    public float planningTimeMax = 1.0f;

    [SerializeField]
    public float confusionTimeMin = 2f;
    [SerializeField]
    public float confusionTimeMax = 5f;
    [SerializeField]
    public float maxConfusedTurnAngle = 60f;

    [SerializeField]
    public float attackTime = 0.5f;

    [SerializeField]
    public float killedPlayerStopTime = 10f;

    [SerializeField]
    public bool debugLogStates = false;

    public AICharacterControl aiCharacterControl;

    private FSMState currentState;

    public GameObject nextPatrolTarget;

    private GameObject lastVisibleTargetObject;

    public Dictionary<State, FSMState> States;
    // Use this for initialization
    void Start()
    {
        aiCharacterControl = GetComponent<AICharacterControl>();

        nextPatrolTarget = new GameObject("InitialGuardPosition");
        if (patrolPath.Count > 0)
        {
            nextPatrolTarget = patrolPath[0];

            patrolPath.Add(patrolPath[0]);
            patrolPath.RemoveAt(0);
        }
        else
        {
            // Return to origin position
            nextPatrolTarget.transform.position = transform.position;
            nextPatrolTarget.transform.rotation = transform.rotation;
        }

        lastVisibleTargetObject = new GameObject("LastVisiblePosition");

        States = new Dictionary<State, FSMState>()
        {
            { State.Idle, new IdleState() },
            { State.Patrol, new PatrolState() },
            { State.Chase, new ChaseState() },
            { State.Confused, new ConfusedState() },
            { State.JogToLastPosition, new JogToLastPositionState() },
            { State.Attacking, new AttackingState() },
            { State.KilledPlayer, new KilledPlayerState() }
        };

        currentState = States[State.Idle];
        currentState.OnEnter(this);
    }

    public Transform lastVisibleTarget;
    public Transform currentVisibleTarget;

    // Update is called once per frame
    void Update()
    {
        currentVisibleTarget = null;
        foreach (Transform t in prioritizedTransforms)
        {
            if (CanSeePosition(t.position))
            {
                currentVisibleTarget = t;
                lastVisibleTargetObject.transform.position = t.position;
                lastVisibleTargetObject.transform.rotation = t.rotation;
                lastVisibleTargetObject.transform.localScale = t.localScale;
                lastVisibleTarget = lastVisibleTargetObject.transform;
                break;
            }
        }

        currentState.OnUpdate(this);
        if (currentState.nextState != null)
        {
            currentState.OnExit(this);
            currentState = currentState.nextState;
            currentState.OnEnter(this);
        }
    }

    public bool CanSeePosition(Vector3 targetPosition)
    {
        Vector3 positionDirection = targetPosition - transform.position;

        float distanceToPlayer = positionDirection.magnitude;

        positionDirection = positionDirection.normalized;

        bool positionIsVisible = false;

        if (distanceToPlayer <= maxVisiblityDistance)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, positionDirection, out hitInfo, distanceToPlayer, visibilityLayerMask.value) == false)
            {
                if (distanceToPlayer < minVisibilityDistance || Vector3.Angle(transform.forward, positionDirection) <= (fieldOfView / 2f))
                {
                    positionIsVisible = true;
                }
            }
        }

        return positionIsVisible;
    }
}

public enum State
{
    Idle,
    Patrol,
    Chase,
    Confused,
    JogToLastPosition,
    Attacking,
    KilledPlayer
}

public abstract class FSMState
{
    protected string _name = "Unnamed FSMState";
    public string name { get { return _name; } }
    public void OnEnter(GuardAI ai)
    {
        if (ai.debugLogStates == true)
        {
            Debug.Log(name + ":OnEnter");
        }
        _onEnter(ai);
    }
    protected abstract void _onEnter(GuardAI ai);
    public void OnUpdate(GuardAI ai)
    {
        if (ai.debugLogStates == true)
        {
            Debug.Log(name + ":OnUpdate");
        }
        _nextState = null;
        _onUpdate(ai);
    }
    protected abstract void _onUpdate(GuardAI ai);
    public void OnExit(GuardAI ai)
    {
        if (ai.debugLogStates == true)
        {
            Debug.Log(name + ":OnExit");
        }
        _onExit(ai);
    }
    protected abstract void _onExit(GuardAI ai);
    protected FSMState _nextState;
    public FSMState nextState { get { return _nextState; } }
}

public class IdleState : FSMState
{
    public IdleState()
    {
        _name = "IdleState";
    }
    protected override void _onEnter(GuardAI ai)
    {
    }

    protected override void _onExit(GuardAI ai)
    {
    }

    protected override void _onUpdate(GuardAI ai)
    {
        if (ai.currentVisibleTarget != null)
        {
            _nextState = new FSMTransition(TransitionType.Reaction, ai.States[State.Chase]);
        }
        else if (ai.patrolPath.Count > 0)
        {
            _nextState = new FSMTransition(TransitionType.Planning, ai.States[State.Patrol]);
        }
    }
}

public class PatrolState : FSMState
{
    public PatrolState()
    {
        _name = "PatrolState";
    }

    protected override void _onEnter(GuardAI ai)
    {
        ai.GetComponent<NavMeshAgent>().speed = ai.walkSpeed;
    }

    protected override void _onExit(GuardAI ai)
    {
        ai.GetComponent<NavMeshAgent>().speed = ai.walkSpeed;
    }

    protected override void _onUpdate(GuardAI ai)
    {
        if (ai.currentVisibleTarget != null)
        {
            _nextState = new FSMTransition(TransitionType.Reaction, ai.States[State.Chase]);
        }
        else
        {
            if (Vector3.Distance(ai.transform.position, ai.nextPatrolTarget.transform.position) < 0.75f)
            {
                ai.nextPatrolTarget = ai.patrolPath[0];
                ai.patrolPath.Add(ai.patrolPath[0]);
                ai.patrolPath.RemoveAt(0);
                _nextState = new FSMTransition(TransitionType.Planning, ai.States[State.Patrol]);
            }
            ai.aiCharacterControl.SetTarget(ai.nextPatrolTarget.transform);
        }
    }
}

public class ChaseState : FSMState
{
    public ChaseState()
    {
        _name = "ChaseState";
    }
    protected override void _onEnter(GuardAI ai)
    {
        ai.GetComponent<NavMeshAgent>().speed = ai.sprintSpeed;
    }

    protected override void _onExit(GuardAI ai)
    {
        ai.GetComponent<NavMeshAgent>().speed = ai.walkSpeed;
    }

    protected override void _onUpdate(GuardAI ai)
    {
        if (ai.currentVisibleTarget != null)
        {
            ai.aiCharacterControl.SetTarget(ai.currentVisibleTarget);
            if (Vector3.Distance(ai.transform.position, ai.currentVisibleTarget.position) < 0.75f)
            {
                _nextState = new FSMTransition(TransitionType.Reaction, ai.States[State.Attacking]);
            }
        }
        else
        {
            ai.aiCharacterControl.SetTarget(ai.lastVisibleTarget);
            // Reached last visible target
            if (Vector3.Distance(ai.transform.position, ai.lastVisibleTarget.position) < 0.75f)
            {
                ai.lastVisibleTarget = null;
                _nextState = new FSMTransition(TransitionType.Planning, ai.States[State.Confused]);
            }
        }
    }
}

public class ConfusedState : FSMState
{
    public ConfusedState()
    {
        _name = "ConfusedState";
    }
    private float exitConfusedStateTime;
    private float nextConfusedTurnTime;
    private Quaternion nextAngle;
    private float nextAngleY;

    protected override void _onEnter(GuardAI ai)
    {
        exitConfusedStateTime = Time.time + UnityEngine.Random.Range(ai.confusionTimeMin, ai.confusionTimeMax);
        nextConfusedTurnTime = Time.time + UnityEngine.Random.Range(0f, ai.reactionTimeMin);
        nextAngleY = UnityEngine.Random.Range(-ai.maxConfusedTurnAngle, ai.maxConfusedTurnAngle);
        Vector3 eulerAngles = ai.transform.rotation.eulerAngles;
        nextAngle = Quaternion.Euler(
            eulerAngles.x,
            eulerAngles.y + nextAngleY,
            eulerAngles.z
        );
    }

    protected override void _onExit(GuardAI ai)
    {
    }

    protected override void _onUpdate(GuardAI ai)
    {
        if (ai.currentVisibleTarget != null)
        {
            _nextState = new FSMTransition(TransitionType.Reaction, ai.States[State.Chase]);
        }
        else
        {
            if (Time.time >= exitConfusedStateTime)
            {
                _nextState = new FSMTransition(TransitionType.Planning, ai.States[State.JogToLastPosition]);
            }
            else
            {
                if (Time.time >= nextConfusedTurnTime)
                {
                    // Turn left and right
                    ai.aiCharacterControl.SetTarget(null);
                    ai.transform.rotation = Quaternion.Slerp(ai.transform.rotation, nextAngle, Time.deltaTime * 5f);

                    if (Quaternion.Angle(ai.transform.rotation, nextAngle) < 0.1f)
                    {
                        float newAngle = UnityEngine.Random.Range(0f, ai.maxConfusedTurnAngle);
                        if (nextAngleY >= 0) newAngle = -newAngle;
                        nextAngleY = newAngle;
                        Vector3 eulerAngles = ai.transform.rotation.eulerAngles;
                        nextAngle = Quaternion.Euler(
                            eulerAngles.x,
                            eulerAngles.y + nextAngleY,
                            eulerAngles.z
                        );

                        nextConfusedTurnTime = Time.time + UnityEngine.Random.Range(ai.planningTimeMin, ai.planningTimeMax);
                    }
                }
            }
        }
    }
}

public class JogToLastPositionState : FSMState
{
    public JogToLastPositionState()
    {
        _name = "JogToInitialPositionState";
    }
    protected override void _onEnter(GuardAI ai)
    {
        ai.GetComponent<NavMeshAgent>().speed = ai.jogSpeed;
    }

    protected override void _onExit(GuardAI ai)
    {
        ai.GetComponent<NavMeshAgent>().speed = ai.walkSpeed;
    }

    protected override void _onUpdate(GuardAI ai)
    {
        if (ai.currentVisibleTarget != null)
        {
            _nextState = new FSMTransition(TransitionType.Reaction, ai.States[State.Chase]);
        }
        else
        {
            ai.aiCharacterControl.SetTarget(ai.nextPatrolTarget.transform);
            if (Vector3.Distance(ai.transform.position, ai.nextPatrolTarget.transform.position) < 0.75f)
            {
                if (Quaternion.Angle(ai.transform.rotation, ai.nextPatrolTarget.transform.rotation) > 0.1f)
                {
                    ai.transform.rotation = Quaternion.Slerp(ai.transform.rotation, ai.nextPatrolTarget.transform.rotation, Time.deltaTime * 5f);
                }
                else
                {
                    _nextState = new FSMTransition(TransitionType.Planning, ai.States[State.Idle]);
                }
            }
        }
    }
}

public class AttackingState : FSMState
{
    public AttackingState()
    {
        _name = "AttackingState";
    }

    private float attackEndTime;
    protected override void _onEnter(GuardAI ai)
    {
        attackEndTime = Time.time + ai.attackTime;
        ai.GetComponent<ThirdPersonCharacter>().TriggerAttack();
    }

    protected override void _onExit(GuardAI ai)
    {
        // End attack animation
    }

    protected override void _onUpdate(GuardAI ai)
    {
        if (Time.time >= attackEndTime)
        {
            if (ai.currentVisibleTarget != null && Vector3.Distance(ai.transform.position, ai.currentVisibleTarget.transform.position) < 0.75f)
            {
                _nextState = new FSMTransition(TransitionType.Reaction, ai.States[State.KilledPlayer]);
            }
            else
            {
                _nextState = new FSMTransition(TransitionType.Reaction, ai.States[State.Chase]);
            }
        }
    }
}

public class KilledPlayerState : FSMState
{
    public KilledPlayerState()
    {
        _name = "KilledPlayerState";
    }
    private float exitStoppedStateTime;
    protected override void _onEnter(GuardAI ai)
    {
        exitStoppedStateTime = Time.time + ai.killedPlayerStopTime;
    }

    protected override void _onExit(GuardAI ai)
    {
    }

    protected override void _onUpdate(GuardAI ai)
    {
        ai.aiCharacterControl.SetTarget(ai.transform);
        if (Time.time >= exitStoppedStateTime)
        {
            _nextState = new FSMTransition(TransitionType.Reaction, ai.States[State.JogToLastPosition]);
        }
    }
}

public enum TransitionType
{
    Reaction,
    Planning
}

public class FSMTransition : FSMState
{
    private TransitionType type;
    private float nextTransitionActionTime;
    private float transitionEndTime;
    private FSMState stateOnTransitionEnd;
    public FSMTransition(TransitionType type, FSMState nextState)
    {
        _name = type + "TransitionTo" + nextState.name;
        this.type = type;
        stateOnTransitionEnd = nextState;
    }

    private Quaternion initialRotation;
    protected override void _onEnter(GuardAI ai)
    {
        transitionEndTime = Time.time;
        nextTransitionActionTime = Time.time;

        initialRotation = ai.transform.rotation;
        ai.aiCharacterControl.SetTarget(ai.transform);

        switch (type)
        {
            case TransitionType.Reaction:
                transitionEndTime += UnityEngine.Random.Range(ai.reactionTimeMin, ai.reactionTimeMax);
                break;
            case TransitionType.Planning:
                transitionEndTime += UnityEngine.Random.Range(ai.planningTimeMin, ai.planningTimeMax);
                break;
            default:
                // no-op
                break;
        }
    }

    protected override void _onUpdate(GuardAI ai)
    {
        // Do actions for this type of transition
        if (Time.time >= nextTransitionActionTime)
        {
            switch (type)
            {
                case TransitionType.Reaction:
                    // Stand still
                    ai.aiCharacterControl.SetTarget(ai.transform);
                    break;
                case TransitionType.Planning:
                    // Stand still
                    ai.aiCharacterControl.SetTarget(ai.transform);
                    break;
                default:
                    // no-op
                    break;
            }
        }

        // If ready to finish transition do so
        if (Time.time >= transitionEndTime)
        {
            _nextState = stateOnTransitionEnd;
        }
    }

    protected override void _onExit(GuardAI ai)
    {
    }
}
