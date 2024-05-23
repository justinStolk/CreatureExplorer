using System;
using UnityEngine;

[Serializable]
public class CreatureState 
{
    [field: SerializeField] public MoodState[] CreatureStates { get; private set; }

    public CreatureState()
    {
        int stateAmounts = Enum.GetNames(typeof(StateType)).Length;

        CreatureStates = new MoodState[stateAmounts];

        for (int x = stateAmounts; x>0; )
        {
            x--;
            CreatureStates[x] = new MoodState((StateType)x, 0.5f);
        }
    }

    public MoodState Find(StateType mood)
    {
        foreach (MoodState state in this.CreatureStates)
        {
            if (state.Equals(mood))
            {
                return state;
            }
        }

        return null;
    }

    public void SetValue(float value, StateType mood)
    {
        foreach (MoodState state in this.CreatureStates)
        {
            if (state.Equals(mood))
            {
                state.SetValue(value);
            }
        }
    }

    public void AddValue(float value, StateType mood)
    {
        foreach (MoodState state in this.CreatureStates)
        {
            if (state.Equals(mood))
            {
                state.AddValue(value);
            }
        }
    }
    
    /// <summary>
    /// does this CreatureState meet the requirements given
    /// </summary>
    /// <param name="requirements">the requirements to satisfy</param>
    /// <returns></returns>
    public bool SatisfiesRequirements(MoodState[] requirements, CreatureState creatureState = null)
    {
        int targetsReached = 0;

        foreach (MoodState target in requirements)
        {
            MoodState thisMood = Find(target.MoodType);

            switch (thisMood?.Operator)
            {
                case StateOperant.Set :
                    if ((target.Operator == StateOperant.GreaterThan && thisMood >= target) || (target.Operator == StateOperant.LessThan && thisMood <= target))
                        targetsReached++;
                    break;

                case StateOperant.Add :
                    if (target.Operator == StateOperant.GreaterThan && creatureState.Find(target.MoodType) + thisMood >= target)
                        targetsReached++;
                    break;

                case StateOperant.Subtract :
                    if (target.Operator == StateOperant.LessThan && creatureState.Find(target.MoodType) - thisMood <= target)
                        targetsReached++;
                    break;

                default :
                    break;
            }      
        }

        return (targetsReached >= requirements.Length);
    }
}

[Serializable]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
public class MoodState
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    [field: SerializeField] public StateType MoodType { get; private set; }
    [field: SerializeField] public StateOperant Operator { get; private set; }
    [field: SerializeField][field: Range(0.0f, 100.0f)]  public float StateValue { get; private set; }

    public MoodState(StateType stateType, float value)
    {
        MoodType = stateType;
        StateValue = value;
    }

    public void SetValue(float newValue)
    {
        StateValue = newValue;
    }
    public void AddValue(float newValue)
    {
        StateValue += newValue;
        StateValue = Mathf.Clamp(StateValue, 0, 99);
    }

    #region operator overrides

    /// <summary>
    /// Returns true if the state type is the same
    /// </summary>
    /// <param name="b">the statepair to compare the type of</param>
    /// <returns></returns>
    public bool Equals(MoodState b)
    {
        return (this.MoodType == b.MoodType);
    }

    /// <summary>
    /// Returns true if the state type is the same
    /// </summary>
    /// <param name="b">the statetype to compare the type of</param>
    /// <returns></returns>
    public bool Equals(StateType b)
    {
        return (this.MoodType == b);
    }

    public override int GetHashCode()
    {
        return this.GetHashCode();
    }
    
    public static bool operator ==(MoodState a, MoodState b)
    {
        try
        {
            return (a.MoodType == b.MoodType && a.StateValue == b.StateValue);
        }
        catch (NullReferenceException)
        {
            try
            {
                a.AddValue(0);
            }
            catch (NullReferenceException)
            {
                try
                {
                    b.AddValue(0);
                }
                catch (NullReferenceException)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
    }

    public static bool operator !=(MoodState a, MoodState b) => !(a == b);

    public static bool operator >=(MoodState a, MoodState b)
    {
        return (a.MoodType == b.MoodType && a.StateValue >= b.StateValue);
    }

    public static bool operator <=(MoodState a, MoodState b)
    {
        return (a.MoodType == b.MoodType && a.StateValue <= b.StateValue);
    }

    public static MoodState operator +(MoodState a, MoodState b)
    {
        MoodState result = a;
        result.AddValue(b.StateValue);

        return result;
    }
    public static MoodState operator -(MoodState a, MoodState b)
    {
        MoodState result = a;
        result.AddValue(-b.StateValue);

        return result;
    }
    #endregion
}

public enum StateType
{
    Annoyance,
    Tiredness,
    Fear,
    Hunger,
    Happiness,
    Boredom,
    Friendliness
}

public enum StateOperant
{
    GreaterThan,
    LessThan,
    Set,
    Add,
    Subtract
}
