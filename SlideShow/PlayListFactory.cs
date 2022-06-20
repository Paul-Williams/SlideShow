using PW.Drawing.Imaging;

namespace SlideShow;
internal class PlayListFactory
{



  private static bool IsSupportedImage(FileInfo file)
  {
    var ext = file.Extension;

    return string.Compare(ext, ".webp", true) == 0 || GdiImageDecoderFormats.IsSupported(ext);
  }


  public (bool Success, string Error, PlayList? PlayList) Create(string fileOrDirectoryPath)
  {
    try
    {
      // If the path is to a file we only want its container directory, otherwise use the full path
      var directoryPath = new DirectoryInfo(Directory.Exists(fileOrDirectoryPath) ? fileOrDirectoryPath : Path.GetDirectoryName(fileOrDirectoryPath)!);


      var filePaths = directoryPath
        .EnumerateFiles("*", new EnumerationOptions() { IgnoreInaccessible = true, RecurseSubdirectories = true})
        .Where(IsSupportedImage)
        .Select(x => x.FullName);

      var playList = new PlayList(filePaths);

      return playList.Count != 0
        ? (true, string.Empty, playList)
        : (false, "No pictures were found.", null);
    }
    catch (Exception ex)
    {
      return (false, ex.Message, null);
    }
  }

}
