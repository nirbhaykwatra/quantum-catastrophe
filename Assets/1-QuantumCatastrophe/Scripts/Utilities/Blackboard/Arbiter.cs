using System;
using System.Collections.Generic;

namespace QC.Utilities.BlackboardSystem
{
    public class Arbiter
    {
        private readonly List<IExpert> m_experts = new();
        
        public List<IExpert> GetExperts => m_experts;
        
        public void RegisterExpert(IExpert expert)
        {
            if (expert == null) return;
            m_experts.Add(expert);
        }
        
        public void DeregisterExpert(IExpert expert)
        {
            if (expert == null) return;
            if (!m_experts.Contains(expert)) return;
            m_experts.Remove(expert);
        }

        public List<Action> BlackboardIteration(Blackboard blackboard)
        {
            IExpert bestExpert = null;
            int highestInsistence = 0;

            foreach (IExpert expert in m_experts)
            {
                int insistence = expert.GetInsistence(blackboard);
                if (insistence > highestInsistence)
                {
                    highestInsistence = insistence;
                    bestExpert = expert;
                }
            }
            
            bestExpert?.Execute(blackboard);
            
            List<Action> actions = new(blackboard.PassedActions);
            blackboard.ClearActions();
            
            return actions;
        }
    }
}