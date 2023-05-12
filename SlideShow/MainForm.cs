using Microsoft.VisualBasic;
using PW.Extensions;
using System.Diagnostics;

namespace SlideShow;

internal partial class MainForm : Form
{
  public MainForm()
  {
    InitializeComponent();
    PlayList = null!;
  }


  const int InitialTimerInterval = 2000;
  const int SlideTimerIncrement = 250;

  private PlayList PlayList { get; }

  // Private ReadOnly Property FileToImage As New PW.Converter(Of File, Image)(Function() PlayList.Current, Function(file) New Bitmap(file.Path, True))

  public MainForm(PlayList playList)
  {
    InitializeComponent();
    Icon = Properties.Resources.SlideShowIcon;
    PlayList = playList ?? throw new ArgumentNullException(nameof(playList));
    AttachEventHandlers();
  }

  private void AttachEventHandlers()
  {
    Load += Form_Load;
    KeyUp += Form_KeyUp;

    PictureBox.DoubleClick += PictureBox_DoubleClick;
    PictureBox.MouseDown += PictureBox_MouseDown;
    PictureBox.Paint += PictureBox_Paint;

    ImageDisplayTimer.Tick += SlideTimer_Tick;

    PlayList.PositionChanged += (s, e) => RenderCurrentImage();
  }


  private static void ShowUserException(Exception ex)
  {
    if (ex == null)
      return;
  }

  private void SlideTimer_Tick(object? sender, EventArgs e)
  {
    try
    {
      PlayList.MoveNext();
    }
    catch (Exception ex)
    {
      ShowUserException(ex);
    }
  }

  public void RenderCurrentImage()
  {
    try
    {
      var newImage = LoadImage(PlayList.Current);
      var oldImage = PictureBox.Image;
      PictureBox.Image = newImage;

      oldImage?.Dispose();
    }
    catch (Exception ex)
    {
      ShowUserException(ex);
    }
  }

  private static Image LoadImage(string filePath) =>
    Path.GetExtension(filePath) == ".webp" ? PW.WebP.WebPDecoder.Load(filePath) : Image.FromFile(filePath);


  private void Form_KeyUp(object? sender, KeyEventArgs e)
  {
    KeyboardHandler(e.KeyCode);
  }

  private void DeleteCurrentImageFile()
  {
    try
    {
      var timerWasEnabled = ImageDisplayTimer.Enabled;
      ImageDisplayTimer.Stop();

      var filePathToDelete = PlayList.Current;

      //if (Program.AskUser("Delete current image?" + Constants.vbCrLf + Constants.vbCrLf + filePathToDelete))
      //{
      // There is a potential race condition here
      // If _PlayList.Current changes between calls to _playList.Current.Path and _playList.RemoveCurrent()
      // The condition would not occur if there were a _playlist.Remove(file) method.
      PlayList.RemoveCurrent();

      // File is kept open while image is displayed, so we must dispose the current image before deleting
      // NB: This has not been tested since changing displaying the image using PictureBox.Image to PictureBox.ImageLocation
      DisposePictureBoxImage();
      PW.IO.FileSystemObjects.FileSystem.SendFileToRecycleBin(filePathToDelete);
      RenderCurrentImage();
      //}
      if (timerWasEnabled) ImageDisplayTimer.Start();
    }
    catch (Exception ex)
    {
      Interaction.MsgBox("Delete current image failed: " + ex.Message, MsgBoxStyle.Exclamation);
    }
  }

  private void DisposePictureBoxImage()
  {
    if (PictureBox.Image != null)
    {
      using (PictureBox.Image)
        PictureBox.Image = null;
    }
  }

  private void ShutDown()
  {
    ImageDisplayTimer.Enabled = false;
    ImageDisplayTimer.Dispose();
    Close();
  }

  private void ToggleSlideShowEnabled()
  {
    ImageDisplayTimer.Enabled = !ImageDisplayTimer.Enabled;
  }

  private void ReduceFrameRate()
  {
    ImageDisplayTimer.Interval += SlideTimerIncrement;
  }

  private void IncreaseFrameRate()
  {
    ImageDisplayTimer.Interval = Math.Max(1, ImageDisplayTimer.Interval - SlideTimerIncrement);
  }

  private void MoveNext() => PlayList.MoveNext();
  private void MoveBack() => PlayList.MoveBack();

  private void KeyboardHandler(Keys key)
  {

    Action? f = key switch
    {
      Keys.Delete => DeleteCurrentImageFile,
      Keys.Escape or Keys.End => ShutDown,
      Keys.Pause or Keys.Space => ToggleSlideShowEnabled,
      Keys.Right => MoveNext,
      Keys.Left => MoveBack,
      Keys.Down => ReduceFrameRate,
      Keys.Up => IncreaseFrameRate,
      _ => null
    };


    f?.Invoke();

  }


  private void Form_Load(object? sender, EventArgs e)
  {
    try
    {
      RenderCurrentImage();
      ImageDisplayTimer.Interval = InitialTimerInterval;
      ImageDisplayTimer.Enabled = true;
    }
    catch (Exception ex)
    {
      Interaction.MsgBox(ex.Message);
    }
  }

  private void PictureBox_Paint(object? sender, PaintEventArgs e)
  {
    var txt = "[" + PlayList.Position + 1 + " of " + PlayList.Count + "] " + PlayList.Current;
    e.Graphics.DrawString(txt, Font, Brushes.WhiteSmoke, 1, 1);
  }

  private void PictureBox_DoubleClick(object? sender, EventArgs e)
  {
    try
    {
      ImageDisplayTimer.Enabled = false;
      var argument = "/select, " + PlayList.Current.Enquote();
      Process.Start("explorer.exe", argument);
    }
    catch (Exception ex)
    {
      ShowUserException(ex);
    }
  }

  private void PictureBox_MouseDown(object? sender, MouseEventArgs e)
  {
    if (e.Button == MouseButtons.Middle)
      Close();
  }
}