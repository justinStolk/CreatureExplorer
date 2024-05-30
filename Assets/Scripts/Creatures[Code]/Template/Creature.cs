using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Planner))]
public class Creature : MonoBehaviour
{
    [field: Header("Creature data")]
    [field: SerializeField] public CreatureData data;
    public Vector3 WaryOff { get; protected set; }
    protected float waryLoudness = 1;

    [SerializeField] private SkinnedMeshRenderer skinRenderer;

    [Header("Debugging")]
    [field: HideArrow, SerializeField] private bool debug;
    [field: ConditionalHide("debug", true), SerializeField] private bool showThoughts;
    [field: ConditionalHide("debug", true), SerializeField] public bool LogDebugs { get; private set; }
    [field: ConditionalHide("debug", true), SerializeField] private TextMeshProUGUI goalText;
    [field: ConditionalHide("debug", true), SerializeField] private TextMeshProUGUI actionText;
    [field: ConditionalHide("debug", true), SerializeField] private TextMeshProUGUI soundText;

    [Header("GOAP")]
    [SerializeField] protected Condition worldState;
    [field: SerializeField] protected CreatureState currentCreatureState { get; private set; }
    [SerializeField] private List<Action> currentPlan;
    public Action CurrentAction { get; private set; }

    protected delegate void CheckSurroundings();
    protected CheckSurroundings surroundCheck;

    private Goal currentGoal;
    protected GameObject currentTarget = null;

    private Planner planner;

    private bool sawPlayer = false;

    protected virtual void Start()
    {
        DevDunk.AutoLOD.AnimatorLODManager.Instance.AddAnimator(GetComponentInChildren<DevDunk.AutoLOD.AnimatorLODObject>());

        GetComponent<NavMeshAgent>().updateUpAxis = false;
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), col);
        }

        if (data.SkinVariants.Length > 0)
        {
            skinRenderer.material = data.SkinVariants[UnityEngine.Random.Range(0, data.SkinVariants.Length)];
            
            //Material randomMat = data.SkinVariants[UnityEngine.Random.Range(0, data.SkinVariants.Length)];
            
            //foreach (SkinnedMeshRenderer renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            //{
            //    renderer.material = randomMat;
            //}
        }

        if (!showThoughts)
        {
            goalText.transform.parent.gameObject.SetActive(false);
        }

        planner = GetComponent<Planner>();

        if (currentCreatureState.CreatureStates.Length <=0)
        {
            currentCreatureState = new CreatureState();
        }

        // Set random friendliness value
        currentCreatureState.Find(StateType.Friendliness).SetValue(UnityEngine.Random.Range(40,90));

        UpdateCreatureState();

        GenerateNewGoal();

        PlayerEventCaster.ListenForSounds(HearPlayer);

        if (surroundCheck == null)
            surroundCheck = new CheckSurroundings(CheckForFood);
        else
            surroundCheck += CheckForFood;

        StartInvoking();
    }

    private void StartInvoking()
    {
        InvokeRepeating("UpdateValues", 0, 1);
        InvokeRepeating("TiltWithGround", 0, data.GroundTiltTimer);

        if (surroundCheck != null)
            InvokeRepeating("LookAtSurroundings", 0, data.CheckSurroundingsTimer);
    }

    private void OnDisable()
    {
        CancelInvoke();
        StopAllCoroutines();
        CurrentAction.enabled = false;
        Destroy(gameObject, data.DecayTimer);
    }

    private void OnEnable()
    {
        StartInvoking();
        if (CurrentAction!= null)
            CurrentAction.enabled = true;
    }

    #region GOAP
    protected virtual void StartAction()
    {
        CurrentAction = currentPlan[0];

        // reset values on action before running it
        CurrentAction.Reset();

        currentTarget = CurrentAction.ActivateAction(this, currentTarget);

        if (showThoughts)
        {
            actionText.text = currentPlan[0].Name;
        } 
    }

    public void FinishAction()
    {
        if (CurrentAction != null)
        {
            if (CurrentAction.finished)
            {
                // Update creatureState with effects of finished action
                UpdateValues(CurrentAction.GoalEffects);
        
                // check if goal has been reached
                if (currentPlan.Count <= 1)
                {
                    GenerateNewGoal();
                    return;
                }
                
                // remove the action that has now finished from the plan
                currentPlan.RemoveAt(0);
        
                StartAction();
            }
        }
    }

    public void FailAction()
    {
        if (CurrentAction != null)
        {
            // if an action has failed try and generate a new goal
            if (CurrentAction.failed)
            {

#if UNITY_EDITOR
                DebugMessage("Action failed! " + CurrentAction.Name);
#endif
                GenerateNewGoal();
            }
        }
    }

    private void GenerateNewGoal()
    {
        if (!planner.GeneratePlan(currentCreatureState, worldState, out currentGoal, out currentPlan) && LogDebugs)
        {
# if UNITY_EDITOR
            Debug.Log("Failed in generating plan, resorting to deault action");
#endif
        }

        // reset values on last action before starting new plan
        if (CurrentAction != null)
            CurrentAction.Reset();

        currentTarget = null;

        StartAction();

        if (LogDebugs)
        {
# if UNITY_EDITOR
            Debug.Log("Generated new goal: " + currentGoal);
#endif
        }

        if (showThoughts)
        {
            goalText.text = currentGoal.Name;
        }
    }

    /// <summary>
    /// Update creatureState with effects set in changesEverySecond
    /// </summary>
    private void UpdateValues()
    {
        // TODO: get rid of magic number
        // Make creature tire faster when it's bedtime
        try
        {
            if (DistantLands.Cozy.CozyWeather.instance.currentTime.IsBetweenTimes(data.Bedtime, data.WakeTime))
                currentCreatureState.AddValue(2f * Time.deltaTime, StateType.Tiredness);
        } catch
        {

#if UNITY_EDITOR
            DebugMessage("Cozyweather is not active");
#endif
        }
        foreach (MoodState change in data.ChangesEverySecond.CreatureStates)
        {
            if (change.Operator == StateOperant.Add)
                currentCreatureState.AddValue(change.StateValue, change.MoodType);
            else if (change.Operator == StateOperant.Subtract)
                currentCreatureState.AddValue(-change.StateValue, change.MoodType);
        }

#if UNITY_EDITOR
        //DebugMessage("updated worldstate of " + gameObject.name);
#endif
        UpdateCreatureState();
    }

    /// <summary>
    /// Add given parameters to creaturestate 
    /// </summary>
    protected void UpdateValues(StateType changeState, float amount, StateOperant operant)
    {
        if (operant == StateOperant.Add)
            currentCreatureState.AddValue(amount * Time.deltaTime, changeState);
        else if (operant == StateOperant.Subtract)
            currentCreatureState.AddValue(-amount * Time.deltaTime, changeState);

        UpdateCreatureState();
    }

    /// <summary>
    /// Update creatureState with effects of finished action
    /// </summary>
    /// <param name="updateWith">the CreatureState containing the Moodstates to update with</param>
    public void UpdateValues(CreatureState updateWith)
    {        
        foreach (MoodState change in updateWith.CreatureStates)
        {
            if (change.Operator == StateOperant.Set)
                currentCreatureState.SetValue(change.StateValue, change.MoodType);
            else if (change.Operator == StateOperant.Add)
                currentCreatureState.AddValue(change.StateValue, change.MoodType);
            else if (change.Operator == StateOperant.Subtract)
                currentCreatureState.AddValue(-change.StateValue, change.MoodType);

#if UNITY_EDITOR
            DebugMessage("updated worldstate of " + change.MoodType.ToString());
#endif
        }

        UpdateCreatureState();
    }

    /// <summary>
    /// set the worldstate to reflect mood values
    /// </summary>
    protected virtual void UpdateCreatureState()
    {
        CheckForInterruptions(StateType.Tiredness, GetComponentInChildren<Sleep>(), "Fell asleep");

        try
        {
            worldState = DistantLands.Cozy.CozyWeather.instance.currentTime.IsBetweenTimes(data.Bedtime, data.WakeTime) ? SetConditionTrue(worldState, Condition.ShouldBeSleeping) : SetConditionFalse(worldState, Condition.ShouldBeSleeping);
        }
        catch
        {

#if UNITY_EDITOR
            DebugMessage("Cozyweather is not active");
#endif
        }

        worldState = (currentCreatureState.Find(StateType.Hunger).StateValue > 50) ? SetConditionTrue(worldState, Condition.IsHungry) : SetConditionFalse(worldState, Condition.IsHungry);
        worldState = (currentCreatureState.Find(StateType.Tiredness).StateValue > 50) ? SetConditionTrue(worldState, Condition.IsSleepy) : SetConditionFalse(worldState, Condition.IsSleepy);
        worldState = (currentCreatureState.Find(StateType.Annoyance).StateValue > 50) ? SetConditionTrue(worldState, Condition.IsAnnoyed) : SetConditionFalse(worldState, Condition.IsAnnoyed);
        worldState = (currentCreatureState.Find(StateType.Fear).StateValue > 50) ? SetConditionTrue(worldState, Condition.IsFrightened) : SetConditionFalse(worldState, Condition.IsFrightened);
        worldState = (currentCreatureState.Find(StateType.Happiness).StateValue > 50) ? SetConditionTrue(worldState, Condition.IsHappy) : SetConditionFalse(worldState, Condition.IsHappy);
    }

    /// <summary>
    /// interrupt the current action if a creaturestate value is higher than the threshold
    /// </summary>
    /// <param name="interruptionSource"></param>
    /// <param name="threshold"></param>
    protected void CheckForInterruptions(StateType interruptionSource, Action associatedAction, string interruptionText = "", float threshold = 99)
    {
        // TODO: refactor, Make it so new plan is generated?
        // stop action and change plan if statevalue is higher that threshold
        if (currentCreatureState.Find(interruptionSource).StateValue >= threshold)
        {
            try
            {
                MoodState moodOperator = CurrentAction.GoalEffects.Find(interruptionSource);
                // if this action doen not impact the mood that is interrupting
                if (moodOperator == null)
                {
                    Interrupt(associatedAction, interruptionText);
                }
                else if (moodOperator.Operator != StateOperant.Subtract)
                {
                    Interrupt(associatedAction, interruptionText);
                }
            } catch (NullReferenceException)
            {
                Interrupt(associatedAction, interruptionText);
            }
        }
    }

    protected async void Interrupt(Action associatedAction, string debugText = "", bool waitForFinishAnimation = false)
    {
#if UNITY_EDITOR
        DebugMessage($"interruption source: {associatedAction.Name}");
#endif

        if (waitForFinishAnimation)
        {
            System.Threading.Tasks.Task animationWaiter = CurrentAction.InterruptAction();
            while (!animationWaiter.IsCompletedSuccessfully)
            {
                await System.Threading.Tasks.Task.Delay(100);
            }
        }
        else
        {
            CurrentAction.Reset();
        }
        
        GetComponent<NavMeshAgent>().SetDestination(transform.position);

        if (showThoughts)
        {
            goalText.text = debugText;
        }

        currentPlan.Clear();
        currentPlan.Add(associatedAction);
        StartAction();
    }
    #endregion

    /// <summary>
    /// Is the attack on this creature successful?
    /// </summary>
    /// <returns></returns>
    public bool AttackSuccess(Vector3 attackSource)
    {
        // TODO: think about what to set the value to beat to

        if (UnityEngine.Random.Range(0, currentCreatureState.Find(StateType.Tiredness).StateValue) > 10)
        {
#if UNITY_EDITOR
            DebugMessage("Die");
#endif

            Animator animator = GetComponentInChildren<Animator>();
            CurrentAction.Stop();

            if (animator != null)
            {
                animator.Rebind();
                this.enabled = false;
                animator.speed = 1;

                animator.SetBool("Die", true);
            }
            else
            {
                this.enabled = false;
            }

            if (showThoughts)
            {
                goalText.text = "DEAD";
                soundText.text = "DEAD";
                actionText.text = "";
            }

            return true;
        }
        else
        {
            ReactToAttack(attackSource);
            return false;
        }
    }

    protected virtual void ReactToAttack(Vector3 attackPos)
    {
        WaryOff = attackPos;
        UpdateValues(data.ReactionToAttack);
        worldState = SetConditionTrue(worldState, Condition.IsNearDanger);

#if UNITY_EDITOR
        DebugMessage("Was Attacked");
#endif
    }

    public void HearPlayer(Vector3 playerPos, float playerLoudness)
    {
        if ((transform.position - playerPos).sqrMagnitude < playerLoudness * data.HearingSensitivity * CurrentAction.Awareness)
            ReactToPlayer(playerPos, playerLoudness);
        else if (sawPlayer)
        {
            ReactToPlayerLeaving(playerPos);
        }
    }

    protected virtual void ReactToPlayer(Vector3 playerPos, float playerLoudness)
    {
        sawPlayer = true;
        if (currentCreatureState.Find(StateType.Friendliness).StateValue < 50)
            UpdateValues(data.ReactionToPlayerFearful);
        else
            UpdateValues(data.ReactionToPlayerFriendly);

#if UNITY_EDITOR
        DebugMessage("Noticed Player");
#endif
    }

    protected virtual void ReactToPlayerLeaving(Vector3 playerPos)
    {
        sawPlayer = false;

#if UNITY_EDITOR
        DebugMessage("Lost sight of Player");
#endif
    }

    protected void LookAtSurroundings()
    {
         surroundCheck.Invoke();
    }

    /// <summary>
    /// Checks for food in neighbourhood and ups the hunger value with the amount of food nearby
    /// </summary>
    public virtual void CheckForFood()
    {
        int foodcount = LookForObjects<Food>.CheckForObjects(transform.position, data.HearingSensitivity).Count;

        currentCreatureState.AddValue(foodcount, StateType.Hunger);

        //DebugMessage($"found {foodcount}, hunger is now {currentCreatureState.Find(StateType.Hunger).StateValue}");
    }

    protected void TiltWithGround()
    {
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit);
        if (transform.up != hit.normal)
        {
            Vector3 tempForward = (Vector3.Cross(hit.normal, transform.right)).normalized;
            if ((transform.forward - tempForward).magnitude > 1)
            {
                tempForward *= -1;
            }
            tempForward = Vector3.Slerp(transform.forward, tempForward, 0.2f);

            Vector3 tempRight = (Vector3.Cross(hit.normal, tempForward)).normalized;
            if ((transform.right - tempRight).magnitude > 1)
            {
                tempRight *= -1;
            }
            tempRight = Vector3.Slerp(transform.right, tempRight, 0.2f);

            Vector3 tempUp = (Vector3.Cross(tempRight, tempForward)).normalized;
            if ((transform.up - tempUp).magnitude > 1)
            {
                tempUp *= -1;
            }

            transform.LookAt(transform.position+tempForward, tempUp); 
        }
    }

    protected Condition SetConditionTrue(Condition currentState, Condition flagToSet)
    {
        return currentState |= flagToSet;
    }

    protected Condition SetConditionFalse(Condition currentState, Condition flagToSet)
    {
        return currentState &= ~flagToSet;
    }

    protected void DebugMessage(string message)
    {
        if (LogDebugs)
            Debug.Log(message);
    }
}
