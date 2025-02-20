namespace GarminR10MauiAdapter
{
    /// <summary>
    /// The type of face path.
    /// When the club face is square to the swing path, the ball will fly straight.
    /// When the club face is open to the swing path, the ball will fade.
    /// When the club face is closed to the swing path, the ball will draw.
    /// </summary>
    public enum FacePathType
    {
        /// <summary>
        /// Square club face to swing path.
        /// </summary>
        Square = 0,

        /// <summary>
        /// Open club face to swing path.
        /// </summary>
        Open = 1,

        /// <summary>
        /// Closed club face to swing path.
        /// </summary>
        Closed = 2
    }
}
