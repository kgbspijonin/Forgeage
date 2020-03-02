using Forgeage;
using Forgeage.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Forgeage
{
    public class InputHandler : Singleton<InputHandler>
    {
        public KeyCode[] Keycodes { get; } = Enum.GetValues(typeof(KeyCode)) as KeyCode[];
        public Dictionary<KeyCode, List<KeyPressDelegate>> Keybinds { get; } = new Dictionary<KeyCode, List<KeyPressDelegate>>();
        public event EventHandler OnKeybindsPrepared;

        void Start()
        {
            OnKeybindsPrepared?.Invoke(this, null);
        }

        void Update()
        {
            foreach (var c in Keybinds.Keys)
            {
                List<KeyPressDelegate> actions;
                Keybinds.TryGetValue(c, out actions);
                if (Input.GetKeyDown(c))
                {
                    actions.ForEach(action => action(ActionStatus.PRESSED));
                }
                if (Input.GetKey(c))
                {
                    actions.ForEach(action => action(ActionStatus.HELD));
                }
                if (Input.GetKeyUp(c))
                {
                    actions.ForEach(action => action(ActionStatus.RELEASED));
                }
            }
        }
    }
}
