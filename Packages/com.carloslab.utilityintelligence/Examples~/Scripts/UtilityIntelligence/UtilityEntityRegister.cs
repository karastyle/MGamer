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
        private List<UtilityAgentController> agents;

        public List<UtilityAgentController> Agents => agents;


        [SerializeField]
        private List<UtilityEntityController> entities;

        public List<UtilityEntityController> Entities => entities;

        private void Start()
        {
            foreach (UtilityAgentController agent in agents)
            {
                agent.Register(world);
            }

            foreach (UtilityEntityController entity in entities)
            {
                entity.Register(world);
            }
        }
    }
}