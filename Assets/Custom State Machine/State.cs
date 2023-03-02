using UnityEngine;
using System.Collections.Generic;

namespace CustomStateMachine
{
    [CreateAssetMenu(menuName=FilePaths.state)]
    public class State : ScriptableObject
    {
        public bool firstTimeRunning = true; // default to true

        public List<Action> actions;

        public List<Connection> connections;

        public void Enter(Manager m)
        {
            // eval connections before all else incase of immediate transition
            foreach (Connection connection in connections)
            {
                connection.Enter(m);
                EvaluateConnection(m, connection);
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

            foreach (Connection connection in connections)
            {
                if (connection.exceededTimeLimit())
                {
                    if (connection.fallbackIfStuck)
                        m.ChangeState(m.fallbackState);
                    else
                        m.ChangeState(connection.nextState);
                }

                EvaluateConnection(m, connection);
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

        void EvaluateConnection(Manager m, Connection c)
        {
            if (c.Evaluate(m))
                m.ChangeState(c.nextState);
        }
    }
}

