using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;

using System.Diagnostics;

namespace obs_sample.motor;

/// <summary>
/// Low speed actuator
/// </summary>
public class Actuator
{
    public int? received_data { get; set; }
    public DateTime request_time { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// High speed pedometer
/// </summary>
public class MetawareData
{
    public string? state { get; set; }
    public int? callback_data { get; set; }
    public DateTime request_time { get; set; } = DateTime.UtcNow;
}

public class Motor
{
    public Subject<Actuator> ActuatorDataSubject { get; set; } = new();
    public Subject<MetawareData> MetawareDataSubject { get; set; } = new();
    public IDisposable resultObsSubscription;

    public Motor()
    {
        resultObsSubscription = Observable.Zip(
            MetawareDataSubject.Buffer(ActuatorDataSubject),
            ActuatorDataSubject,
            (metaware, actuator) =>
            {
                return new { metaware, actuator };
            }
        ).Subscribe((c)=>{
            var totalMeta = c.metaware.Count;
            var cull = (int)Math.Ceiling(totalMeta * .1);
            var remaining = c.metaware.Skip(cull).Take(totalMeta - 2 * cull);
            Console.WriteLine($"Actuator | Time: {c.actuator.request_time} | {c.actuator.received_data} || Metaware | Total {c.metaware.Count} | Culled {2*cull} | Remaining {remaining.Count()} | Time: {c.metaware.Last().request_time} | {remaining.Average(m => m.callback_data)}");
        });

    }

    public void OnData(MetawareData data)
    {
        MetawareDataSubject.OnNext(data);
    }

    public void OnData(Actuator data)
    {
        ActuatorDataSubject.OnNext(data);
    }

}
