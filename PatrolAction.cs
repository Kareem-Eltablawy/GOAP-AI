using UnityEngine;

public class PatrolAction : GoapAction
{
    private GoapAgent goapAgent;
    
    protected override void Awake()
    {
        base.Awake();
        goapAgent = GetComponent<GoapAgent>();
        
        ActionName = "Patrol";
        Cost = 1f;
        
        Preconditions.Add(WorldStateKey.AtWaypoint, false);
        Effects.Add(WorldStateKey.AtWaypoint, true);
        Effects.Add(WorldStateKey.PatrolComplete, true);
        
        RequiresInRange = true;
    }
    
    public override bool PrePerform()
    {
        Target = goapAgent.GetNextWaypoint();
        if (Target == null)
        {
            Debug.LogError("No waypoints set for patrol action");
            return false;
        }
        return true;
    }
    
    public override bool PostPerform()
    {
        // Patrol action completed successfully
        return true;
    }
}
