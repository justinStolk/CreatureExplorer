using System;
using System.Collections.Generic;
using UnityEngine;

public class Planner : MonoBehaviour
{
    [Header("Priority curves")]
    [SerializeField] private AnimationCurve AnnoyancePriority; 
    [SerializeField] private AnimationCurve FearPriority, HungerPriority, TirednessPriority, HappinessPriority, BoredomPriority;

    [Header("Goals")]
    [SerializeField] private Goal[] possibleGoals;
    [field: SerializeField] private Goal defaultGoal;
    [field: SerializeField] private Action defaultAction;

    [Header("Debugging")]
    [Tooltip("Initialised automatically")]
    [SerializeField] private Action[] possibleActions;

    private Dictionary<StateType, AnimationCurve> curvesPerMood;
    private Dictionary<float, StateType> moodPriorities;
    private int prioIndex = 0;
    private float[] prioKeys;
    private float currentPrioKey;

    private bool debug;

    private void Awake()
    {
        curvesPerMood = new Dictionary<StateType, AnimationCurve>();

        // TODO: refactor?
        curvesPerMood.Add(StateType.Annoyance, AnnoyancePriority);
        curvesPerMood.Add(StateType.Boredom, BoredomPriority);
        curvesPerMood.Add(StateType.Fear, FearPriority);
        curvesPerMood.Add(StateType.Happiness, HappinessPriority);
        curvesPerMood.Add(StateType.Hunger, HungerPriority);
        curvesPerMood.Add(StateType.Tiredness, TirednessPriority);

        moodPriorities = new Dictionary<float, StateType>();

#if UNITY_EDITOR
        debug = GetComponent<Creature>().LogDebugs;
        #else
        debug = false;
#endif
    }

    private void OnValidate()
    {
        possibleActions = GetComponentsInChildren<Action>();
    }

    public bool GeneratePlan(CreatureState currentState, Condition worldState, out Goal goal, out List<Action> plan)
    {
        goal = defaultGoal;
        plan = new List<Action>();

        // try generating a plan from the highest priority action. if no plan viable, take next highest priority 
        prioIndex = 0;
        UpdatePriorities(currentState);
        do
        {
            if (prioIndex > Enum.GetValues(typeof(StateType)).Length-1)
            {
                if (debug)
                    Debug.LogError("no valid course of action found");
                goal = defaultGoal;
                plan.Clear();
                plan.Add(defaultAction);
                return false;
            }

            goal = GenerateGoal(currentState);
            prioIndex++;
        } while (!PlanActions(goal, currentState, worldState, out plan));

        if (plan.Count < 1)
        {
            if (debug) 
                Debug.LogError("no valid course of action found");
            goal = defaultGoal;
            plan.Clear();
            plan.Add(defaultAction);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Update the priority values of each mood type
    /// </summary>
    /// <param name="currentState"></param>
    public void UpdatePriorities(CreatureState currentState)
    {
        CreatureState planState = currentState;

        moodPriorities.Clear();

        float prio = 0;

        // TODO: find better way to avoid duplicate keys
        foreach (MoodState state in planState.CreatureStates)
        {
            prio = curvesPerMood[state.MoodType].Evaluate(state.StateValue / 100);

            try
            {
                moodPriorities.Add(prio, state.MoodType);
            }
            catch (ArgumentException)
            {
                if (debug)
                    Debug.Log($"2nd try adding prio of {state.MoodType}");

                if (!moodPriorities.TryAdd(Mathf.Clamp(prio + (int)state.MoodType * 0.0001f, 0, 1), state.MoodType))
                {
                    if (debug)
                        Debug.Log("2nd try failed");
                    moodPriorities.Add(Mathf.Clamp(prio - (int)state.MoodType * 0.0001f, 0, 1), state.MoodType);
                }
            }
        }

        prioKeys = new float[moodPriorities.Count];
        moodPriorities.Keys.CopyTo(prioKeys, 0);
        Array.Sort(prioKeys);
        //Debug.Log($"prioKeys: {prioKeys[0]}, {prioKeys[1]}, {prioKeys[2]}, {prioKeys[3]}, {prioKeys[4]}, {prioKeys[5]}");


        /*
        foreach (KeyValuePair<float, StateType> kvp in moodPriorities)
        {
            Debug.Log($"prio of {kvp.Value} is {kvp.Key}");
        }
        */
    }

    private Goal GenerateGoal(CreatureState currentState)
    {
        List<Goal> goalChoices = new List<Goal>();

        // PrioKeys is sorted from low to high, start with highest prio
        int goalPrioIndex = prioKeys.Length-1-prioIndex;

        if (goalPrioIndex < 0)
        {
# if UNITY_EDITOR
            Debug.LogError("goalPrioIndex was <0");
#endif
            return defaultGoal;
        }

        currentPrioKey = prioKeys[goalPrioIndex];

        while (goalChoices.Count < 1 && goalPrioIndex >= 0)
        {
            foreach (Goal g in possibleGoals)
            {
                foreach (MoodState mood in g.Target)
                {
                    if (mood.MoodType == moodPriorities[prioKeys[goalPrioIndex]])
                    {
                        goalChoices.Add(g);
                    }
                }
            }
            goalPrioIndex--;
        }

        // Return default goal if the priority doesn't yield a good one
        if (goalChoices.Count < 1)
        {
            return defaultGoal;
        }

        return goalChoices[UnityEngine.Random.Range(0, goalChoices.Count)];
    }

    private bool PlanActions(Goal goal, CreatureState currentStats, Condition worldState, out List<Action> plan)
    {
        plan = new List<Action>();

        if (goal == null)
        {
            plan.Add(defaultAction);
            return false;
        } 
        else if (goal.Target.Length <= 0)
        {
            plan.Add(defaultAction);
            return true;
        }

        List<Plan> possiblePlans = new List<Plan>();

        // TODO: figure out way for this to not be necessary
        CreatureState planStats = new CreatureState();
        foreach (MoodState mood in planStats.CreatureStates)
        {
            mood.SetValue(currentStats.Find(mood.MoodType).StateValue);
        }

        MoodState mainMood = goal.Target[0];
        foreach (MoodState mood in goal.Target)
        {
            if (mood.MoodType == moodPriorities[currentPrioKey])
                mainMood = mood;
        }

        Condition planWorldState = worldState;
        
        MoodState[] goalPrerequisites = goal.Target;
        List<Action> viableActions = new List<Action>();

        // TODO: look at the impact of total plans on goal target?
        // Look for actions that satisfy goal target
        viableActions.Clear();

        foreach (Action a in possibleActions)
        {
            if (a.GoalEffects.SatisfiesRequirements(goalPrerequisites, planStats))
            {
                viableActions.Add(a);
            }
        }

        if (viableActions.Count < 1)
        {
            plan.Clear();
            plan.Add(defaultAction);
            return false;
        }

        for (int x = viableActions.Count-1; x >= 0; x--)
        {
            viableActions[x].CalculateCostAndReward(currentStats, goal.Target[0], currentPrioKey);
            possiblePlans.Add(new Plan(viableActions[x], planWorldState));
        }

        int failsafe = 0;
        int completionCounter = 0;

        // While there are incomplete plans that have the potential to be completed, expand the incomplete plans
        while (completionCounter < possiblePlans.Count)
        {
            completionCounter = 0;

            List<Plan> updatedPlans = new List<Plan>();
            List<Plan> extraPlans = new List<Plan>();

            foreach (Plan p in possiblePlans)
            {
                if (debug)
                    Debug.Log($"looking into plan ending in {p.ActionList[0]}");

                bool needNewPlan = false;
                if (p.PlanComplete)
                {
                    completionCounter++;
                    continue;
                }

                // make sure Unity doesn't crash
                failsafe++;
                if (failsafe > 150)
                {
                    if (debug) 
                        Debug.LogError("took too long to generate plan");
                    plan.Clear();
                    plan.Add(defaultAction);
                    return false;
                }

                foreach (Action a in possibleActions)
                {
                    if (p.ActionList.Contains(a))
                        continue;

                    // if the action satisfies the current requirements and hasn't failed
                    if (a.SatisfiesRequirements(p.currentActionPrerequisites, p.planWorldState) && !a.failed)
                    {
                        if (!needNewPlan)
                        {
                            needNewPlan = true;
                            updatedPlans.Add(p);
                        }

                        Plan newPlan = new Plan(p);
                        extraPlans.Add(newPlan);

                        a.CalculateCostAndReward(currentStats, mainMood, currentPrioKey);
                        newPlan.AddAction(a);

                        if (a.RequirementsSatisfied(newPlan.planWorldState))
                        {
                            newPlan.MarkComplete();
                            completionCounter++;
                        }                        
                    }
                    // TODO: make partial satisfaction possible?
                }

                // if no new course of action is found even though the plan isn't complete delete plan from list of possible plans
                if (!needNewPlan)
                {
                    updatedPlans.Add(p);
                }

            }

            // Remove the plans that have been updated, then add the updated versions to possibleplans
            foreach(Plan update in updatedPlans)
            {
                possiblePlans.Remove(update);
            }

            possiblePlans.AddRange(extraPlans);

            if (possiblePlans.Count < 1)
            {
                if (debug) 
                    Debug.Log($"goal {goal.Name} not possible");
                return false;
            }
        }

        float bestCostRewardRatio = 0;
        foreach (Plan p in possiblePlans)
        {
            if (debug)
                Debug.Log($"reward/cost of plan Beginning with {p.ActionList[p.ActionList.Count-1]} and ending with {p.ActionList[0]} is {p.CostRewardRatio}");
            if ((p.CostRewardRatio) > bestCostRewardRatio)
            {
                bestCostRewardRatio = p.CostRewardRatio;
                plan = p.ActionList; 
            }
        }
        plan.Reverse();

        if (debug) 
            Debug.Log($"generated plan for goal {goal.Name}");
        return true;
    }
}
