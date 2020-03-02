using Forgeage.Extensions;
using System;
using UnityEngine;

namespace Forgeage
{
	public class Action : IExecutable
    {
        public Action(Sprite UITexture, ActionDelegate action)
        {
            this.UITexture = UITexture;
            this.action = action;
        }

        public Sprite UITexture;
        public ActionDelegate action;

        public void Execute()
        {
            action.Invoke();
        }
    }
}
