namespace Sino.Extensions.Apollo.Core.Schedule
{
    public interface ISchedulePolicy
    {
        int Fail();
        void Success();
    }
}
