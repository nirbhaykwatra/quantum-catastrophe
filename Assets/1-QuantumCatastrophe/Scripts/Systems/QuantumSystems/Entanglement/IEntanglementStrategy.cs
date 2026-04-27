using UnityEngine;

namespace QC.Systems.Entanglement
{

    /// <summary>
    /// Defines a single attribute that is propagated from a source to a target
    /// during entanglement. Implement this interface on ScriptableObjects to
    /// create designer-configurable entanglement behaviours.
    /// </summary>
    public interface IEntanglementStrategy
    {
        /// <summary>
        /// Called every FixedUpdate by the EntanglementManager.
        /// Reads the relevant attribute from <paramref name="source"/> and writes
        /// an attenuated version of it to <paramref name="target"/>.
        /// </summary>
        /// <param name="source">The object whose attribute is being read (first selected).</param>
        /// <param name="target">The object that receives the propagated attribute (second selected).</param>
        /// <param name="impedanceFactor">
        /// A [0..1] value representing how blocked the target is.
        /// 0 = completely free, 1 = completely blocked.
        /// Strategies should scale their effect by (1 - impedanceFactor).
        /// </param>
        void Apply(EntanglableComponent source, EntanglableComponent target, float impedanceFactor);

        /// <summary>
        /// Called when the entanglement pair is first formed, allowing strategies
        /// to cache references or set initial state.
        /// </summary>
        void OnEntangled(EntanglableComponent source, EntanglableComponent target);

        /// <summary>
        /// Called when the entanglement pair is broken, allowing strategies to
        /// clean up any state they have set on source or target.
        /// </summary>
        void OnDisentangled(EntanglableComponent source, EntanglableComponent target);
    }
}