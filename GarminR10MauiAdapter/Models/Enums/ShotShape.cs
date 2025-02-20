namespace GarminR10MauiAdapter
{
    /// <summary>
    /// The shot type or classification.
    /// https://noobnorm.com/hit-golf-ball-straight/
    /// https://hackmotion.com/golf-ball-flight-laws/ 
    /// </summary>
    public enum ShotShape
    {
        /// <summary>
        /// Club face is square to the swing path.
        /// </summary>
        Straight = 0,

        //Shots that land left of the target (right handed player). Closest to farthest from the target.

        /// <summary>
        /// Club face is left of the target, and open to the swing path.
        /// Also known as a Pull Fade or Pull Slice.
        /// </summary>
        Fade,

        /// <summary>
        /// Club face is left of the target, and square to the swing path.
        /// Also known as a Pull Straight.
        /// </summary> 
        Pull,

        /// <summary>
        /// Club face is to square to target, and closed to the swing path.
        /// Also known as a Straight Draw.
        /// </summary> 
        Hook,

        /// <summary>
        /// Club face is left of the target, and closed to the swing path.
        /// Also known as a Pull Hook.
        /// </summary> 
        PullDraw,


        //Shots that land right of the target (right handed player). Closest to farthest from the target.

        /// <summary>
        /// Club face is right of the target, and closed to the swing path.
        /// Also known as a Push Draw. 
        /// </summary> 
        Draw,

        /// <summary>
        /// Club face is right of the target, and square to the swing path.
        /// Also known as a Push Straight.
        /// </summary> 
        Push,

        /// <summary>
        /// Club face is to square to target, and open to the swing path.
        /// Also known as a Straight Fade or Straight Slice.
        /// </summary> 
        Slice,

        /// <summary>
        /// Club face is right of the target, and open to the swing path.
        /// Also known as a Push Fade.
        /// </summary> 
        PushSlice
    }
}

