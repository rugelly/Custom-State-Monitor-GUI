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

        private void Awake()
        {
            currentState = initialState;
        }

        private void Start()
        {
            currentState.firstTimeRunning = true;
        }

        private void Update()
        {
            if (currentState.firstTimeRunning)
            {
                currentState.firstTimeRunning = false;
                currentState.Enter(this);
            }

            currentState.Tick(this);
        }

        public void ChangeState(State newState)
        {
            currentState.Exit(this);
            currentState.firstTimeRunning = true;
            currentState = newState;
        }
    }
}
