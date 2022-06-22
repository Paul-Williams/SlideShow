using CSharpFunctionalExtensions;
using PW.IO.FileSystemObjects;
using System.IO;

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


    ValidateCommandLine(args)
      .Bind(() => PlayListFactory.Create(args[0]))
      .Tap(playlist => { if (AskUser("Shuffle list?")) playlist.Shuffle(); })
      .Match(
        onSuccess: playlist => Application.Run(new MainForm(playlist)),
        onFailure: ShowError);

  }


  private static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
  {
    ShowError(e.ExceptionObject.ToString() ?? "Unknown Exception");
    Application.Exit();
  }


  public static void ShowError(string error) => MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);


  public static bool AskUser(string question) =>
    MessageBox.Show(question, "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;


  private static Result ValidateCommandLine(string[] args)
  {
    return Result
    .SuccessIf(args.Length != 0, "No command line argument was supplied. The full path to an image or to a directory containing images is required.")
    .Bind(() => Result.SuccessIf(new FileSystemPath(args[0]).Exists, "Directory or File not found: " + args[0]));
  }
}