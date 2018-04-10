internal class IdleState : IState
{
    /// <summary>
    ///     A reference to the parent
    /// </summary>
    private Enemy parent;

    /// <summary>
    ///     This is called whenever we enter the state
    /// </summary>
    /// <param name="parent">The parent enemy</param>
    public void Enter(Enemy parent)
    {
        this.parent = parent;

        this.parent.Reset();
    }

    /// <summary>
    ///     This is called whenever we exit the state
    /// </summary>
    public void Exit()
    {
    }

    /// <summary>
    ///     This is called as long as we are inside the state
    /// </summary>
    public void Update()
    {
        //Change into follow state if the player is close
        if (parent.Target != null) //If we have a target , then we need to follow it.
            parent.ChangeState(new FollowState());
    }
}