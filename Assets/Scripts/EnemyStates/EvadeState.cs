using UnityEngine;

public class EvadeState : IState
{
    /// <summary>
    ///     A reference to the enemy parent
    /// </summary>
    private Enemy parent;

    public void Enter(Enemy parent)
    {
        this.parent = parent;
    }

    public void Exit()
    {
        parent.Direction = Vector2.zero;
        parent.Reset();
    }

    public void Update()
    {
        //Makes sure that we can run back to the original position when we are evading        
        parent.Direction = (parent.StartPosition - parent.transform.position).normalized;

        parent.transform.position = Vector2.MoveTowards
            (parent.transform.position, parent.StartPosition, parent.Speed * Time.deltaTime);

        //Calculates the distance between the enemy and the startpostion
        var distance = Vector2.Distance(parent.StartPosition, parent.transform.position);

        //If the distance is less than 2 (trashhold) then we are back home and we need to idle
        if (distance <= 2) parent.ChangeState(new IdleState());
    }
}