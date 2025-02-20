using GarminR10MauiAdapter.OpenConnect;

namespace GarminR10MauiAdapter
{
    internal class GolfShotClassifier
    {
        /// <summary>
        /// If the face is open, square, or closed.
        /// </summary>
        /// <param name="spinAxis"></param>
        /// <param name="playerHanded">Assumed to be right hand if not specified.</param>
        /// <returns></returns>
        internal static FacePathType? GetFacePathType(float? spinAxis, Handed? playerHanded = Handed.RH)
        {
            FacePathType? facePath = null;

            if (spinAxis != null)
            {
                if (spinAxis < -5)
                {
                    //The shot curves left and is a draw. Club face is closed.
                    facePath = FacePathType.Closed;
                }
                else if (spinAxis > 5)
                {
                    //The shot curves right and is a draw. Club face is open.
                    facePath = FacePathType.Open;
                }
                else
                {
                    //The shot has little curve and is fairly straight. Club face is straight.
                    facePath = FacePathType.Square;
                }

                //Check to see if golfer is left handed, and adjust the classification accordingly.
                if (playerHanded == Handed.LH)
                {
                    if (facePath == FacePathType.Open)
                    {
                        facePath = FacePathType.Closed;
                    }
                    else if (facePath == FacePathType.Closed)
                    {
                        facePath = FacePathType.Open;
                    }
                }
            }

            return facePath;
        }

        /// <summary>
        /// Attempts to classify the shot shape based on the launch monitor data.
        /// </summary>
        /// <param name="spinAxis"></param>
        /// <param name="horizontalLaunchAngle"></param>
        /// <param name="playerHanded">Assumed to be right hand if not specified.</param>
        /// <returns></returns>
        internal static ShotShape? GetShotShape(float? spinAxis, float? horizontalLaunchAngle, Handed? playerHanded = Handed.RH)
        {
            ShotShape? shotType = null;

            FacePathType? facePath = GetFacePathType(spinAxis, playerHanded);

            if (facePath != null && horizontalLaunchAngle != null)
            {
                //Horizontal launch angle tells us the initial direction the ball is traveling and is directly related to the face angle and club path.

                if (facePath == FacePathType.Closed)
                {
                    //Club face is closed and is a draw: pull draw, striaght draw, push draw.
                    if (horizontalLaunchAngle < -2.5) 
                    {
                        //The shot type of pull.
                        shotType = ShotShape.PullDraw;
                    }
                    else if (horizontalLaunchAngle > -1) 
                    {
                        //The shot type of push.
                        shotType = ShotShape.Draw;
                    }
                    else //Straight to target line.
                    {
                        //The shot type of straight.
                        shotType = ShotShape.Hook;
                    }
                }
                else if (facePath == FacePathType.Open)
                {
                    //Club face is open and is a fade (slice): fade(Pull slice), slice (Straight slice), push slice.
                    if (horizontalLaunchAngle < 1)
                    {
                        //The shot is type of pull.
                        shotType = ShotShape.Fade;
                    }
                    else if (horizontalLaunchAngle > 2.5)
                    {
                        //The shot is type of push.
                        shotType = ShotShape.PushSlice;
                    }
                    else
                    {
                        //The shot type of straight.
                        shotType = ShotShape.Slice;
                    }
                }
                else
                {
                    //Club face is fairly straight, thus it's square: Pull straight, straight, push straight.
                    if (horizontalLaunchAngle < -2)
                    {
                        //The shot type of pull.
                        shotType = ShotShape.Pull;
                    }
                    else if (horizontalLaunchAngle > 2)
                    {
                        //The shot type of push.
                        shotType = ShotShape.Push;
                    }
                    else
                    {
                        //The shot type of straight.
                        shotType = ShotShape.Straight;
                    }
                }
            }

            return shotType;
        }
    }
}
