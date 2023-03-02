using UnityEngine;

namespace CustomStateMachine
{
    public abstract class Connection : ScriptableObject
    {
        public State nextState;

        float _timeElapsed;
        [SerializeField]
        float timeElapsed 
        { 
            get => Mathf.Clamp(_timeElapsed, Duration.zero, Duration.max); 
            set => _timeElapsed = value;
        }

        [SerializeField, Range(Duration.zero, Duration.max)]
        float timeLimit = 0;

        public bool fallbackIfStuck = false;

        public bool exceededTimeLimit()
        {
            if (timeLimit > Duration.zero)
            {
                timeElapsed += Time.deltaTime;
                return timeElapsed >= timeLimit;
            }
            else
                return false;
        }

        public void Enter(Manager m)
        {
            timeElapsed = 0;
        }

        public abstract bool Evaluate(Manager m); // the only abstract method. The state calls all the other stuff

        public void Exit()
        {
            timeElapsed = 0;
        }
    }
}
