// <copyright file="UtilityEntityRegister.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class UtilityEntityRegister : MonoBehaviour
    {
        [SerializeField]
        private UtilityWorldController world;

        public UtilityWorldController World => world;

        [SerializeField]
        private List<UtilityAgentController> agents = new();

        public List<UtilityAgentController> Agents => agents;

        [SerializeField]
        private List<UtilityEntityController> entities = new();

        public List<UtilityEntityController> Entities => entities;

        private void Start()
        {
            // 启动时统一注册
            foreach (UtilityAgentController agent in agents)
                RegisterAgent(agent);

            foreach (UtilityEntityController entity in entities)
                RegisterEntity(entity);
        }

        // ✅ 注册智能体
        public void RegisterAgent(UtilityAgentController agent)
        {
            if (agent == null || world == null) return;

            if (!agents.Contains(agent))
                agents.Add(agent);

            agent.Register(world);
        }

        // ✅ 取消注册智能体
        public void UnregisterAgent(UtilityAgentController agent)
        {
            if (agent == null || world == null) return;

            if (agents.Contains(agent))
                agents.Remove(agent);

            agent.Unregister();
        }

        // ✅ 注册实体
        public void RegisterEntity(UtilityEntityController entity)
        {
            if (entity == null || world == null) return;

            if (!entities.Contains(entity))
                entities.Add(entity);

            entity.Register(world);
        }

        // ✅ 取消注册实体
        public void UnregisterEntity(UtilityEntityController entity)
        {
            if (entity == null || world == null) return;

            if (entities.Contains(entity))
                entities.Remove(entity);

            entity.Unregister();
        }
    }
}
