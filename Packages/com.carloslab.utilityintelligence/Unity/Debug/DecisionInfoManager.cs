// <copyright file="DecisionInfoManager.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence
{
    public class DecisionInfoManager : MonoBehaviour
    {
        [SerializeField]
        private bool showDecisionInfo;

        [SerializeField]
        private DecisionInfo decisionInfoPrefab;

        private DecisionInfo decisionInfo;

        private Transform worldCanvas;

        private UtilityAgentController agentController;

        private void Awake()
        {
            worldCanvas = GameObject.Find("WorldCanvas").transform;
            agentController = GetComponentInParent<UtilityAgentController>();
        }

        private void Start()
        {
            decisionInfo = Instantiate(decisionInfoPrefab, worldCanvas, false);
            decisionInfo.Init(transform);
            decisionInfo.Show(showDecisionInfo);
        }

        private void OnEnable()
        {
            agentController.Intelligence.DecisionChanged += Intelligence_OnDecisionChanged;
        }

        private void OnDisable()
        {
            agentController.Intelligence.DecisionChanged -= Intelligence_OnDecisionChanged;
        }

        private void OnDestroy()
        {
            if (decisionInfo != null)
                Destroy(decisionInfo.gameObject);
        }

        private void OnValidate()
        {
            if (decisionInfo == null) return;

            decisionInfo.Show(showDecisionInfo);
        }

        private void Intelligence_OnDecisionChanged(Decision decision)
        {
            decisionInfo.UpdateDecision(decision);
        }
    }
}
