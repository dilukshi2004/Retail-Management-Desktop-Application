using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace RetailApp.Forms
{
    partial class SaleDetailForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Name = "SaleDetailForm";
            Text = "SaleDetailForm";
            ResumeLayout(false);
        }
    }
}