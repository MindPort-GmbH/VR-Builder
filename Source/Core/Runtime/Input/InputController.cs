// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

#if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_PACKAGE
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using VRBuilder.Core.Registry;
using VRBuilder.Unity;

namespace VRBuilder.Core.Input
{
    /// <summary>
    /// Central controller for input via the new Input System using C# events.
    /// </summary>
    public abstract class InputController : UnitySceneSingleton<InputController>, IInputController
    {
        public class InputEventArgs : EventArgs
        {
            public readonly object Context;

            public InputEventArgs(object context)
            {
                Context = context;
            }
        }

        public class InputFocusEventArgs : EventArgs
        {
            public readonly IInputFocus InputFocus;

            public InputFocusEventArgs(IInputFocus inputFocus)
            {
                InputFocus = inputFocus;
            }
        }

        /// <summary>
        /// Information of the listener registered.
        /// </summary>
        protected struct ListenerInfo
        {
            public readonly IInputActionListener ActionListener;
            public readonly Action<InputEventArgs> Action;

            public ListenerInfo(IInputActionListener actionListener, Action<InputEventArgs> action)
            {
                ActionListener = actionListener;
                Action = action;
            }
        }

        /// <summary>
        /// Will be called when an object is focused.
        /// </summary>
        public EventHandler<InputFocusEventArgs> OnFocused;

        /// <summary>
        /// Will be called when the focus on an object is released.
        /// </summary>
        public EventHandler<InputFocusEventArgs> OnFocusReleased;

        /// <summary>
        /// Currently focused object.
        /// </summary>
        protected IInputFocus CurrentInputFocus { get; set; } = null;

        /// <summary>
        /// Registered listener.
        /// </summary>
        protected Dictionary<string, List<ListenerInfo>> ListenerDictionary { get; } = new Dictionary<string, List<ListenerInfo>>();

        /// <summary>
        /// Registers an action event to input.
        /// </summary>
        /// <param name="listener">The listener owning the action.</param>
        /// <param name="action">The action method which will be called.</param>
        public void RegisterEvent(IInputActionListener listener, Action<InputEventArgs> action)
        {
            string actionName = action.Method.Name;
            if (ListenerDictionary.ContainsKey(actionName) == false)
            {
                ListenerDictionary.Add(actionName, new List<ListenerInfo>());
            }

            List<ListenerInfo> infoList = ListenerDictionary[actionName];

            infoList.Add(new ListenerInfo(listener, action));
            infoList.Sort((l1, l2) => l1.ActionListener.Priority.CompareTo(l2.ActionListener.Priority) * -1);
        }

        /// <summary>
        /// Unregisters the given listeners action.
        /// </summary>
        public void UnregisterEvent(IInputActionListener listener, Action<InputEventArgs> action)
        {
            string actionName = action.Method.Name;
            List<ListenerInfo> infoList = ListenerDictionary[actionName];
            infoList.RemoveAll(info => info.ActionListener == listener && info.Action.Method.Name == actionName);
        }

        /// <summary>
        /// Focus the given input focus target.
        /// </summary>
        public abstract void Focus(IInputFocus target);

        /// <summary>
        /// Releases the focus, if possible.
        /// </summary>
        public abstract void ReleaseFocus();


        protected override void Awake()
        {
            base.Awake();
            Setup();
        }

        protected virtual void Reset()
        {
            Setup();
        }

        /// <summary>
        /// will be called on Reset (in editor time) and Awake (in play mode).
        /// Intended to setup the input controller properly.
        /// </summary>
        protected abstract void Setup();

        public void SetConfiguration(IInputConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Initialize()
        {
        }

        private InputActionAsset inputActionAsset;
        private IInputConfiguration configuration;

        /// <summary>
        /// Current active InputActionAsset.
        /// </summary>
        public virtual InputActionAsset CurrentInputActionAsset
        {
            get
            {
                if (inputActionAsset == null)
                {
                    inputActionAsset = Resources.Load<InputActionAsset>(configuration.CustomInputActionAssetPath);
                    if (inputActionAsset == null)
                    {
                        inputActionAsset = Resources.Load<InputActionAsset>(configuration.DefaultInputActionAssetPath);
                    }
                }

                return inputActionAsset;
            }

            set => inputActionAsset = value;
        }

        public void SetupInputActions()
        {
#if UNITY_EDITOR
            var defaultBindings = Resources.Load<InputActionAsset>(configuration.DefaultInputActionAssetPath);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(defaultBindings),
                $"Assets/MindPort/VR Builder/Resources/{configuration.CustomInputActionAssetPath}.inputactions");
#endif
        }

        public void LoadInputActions()
        {
            CurrentInputActionAsset = Resources.Load<InputActionAsset>(configuration.CustomInputActionAssetPath);
        }

        public bool UsesCustomKeyBindingAsset()
        {
#if UNITY_EDITOR
            return AssetDatabase.GetAssetPath(CurrentInputActionAsset)
                .Equals("Assets/MindPort/VR Builder/Resources" + configuration.CustomInputActionAssetPath);
#endif
        }
    }
}
#endif