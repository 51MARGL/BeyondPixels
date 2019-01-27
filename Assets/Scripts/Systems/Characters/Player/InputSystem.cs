using BeyondPixels.Components.Characters.Common;
using BeyondPixels.Components.Characters.Player;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BeyondPixels.Systems.Characters.Player
{
    public class InputSystem : JobComponentSystem
    {        
        private struct InputJob : IJobProcessComponentData<InputComponent, MovementComponent>
        {
            public Vector2 Direction;
            public int AttackPressed;
            public int MouseClicked;
            public Vector3 MousePosition;
            public int ActionButtonPressed;

            public void Execute(ref InputComponent inputComponent, ref MovementComponent movementComponent)
            {
                movementComponent.Direction = Direction;
                inputComponent.InputDirection = Direction;

                inputComponent.AttackButtonPressed = AttackPressed;

                inputComponent.MouseButtonClicked = MouseClicked;
                inputComponent.MousePosition = MousePosition;

                inputComponent.ActionButtonPressed = ActionButtonPressed;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var direction = Vector2.zero;
            var attackPressed = 0;
            var mouseClicked = 0;
            var actionButtonPressed = 0;
            if (Input.GetKey(KeyCode.A))
                direction += Vector2.left;
            if (Input.GetKey(KeyCode.D))
                direction += Vector2.right;
            if (Input.GetKey(KeyCode.W))
                direction += Vector2.up;
            if (Input.GetKey(KeyCode.S))
                direction += Vector2.down;
            if (Input.GetKeyDown(KeyCode.Space))
                attackPressed = 1;
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
                mouseClicked = 1;
            if (Input.GetKeyDown(KeyCode.Alpha1))
                actionButtonPressed = 1;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                actionButtonPressed = 2;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                actionButtonPressed = 3;

            return new InputJob
            {
                Direction = direction,
                AttackPressed = attackPressed,
                MouseClicked = mouseClicked,
                MousePosition = Input.mousePosition,
                ActionButtonPressed = actionButtonPressed
            }.Schedule(this, inputDeps);
        }
    }
}