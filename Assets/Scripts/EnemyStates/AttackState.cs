using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IState
{
    /// <summary>
    /// A reference to the state's parent
    /// </summary>
    private Enemy parent;

    private float attackCooldown = 3;

    private float extraRange = .1f;

    /// <summary>
    /// The state's constructor
    /// </summary>
    /// <param name="parent"></param>
    public void Enter(Enemy parent)
    {
        this.parent = parent; 
    }

    public void Exit()
    {
        
    }

    public void Update()
    {
        Debug.Log("Attacking");

        //Makes sure that we only attack when we are off cooldown
        if (parent.AttackTime >= attackCooldown && !parent.IsAttacking)
        {
            //Resets the attack timer
            parent.AttackTime = 0;

            //Starts the attack
            parent.StartCoroutine(Attack());
        }

        if (parent.Target != null) //If we have a target then we need to check if we can attack it or if we need to follow it
        {
            //calculates the distance between the target and the enemy
            float distance = Vector2.Distance(parent.Target.position, parent.transform.position);

            //If the distance is larget than the attackrange, then we need to move
            if (distance >= parent.AttackRange+extraRange && !parent.IsAttacking)
            {
                //Follows the target
                parent.ChangeState(new FollowState());
            }
       

        }
        else//If we lost the target then we need to idle
        {
            parent.ChangeState(new IdleState());
        }
    }

    /// <summary>
    /// Makes the enemy attack the player
    /// </summary>
    /// <returns></returns>
    public IEnumerator Attack()
    {
        parent.IsAttacking = true;

        parent.Animator.SetTrigger("Attack");

        yield return new WaitForSeconds(parent.Animator.GetCurrentAnimatorStateInfo(2).length);

        parent.IsAttacking = false;
    }

}
