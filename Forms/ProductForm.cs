using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using RetailApp.Data;
using RetailApp.Models;

namespace RetailApp.Forms
{
    public partial class ProductForm : Form
    {
        private Panel pnlTop, pnlRight;
        private Label lblTitle;
        private DataGridView dgvProducts;

        private Label lblSKU, lblName, lblCategory, lblPrice, lblUnit, lblStock, lblReorderLevel;
        private TextBox txtSKU, txtName, txtCategory, txtPrice, txtUnit, txtStock, txtReorderLevel;
        private Button btnAdd, btnUpdate, btnDelete, btnClear, btnClose;

        private int? selectedProductId = null;

        public ProductForm()
        {
            InitializeComponent();
            BuildUi();
            LoadProducts();
        }

        private void BuildUi()
        {
            this.Text = "Product Management";
            this.ClientSize = new Size(1050, 650);
            this.MinimumSize = new Size(900, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);

            // --- Top title bar ---
            pnlTop = new Panel { Dock = DockStyle.Top, Height = 55 };
            lblTitle = new Label
            {
                Text = "Product Management",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 12)
            };
            pnlTop.Controls.Add(lblTitle);

            // --- Right side: input form + action buttons ---
            pnlRight = new Panel { Dock = DockStyle.Right, Width = 300 };

            int y = 20;
            (lblSKU, txtSKU) = AddField(pnlRight, "SKU:", ref y);
            (lblName, txtName) = AddField(pnlRight, "Name:", ref y);
            (lblCategory, txtCategory) = AddField(pnlRight, "Category:", ref y);
            (lblPrice, txtPrice) = AddField(pnlRight, "Unit Price (Rs.):", ref y);
            (lblUnit, txtUnit) = AddField(pnlRight, "Unit (e.g. kg, pcs, L):", ref y);
            (lblStock, txtStock) = AddField(pnlRight, "Stock Qty:", ref y);
            (lblReorderLevel, txtReorderLevel) = AddField(pnlRight, "Reorder Level:", ref y);

            y += 15;
            btnAdd = new Button { Text = "Add", Location = new Point(15, y), Size = new Size(125, 34), BackColor = Color.FromArgb(46, 160, 67), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnUpdate = new Button { Text = "Update", Location = new Point(150, y), Size = new Size(125, 34), BackColor = Color.FromArgb(41, 128, 185), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate.FlatAppearance.BorderSize = 0;

            y += 44;
            btnDelete = new Button { Text = "Delete", Location = new Point(15, y), Size = new Size(125, 34), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnClear = new Button { Text = "Clear", Location = new Point(150, y), Size = new Size(125, 34), FlatStyle = FlatStyle.Flat };

            y += 55;
            btnClose = new Button { Text = "Close", Location = new Point(15, y), Size = new Size(260, 34), FlatStyle = FlatStyle.Flat };
            btnClose.Click += (s, e) => this.Close();

            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += btnClear_Click;

            pnlRight.Controls.Add(btnAdd);
            pnlRight.Controls.Add(btnUpdate);
            pnlRight.Controls.Add(btnDelete);
            pnlRight.Controls.Add(btnClear);
            pnlRight.Controls.Add(btnClose);

            // --- Grid fills the remaining space ---
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                EnableHeadersVisualStyles = false
            };
            dgvProducts.RowTemplate.Height = 30;
            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvProducts.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
            dgvProducts.SelectionChanged += dgvProducts_SelectionChanged;
            dgvProducts.CellFormatting += dgvProducts_CellFormatting;

            // Add order matters for docking: Fill first, then edge-docked panels
            Controls.Add(dgvProducts);
            Controls.Add(pnlRight);
            Controls.Add(pnlTop);
        }

        // Helper: creates a Label + TextBox pair inside a parent panel, advances y for the next pair
        private (Label, TextBox) AddField(Panel parent, string labelText, ref int y)
        {
            var label = new Label { Text = labelText, Location = new Point(15, y), Size = new Size(250, 20) };
            var textBox = new TextBox { Location = new Point(15, y + 20), Size = new Size(250, 24) };
            parent.Controls.Add(label);
            parent.Controls.Add(textBox);
            y += 55;
            return (label, textBox);
        }

        private void dgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvProducts.Columns[e.ColumnIndex].Name == "UnitPrice" && e.Value != null)
            {
                e.Value = $"Rs. {Convert.ToDecimal(e.Value):0.00}";
                e.FormattingApplied = true;
            }
        }

        private void LoadProducts()
        {
            var products = new List<Product>();
            using var connection = new SqliteConnection(DatabaseHelper.ConnectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT ProductId, SKU, Name, Category, UnitPrice, Unit, StockQuantity, ReorderLevel FROM Products ORDER BY Name";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    ProductId = reader.GetInt32(0),
                    SKU = reader.GetString(1),
                    Name = reader.GetString(2),
                    Category = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    UnitPrice = reader.GetDecimal(4),
                    Unit = reader.IsDBNull(5) ? "pcs" : reader.GetString(5),
                    StockQuantity = reader.GetInt32(6),
                    ReorderLevel = reader.GetInt32(7)
                });
            }
            dgvProducts.DataSource = null;
            dgvProducts.DataSource = products;

            if (dgvProducts.Columns["ProductId"] != null)
                dgvProducts.Columns["ProductId"].Visible = false;
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow?.DataBoundItem is Product p)
            {
                selectedProductId = p.ProductId;
                txtSKU.Text = p.SKU;
                txtName.Text = p.Name;
                txtCategory.Text = p.Category;
                txtPrice.Text = p.UnitPrice.ToString();
                txtUnit.Text = p.Unit;
                txtStock.Text = p.StockQuantity.ToString();
                txtReorderLevel.Text = p.ReorderLevel.ToString();
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtSKU.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("SKU and Name are required.");
                return false;
            }
            if (!decimal.TryParse(txtPrice.Text, out _))
            {
                MessageBox.Show("Price must be a valid number.");
                return false;
            }
            if (!int.TryParse(txtStock.Text, out _) || !int.TryParse(txtReorderLevel.Text, out _))
            {
                MessageBox.Show("Stock and Reorder Level must be whole numbers.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                txtUnit.Text = "pcs"; // sensible default instead of blocking submission
            }
            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            using var connection = new SqliteConnection(DatabaseHelper.ConnectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO Products (SKU, Name, Category, UnitPrice, Unit, StockQuantity, ReorderLevel)
                                 VALUES ($sku, $name, $category, $price, $unit, $stock, $reorder)";
            cmd.Parameters.AddWithValue("$sku", txtSKU.Text.Trim());
            cmd.Parameters.AddWithValue("$name", txtName.Text.Trim());
            cmd.Parameters.AddWithValue("$category", txtCategory.Text.Trim());
            cmd.Parameters.AddWithValue("$price", decimal.Parse(txtPrice.Text));
            cmd.Parameters.AddWithValue("$unit", txtUnit.Text.Trim());
            cmd.Parameters.AddWithValue("$stock", int.Parse(txtStock.Text));
            cmd.Parameters.AddWithValue("$reorder", int.Parse(txtReorderLevel.Text));

            try
            {
                cmd.ExecuteNonQuery();
                LoadProducts();
                ClearFields();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                MessageBox.Show("A product with this SKU already exists.");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedProductId == null)
            {
                MessageBox.Show("Select a product from the list first.");
                return;
            }
            if (!ValidateInput()) return;

            using var connection = new SqliteConnection(DatabaseHelper.ConnectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"UPDATE Products SET SKU=$sku, Name=$name, Category=$category,
                                 UnitPrice=$price, Unit=$unit, StockQuantity=$stock, ReorderLevel=$reorder
                                 WHERE ProductId=$id";
            cmd.Parameters.AddWithValue("$sku", txtSKU.Text.Trim());
            cmd.Parameters.AddWithValue("$name", txtName.Text.Trim());
            cmd.Parameters.AddWithValue("$category", txtCategory.Text.Trim());
            cmd.Parameters.AddWithValue("$price", decimal.Parse(txtPrice.Text));
            cmd.Parameters.AddWithValue("$unit", txtUnit.Text.Trim());
            cmd.Parameters.AddWithValue("$stock", int.Parse(txtStock.Text));
            cmd.Parameters.AddWithValue("$reorder", int.Parse(txtReorderLevel.Text));
            cmd.Parameters.AddWithValue("$id", selectedProductId.Value);
            cmd.ExecuteNonQuery();

            LoadProducts();
            ClearFields();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProductId == null)
            {
                MessageBox.Show("Select a product from the list first.");
                return;
            }
            var confirm = MessageBox.Show("Delete this product?", "Confirm", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            using var connection = new SqliteConnection(DatabaseHelper.ConnectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Products WHERE ProductId = $id";
            cmd.Parameters.AddWithValue("$id", selectedProductId.Value);

            try
            {
                cmd.ExecuteNonQuery();
                LoadProducts();
                ClearFields();
            }
            catch (SqliteException ex) when (ex.Message.Contains("FOREIGN KEY"))
            {
                MessageBox.Show(
                    "This product can't be deleted because it already appears in a completed sale or stock adjustment.\n\n" +
                    "Deleting it would break that historical record. If you no longer sell this product, consider setting its stock to 0 instead of removing it entirely.",
                    "Cannot Delete Product",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void btnClear_Click(object sender, EventArgs e) => ClearFields();

        private void ClearFields()
        {
            selectedProductId = null;
            txtSKU.Clear();
            txtName.Clear();
            txtCategory.Clear();
            txtPrice.Clear();
            txtUnit.Clear();
            txtStock.Clear();
            txtReorderLevel.Clear();
        }
    }
}