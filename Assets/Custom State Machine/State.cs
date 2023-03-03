using UnityEngine;
using System.Collections.Generic;

namespace CustomStateMachine
{
    [CreateAssetMenu(menuName=FilePaths.state)]
    public class State : ScriptableObject
    {
        public List<Action> actions;

        public List<Connection> connections;

        public void Enter(Manager m)
        {
            foreach (Connection connection in connections)
            {
                connection.Enter(m);
            }

            foreach (Action action in actions)
            {
                action.Enter(m);
            }
        }

        public void Tick(Manager m)
        {
            foreach (Action action in actions)
            {
                action.Tick(m);
            }
        }

        public void Exit(Manager m)
        {
            foreach (Connection connection in connections)
            {
                connection.Exit();
            }

            foreach (Action action in actions)
            {
                action.Exit(m);
            }
        }
    }
}

