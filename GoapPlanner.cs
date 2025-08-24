using System.Collections.Generic;
using System.Linq;

public class GoapPlanner
{
    private class Node
    {
        public Node parent;
        public float runningCost;
        public WorldState state;
        public GoapAction action;
        
        public Node(Node parent, float runningCost, WorldState state, GoapAction action)
        {
            this.parent = parent;
            this.runningCost = runningCost;
            this.state = state;
            this.action = action;
        }
    }
    
    public Queue<GoapAction> Plan(GoapAgent agent, HashSet<GoapAction> availableActions, 
        WorldState worldState, WorldState goal)
    {
        // Reset all actions
        foreach (GoapAction action in availableActions)
        {
            action.ResetAction();
        }
        
        // Check what actions can run
        HashSet<GoapAction> usableActions = new HashSet<GoapAction>();
        foreach (GoapAction action in availableActions)
        {
            if (action.IsAchievableGiven(worldState.States))
                usableActions.Add(action);
        }
        
        // Build the tree
        List<Node> leaves = new List<Node>();
        Node start = new Node(null, 0, worldState.Clone(), null);
        
        bool success = BuildGraph(start, leaves, usableActions, goal);
        
        if (!success)
        {
            Debug.Log("NO PLAN");
            return null;
        }
        
        // Find the cheapest leaf
        Node cheapest = null;
        foreach (Node leaf in leaves)
        {
            if (cheapest == null || leaf.runningCost < cheapest.runningCost)
                cheapest = leaf;
        }
        
        // Work back through the parents
        List<GoapAction> result = new List<GoapAction>();
        Node n = cheapest;
        while (n != null)
        {
            if (n.action != null)
            {
                result.Insert(0, n.action);
            }
            n = n.parent;
        }
        
        // Make a queue
        Queue<GoapAction> queue = new Queue<GoapAction>();
        foreach (GoapAction action in result)
        {
            queue.Enqueue(action);
        }
        
        return queue;
    }
    
    private bool BuildGraph(Node parent, List<Node> leaves, HashSet<GoapAction> usableActions, WorldState goal)
    {
        bool foundOne = false;
        
        foreach (GoapAction action in usableActions)
        {
            if (action.IsAchievableGiven(parent.state.States))
            {
                WorldState currentState = parent.state.Clone();
                
                // Apply action effects to the current state
                foreach (var effect in action.Effects)
                {
                    currentState.SetState(effect.Key, effect.Value);
                }
                
                Node node = new Node(parent, parent.runningCost + action.Cost, currentState, action);
                
                if (GoalAchieved(goal, currentState))
                {
                    leaves.Add(node);
                    foundOne = true;
                }
                else
                {
                    HashSet<GoapAction> subset = ActionSubset(usableActions, action);
                    bool found = BuildGraph(node, leaves, subset, goal);
                    if (found)
                        foundOne = true;
                }
            }
        }
        
        return foundOne;
    }
    
    private HashSet<GoapAction> ActionSubset(HashSet<GoapAction> actions, GoapAction removeMe)
    {
        HashSet<GoapAction> subset = new HashSet<GoapAction>();
        foreach (GoapAction action in actions)
        {
            if (!action.Equals(removeMe))
                subset.Add(action);
        }
        return subset;
    }
    
    private bool GoalAchieved(WorldState goal, WorldState state)
    {
        foreach (var g in goal.States)
        {
            if (!state.States.ContainsKey(g.Key) || state.States[g.Key] != g.Value)
                return false;
        }
        return true;
    }
}
