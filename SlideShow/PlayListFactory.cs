using CSharpFunctionalExtensions;
using PW.Drawing.Imaging;

namespace SlideShow;
internal static class PlayListFactory
{



  private static bool IsSupportedImage(FileInfo file)
  {
    var ext = file.Extension;

    return string.Compare(ext, ".webp", true) == 0 || GdiImageDecoderFormats.IsSupported(ext);
  }


  public static Result<PlayList> Create(string fileOrDirectoryPath)
  {
    try
    {
      // If the path is to a file we only want its container directory, otherwise use the full path
      var directoryPath = new DirectoryInfo(Directory.Exists(fileOrDirectoryPath) ? fileOrDirectoryPath : Path.GetDirectoryName(fileOrDirectoryPath)!);


      var filePaths = directoryPath
        .EnumerateFiles("*", new EnumerationOptions() { IgnoreInaccessible = true, RecurseSubdirectories = true})
        .Where(IsSupportedImage)
        .Select(x => x.FullName)
        .ToArray();

      // EnumerateFiles() does not always properly sort file name sequences containing numbers.
      Array.Sort(filePaths, PW.Collections.StringNaturalComparer.AscendingComparer);

      var playList = new PlayList(filePaths);

      return playList.Count != 0
        ? playList
        : Result.Failure<PlayList>("No pictures were found.");
    }
    catch (Exception ex)
    {
      return Result.Failure<PlayList>(ex.Message);
    }
  }

}
