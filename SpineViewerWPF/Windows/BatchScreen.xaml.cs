using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Translator;

namespace SpineViewerWPF.Windows
{
    /// <summary>
    /// Логика взаимодействия для BatchScreen.xaml
    /// </summary>
    public partial class BatchScreen : Window
    {
        private MainWindow _window;
        public BatchScreen(MainWindow window)
        {
            InitializeComponent();
            _window = window;
        }

        private void btn_SelectCatalog_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            tb_catalog.Text = openFolderDialog.Folder;
        }

        private void btn_SaveCatalog_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            tb_saveCatalog.Text = openFolderDialog.Folder;
        }

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            bool saveAllAnimations = cb_allAnimations.IsChecked.Value;
            string spineCatalog = tb_catalog.Text;
            string saveCatalog = tb_saveCatalog.Text;

            int w = Convert.ToInt32(tb_width.Text);
            int h = Convert.ToInt32(tb_height.Text);
            App.globalValues.FrameWidth = w;
            App.globalValues.FrameHeight = h;
            App.canvasWidth = w;
            App.canvasHeight = h;
            App.globalValues.CoordinatedInCenter = rb_center.IsChecked.Value;
            _window.StartBatchScreen(cb_Version.SelectionBoxItem.ToString(), spineCatalog, saveCatalog, saveAllAnimations, cb_autodetect.IsChecked.Value);
            this.Close();
        }
    }
}
