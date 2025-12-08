// <copyright file="ActionTask.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using CarlosLab.Common;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class ActionTask : Task
    {
        #region IModelWithId

        [DataMember(Name = nameof(Id))]
        private string id;

        public string Id => id;

        #endregion

        #region Properties

        private Transform transform;

        protected Transform Transform => transform;

        private GameObject gameObject;

        protected GameObject GameObject => gameObject;

        private UtilityAgentController agentController;

        protected UtilityAgentController AgentController => agentController;

        #endregion

        #region Lifecyle Functions

        internal sealed override void Awake()
        {
            if (Awakened)
                return;

            agentController = GetComponent<UtilityAgentController>();
            transform = GetComponent<Transform>();
            gameObject = Transform.gameObject;
            base.Awake();
        }

        internal void LateUpdate(float deltaTime)
        {
            if (IsRunning)
                OnLateUpdate(deltaTime);
        }

        protected virtual void OnLateUpdate(float deltaTime)
        {
        }

        internal void FixedUpdate(float deltaTime)
        {
            if (IsRunning)
                OnFixedUpdate(deltaTime);
        }

        protected virtual void OnFixedUpdate(float deltaTime)
        {
        }

        #endregion

        #region Context

        protected Transform TargetTransform { get; private set; }

        protected GameObject TargetGameObject { get; private set; }
        protected UtilityAgent TargetAgent { get; private set; }
        protected UtilityEntity TargetEntity { get; private set; }
        protected IEntityFacade TargetFacade { get; private set; }


        protected override void OnContextChanged(DecisionContext newContext)
        {
            TargetFacade = newContext.TargetFacade;
            TargetEntity = newContext.Target;
            TargetAgent = TargetEntity as UtilityAgent;

            TargetTransform = TargetEntity?.GetComponent<Transform>();
            TargetGameObject = TargetTransform?.gameObject;
        }

        #endregion

        #region Coroutine Functions

        protected void StartCoroutine(string methodName)
        {
            agentController.StartCoroutine(methodName);
        }

        protected Coroutine StartCoroutine(IEnumerator routine)
        {
            return agentController.StartCoroutine(routine);
        }

        protected Coroutine StartCoroutine(string methodName, object value)
        {
            return agentController.StartCoroutine(methodName, value);
        }

        protected void StopCoroutine(string methodName)
        {
            agentController.StopCoroutine(methodName);
        }

        protected void StopCoroutine(IEnumerator routine)
        {
            agentController.StopCoroutine(routine);
        }

        protected void StopAllCoroutines()
        {
            agentController.StopAllCoroutines();
        }

        #endregion

        #region Collision/Trigger 3D

        internal virtual void CollisionEnter(Collision collision)
        {
            if (IsRunning)
                OnCollisionEnter(collision);
        }

        internal virtual void CollisionStay(Collision collision)
        {
            if (IsRunning)
                OnCollisionStay(collision);
        }

        internal virtual void CollisionExit(Collision collision)
        {
            if (IsRunning)
                OnCollisionExit(collision);
        }

        internal virtual void ControllerColliderHit(ControllerColliderHit hit)
        {
            if (IsRunning)
                OnControllerColliderHit(hit);
        }

        internal virtual void TriggerEnter(Collider other)
        {
            if (IsRunning)
                OnTriggerEnter(other);
        }

        internal virtual void TriggerStay(Collider other)
        {
            if (IsRunning)
                OnTriggerStay(other);
        }

        internal virtual void TriggerExit(Collider other)
        {
            if (IsRunning)
                OnTriggerExit(other);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
        }

        protected virtual void OnCollisionStay(Collision collision)
        {
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
        }

        protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
        {
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
        }

        protected virtual void OnTriggerStay(Collider other)
        {
        }


        protected virtual void OnTriggerExit(Collider other)
        {
        }

        #endregion

        #region Collision/Trigger 2D

        internal virtual void CollisionEnter2D(Collision2D collision)
        {
            if (IsRunning)
                OnCollisionEnter2D(collision);
        }

        internal virtual void CollisionStay2D(Collision2D collision)
        {
            if (IsRunning)
                OnCollisionStay2D(collision);
        }

        internal virtual void CollisionExit2D(Collision2D collision)
        {
            if (IsRunning)
                OnCollisionExit2D(collision);
        }

        internal virtual void TriggerEnter2D(Collider2D collision)
        {
            if (IsRunning)
                OnTriggerEnter2D(collision);
        }

        internal virtual void TriggerStay2D(Collider2D collision)
        {
            if (IsRunning)
                OnTriggerStay2D(collision);
        }

        internal virtual void TriggerExit2D(Collider2D collision)
        {
            if (IsRunning)
                OnTriggerExit2D(collision);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
        }

        protected virtual void OnCollisionExit2D(Collision2D collision)
        {
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
        }

        protected virtual void OnTriggerStay2D(Collider2D collision)
        {
        }

        protected virtual void OnTriggerExit2D(Collider2D collision)
        {
        }

        #endregion

        #region Animations

        internal virtual void AnimatorMove()
        {
            if (IsRunning)
                OnAnimatorMove();
        }

        internal virtual void AnimatorIK(int layerIndex)
        {
            if (IsRunning)
                OnAnimatorIK(layerIndex);
        }

        protected virtual void OnAnimatorMove()
        {
        }

        protected virtual void OnAnimatorIK(int layerIndex)
        {
        }

        #endregion
    }
}