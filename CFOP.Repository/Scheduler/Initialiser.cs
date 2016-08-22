using FluentScheduler;

namespace CFOP.Repository.Scheduler
{
    public static class Initialiser
    {
        public static void init()
        {
            JobManager.Initialize(new Schedule());
        }
    }
}
