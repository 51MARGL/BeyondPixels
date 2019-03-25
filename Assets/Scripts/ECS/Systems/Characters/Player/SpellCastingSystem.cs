using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.SceneBootstraps;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class SpellCastingSystem : JobComponentSystem
    {
        private struct SpellCastingJob : IJobChunk
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
            public ArchetypeChunkComponentType<SpellCastingComponent> SpellCastingComponentType;
            [ReadOnly]
            public ArchetypeChunkComponentType<ActiveSpellComponent> ActiveSpellComponentType;

            [ReadOnly]
            public float CurrentTime;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entities = chunk.GetNativeArray(EntityType);
                var inputComponents = chunk.GetNativeArray(InputComponentType);
                var spellCastingComponents = chunk.GetNativeArray(SpellCastingComponentType);
                var targetComponents = chunk.GetNativeArray(TargetComponentType);

                for (int i = 0; i < chunk.Count; i++)
                {
                    if (inputComponents[i].AttackButtonPressed == 1
                        || !inputComponents[i].InputDirection.Equals(float2.zero))
                    {
                        CommandBuffer.RemoveComponent<SpellCastingComponent>(chunkIndex, entities[i]);
                        return;
                    }
                    for (int sChunkIndex = 0; sChunkIndex < ActiveSpellChunks.Length; sChunkIndex++)
                    {
                        var spellChunk = ActiveSpellChunks[sChunkIndex];
                        var spellEntities = spellChunk.GetNativeArray(EntityType);
                        var spellComponents = spellChunk.GetNativeArray(ActiveSpellComponentType);
                        for (int sI = 0; sI < spellChunk.Count; sI++)
                        {
                            if (spellEntities[sI] == spellCastingComponents[i].ActiveSpell)
                            {
                                var spellPrefab = SpellBookManagerComponent.Instance.SpellBook.Spells[spellComponents[sI].SpellIndex];
                                if (spellPrefab.TargetRequired && !chunk.Has(TargetComponentType))
                                {
                                    CommandBuffer.RemoveComponent<SpellCastingComponent>(chunkIndex, entities[i]);
                                    return;
                                }

                                if (spellCastingComponents[i].StartedAt + spellPrefab.CastTime > CurrentTime)
                                    return;

                                var target = Entity.Null;
                                if (spellPrefab.SelfTarget)
                                    target = entities[i];
                                else if (spellPrefab.TargetRequired)
                                    target = targetComponents[i].Target;

                                CommandBuffer.RemoveComponent<SpellCastingComponent>(chunkIndex, entities[i]);
                                CommandBuffer.AddComponent(chunkIndex, spellEntities[sI], new CoolDownComponent
                                {
                                    CoolDownTime = spellPrefab.CoolDown
                                });
                                CommandBuffer.AddComponent(chunkIndex, spellEntities[sI], new InstantiateSpellComponent
                                {
                                    Caster = entities[i],
                                    Target = target
                                });

                                return;
                            }
                        }
                    }
                }
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private ComponentGroup _playerCastingGroup;
        private ComponentGroup _activeSpellGroup;

        protected override void OnCreateManager()
        {
            _endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            _playerCastingGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(InputComponent),
                    ComponentType.ReadOnly<CharacterComponent>(),
                    ComponentType.ReadOnly<SpellCastingComponent>()
                },
                None = new ComponentType[]
                {
                     ComponentType.ReadOnly<AttackComponent>()
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
            var spellCastingJobHandle = new SpellCastingJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                ActiveSpellChunks = _activeSpellGroup.CreateArchetypeChunkArray(Allocator.TempJob),
                EntityType = GetArchetypeChunkEntityType(),
                InputComponentType = GetArchetypeChunkComponentType<InputComponent>(),
                SpellCastingComponentType = GetArchetypeChunkComponentType<SpellCastingComponent>(true),
                TargetComponentType = GetArchetypeChunkComponentType<TargetComponent>(true),
                ActiveSpellComponentType = GetArchetypeChunkComponentType<ActiveSpellComponent>(true),
                CurrentTime = Time.time
            }.Schedule(_playerCastingGroup, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(spellCastingJobHandle);
            return spellCastingJobHandle;
        }
    }
}
