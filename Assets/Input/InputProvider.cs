using System;
using System.Collections.Generic;
using Keres.Core;
using Keres.Input.Core;
using UnityEngine;

namespace Keres.Input
{
    public enum InputMode : byte
    {
        Gameplay = 0,
        Menu = 1,
        Cutscene = 2,
        Dialogue = 3,
    }

    /// <summary>
    /// L1 - Semantic input facade.
    ///
    /// Pipeline (no .inputactions asset anywhere):
    ///
    ///     InputSystem.onEvent (low-level device events)
    ///             ↓
    ///     InputSystemRawProvider  (Core)
    ///             ↓
    ///     InputQueue → InputState (Core)
    ///             ↓
    ///     InputProvider (this) — semantic API + mode stack + CommandBuffer
    ///
    /// Bindings are plain serializable KeyBindings, rebindable in the
    /// Inspector without any generated code or asset.
    /// </summary>
    public sealed class InputProvider : MonoBehaviour
    {
        // ============================================================
        // SEMANTIC NAMES
        // ============================================================

        public const string MoveUp = "MoveUp";
        public const string MoveDown = "MoveDown";
        public const string MoveLeft = "MoveLeft";
        public const string MoveRight = "MoveRight";
        public const string Attack = "Attack";
        public const string Dodge = "Dodge";
        public const string Jump = "Jump";
        public const string Interact = "Interact";

        // ============================================================
        // SERIALIZATION
        // ============================================================

        [Header("Bindings (no .inputactions needed)")]
        [SerializeField]
        private KeyBinding[] bindings = DefaultBindings();

        [Header("Look")]
        [Tooltip("Raw mouse deltas are device counts; scale them here.")]
        [SerializeField]
        private float lookSensitivity = 0.01f;

        // ============================================================
        // RUNTIME
        // ============================================================

        [Serializable]
        public struct KeyBinding
        {
            public string Name;
            public InputControl Control;
        }

        private readonly Stack<InputMode> modeStack = new();
        private readonly CommandBuffer commandBuffer = new();
        private readonly InputManager inputManager = new();

        private Dictionary<string, InputControl> controlMap;
        private bool initialized;

        // ============================================================
        // STATE
        // ============================================================

        public InputMode Mode => modeStack.Count > 0 ? modeStack.Peek() : InputMode.Gameplay;

        public bool Active => initialized && Mode == InputMode.Gameplay;

        public bool IsInitialized => initialized;

        public InputState State => inputManager.State;

        // ============================================================
        // LIFECYCLE
        // ============================================================

        private void Awake()
        {
            ServiceLocator.Register(this);

            if (!BuildControlMap())
            {
                enabled = false;
                return;
            }

            inputManager.SetProvider(new InputSystemRawProvider());

            initialized = true;
        }

        private void OnDestroy()
        {
            inputManager.Shutdown();

            if (ReferenceEquals(ServiceLocator.Get<InputProvider>(), this))
                ServiceLocator.Unregister<InputProvider>();
        }

        private void Update()
        {
            if (!initialized)
                return;

            inputManager.BeginFrame();

            FeedCommands();

            commandBuffer.Tick();
        }

        // ============================================================
        // BINDINGS
        // ============================================================

        private bool BuildControlMap()
        {
            controlMap = new Dictionary<string, InputControl>(bindings.Length);

            foreach (KeyBinding binding in bindings)
            {
                if (string.IsNullOrEmpty(binding.Name))
                    continue;

                controlMap[binding.Name] = binding.Control;
            }

            if (
                !controlMap.ContainsKey(MoveUp)
                || !controlMap.ContainsKey(MoveDown)
                || !controlMap.ContainsKey(MoveLeft)
                || !controlMap.ContainsKey(MoveRight)
            )
            {
                Debug.LogError(
                    $"{nameof(InputProvider)} requires {MoveUp}/{MoveDown}/{MoveLeft}/{MoveRight} bindings.",
                    this
                );

                return false;
            }

            return true;
        }

        private bool TryGetControl(string action, out InputControl control)
        {
            if (controlMap == null)
            {
                control = InputControl.None;
                return false;
            }

            return controlMap.TryGetValue(action, out control);
        }

        private static KeyBinding[] DefaultBindings()
        {
            return new[]
            {
                new KeyBinding { Name = MoveUp, Control = InputControl.KeyW },
                new KeyBinding { Name = MoveDown, Control = InputControl.KeyS },
                new KeyBinding { Name = MoveLeft, Control = InputControl.KeyA },
                new KeyBinding { Name = MoveRight, Control = InputControl.KeyD },
                new KeyBinding { Name = Attack, Control = InputControl.MouseLeft },
                new KeyBinding { Name = Dodge, Control = InputControl.KeyLeftShift },
                new KeyBinding { Name = Jump, Control = InputControl.KeySpace },
                new KeyBinding { Name = Interact, Control = InputControl.KeyE },
            };
        }

        // ============================================================
        // SEMANTIC STATE
        // ============================================================

        public Vector2 GetMove()
        {
            if (!Active)
                return Vector2.zero;

            Vector2 move = Vector2.zero;

            if (inputManager.State.Held(controlMap[MoveUp]))
                move.y += 1f;
            if (inputManager.State.Held(controlMap[MoveDown]))
                move.y -= 1f;
            if (inputManager.State.Held(controlMap[MoveLeft]))
                move.x -= 1f;
            if (inputManager.State.Held(controlMap[MoveRight]))
                move.x += 1f;

            return move;
        }

        public Vector2 GetLook()
        {
            if (!Active)
                return Vector2.zero;

            return new Vector2(inputManager.State.DeltaX, inputManager.State.DeltaY)
                * lookSensitivity;
        }

        public bool Pressed(string action)
        {
            if (!Active || !TryGetControl(action, out InputControl control))
                return false;

            return inputManager.State.Pressed(control);
        }

        public bool Held(string action)
        {
            if (!Active || !TryGetControl(action, out InputControl control))
                return false;

            return inputManager.State.Held(control);
        }

        public bool Released(string action)
        {
            if (!Active || !TryGetControl(action, out InputControl control))
                return false;

            return inputManager.State.Released(control);
        }

        // ============================================================
        // COMMAND PIPELINE
        // ============================================================

        public void FeedCommand(string action)
        {
            if (Pressed(action))
                commandBuffer.Feed(action);
        }

        private void FeedCommands()
        {
            FeedCommand(Attack);
            FeedCommand(Dodge);
            FeedCommand(Jump);
            FeedCommand(Interact);
        }

        public bool GetCommand(string action)
        {
            if (!Active)
                return false;

            return commandBuffer.Consume(action);
        }

        // ============================================================
        // MODE / CONTEXT STACK
        // ============================================================

        public void PushMode(InputMode mode)
        {
            modeStack.Push(mode);
        }

        public bool TryPopMode()
        {
            if (modeStack.Count == 0)
                return false;

            modeStack.Pop();

            return true;
        }

        public void PopMode()
        {
            if (!TryPopMode())
                throw new InvalidOperationException(
                    "Cannot PopMode(): the input mode stack is empty."
                );
        }

        public void ClearModes()
        {
            modeStack.Clear();
        }

        /// <summary>
        /// Pushes a mode and returns a disposable scope.
        /// The previous mode is restored automatically on dispose.
        /// </summary>
        public IDisposable PushScope(InputMode mode)
        {
            PushMode(mode);

            return new InputModeScope(this);
        }

        private sealed class InputModeScope : IDisposable
        {
            private InputProvider owner;
            private bool disposed;

            public InputModeScope(InputProvider owner)
            {
                this.owner = owner;
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;

                if (owner != null)
                {
                    owner.TryPopMode();
                    owner = null;
                }
            }
        }

        // ============================================================
        // DIAGNOSTICS
        // ============================================================

        public InputDiagnostics GetDiagnostics()
        {
            Keres.Input.Core.Diagnostics d = inputManager.Diagnostics;

            return new InputDiagnostics(
                initialized,
                Mode,
                d.Frames,
                d.EventsEnqueued,
                d.EventsDequeued,
                d.EventsDropped,
                d.MonotonicViolations,
                d.QueueDepthMax,
                d.AvgCaptureConsumeMs,
                d.MaxCaptureConsumeMs
            );
        }

        public readonly struct InputDiagnostics
        {
            public readonly bool Initialized;
            public readonly InputMode Mode;

            public readonly long Frames;
            public readonly long EventsEnqueued;
            public readonly long EventsDequeued;
            public readonly long EventsDropped;
            public readonly long MonotonicViolations;
            public readonly int QueueDepthMax;

            public readonly double AvgCaptureConsumeMs;
            public readonly double MaxCaptureConsumeMs;

            public InputDiagnostics(
                bool initialized,
                InputMode mode,
                long frames,
                long eventsEnqueued,
                long eventsDequeued,
                long eventsDropped,
                long monotonicViolations,
                int queueDepthMax,
                double avgCaptureConsumeMs,
                double maxCaptureConsumeMs
            )
            {
                Initialized = initialized;
                Mode = mode;
                Frames = frames;
                EventsEnqueued = eventsEnqueued;
                EventsDequeued = eventsDequeued;
                EventsDropped = eventsDropped;
                MonotonicViolations = monotonicViolations;
                QueueDepthMax = queueDepthMax;
                AvgCaptureConsumeMs = avgCaptureConsumeMs;
                MaxCaptureConsumeMs = maxCaptureConsumeMs;
            }
        }
    }
}
