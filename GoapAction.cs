public abstract class GoapAction : MonoBehaviour
{
    public string ActionName = "Action";
    public float Cost = 1f;
    
    public Dictionary<WorldStateKey, bool> Preconditions { get; protected set; }
    public Dictionary<WorldStateKey, bool> Effects { get; protected set; }
    
    public bool RequiresInRange = false;
    public Transform Target;
    
    protected GoapAgent agent;
    
    protected virtual void Awake()
    {
        Preconditions = new Dictionary<WorldStateKey, bool>();
        Effects = new Dictionary<WorldStateKey, bool>();
        agent = GetComponent<GoapAgent>();
    }
    
    public virtual bool IsAchievableGiven(Dictionary<WorldStateKey, bool> conditions)
    {
        foreach (var precondition in Preconditions)
        {
            if (!conditions.ContainsKey(precondition.Key) || 
                conditions[precondition.Key] != precondition.Value)
                return false;
        }
        return true;
    }
    
    public virtual void ResetAction()
    {
        Target = null;
    }
    
    public abstract bool PrePerform();
    public abstract bool PostPerform();
}
