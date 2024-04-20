using DigitizingNoteFs.Domain.IServices;
using Microsoft.Win32;
using System.Windows;


namespace DigitizingNoteFs.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ITestService testService { get; set; }

        public MainWindow(ITestService testService)
        {
            this.testService = testService;
            InitializeComponent();

        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // xls, xlsx, csv
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.csv";
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFileName = openFileDialog.FileName;
                FilePathTextBox.Text = selectedFileName;
            }
        }
    }
}