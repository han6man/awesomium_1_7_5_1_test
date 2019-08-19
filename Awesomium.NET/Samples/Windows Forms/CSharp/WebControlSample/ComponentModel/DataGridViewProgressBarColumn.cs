/********************************************************************************
 *    Project   : Awesomium.NET (TabbedFormsSample)
 *    File      : DataGridViewProgressBarColumn.cs
 *    Version   : 1.7.0.0 
 *    Date      : 3/5/2013
 *    Author    : Perikles C. Stephanidis (perikles@awesomium.com)
 *    Copyright : ©2013 Awesomium Technologies LLC
 *    
 *    This code is provided "AS IS" and for demonstration purposes only,
 *    without warranty of any kind.
 *     
 *-------------------------------------------------------------------------------
 *
 *    Notes     :
 *
 *     Creates a DataGridView Column with cells that render a progress-bar.
 *    
 *    
 ********************************************************************************/

#region Using
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Drawing2D;
#endregion

namespace TabbedFormsSample.ComponentModel
{

    #region DataGridViewProgressBarColumn
    public class DataGridViewProgressBarColumn : DataGridViewImageColumn
    {
        public DataGridViewProgressBarColumn()
        {
            this.CellTemplate = new DataGridViewProgressBarCell();
        }
    }
    #endregion

    #region DataGridViewProgressBarCell
    public class DataGridViewProgressBarCell : DataGridViewImageCell
    {

        public DataGridViewProgressBarCell()
        {
            this.ValueType = typeof( int );
        }

        protected override object GetFormattedValue( object value,
            int rowIndex,
            ref DataGridViewCellStyle cellStyle,
            TypeConverter valueTypeConverter,
            TypeConverter formattedValueTypeConverter,
            DataGridViewDataErrorContexts context )
        {

            Bitmap bm = new Bitmap( OwningColumn.Width, OwningRow.Height );

            int progressVal = Convert.ToInt32( value );
            int lWidth = ( OwningColumn.Width * ( progressVal ) / 100 );

            using ( Graphics g = Graphics.FromImage( bm ) )
            {
                using ( ProgressBar pb = new ProgressBar
                {
                    Minimum = 0,
                    Maximum = 100,
                    Height = bm.Height,
                    Width = bm.Width,
                    Value = progressVal
                } )
                {
                    pb.DrawToBitmap( bm, new Rectangle( 0, 0, bm.Width, bm.Height ) );

                    Rectangle myRectangle = new Rectangle( 1, 1, Math.Max( lWidth - 2, 1 ), Math.Max( bm.Height - 2, 1 ) );

                    using ( LinearGradientBrush myLinearGradientBrush =
                        new LinearGradientBrush( myRectangle,
                            Color.LightGreen,
                            Color.MediumSeaGreen,
                            LinearGradientMode.Vertical ) )
                    {
                        myLinearGradientBrush.SetBlendTriangularShape( (float).5 );
                        g.FillRectangle( myLinearGradientBrush, myRectangle );
                    }

                    // Draw the text if value is greater than 0.
                    if ( ( cellStyle != null ) && ( progressVal > 0 ) )
                    {
                        using ( SolidBrush fontBrush = new SolidBrush( cellStyle.ForeColor ) )
                        {
                            string formattedValue = String.Format( "{0}%", progressVal );
                            SizeF fontSize = g.MeasureString( formattedValue, cellStyle.Font, OwningColumn.Width );
                            PointF fontPlacement = new PointF(
                                Math.Max( ( OwningColumn.Width / 2 ) - ( fontSize.Width / 2 ), 1 ),
                                Math.Max( ( OwningRow.Height / 2 ) - ( fontSize.Height / 2 ), 1 ) );

                            g.DrawString( formattedValue,
                                cellStyle.Font,
                                fontBrush,
                                fontPlacement );
                        }
                    }
                }
            }

            return bm;
        }
    }
    #endregion

}