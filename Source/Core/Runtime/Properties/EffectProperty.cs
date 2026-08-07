// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.User;
using VRBuilder.Core.Utils.ParticleMachines;

namespace VRBuilder.Core.Properties
{
    //TODO: btw this was broken before. we have a list of confettiMachines, but only one is used
    public class EffectProperty : ProcessSceneObjectProperty, IEffectProperty
    {
        private GameObject confettiPrefab;
        private List<GameObject> confettiMachines = new List<GameObject>();
        private IParticleMachine particleMachine;
        public event Action ConfettiMachineCreatedAction;
        public int Count => confettiMachines.Count;

        public bool LoadConfettiMachinePrefab(string confettiMachinePrefabPath)
        {
            return confettiPrefab = Resources.Load<GameObject>(confettiMachinePrefabPath);
        }

        public void ClearConfettiMachines()
        {
            foreach (GameObject confettiMachine in confettiMachines)
            {
                Destroy(confettiMachine);
            }

            confettiMachines.Clear();
        }

        public void ActivateConfettiMachine(float dataAreaRadius, float dataDuration) => particleMachine.Activate(dataAreaRadius, dataDuration);

        public void CreateConfettiMachine()
        {
            CreateConfettiMachine(transform.position);
        }

        public void CreateConfettiMachine(Vector3 spawnPosition)
        {
            GameObject instantiatedPrefab = Instantiate(confettiPrefab, spawnPosition, Quaternion.Euler(90, 0, 0));
            OnConfettiMachineCreated(instantiatedPrefab);
        }

        private void OnConfettiMachineCreated(GameObject confettiMachine)
        {
            if (confettiMachine == null)
            {
                ForwardingLogger.LogWarning("The provided prefab is missing.");
                return;
            }

            if (confettiMachine.GetComponent(typeof(IParticleMachine)) == null)
            {
                ForwardingLogger.LogWarning("The provided prefab does not have any component of type \"IParticleMachine\".");
                return;
            }

            confettiMachines.Add(confettiMachine);

            // Change the settings and activate the machine
            particleMachine = confettiMachine.GetComponent<IParticleMachine>();

            ConfettiMachineCreatedAction?.Invoke();
        }

        public void CreateConfettiMachineAboveUser(float distanceAboveUser = 0f)
        {
            var userObj = ServiceRegistry.Get<IUserService>()?.User as UserSceneObject;
            if (userObj == null) return;

            Camera cam = userObj.GetComponentInChildren<Camera>();
            if (cam == null) return;

            var spawnPosition = cam.transform.position;
            spawnPosition.y += distanceAboveUser;
            CreateConfettiMachine(spawnPosition);
        }
    }
}