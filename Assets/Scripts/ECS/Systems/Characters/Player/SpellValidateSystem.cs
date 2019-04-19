using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Spells;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class SpellValidateSystem : JobComponentSystem
    {
        private struct SpellValidateJob : IJobChunk
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> ActiveSpellChunks;
            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;
            [ReadOnly]
            public ArchetypeChunkComponentType<TargetComponent> TargetComponentType;
            public ArchetypeChunkComponentType<InputComponent> InputComponentType;
            [ReadOnly]
            public ArchetypeChunkComponentType<ActiveSpellComponent> ActiveSpellComponentType;

            [ReadOnly]
            public float CurrentTime;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entities = chunk.GetNativeArray(this.EntityType);
                var inputComponents = chunk.GetNativeArray(this.InputComponentType);
                for (var i = 0; i < chunk.Count; i++)
                {
                    for (var sChunkIndex = 0; sChunkIndex < this.ActiveSpellChunks.Length; sChunkIndex++)
                    {
                        if (inputComponents[i].ActionButtonPressed == 0)
                            return;

                        var inputComponent = inputComponents[i];
                        var spellChunk = this.ActiveSpellChunks[sChunkIndex];
                        var spellEntities = spellChunk.GetNativeArray(this.EntityType);
                        var spellComponents = spellChunk.GetNativeArray(this.ActiveSpellComponentType);

                        for (var sI = 0; sI < spellChunk.Count; sI++)
                        {
                            if (spellComponents[sI].Owner == entities[i]
                                && inputComponent.ActionButtonPressed == spellComponents[sI].ActionIndex)
                            {
                                var spellPrefab = SpellBookManagerComponent.Instance.SpellBook.Spells[spellComponents[sI].SpellIndex];
                                if (spellPrefab.TargetRequired && !chunk.Has(this.TargetComponentType))
                                {
                                    inputComponent.ActionButtonPressed = 0;
                                    inputComponents[i] = inputComponent;
                                    return;
                                }

                                this.CommandBuffer.AddComponent(chunkIndex, entities[i], new SpellCastingComponent
                                {
                                    SpellIndex = spellComponents[sI].SpellIndex,
                                    ActiveSpell = spellEntities[sI],
                                    StartedAt = CurrentTime
                                });

                                inputComponent.ActionButtonPressed = 0;
                                inputComponents[i] = inputComponent;
                                return;
                            }
                        }
                        inputComponent.ActionButtonPressed = 0;
                        inputComponents[i] = inputComponent;
                    }
                }
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private ComponentGroup _playerCastGroup;
        private ComponentGroup _activeSpellGroup;

        protected override void OnCreateManager()
        {
            this._endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            this._playerCastGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(InputComponent), ComponentType.ReadOnly<CharacterComponent>()
                },
                None = new ComponentType[]
                {
                     ComponentType.ReadOnly<AttackComponent>(),
                     ComponentType.ReadOnly<SpellCastingComponent>()
                }
            });
            this._activeSpellGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    ComponentType.ReadOnly<ActiveSpellComponent>()
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<CoolDownComponent>(),
                    ComponentType.ReadOnly<InstantiateSpellComponent>()
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var validateSpellJobHandle = new SpellValidateJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                ActiveSpellChunks = this._activeSpellGroup.CreateArchetypeChunkArray(Allocator.TempJob),
                EntityType = this.GetArchetypeChunkEntityType(),
                InputComponentType = this.GetArchetypeChunkComponentType<InputComponent>(),
                TargetComponentType = this.GetArchetypeChunkComponentType<TargetComponent>(true),
                ActiveSpellComponentType = this.GetArchetypeChunkComponentType<ActiveSpellComponent>(true),
                CurrentTime = Time.time
            }.Schedule(this._playerCastGroup, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(validateSpellJobHandle);
            return validateSpellJobHandle;
        }
    }
}
