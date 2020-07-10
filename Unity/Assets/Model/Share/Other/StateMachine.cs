using System.Collections.Generic;
#if !SERVER
using UnityEngine;
#endif

namespace ETModel
{
    public class StateMachine<T> where T : new()
    {
        T m_Target;

        public void SetTarget(T target)
        {
            m_Target = target;
        }

        public T GetTarget()
        {
            return m_Target;
        }

        public class State
        {
            public StateMachine<T> StateMachine { get; set; }

            public T GetTarget()
            {
                return StateMachine.GetTarget();
            }

            public virtual void OnEnter()
            {

            }

            public virtual void OnUpdate()
            {

            }

            public virtual void OnLeave()
            {

            }

            public void TriggerEvent(int EventID)
            {
                StateMachine.TriggerEvent(EventID, 0.0f);
            }

            public void TriggerEvent(int EventID, float WaitSeconds)
            {
                StateMachine.TriggerEvent(EventID, WaitSeconds);
            }
        }

        public bool ShowDebugInfo = false;

        class WaitAndChangeState : State
        {
            public StateMachine<T> m_StateManager = null;

            public float m_Timer = 0.0f;

            public State m_NextState = null;

            public override void OnEnter()
            {

            }

            public override void OnUpdate()
            {
#if !SERVER
                m_Timer -= Time.deltaTime;
                if (m_Timer < 0 && m_NextState != null)
                {
                    m_StateManager.ChangeState(m_NextState);
                }
#else
                if (m_Timer < System.DateTime.Now.Ticks && m_NextState != null)
                {
                    m_StateManager.ChangeState(m_NextState);
                }
#endif
            }

            public override void OnLeave()
            {
            }
        }

        public State GetCurrent()
        {
            return m_Curent;
        }

        private int _currentEventID = -1;
        public int currentEventID
        {
            private set
            {
                _currentEventID = value;
            }
            get
            {
                return _currentEventID;
            }
        }

        State m_Curent = null;

        State m_New = null;

        class Transition
        {
            public State From { get; set; }

            public State To { get; set; }

            public int EventID;
        }

        List<Transition> m_TransitionList = new List<Transition>();

        Transition GetTransition(State From, State To)
        {
            for (int i = 0; i < m_TransitionList.Count; i++)
            {
                if (m_TransitionList[i].From == From && m_TransitionList[i].To == To)
                {
                    return m_TransitionList[i];
                }
            }
            return null;
        }

        Transition GetAnyStateTransition(int EventID)
        {
            for (int i = 0; i < m_TransitionList.Count; i++)
            {
                if (m_TransitionList[i].From == null && m_TransitionList[i].EventID == EventID)
                {
                    return m_TransitionList[i];
                }
            }
            return null;
        }

        Transition GetCurrentStateTransition(int EventID)
        {
            for (int i = 0; i < m_TransitionList.Count; i++)
            {
                if (m_TransitionList[i].From == m_Curent && m_TransitionList[i].EventID == EventID)
                {
                    return m_TransitionList[i];
                }
            }
            return null;
        }

        public void SetAnyStateTransition(State To, int EventID)
        {
            SetTransition(null, To, EventID);
        }

        public void SetTransition(State From, State To, int EventID)
        {
            if (GetTransition(From, To) != null)
            {
                Log.Error("GameStateManager.SetTransition fail, already set!");
                return;
            }

            Transition _NewTransition = new Transition();
            _NewTransition.From = From;
            _NewTransition.To = To;
            _NewTransition.EventID = EventID;

            m_TransitionList.Add(_NewTransition);
        }

        public void TriggerEvent(int EventID)
        {
            TriggerEvent(EventID, 0.0f);
        }

        public void TriggerEvent(int EventID, float WaitSeconds)
        {
            currentEventID = EventID;
            //先檢查AnyState
            Transition _AnyStateTransition = GetAnyStateTransition(EventID);
            if (_AnyStateTransition != null)
            {
                if (WaitSeconds == 0.0f)
                    ChangeState(_AnyStateTransition.To);
                else
                    ChangeState(_AnyStateTransition.To, WaitSeconds);
                return;
            }

            //檢查目前的State
            Transition _CurrentStateTransition = GetCurrentStateTransition(EventID);
            if (_CurrentStateTransition != null)
            {
                if (WaitSeconds == 0.0f)
                    ChangeState(_CurrentStateTransition.To);
                else
                    ChangeState(_CurrentStateTransition.To, WaitSeconds);
                return;
            }

            if (ShowDebugInfo)
            {
                Log.Trace("GameStateManager.TriggerEvent, no match transition. event = " + EventID);
                return;
            }
        }

        public S CreateState<S>() where S : State, new()
        {
            S _NewState = new S();
            _NewState.StateMachine = this;
            return _NewState;
        }

        public void ChangeState(State NewState, float WaitSeconds)
        {
            if (NewState == null)
            {
                Log.Error("GameStateManager.ChangeState fail, m_New is not null!");
                return;
            }

            if (ShowDebugInfo)
            {
                Log.Trace("Change State " + NewState.GetType().ToString());
            }

            WaitAndChangeState _WaitState = new WaitAndChangeState();
            _WaitState.m_NextState = NewState;
            _WaitState.m_StateManager = this;
            _WaitState.m_Timer = WaitSeconds;
            m_New = _WaitState;
        }

        public void ChangeState(State NewState)
        {
            if (NewState != null)
            {
                m_New = NewState;
            }
            else
            {
                Log.Error("GameStateManager.ChangeState fail, m_New is null!");
            }
        }

        public void UpdateState()
        {
            if (m_New != null)
            {
                if (m_Curent != null)
                {
                    if (ShowDebugInfo)
                    {
                        Log.Trace("Leave State " + m_Curent.GetType().ToString());
                    }
                    m_Curent.OnLeave();
                }

                m_Curent = m_New;

                m_New = null;

                if (m_Curent != null)
                {
                    if (ShowDebugInfo)
                    {
                        Log.Trace("Enter State " + m_Curent.GetType().ToString());
                    }

                    m_Curent.OnEnter();
                    return;
                }
            }

            if (m_Curent != null)
            {
                m_Curent.OnUpdate();
            }
        }
    }
}