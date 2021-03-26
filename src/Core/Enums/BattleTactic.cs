namespace CraigStars
{
    /// <summary>
    /// Different orders ships can have during battle
    /// </summary>
    public enum BattleTactic
    {
        // RUN AWAY!
        Disengage,
        // MaximizeDamage until we are damaged, then disengage        
        DisengageIfChallenged,
        // If in range of enemy weapons, move away. Only fire if cornered or if from a safe range
        MinimizeDamageToSelf,
        MaximizeNetDamage,
        MaximizeDamageRatio,
        MaximizeDamage
    }
}
