using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpineViewerWPF.Windows
{
    /// <summary>
    /// Open.xaml 的互動邏輯
    /// </summary>
    public partial class Open : Window
    {

        private MainWindow _window;

        public Open(MainWindow main)
        {
            InitializeComponent();

            _window = main;
            string spineVersion = App.globalValues.SelectSpineVersion.ToString();
            if (spineVersion != "")
            {
                cb_Version.SelectedValue = spineVersion;
            }
            if (App.globalValues.SelectAtlasFile != "")
            {
                tb_Atlas_File.Text = App.globalValues.SelectAtlasFile;
            }
            if (App.globalValues.SelectSpineFile != "") {
                tb_JS_file.Text = App.globalValues.SelectSpineFile;
            }
            if (App.globalValues.Scale != 1 && App.globalValues.Scale != 0)
            {
                tb_Canvas_scale.Text = App.globalValues.Scale.ToString();
            }
   


            tb_Canvas_X.Text = App.canvasWidth.ToString();
            tb_Canvas_Y.Text = App.canvasHeight.ToString();
        }

        private void btn_Altas_Open_Click(object sender, RoutedEventArgs e)
        {
           bool isSelect = SelectFile("Spine Altas File (*.atlas, *.atlas.txt)|*.atlas;*.atlas.txt", tb_Atlas_File);
            if (isSelect)
            {
                App.globalValues.SelectAtlasFile = tb_Atlas_File.Text;
                if (!Common.CheckSpineFile(App.globalValues.SelectAtlasFile))
                {
                    MessageBox.Show("Can not found Spine Json or Binary file！");

                    bool isSelectSp = SelectFile("Spine Json File (*.json)|*.json|Spine Binary File (*.skel)|*.skel", tb_JS_file);
                    if (isSelectSp)
                    {
                        App.globalValues.SelectSpineFile = tb_JS_file.Text;
                    }
                }
                else
                {
                    tb_JS_file.Text = App.globalValues.SelectSpineFile;
                    if (cb_detect.IsChecked.Value)
                        Autodetect();
                }
                //var atlasSize = Common.GetAtlasSize(App.globalValues.SelectAtlasFile);
                //if (atlasSize != null)
                //{
                //    tb_Canvas_X.Text = atlasSize.Value.Width.ToString();
                //    tb_Canvas_Y.Text = atlasSize.Value.Height.ToString();
                //}
            }
        }

        private void Autodetect()
        {
            if (File.Exists(App.globalValues.SelectSpineFile))
            {
                bool detectSuccess = false;
                App.globalValues.SkeletonHeader = new PublicFunction.SkeletonBinaryHeader();
                if (Common.IsBinaryData(App.globalValues.SelectSpineFile))
                {
                    FileStream fs = new FileStream(App.globalValues.SelectSpineFile, FileMode.Open);
                    
                    if (App.globalValues.SkeletonHeader.ReadFromBinary(fs))
                    {
                        detectSuccess = true;
                    }
                    fs.Close();
                }
                else
                {
                    string jsonText = File.ReadAllText(App.globalValues.SelectSpineFile);
                    if (App.globalValues.SkeletonHeader.ReadFromJSON(jsonText))
                    {
                        detectSuccess = true;
                    }
                }

                if (detectSuccess)
                {
                    int w = Convert.ToInt32(App.globalValues.SkeletonHeader.Width);
                    int h = Convert.ToInt32(App.globalValues.SkeletonHeader.Height);
                    if (w > 4096 || h > 4096)
                    {
                        if (w > 4096 && w > h)
                        {
                            float scale = (float)Math.Truncate((4096f / w) * 100f) / 100f;
                            w = 4096;
                            h = (int)(h * scale);
                            tb_Canvas_scale.Text = scale.ToString();
                        }
                        else
                        {
                            float scale = (float)Math.Truncate((4096f / h) * 100f) / 100f;
                            h = 4096;
                            w = (int)(w * scale);
                            tb_Canvas_scale.Text = scale.ToString();
                        }
                    }
                    tb_Canvas_X.Text = w.ToString();
                    tb_Canvas_Y.Text = h.ToString();
                    if (Common.SpineVersions.Contains(App.globalValues.SkeletonHeader.Version))
                    {
                        cb_Version.SelectedItem = App.globalValues.SkeletonHeader.Version;
                    }
                    else
                    {
                        string v = Common.SpineVersions.LastOrDefault(a => a.Substring(0, 3) == App.globalValues.SkeletonHeader.Version.Substring(0, 3));
                        if (!string.IsNullOrEmpty(v))
                            cb_Version.SelectedValue = v;
                    }

                }
            }
        }

        private void btn_JS_Open_Click(object sender, RoutedEventArgs e)
        {
           bool isSelect =  SelectFile("Spine Json File (*.json)|*.json|Spine Binary File (*.skel)|*.skel", tb_JS_file);
            if (isSelect)
            {
                tb_JS_file.Text = App.globalValues.SelectSpineFile;
            }
        }

        private bool SelectFile(string filter,TextBox textBox)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (Directory.Exists(App.lastDir))
            {
                openFileDialog.InitialDirectory = App.lastDir;
            }
            else
            {
                openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            }
            openFileDialog.Filter = filter; ;
            if (openFileDialog.ShowDialog() == true)
            {
                textBox.Text = openFileDialog.FileName;
                App.lastDir = Common.GetDirName(openFileDialog.FileName);
                return true;
            }
            return false;

        }

        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            if (cb_Version.SelectedIndex == -1)
            {
                System.Windows.MessageBox.Show("Please Select Spine Version！");
                return;
            }
            if(tb_Atlas_File.Text.Trim() == "")
            {
                System.Windows.MessageBox.Show("Please Select Atlas File！");
                return;
            }
            if (tb_JS_file.Text.Trim() == "")
            {
                System.Windows.MessageBox.Show("Please Select Json or Skel File！");
                return;
            }

            double setWidth;
            double setHeight;
            double scale;
            if (!double.TryParse(tb_Canvas_X.Text,out setWidth) || !double.TryParse(tb_Canvas_Y.Text, out setHeight) || !double.TryParse(tb_Canvas_scale.Text, out scale))
            {
                System.Windows.MessageBox.Show("Please Set Currect Canvas Value！");
                return;
            }
            App.globalValues.FrameWidth = setWidth;
            App.globalValues.FrameHeight = setHeight;
            App.canvasWidth = setWidth;
            App.canvasHeight = setHeight;
            App.globalValues.Scale = (float)scale;
            App.globalValues.CoordinatedInCenter = rb_center.IsChecked.Value;
            App.isNew = true;

            if (tb_Muilt_Texture.Text.Trim() != "")
            {
                List<string> muiltTextureList = tb_Muilt_Texture.Text.Split(',').ToList();
                muiltTextureList.Insert(0, "");
                App.mulitTexture = muiltTextureList.ToArray();
            }
            else {
                App.mulitTexture = null;
            }


            _window.LoadPlayer(cb_Version.SelectionBoxItem.ToString());
            this.Close();

        }

        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void TextBox_PreviewDrop(object sender, DragEventArgs e)
        {
            object text = e.Data.GetData(DataFormats.FileDrop);
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                if(tb.Name == "tb_Atlas_File")
                {
                    if(((string[])text)[0].IndexOf(".atlas") != -1)
                    {
                        tb_Atlas_File.Text = ((string[])text)[0];
                        App.globalValues.SelectAtlasFile = tb_Atlas_File.Text;
                        if (!Common.CheckSpineFile(App.globalValues.SelectAtlasFile))
                        {
                            MessageBox.Show("Can not found Spine Json or Binary file！");

                            bool isSelectSp = SelectFile("Spine Json File (*.json)|*.json|Spine Binary File (*.skel)|*.skel", tb_JS_file);
                            if (isSelectSp)
                            {
                                App.globalValues.SelectSpineFile = tb_JS_file.Text;
                            }
                        }
                        else
                        {
                            tb_JS_file.Text = App.globalValues.SelectSpineFile;
                        }
                    }
                }
                else if (tb.Name == "tb_JS_file")
                {
                    if (((string[])text)[0].IndexOf(".json") > 0 || ((string[])text)[0].IndexOf(".skel") > 0)
                    {
                        App.globalValues.SelectSpineFile = ((string[])text)[0];
                        tb_JS_file.Text = App.globalValues.SelectSpineFile;
                    }
                }


                   
            }
        }


    }
}
