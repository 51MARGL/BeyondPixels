using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Spells;

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
            [ReadOnly]
            public ArchetypeChunkComponentType<MagicStatComponent> MagicStatComponentType;
            public ArchetypeChunkComponentType<InputComponent> InputComponentType;
            [ReadOnly]
            public ArchetypeChunkComponentType<SpellCastingComponent> SpellCastingComponentType;
            [ReadOnly]
            public ArchetypeChunkComponentType<ActiveSpellComponent> ActiveSpellComponentType;

            [ReadOnly]
            public float CurrentTime;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entities = chunk.GetNativeArray(this.EntityType);
                var inputComponents = chunk.GetNativeArray(this.InputComponentType);
                var spellCastingComponents = chunk.GetNativeArray(this.SpellCastingComponentType);
                var targetComponents = chunk.GetNativeArray(this.TargetComponentType);
                var magicStatComponents = chunk.GetNativeArray(this.MagicStatComponentType);

                for (var i = 0; i < chunk.Count; i++)
                {
                    if (inputComponents[i].AttackButtonPressed == 1
                        || !inputComponents[i].InputDirection.Equals(float2.zero))
                    {
                        this.CommandBuffer.RemoveComponent<SpellCastingComponent>(chunkIndex, entities[i]);
                        return;
                    }
                    for (var sChunkIndex = 0; sChunkIndex < this.ActiveSpellChunks.Length; sChunkIndex++)
                    {
                        var spellChunk = this.ActiveSpellChunks[sChunkIndex];
                        var spellEntities = spellChunk.GetNativeArray(this.EntityType);
                        var spellComponents = spellChunk.GetNativeArray(this.ActiveSpellComponentType);
                        for (var sI = 0; sI < spellChunk.Count; sI++)
                        {
                            if (spellEntities[sI] == spellCastingComponents[i].ActiveSpell)
                            {
                                var spellPrefab = SpellBookManagerComponent.Instance.SpellBook.Spells[spellComponents[sI].SpellIndex];
                                if (spellPrefab.TargetRequired && !chunk.Has(this.TargetComponentType))
                                {
                                    this.CommandBuffer.RemoveComponent<SpellCastingComponent>(chunkIndex, entities[i]);
                                    return;
                                }

                                var magicStatComponent = magicStatComponents[i];
                                var castTime = math.max(1f, spellPrefab.CastTime -
                                                            (spellPrefab.CastTime / 100f * magicStatComponent.CurrentValue));

                                if (spellCastingComponents[i].StartedAt + castTime > this.CurrentTime)
                                    return;

                                var target = Entity.Null;
                                if (spellPrefab.SelfTarget)
                                    target = entities[i];
                                else if (spellPrefab.TargetRequired)
                                    target = targetComponents[i].Target;

                                var coolDown = math.max(1f, spellPrefab.CoolDown -
                                                            (spellPrefab.CoolDown / 100f * magicStatComponent.CurrentValue));

                                this.CommandBuffer.RemoveComponent<SpellCastingComponent>(chunkIndex, entities[i]);
                                this.CommandBuffer.AddComponent(chunkIndex, spellEntities[sI], new CoolDownComponent
                                {
                                    CoolDownTime = coolDown
                                });
                                this.CommandBuffer.AddComponent(chunkIndex, spellEntities[sI], new InstantiateSpellComponent
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
            this._endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            this._playerCastingGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(InputComponent),
                    ComponentType.ReadOnly<MagicStatComponent>(),
                    ComponentType.ReadOnly<CharacterComponent>(),
                    ComponentType.ReadOnly<SpellCastingComponent>()
                },
                None = new ComponentType[]
                {
                     ComponentType.ReadOnly<AttackComponent>()
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
            var spellCastingJobHandle = new SpellCastingJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                ActiveSpellChunks = this._activeSpellGroup.CreateArchetypeChunkArray(Allocator.TempJob),
                EntityType = this.GetArchetypeChunkEntityType(),
                InputComponentType = this.GetArchetypeChunkComponentType<InputComponent>(),
                MagicStatComponentType = this.GetArchetypeChunkComponentType<MagicStatComponent>(true),
                SpellCastingComponentType = this.GetArchetypeChunkComponentType<SpellCastingComponent>(true),
                TargetComponentType = this.GetArchetypeChunkComponentType<TargetComponent>(true),
                ActiveSpellComponentType = this.GetArchetypeChunkComponentType<ActiveSpellComponent>(true),
                CurrentTime = Time.time
            }.Schedule(this._playerCastingGroup, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(spellCastingJobHandle);
            return spellCastingJobHandle;
        }
    }
}
