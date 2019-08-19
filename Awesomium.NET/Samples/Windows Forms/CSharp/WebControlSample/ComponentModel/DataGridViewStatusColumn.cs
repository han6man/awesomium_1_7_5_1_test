/********************************************************************************
 *    Project   : Awesomium.NET (TabbedFormsSample)
 *    File      : DataGridViewStatusColumn.cs
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
 *    Creates a DataGridView Column with cells that render a progress-bar.
 *    
 *    
 ********************************************************************************/

#region Using
using System;
using System.Windows.Forms;
using System.ComponentModel;
#endregion

namespace TabbedFormsSample.ComponentModel
{

    #region DataGridViewProgressBarColumn
    public class DataGridViewStatusColumn : DataGridViewImageColumn
    {
        public DataGridViewStatusColumn()
        {
            this.CellTemplate = new DataGridViewStatusCell();
        }
    }
    #endregion

    #region DataGridViewStatusCell
    public class DataGridViewStatusCell : DataGridViewImageCell
    {

        public DataGridViewStatusCell()
        {
            this.ValueType = typeof( bool );
        }

        protected override object GetFormattedValue( object value,
            int rowIndex,
            ref DataGridViewCellStyle cellStyle,
            TypeConverter valueTypeConverter,
            TypeConverter formattedValueTypeConverter,
            DataGridViewDataErrorContexts context )
        {
            bool statusVal = (bool)value;
            return statusVal ? Properties.Resources.development_51 : Properties.Resources.development_52;
        }
    }
    #endregion

}