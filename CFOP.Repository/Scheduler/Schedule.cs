using System;
using CFOP.Repository.Data;
using FluentScheduler;

namespace CFOP.Repository.Scheduler
{
    public class Schedule : Registry
    {
        public Schedule()
        {
            foreach (var medicationSchedule in Store.AllMedicationSchedules())
            {
                var instruction =
                    $"Take {medicationSchedule.Quantity} {medicationSchedule.Course.Medicine.Unit} of {medicationSchedule.Course.Medicine.Name}. {medicationSchedule.Notes}";
                Schedule(() => Console.WriteLine(instruction)).ToRunEvery(0).Weeks().On(medicationSchedule.DayOfWeek).At(medicationSchedule.Time.Hours, medicationSchedule.Time.Minutes);
            }
        }
    }
}
