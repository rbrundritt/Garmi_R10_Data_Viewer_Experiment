using Microsoft.ML.Data;

namespace GarminR10DataViewer.ML.Models
{
    internal class GolfShotModelOutput
    {
        [ColumnName(@"Features")]
        public float[] Features { get; set; }

        [ColumnName(@"Score")]
        public float Score { get; set; }
    }
}
