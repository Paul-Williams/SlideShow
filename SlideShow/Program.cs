using Microsoft.VisualBasic;
using OneOf.Types;
using PW.Drawing.Imaging;

namespace SlideShow;

internal static class Program
{
  /// <summary>
  ///  The main entry point for the application.
  /// </summary>
  [STAThread]
  static void Main(string[] args)
  {
    // To customize application configuration such as set high DPI settings or default font,
    // see https://aka.ms/applicationconfiguration.
    ApplicationConfiguration.Initialize();
    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
    AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;


    var commandlineValidation = ValidateCommandLine(args);

    if (!commandlineValidation.Success)
    {
      MessageBox.Show(commandlineValidation.Error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      return;
    }



    var playlistResult = new PlayListFactory().Create(args[0]);

    if (!playlistResult.Success)
    {
      MessageBox.Show(playlistResult.Error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      return;
    }

    if (AskUser("Shuffle list?")) playlistResult.PlayList!.Shuffle();
    Application.Run(new MainForm(playlistResult.PlayList!));

  }


  private static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
  {
    Interaction.MsgBox(e.ExceptionObject.ToString() ?? "Unknown Exception", MsgBoxStyle.Critical | MsgBoxStyle.OkOnly, "Unhandled Exception");
    Application.Exit();
  }



  public static bool AskUser(string question)
  {
    return Interaction.MsgBox(question, MsgBoxStyle.Question | MsgBoxStyle.YesNo) == MsgBoxResult.Yes;
  }

  public static (bool Success, string Error) ValidateCommandLine(string[] args) // (Success As Boolean, [Error] As String)
  {
    if ((args.Length == 0))
      return (false, "No command line argument was supplied. The full path to an image or to a directory containing images is required.");

    var commandLine = args[0];


    return Directory.Exists(commandLine) ? (true, string.Empty)
      : !File.Exists(commandLine) == false ? (false, "File not found: " + commandLine)
      : !GdiImageDecoderFormats.IsSupported(Path.GetExtension(commandLine)) ? (false, "File must be of type: " + GdiImageDecoderFormats.All())
      : (true, string.Empty);
  }
}