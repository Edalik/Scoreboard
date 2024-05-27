using System.Threading;
using System.Threading.Tasks;

namespace Scoreboard.Modules.Main.Models.Abstractions;

internal interface IMainService
{
    /// <summary>
    ///     Считать видео
    /// </summary>
    /// <param name="mainModel"></param>
    Task CaptureVideo(IMainModel mainModel, CancellationToken cancellationToken, string? fileName);
}
