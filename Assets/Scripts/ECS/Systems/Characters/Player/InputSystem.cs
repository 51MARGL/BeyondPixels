using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Game;

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
        [ExcludeComponent(typeof(InCutsceneComponent))]
        private struct InputJob : IJobForEach<InputComponent, MovementComponent>
        {
            public float2 Direction;
            public int AttackPressed;
            public int MouseClicked;
            public float3 MousePosition;
            public int ActionButtonPressed;
            public int SelectTargetButtonPressed;

            public void Execute(ref InputComponent inputComponent, ref MovementComponent movementComponent)
            {
                movementComponent.Direction = this.Direction;
                inputComponent.InputDirection = this.Direction;

                inputComponent.AttackButtonPressed = this.AttackPressed;

                inputComponent.MouseButtonClicked = this.MouseClicked;
                inputComponent.MousePosition = this.MousePosition;
                inputComponent.SelectTargetButtonPressed = this.SelectTargetButtonPressed;

                if (this.ActionButtonPressed > 0)
                {
                    inputComponent.ActionButtonPressed = this.ActionButtonPressed;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (Time.timeScale == 0f)
            {
                return inputDeps;
            }

            var direction = float2.zero;
            var attackPressed = 0;
            var mouseClicked = 0;
            var actionButtonPressed = 0;
            var selectTargetButtonPressed = 0;

            if (Input.GetKey(SettingsManager.Instance.GetKeyBindValue(KeyBindName.Left)))
            {
                direction += new float2(-1, 0);
            }

            if (Input.GetKey(SettingsManager.Instance.GetKeyBindValue(KeyBindName.Right)))
            {
                direction += new float2(1, 0);
            }

            if (Input.GetKey(SettingsManager.Instance.GetKeyBindValue(KeyBindName.Up)))
            {
                direction += new float2(0, 1);
            }

            if (Input.GetKey(SettingsManager.Instance.GetKeyBindValue(KeyBindName.Down)))
            {
                direction += new float2(0, -1);
            }

            if (Input.GetKeyDown(SettingsManager.Instance.GetKeyBindValue(KeyBindName.Attack)))
            {
                attackPressed = 1;
            }

            if (Input.GetMouseButtonDown(0))
            {
                mouseClicked = 1;
            }

            if (Input.GetKeyDown(SettingsManager.Instance.GetKeyBindValue(KeyBindName.Action1)))
            {
                actionButtonPressed = 1;
            }

            if (Input.GetKeyDown(SettingsManager.Instance.GetKeyBindValue(KeyBindName.Action2)))
            {
                actionButtonPressed = 2;
            }

            if (Input.GetKeyDown(SettingsManager.Instance.GetKeyBindValue(KeyBindName.Action3)))
            {
                actionButtonPressed = 3;
            }

            if (Input.GetKeyDown(SettingsManager.Instance.GetKeyBindValue(KeyBindName.PickTarget)))
            {
                selectTargetButtonPressed = 1;
            }

            return new InputJob
            {
                Direction = direction,
                AttackPressed = attackPressed,
                MouseClicked = mouseClicked,
                MousePosition = Input.mousePosition,
                ActionButtonPressed = actionButtonPressed,
                SelectTargetButtonPressed = selectTargetButtonPressed
            }.Schedule(this, inputDeps);
        }
    }
}