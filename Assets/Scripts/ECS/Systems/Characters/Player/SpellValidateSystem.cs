using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.SceneBootstraps;
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
                var entities = chunk.GetNativeArray(EntityType);
                var inputComponents = chunk.GetNativeArray(InputComponentType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    for (int sChunkIndex = 0; sChunkIndex < ActiveSpellChunks.Length; sChunkIndex++)
                    {
                        if (inputComponents[i].ActionButtonPressed == 0)
                            return;

                        var inputComponent = inputComponents[i];
                        var spellChunk = ActiveSpellChunks[sChunkIndex];
                        var spellEntities = spellChunk.GetNativeArray(EntityType);
                        var spellComponents = spellChunk.GetNativeArray(ActiveSpellComponentType);

                        for (int sI = 0; sI < spellChunk.Count; sI++)
                        {
                            if (spellComponents[sI].Owner == entities[i]
                                && inputComponent.ActionButtonPressed == spellComponents[sI].ActionIndex)
                            {
                                var spellPrefab = DungeonBootstrap.spellBook.Spells[spellComponents[sI].SpellIndex];
                                if (spellPrefab.TargetRequired && !chunk.Has(TargetComponentType))
                                {
                                    inputComponent.ActionButtonPressed = 0;
                                    inputComponents[i] = inputComponent;
                                    return;
                                }

                                CommandBuffer.AddComponent(chunkIndex, entities[i], new SpellCastingComponent
                                {
                                    SpellIndex = spellComponents[sI].SpellIndex,
                                    ActiveSpell = spellEntities[sI],
                                    StartedAt = CurrentTime
                                });

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
            _endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            _playerCastGroup = GetComponentGroup(new EntityArchetypeQuery
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
            _activeSpellGroup = GetComponentGroup(new EntityArchetypeQuery
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
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                ActiveSpellChunks = _activeSpellGroup.CreateArchetypeChunkArray(Allocator.TempJob),
                EntityType = GetArchetypeChunkEntityType(),
                InputComponentType = GetArchetypeChunkComponentType<InputComponent>(),
                TargetComponentType = GetArchetypeChunkComponentType<TargetComponent>(true),
                ActiveSpellComponentType = GetArchetypeChunkComponentType<ActiveSpellComponent>(true),
                CurrentTime = Time.time
            }.Schedule(_playerCastGroup, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(validateSpellJobHandle);
            return validateSpellJobHandle;
        }
    }
}
