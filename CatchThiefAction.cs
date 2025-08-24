using UnityEngine;

public class CatchThiefAction : GoapAction
{
    private float catchRadius = 2f;
    
    protected override void Awake()
    {
        base.Awake();
        
        ActionName = "CatchThief";
        Cost = 1f;
        
        Preconditions.Add(WorldStateKey.SeeThief, true);
        Effects.Add(WorldStateKey.ThiefCaught, true);
        
        RequiresInRange = true;
    }
    
    public override bool PrePerform()
    {
        // Find the thief
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Thief"))
            {
                Target = collider.transform;
                return true;
            }
        }
        
        // No thief found
        return false;
    }
    
    public override bool PostPerform()
    {
        if (Target != null && Vector3.Distance(transform.position, Target.position) <= catchRadius)
        {
            // "Catch" the thief by destroying it
            Destroy(Target.gameObject);
            return true;
        }
        
        // Failed to catch thief
        return false;
    }
}
