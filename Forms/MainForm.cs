namespace RetailApp.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            var form = new ProductForm();
            form.ShowDialog();
            // ShowDialog blocks MainForm until ProductForm is closed — keeps flow simple
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            var form = new SalesForm();
            form.ShowDialog();
        }

        private void btnStock_Click(object sender, EventArgs e)
        {
            var form = new StockForm();
            form.ShowDialog();
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            var form = new HistoryForm();
            form.ShowDialog();
        }
        private void btnReport_Click(object sender, EventArgs e)
        {
            var form = new ReportForm();
            form.ShowDialog();
        }



    }
}
