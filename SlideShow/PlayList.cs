using PW.Collections;

namespace SlideShow;

internal class PlayList : CursoredList<string>
{
  public PlayList(IEnumerable<string> filePaths)
  {
    AddRange(filePaths);
  }
}
