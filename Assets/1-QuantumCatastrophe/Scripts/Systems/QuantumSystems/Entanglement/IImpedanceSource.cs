namespace QC.Systems.Entanglement
{
    /// <summary>
    /// Represents a single source of physical impedance on an EntanglableComponent.
    /// Multiple sources are combined by EntanglableComponent.SampleImpedance().
    /// Implement this interface on MonoBehaviours to create designer-configurable
    /// impedance contributions.
    /// </summary>
    public interface IImpedanceSource
    {
        /// <summary>
        /// Returns the current impedance contributed by this source.
        /// </summary>
        /// <returns>A value in [0..1]. 0 = no resistance, 1 = fully blocked.</returns>
        float GetImpedance();
    }
}