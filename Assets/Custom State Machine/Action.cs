using UnityEngine;

namespace CustomStateMachine
{
    public abstract class Action : ScriptableObject
    {
        public abstract void Enter(Manager m);

        public abstract void Tick(Manager m);

        public abstract void Exit(Manager m);
    }
}