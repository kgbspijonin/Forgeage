using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Forgeage
{
	public class ActionHandler : Singleton<ActionHandler>
	{
        public ObservableCollection<EntityController> Selected { get; private set; } = new ObservableCollection<EntityController>();
    }
}
