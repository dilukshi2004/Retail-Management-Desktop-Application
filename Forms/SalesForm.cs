using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using RetailApp.Data;
using RetailApp.Models;

namespace RetailApp.Forms
{
    public partial class SalesForm : Form
    {
        private ComboBox cboProducts;
        private TextBox txtQuantity;
        private Button btnAddToCart, btnRemoveItem, btnCompleteSale, btnNewSale;
        private DataGridView dgvCart;
        private Label lblTotal;

        private List<Product> allProducts = new();
        private List<SaleItem> cartItems = new();

        public SalesForm()
        {
            InitializeComponent();
            BuildUi();
            LoadProductsIntoComboBox();
        }

        private void BuildUi()
        {
            this.Text = "Sales";
            this.ClientSize = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblProduct = new Label { Text = "Product:", Location = new Point(20, 20), Size = new Size(80, 20) };
            cboProducts = new ComboBox
            {
                Location = new Point(20, 42),
                Size = new Size(300, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblQty = new Label { Text = "Quantity:", Location = new Point(340, 20), Size = new Size(80, 20) };
            txtQuantity = new TextBox { Location = new Point(340, 42), Size = new Size(80, 28), Text = "1" };

            btnAddToCart = new Button { Text = "Add to Cart", Location = new Point(440, 41), Size = new Size(120, 30) };
            btnAddToCart.Click += btnAddToCart_Click;

            dgvCart = new DataGridView
            {
                Location = new Point(20, 90),
                Size = new Size(650, 300),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

            lblTotal = new Label
            {
                Text = "Total: Rs. 0.00",
                Location = new Point(430, 400),
                Size = new Size(240, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            btnRemoveItem = new Button { Text = "Remove Selected", Location = new Point(20, 400), Size = new Size(140, 30) };
            btnRemoveItem.Click += btnRemoveItem_Click;

            btnCompleteSale = new Button { Text = "Complete Sale", Location = new Point(20, 445), Size = new Size(140, 35) };
            btnCompleteSale.Click += btnCompleteSale_Click;

            btnNewSale = new Button { Text = "New Sale", Location = new Point(180, 445), Size = new Size(140, 35) };
            btnNewSale.Click += btnNewSale_Click;

            Controls.Add(lblProduct);
            Controls.Add(cboProducts);
            Controls.Add(lblQty);
            Controls.Add(txtQuantity);
            Controls.Add(btnAddToCart);
            Controls.Add(dgvCart);
            Controls.Add(lblTotal);
            Controls.Add(btnRemoveItem);
            Controls.Add(btnCompleteSale);
            Controls.Add(btnNewSale);
        }

        private void LoadProductsIntoComboBox()
        {
            allProducts.Clear();
            using var connection = new SqliteConnection(DatabaseHelper.ConnectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT ProductId, SKU, Name, UnitPrice, StockQuantity FROM Products ORDER BY Name";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                allProducts.Add(new Product
                {
                    ProductId = reader.GetInt32(0),
                    SKU = reader.GetString(1),
                    Name = reader.GetString(2),
                    UnitPrice = reader.GetDecimal(3),
                    StockQuantity = reader.GetInt32(4)
                });
            }

            cboProducts.DataSource = null;
            cboProducts.DataSource = allProducts;
            cboProducts.DisplayMember = "Name";
            cboProducts.ValueMember = "ProductId";
        }

        private void btnAddToCart_Click(object sender, EventArgs e)
        {
            if (cboProducts.SelectedItem is not Product selected)
            {
                MessageBox.Show("Select a product first.");
                return;
            }
            if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Enter a valid quantity.");
                return;
            }

            int alreadyInCart = cartItems.Where(i => i.ProductId == selected.ProductId).Sum(i => i.Quantity);
            if (qty + alreadyInCart > selected.StockQuantity)
            {
                MessageBox.Show($"Not enough stock. Available: {selected.StockQuantity}, already in cart: {alreadyInCart}.");
                return;
            }

            var existing = cartItems.FirstOrDefault(i => i.ProductId == selected.ProductId);
            if (existing != null)
            {
                existing.Quantity += qty;
                existing.LineTotal = existing.Quantity * existing.UnitPrice;
            }
            else
            {
                cartItems.Add(new SaleItem
                {
                    ProductId = selected.ProductId,
                    ProductName = selected.Name,
                    Quantity = qty,
                    UnitPrice = selected.UnitPrice,
                    LineTotal = qty * selected.UnitPrice
                });
            }

            RefreshCart();
            txtQuantity.Text = "1";
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvCart.CurrentRow?.DataBoundItem is SaleItem item)
            {
                cartItems.Remove(item);
                RefreshCart();
            }
        }

        private void RefreshCart()
        {
            dgvCart.DataSource = null;
            dgvCart.DataSource = cartItems;

            foreach (var col in new[] { "SaleItemId", "SaleId", "ProductId" })
            {
                if (dgvCart.Columns[col] != null)
                    dgvCart.Columns[col].Visible = false;
            }

            decimal total = cartItems.Sum(i => i.LineTotal);
            lblTotal.Text = $"Total: Rs. {total:0.00}";
        }

        private void btnCompleteSale_Click(object sender, EventArgs e)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show("Cart is empty.");
                return;
            }

            using var connection = new SqliteConnection(DatabaseHelper.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string invoiceNo = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                decimal total = cartItems.Sum(i => i.LineTotal);

                var saleCmd = connection.CreateCommand();
                saleCmd.Transaction = transaction;
                saleCmd.CommandText = @"INSERT INTO Sales (InvoiceNo, SaleDate, TotalAmount)
                                         VALUES ($invoice, $date, $total);
                                         SELECT last_insert_rowid();";
                saleCmd.Parameters.AddWithValue("$invoice", invoiceNo);
                saleCmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                saleCmd.Parameters.AddWithValue("$total", total);
                long saleId = (long)saleCmd.ExecuteScalar();

                foreach (var item in cartItems)
                {
                    var itemCmd = connection.CreateCommand();
                    itemCmd.Transaction = transaction;
                    itemCmd.CommandText = @"INSERT INTO SaleItems (SaleId, ProductId, Quantity, UnitPrice, LineTotal)
                                             VALUES ($saleId, $productId, $qty, $price, $lineTotal)";
                    itemCmd.Parameters.AddWithValue("$saleId", saleId);
                    itemCmd.Parameters.AddWithValue("$productId", item.ProductId);
                    itemCmd.Parameters.AddWithValue("$qty", item.Quantity);
                    itemCmd.Parameters.AddWithValue("$price", item.UnitPrice);
                    itemCmd.Parameters.AddWithValue("$lineTotal", item.LineTotal);
                    itemCmd.ExecuteNonQuery();

                    var stockCmd = connection.CreateCommand();
                    stockCmd.Transaction = transaction;
                    stockCmd.CommandText = "UPDATE Products SET StockQuantity = StockQuantity - $qty WHERE ProductId = $productId";
                    stockCmd.Parameters.AddWithValue("$qty", item.Quantity);
                    stockCmd.Parameters.AddWithValue("$productId", item.ProductId);
                    stockCmd.ExecuteNonQuery();
                }

                transaction.Commit();

                var receiptItems = new List<SaleItem>(cartItems); // snapshot before we clear the cart
                var receiptForm = new ReceiptForm(invoiceNo, DateTime.Now, receiptItems, total);
                receiptForm.ShowDialog();

                cartItems.Clear();
                RefreshCart();
                LoadProductsIntoComboBox(); // refresh stock numbers for the next sale

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show("Sale failed: " + ex.Message);
            }
        }

        private void btnNewSale_Click(object sender, EventArgs e)
        {
            cartItems.Clear();
            RefreshCart();
        }
    }
}