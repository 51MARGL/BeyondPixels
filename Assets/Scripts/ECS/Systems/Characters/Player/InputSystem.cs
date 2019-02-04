﻿using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class InputSystem : JobComponentSystem
    {        
        [BurstCompile]
        private struct InputJob : IJobProcessComponentData<InputComponent, MovementComponent>
        {
            public float2 Direction;
            public int AttackPressed;
            public int MouseClicked;
            public float3 MousePosition;
            public int ActionButtonPressed;

            public void Execute(ref InputComponent inputComponent, ref MovementComponent movementComponent)
            {
                movementComponent.Direction = Direction;
                inputComponent.InputDirection = Direction;

                inputComponent.AttackButtonPressed = AttackPressed;

                inputComponent.MouseButtonClicked = MouseClicked;
                inputComponent.MousePosition = MousePosition;

                if (ActionButtonPressed > 0)
                    inputComponent.ActionButtonPressed = ActionButtonPressed;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var direction = float2.zero;
            var attackPressed = 0;
            var mouseClicked = 0;
            var actionButtonPressed = 0;
            if (Input.GetKey(KeyCode.A))
                direction += new float2(-1, 0);
            if (Input.GetKey(KeyCode.D))
                direction += new float2(1, 0);
            if (Input.GetKey(KeyCode.W))
                direction += new float2(0, 1);
            if (Input.GetKey(KeyCode.S))
                direction += new float2(0, -1);
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