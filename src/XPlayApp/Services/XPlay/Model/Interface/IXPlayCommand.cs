using XPlayApp.Services.XPlay.Enum;

namespace XPlayApp.Services.XPlay.Model
{
    public interface IXPlayCommand
    {
        string id { get; set; }

        CommandType type { get; set; }
    }
}
