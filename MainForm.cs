using MetroSuite;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System;
using ColorfulSoft.DeOldify;

public partial class MainForm : MetroForm
{
    private Image realImage, originalPreviewImage, processedRealImage, realDeOldifyArtistic, realDeOldifyStable;
    private int previewWidth, previewHeight;

    public MainForm()
    {
        InitializeComponent();
        CheckForIllegalCrossThreadCalls = false;
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

        pictureBox1.AllowDrop = true;
        pictureBox2.AllowDrop = true;

        pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
        pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;

        pictureBox1.MouseWheel += (s, e) => ResizePreviewImages(e);
        pictureBox2.MouseWheel += (s, e) => ResizePreviewImages(e);

        pictureBox1.DoubleClick += (s, e) => RestoreOriginalPreviewImage();
        pictureBox2.DoubleClick += (s, e) => RestoreOriginalPreviewImage();

        pictureBox1.DragEnter += (s, e) => HandleDragEnter(e);
        pictureBox2.DragEnter += (s, e) => HandleDragEnter(e);

        pictureBox1.DragDrop += (s, e) => HandleDragDrop(e);
        pictureBox2.DragDrop += (s, e) => HandleDragDrop(e);

        FormClosing += (s, e) => Environment.Exit(0);

        guna2TrackBar1.Scroll += (s, e) => label2.Text = $"Brightness (Method 1): {guna2TrackBar1.Value}";
        guna2TrackBar2.Scroll += (s, e) => label3.Text = $"Brightness (Method 2): {guna2TrackBar2.Value}";
        guna2TrackBar3.Scroll += (s, e) => label4.Text = $"Contrast: {guna2TrackBar3.Value}";
        guna2TrackBar4.Scroll += (s, e) => label5.Text = $"Gamma: {guna2TrackBar4.Value}";
        guna2TrackBar5.Scroll += (s, e) => label9.Text = $"Faces blur intensity: {guna2TrackBar5.Value}";

        UpdatePreviewImage();

        for (int i = 0; i < System.IO.Directory.GetFiles("models\\frontal_face_recognition").Length; i++)
        {
            guna2ComboBox1.Items.Add($"Model {i + 1}");
        }

        for (int i = 0; i < System.IO.Directory.GetFiles("models\\profile_face_recognition").Length; i++)
        {
            guna2ComboBox2.Items.Add($"Model {i + 1}");
        }

        guna2ComboBox1.SelectedIndex = 0;
        guna2ComboBox2.SelectedIndex = 0;
        guna2ComboBox3.SelectedIndex = 1;

        DeOldifyArtistic.Initialize();
        DeOldifyStable.Initialize();
    }

    private void ResizePreviewImages(MouseEventArgs e)
    {
        try
        {
            if (pictureBox1.Image == null)
            {
                return;
            }

            if (e.Delta > 0)
            {
                previewWidth += 25;
                previewHeight += 25;

                pictureBox1.Image = Utils.ResizeImageKeepingAspectRatio(originalPreviewImage, previewWidth, previewHeight);
                pictureBox2.Image = pictureBox1.Image;
            }
            else if (e.Delta < 0)
            {
                previewWidth -= 25;
                previewHeight -= 25;

                pictureBox1.Image = Utils.ResizeImageKeepingAspectRatio(originalPreviewImage, previewWidth, previewHeight);
                pictureBox2.Image = pictureBox1.Image;
            }
        }
        catch
        {
            RestoreOriginalPreviewImage();
        }
    }

    private void RestoreOriginalPreviewImage()
    {
        if (pictureBox1.Image == null)
        {
            return;
        }

        pictureBox1.Image = originalPreviewImage;
        pictureBox2.Image = originalPreviewImage;

        previewWidth = originalPreviewImage.Width;
        previewHeight = originalPreviewImage.Height;
    }

    public void HandleDragDrop(DragEventArgs e)
    {
        try
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = ((string[])e.Data.GetData(DataFormats.FileDrop));
                realImage = Image.FromFile(filePaths[0]);
                originalPreviewImage = Utils.ResizeImageKeepingAspectRatio(realImage, pictureBox1.Width, pictureBox1.Height);

                previewWidth = originalPreviewImage.Width;
                previewHeight = originalPreviewImage.Height;

                pictureBox1.Image = originalPreviewImage;
                pictureBox2.Image = originalPreviewImage;

                guna2Button1.Enabled = true;
                realDeOldifyArtistic = null;
                realDeOldifyStable = null;
            }
        }
        catch
        {
            MessageBox.Show("Failed to load the image file.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void HandleDragEnter(DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    public void UpdatePreviewImage()
    {
        new Thread(() =>
        {
            while (true)
            {
                Thread.Sleep(1000);

                if (pictureBox1.Image == null)
                {
                    continue;
                }

                try
                {
                    Image theOriginalImage = realImage;
                    theOriginalImage = ApplyModifications(theOriginalImage);
                    processedRealImage = theOriginalImage;
                    theOriginalImage = Utils.ResizeImageKeepingAspectRatio(theOriginalImage, pictureBox1.Width, pictureBox1.Height);
                    pictureBox2.Image = Utils.ResizeImageKeepingAspectRatio(theOriginalImage, previewWidth, previewHeight);
                }
                catch
                {

                }
            }
        }).Start();
    }

    private void guna2Button1_Click(object sender, EventArgs e)
    {
        if (saveFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            processedRealImage.Save(saveFileDialog1.FileName);
            MessageBox.Show("Processed image succesfully saved!", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public Image ApplyModifications(Image image)
    {
        if (guna2CheckBox8.Checked)
        {
            if (guna2ComboBox3.SelectedIndex == 0)
            {
                if (realDeOldifyArtistic == null)
                {
                    realDeOldifyArtistic = DeOldifyArtistic.Colorize((Bitmap)realImage);
                }

                image = realDeOldifyArtistic;
            }
            else
            {
                if (realDeOldifyStable == null)
                {
                    realDeOldifyStable = DeOldifyStable.Colorize((Bitmap)realImage);
                }

                image = realDeOldifyStable;
            }
        }

        if (guna2CheckBox4.Checked && guna2TrackBar4.Value != 0)
        {
            image = BasicImageProcessing.AdjustGamma((Bitmap)image, (25.0D / guna2TrackBar4.Value));
        }

        if (guna2CheckBox1.Checked && guna2TrackBar1.Value != 0)
        {
            image = BasicImageProcessing.AdjustBrightnessMethod1(image, guna2TrackBar1.Value / 20.0F);
        }

        if (guna2CheckBox2.Checked && guna2TrackBar2.Value != 0)
        {
            image = BasicImageProcessing.AdjustBrightnessMethod2((Bitmap)image, guna2TrackBar2.Value);
        }

        if (guna2CheckBox7.Checked && guna2TrackBar5.Value != 0)
        {
            image = AiImageProcessing.BlurAllFaces(image, guna2ComboBox1.SelectedIndex, guna2ComboBox2.SelectedIndex, guna2TrackBar5.Value);
        }

        if (guna2CheckBox3.Checked && guna2TrackBar3.Value != 0)
        {
            image = BasicImageProcessing.AdjustContrast((Bitmap)image, guna2TrackBar3.Value);
        }

        if (guna2CheckBox5.Checked)
        {
            image = BasicImageProcessing.MakeNegative((Bitmap)image);
        }

        if (guna2CheckBox6.Checked)
        {
            image = BasicImageProcessing.MakeBlackAndWhite((Bitmap)image);
        }

        return image;
    }
}