namespace GarminR10DataViewer
{
    internal static class Extensions
    {
        public static void SetStrokeStyle(this ICanvas canvas, Color color, float strokeWidth, LineCap strokeCap = LineCap.Round, LineJoin strokeJoin = LineJoin.Round)
        {
            canvas.StrokeColor = color;
            canvas.StrokeSize = strokeWidth;
            canvas.StrokeLineCap = strokeCap;
            canvas.StrokeLineJoin = strokeJoin;
        }

        public static void SetFont(this ICanvas canvas, Microsoft.Maui.Graphics.Font font, float fontSize, Color fontColor)
        {
            canvas.Font = font;
            canvas.FontSize = fontSize;
            canvas.FontColor = fontColor;
        }

        public static void SetHatchPatternFill(this ICanvas canvas, Color hatchColor, Color backgroundColor)
        {
            IPattern pattern;

            // Create a 10x10 template for the pattern
            using (PictureCanvas picture = new PictureCanvas(0, 0, 20, 20))
            {
                picture.FillColor = backgroundColor;
                picture.FillRectangle(0, 0, 20, 20);

                picture.StrokeColor = hatchColor;
                picture.StrokeSize = 6;
                picture.DrawLine(-3,3, 3,-3);
                picture.DrawLine(0, 20, 20, 0);
                picture.DrawLine(17,23,23,17);

                pattern = new PicturePattern(picture.Picture, 20, 20);
            }

            PatternPaint patternPaint = new PatternPaint
            {
                Pattern = pattern
            };
            canvas.SetFillPaint(patternPaint, RectF.Zero);
        }
    }
}
