using UnityEngine;

// Sigue revisando los nodos de alrededor mientras no tenga una posición nueva.
public class WolfSearchState : WolfState
{
    private float timer;

    public WolfSearchState(Wolf wolf) : base(wolf) { }

    public override void Enter()
    {
        timer = 0f;
        Announce(WolfStateID.Search);
        GoToRandomNeighbor();
    }

    public override void Update()
    {
        if (wolf.SeesHunter)
        {
            _fsm.ChangeState(WolfStateID.Chase);
            return;
        }

        if (wolf.HasAlert)
        {
            _fsm.ChangeState(WolfStateID.Investigate);
            return;
        }

        timer += Time.deltaTime;
        if (timer >= wolf.SearchDuration)
        {
            _fsm.ChangeState(WolfStateID.Patrol);
            return;
        }

        if (wolf.Follower.TryGetNextPoint(out Vector3 point)) wolf.MoveTowards(point, wolf.PatrolSpeed);
        else GoToRandomNeighbor();
    }

    private void GoToRandomNeighbor()
    {
        GraphNode here = wolf.Follower.Graph.GetClosestNode(wolf.transform.position, true);
        if (here == null) return;

        GraphNode next = here.Neighbors[Random.Range(0, here.Neighbors.Count)];
        wolf.Follower.SetGoal(next.IsSafeZone ? here : next);
    }

    public override void Exit() { }
}
