﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Switch_Toolbox.Library.Forms
{
    public partial class STPropertyGrid : UserControl
    {
        public STPropertyGrid()
        {
            InitializeComponent();

            propertyGrid1.PropertySort = PropertySort.Categorized;
         /*   this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.CategoryForeColor = Switch_Toolbox.Library.Forms.FormThemes.BaseTheme.FormForeColor;
            this.propertyGrid1.CategorySplitterColor = Switch_Toolbox.Library.Forms.FormThemes.BaseTheme.MDIChildBorderColor;
            this.propertyGrid1.CommandsActiveLinkColor = System.Drawing.Color.Red;
            this.propertyGrid1.CommandsBorderColor = Switch_Toolbox.Library.Forms.FormThemes.BaseTheme.MDIParentBackColor;
            this.propertyGrid1.DisabledItemForeColor = Switch_Toolbox.Library.Forms.FormThemes.BaseTheme.DisabledItemColor;
            this.propertyGrid1.HelpBackColor = Switch_Toolbox.Library.Forms.FormThemes.BaseTheme.FormBackColor;
            this.propertyGrid1.HelpBorderColor = Switch_Toolbox.Library.Forms.FormThemes.BaseTheme.FormBackColor;
            this.propertyGrid1.HelpForeColor = Switch_Toolbox.Library.Forms.FormThemes.BaseTheme.FormForeColor;
            this.propertyGrid1.LineColor = Switch_Toolbox.Library.Forms.FormThemes.BaseTheme.MDIChildBorderColor;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.SelectedItemWithFocusForeColor = Switch_Toolbox.Library.Forms.FormThemes.BaseTheme.FormContextMenuSelectColor;
            this.propertyGrid1.Size = new System.Drawing.Size(613, 532);
            this.propertyGrid1.TabIndex = 0;
            this.propertyGrid1.ToolbarVisible = false;
            this.propertyGrid1.ViewBackColor = Switch_Toolbox.Library.Forms.FormThemes.BaseTheme.FormBackColor;
            this.propertyGrid1.ViewBorderColor = Switch_Toolbox.Library.Forms.FormThemes.BaseTheme.FormBackColor;
            this.propertyGrid1.ViewForeColor = Switch_Toolbox.Library.Forms.FormThemes.BaseTheme.FormForeColor;*/
        }

        public bool DisableHintDisplay
        {
            get
            {
                return propertyGrid1.HelpVisible;
            }
            set
            {
                propertyGrid1.HelpVisible = value;
            }
        }

        Action OnPropertyChanged;
        public void LoadProperty(object selectedObject, Action onPropertyChanged)
        {
            OnPropertyChanged = onPropertyChanged;
            propertyGrid1.SelectedObject = selectedObject;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            OnPropertyChanged();
        }
    }
}
