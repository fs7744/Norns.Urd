using System.Threading.Tasks;

namespace Norns.Urd.Utils
{
    public static class TaskUtils
    {
        public static readonly ValueTask CompletedValueTask = new ValueTask(Task.CompletedTask);
    }
}