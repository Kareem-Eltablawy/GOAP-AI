using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class GoapAgent : MonoBehaviour
{
    private enum AgentState
    {
        Idle,
        Moving,
        PerformingAction
    }
    
    private AgentState currentState = AgentState.Idle;
    private HashSet<GoapAction> availableActions;
    private Queue<GoapAction> currentActions;
    private GoapPlanner planner;
    private WorldState worldState;
    private NavMeshAgent navAgent;
    private GoapAction currentAction;
    
    // Patrol settings
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    
    // Vision settings
    public float visionRadius = 10f;
    public LayerMask visionLayers;
    
    void Start()
    {
        availableActions = new HashSet<GoapAction>();
        currentActions = new Queue<GoapAction>();
        planner = new GoapPlanner();
        worldState = new WorldState();
        navAgent = GetComponent<NavMeshAgent>();
        
        LoadActions();
    }
    
    void Update()
    {
        UpdateWorldState();
        
        switch (currentState)
        {
            case AgentState.Idle:
                PlanAndExecute();
                break;
                
            case AgentState.Moving:
                if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
                {
                    if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
                    {
                        currentState = AgentState.PerformingAction;
                        if (currentAction != null && currentAction.PrePerform())
                        {
                            // PrePerform successful, action will continue in PerformingAction state
                        }
                        else
                        {
                            // Action failed, go back to idle
                            currentState = AgentState.Idle;
                        }
                    }
                }
                break;
                
            case AgentState.PerformingAction:
                if (currentAction != null && currentAction.PostPerform())
                {
                    CompleteAction();
                }
                break;
        }
    }
    
    private void UpdateWorldState()
    {
        // Update world state based on current situation
        worldState.SetState(WorldStateKey.SeeThief, CheckIfSeeThief());
        worldState.SetState(WorldStateKey.AtWaypoint, CheckIfAtWaypoint());
    }
    
    private void PlanAndExecute()
    {
        WorldState goal = CreateGoalState();
        Queue<GoapAction> plan = planner.Plan(this, availableActions, worldState, goal);
        
        if (plan != null && plan.Count > 0)
        {
            currentActions = plan;
            ExecuteNextAction();
        }
        else
        {
            // No plan found, wait a bit before trying again
            Debug.Log("Failed to find plan");
        }
    }
    
    private void ExecuteNextAction()
    {
        if (currentActions.Count == 0)
        {
            currentState = AgentState.Idle;
            return;
        }
        
        currentAction = currentActions.Peek();
        
        if (currentAction.RequiresInRange && currentAction.Target == null)
        {
            Debug.Log("Action requires a target but has none");
            currentState = AgentState.Idle;
            return;
        }
        
        if (currentAction.RequiresInRange)
        {
            currentState = AgentState.Moving;
            navAgent.SetDestination(currentAction.Target.position);
        }
        else
        {
            currentState = AgentState.PerformingAction;
            if (currentAction.PrePerform())
            {
                // PrePerform successful, action will continue in PerformingAction state
            }
            else
            {
                // Action failed, go back to idle
                currentState = AgentState.Idle;
            }
        }
    }
    
    private void CompleteAction()
    {
        if (currentActions.Count > 0)
            currentActions.Dequeue();
            
        currentAction = null;
        ExecuteNextAction();
    }
    
    public void AddAction(GoapAction action)
    {
        availableActions.Add(action);
    }
    
    public GoapAction GetAction(Type actionType)
    {
        return availableActions.FirstOrDefault(action => action.GetType() == actionType);
    }
    
    public T GetAction<T>() where T : GoapAction
    {
        return (T)availableActions.FirstOrDefault(action => action is T);
    }
    
    public void RemoveAction(GoapAction action)
    {
        availableActions.Remove(action);
    }
    
    public bool HasActionPlan()
    {
        return currentActions.Count > 0;
    }
    
    private WorldState CreateGoalState()
    {
        WorldState goal = new WorldState();
        
        if (worldState.GetState(WorldStateKey.SeeThief))
        {
            goal.SetState(WorldStateKey.ThiefCaught, true);
        }
        else
        {
            goal.SetState(WorldStateKey.PatrolComplete, true);
        }
        
        return goal;
    }
    
    private bool CheckIfSeeThief()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, visionRadius, visionLayers);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Thief"))
            {
                return true;
            }
        }
        return false;
    }
    
    private bool CheckIfAtWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return false;
        
        Transform currentWaypoint = waypoints[currentWaypointIndex];
        return Vector3.Distance(transform.position, currentWaypoint.position) < 2f;
    }
    
    public Transform GetNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return null;
        
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        return waypoints[currentWaypointIndex];
    }
    
    public Transform GetCurrentWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return null;
        return waypoints[currentWaypointIndex];
    }
    
    private void LoadActions()
    {
        GoapAction[] actions = GetComponents<GoapAction>();
        foreach (GoapAction action in actions)
        {
            availableActions.Add(action);
        }
        
        Debug.Log($"Loaded {availableActions.Count} actions");
    }
    
    // Visualization
    void OnDrawGizmos()
    {
        // Draw vision radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
        
        // Draw current path if moving
        if (navAgent != null && navAgent.hasPath)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, navAgent.destination);
            Gizmos.DrawWireSphere(navAgent.destination, 0.5f);
        }
    }
}
