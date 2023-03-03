using UnityEngine;

namespace CustomStateMachine
{
    public class Manager : MonoBehaviour
    {
        [SerializeField]
        public State currentState;

        [SerializeField]
        public State initialState;

        [SerializeField]
        public State fallbackState;

        [SerializeField]
        bool firstRun;

        private void Awake()
        {
            currentState = initialState;
            firstRun = true;
        }

        private void Start()
        {
            // check if state needs to be immediately changed
            // the value of firstRun can be kept as-is
            foreach (var connection in currentState.connections)
            {
                if (connection.Evaluate(this))
                {
                    currentState = connection.nextState;
                    break;
                }
            }
        }

        private void Update()
        {
            foreach (Connection connection in currentState.connections)
            {
                if (connection.exceededTimeLimit())
                {
                    currentState.Exit(this);
                    currentState = connection.fallbackIfStuck ? fallbackState : connection.nextState;
                    firstRun = true;
                    return;
                }

                if (connection.Evaluate(this))
                {
                    currentState.Exit(this);
                    currentState = connection.nextState;
                    firstRun = true;
                    return;
                }
            }

            if (firstRun)
            {
                currentState.Enter(this);
                firstRun = false;
            }

            currentState.Tick(this);
        }
    }
}
