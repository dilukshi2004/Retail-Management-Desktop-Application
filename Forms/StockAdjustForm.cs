using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using RetailApp.Data;

namespace RetailApp.Forms
{
    public partial class StockAdjustForm : Form
    {
        private readonly int productId;
        private readonly int currentStock;

        private Label lblProduct, lblCurrent, lblChange, lblReason;
        private TextBox txtChange;
        private ComboBox cboReason;
        private Button btnSave, btnCancel;

        public StockAdjustForm(int productId, string productName, int currentStock)
        {
            this.productId = productId;
            this.currentStock = currentStock;
            InitializeComponent();
            BuildUi(productName);
        }

        private void BuildUi(string productName)
        {
            this.Text = "Adjust Stock";
            this.ClientSize = new Size(340, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblProduct = new Label { Text = $"Product: {productName}", Location = new Point(15, 15), Size = new Size(300, 20) };
            lblCurrent = new Label { Text = $"Current Stock: {currentStock}", Location = new Point(15, 40), Size = new Size(300, 20) };

            lblChange = new Label { Text = "Change Quantity (+/-):", Location = new Point(15, 75), Size = new Size(180, 20) };
            txtChange = new TextBox { Location = new Point(15, 97), Size = new Size(100, 27) };

            lblReason = new Label { Text = "Reason:", Location = new Point(15, 130), Size = new Size(100, 20) };
            cboReason = new ComboBox
            {
                Location = new Point(15, 152),
                Size = new Size(150, 27),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboReason.Items.AddRange(new[] { "Restock", "Correction", "Damage" });
            cboReason.SelectedIndex = 0;

            btnSave = new Button { Text = "Save", Location = new Point(15, 195), Size = new Size(100, 32) };
            btnSave.Click += btnSave_Click;

            btnCancel = new Button { Text = "Cancel", Location = new Point(125, 195), Size = new Size(100, 32) };
            btnCancel.Click += (s, e) => this.Close();

            Controls.Add(lblProduct);
            Controls.Add(lblCurrent);
            Controls.Add(lblChange);
            Controls.Add(txtChange);
            Controls.Add(lblReason);
            Controls.Add(cboReason);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtChange.Text, out int change) || change == 0)
            {
                MessageBox.Show("Enter a non-zero whole number (use a minus sign for reductions, e.g. -5).");
                return;
            }

            if (currentStock + change < 0)
            {
                MessageBox.Show($"That would take stock below zero (current: {currentStock}).");
                return;
            }

            string reason = cboReason.SelectedItem.ToString();

            using var connection = new SqliteConnection(DatabaseHelper.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var updateCmd = connection.CreateCommand();
                updateCmd.Transaction = transaction;
                updateCmd.CommandText = "UPDATE Products SET StockQuantity = StockQuantity + $change WHERE ProductId = $id";
                updateCmd.Parameters.AddWithValue("$change", change);
                updateCmd.Parameters.AddWithValue("$id", productId);
                updateCmd.ExecuteNonQuery();

                var logCmd = connection.CreateCommand();
                logCmd.Transaction = transaction;
                logCmd.CommandText = @"INSERT INTO StockAdjustments (ProductId, ChangeQty, Reason, AdjustmentDate)
                                        VALUES ($id, $change, $reason, $date)";
                logCmd.Parameters.AddWithValue("$id", productId);
                logCmd.Parameters.AddWithValue("$change", change);
                logCmd.Parameters.AddWithValue("$reason", reason);
                logCmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                logCmd.ExecuteNonQuery();

                transaction.Commit();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show("Adjustment failed: " + ex.Message);
            }
        }
    }
}