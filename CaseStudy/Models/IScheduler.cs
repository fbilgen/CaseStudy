using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaseStudy.Models
{
    public interface IScheduler
    {
        Task HandleScheduleAsync();
    }
}
