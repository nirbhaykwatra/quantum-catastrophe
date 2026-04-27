using System;
using System.Collections.Generic;
using QC.Utilities.ServiceLocation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace QC.Utilities.BlackboardSystem
{
    public class BlackboardController : MonoBehaviour
    {
        [InlineEditor, SerializeField] private BlackboardData m_blackboardData;
        private readonly Blackboard m_blackboard = new();
        private readonly Arbiter m_arbiter = new();
        
        private void Awake()
        {
            ServiceLocator.Global.Register(this);
            m_blackboardData.SetValuesOnBlackboard(m_blackboard);
            //m_blackboard.Debug();
        }
        
        public Blackboard GetBlackboard() => m_blackboard;
        public void RegisterExpert(IExpert expert) => m_arbiter.RegisterExpert(expert);
        public void DeregisterExpert(IExpert expert) => m_arbiter.DeregisterExpert(expert);
        public List<IExpert> GetExperts => m_arbiter.GetExperts;

        private void Update()
        {
            foreach (Action action in m_arbiter.BlackboardIteration(m_blackboard))
            {
                action();
            }
        }
        
        
    }
}