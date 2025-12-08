// <copyright file="EntityLifecycleExample.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using System.Collections.Generic;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class EntityLifecycleExample : MonoBehaviour
    {
        [SerializeField]
        private List<UtilityAgentController> agents;

        [SerializeField]
        private UtilityWorldController world;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                var agentController = agents[0];
                bool enabled = agentController.IsEnabled;
                enabled = !enabled;
                if (enabled)
                    agentController.SetEnableImmediate(enabled); // Use EntityController.SetEnableImmediate outside of action tasks, this will run immediately without queueing
                else
                    agentController.SetEnable(enabled); // Use EntityController.SetEnable within action tasks; this will be queued to run after all action tasks have executed.
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.A))
            {
                var agentController = agents[0];
                bool active = agentController.IsActive;

                active = !active;

                if (active)
                    agentController.gameObject.SetActive(active); // Use GameObject.SetActive() outside of action tasks, this will run immediately without queueing
                else
                    agentController.SetActive(active); // Use EntityController.SetActive() within action tasks; this will be queued to run after all action tasks have executed.
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                var agentController = agents[0];
                bool registered = agentController.IsRegistered;
                registered = !registered;
                if (registered)
                    agentController.RegisterImmediate(world); // Use EntityController.RegisterImmediate()/UnregisterImmediate() outside of action tasks, this will run immediately without queueing
                else
                    agentController.Unregister(); // Use EntityController.Register()/Unregister() within action tasks; this will be queued to run after all action tasks have executed.
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.D))
            {
                var agentController = agents[0];
                Destroy(agentController.gameObject); // Use GameObject.Destroy() outside of action tasks, this will run immediately without queueing
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.W))
            {
                var agentController = agents[0];
                agentController.Destroy(); // Use EntityController.Destroy() within action tasks; this will be queued to run after all action tasks have executed.
            }
        }
    }
}