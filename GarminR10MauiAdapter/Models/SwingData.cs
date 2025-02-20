namespace GarminR10MauiAdapter
{
    /// <summary>
    /// Metrics of the swing.
    /// </summary>
    public class SwingData
    {
        /// <summary>
        /// How long the backswing takes.
        /// </summary>
        public TimeSpan BackswingDuration { get; set; }

        /// <summary>
        /// How long the downswing takes.
        /// </summary>
        public TimeSpan DownswingDuration { get; set; }

        /// <summary>
        /// Tempo of the swing. A ratio of backswing to downswing. Ideal tempo value for golf is 3. Backswing takes 3 times as long as downswing.
        /// </summary>
        public float? Tempo { get; set; } = null;
    }
}
