using System;
using System.Threading.Tasks;

using Xunit;
using Moq;
using Bogus;
using obs_sample.motor;

namespace obs_sample.tests;

public class MotorTests
{
    [Fact]
    public Task TestObservableAsync()
    {
        Randomizer.Seed = new Random((int)0xDEAD);

        var motor = new Motor();
        var actuatorInterval = TimeSpan.FromMilliseconds(1000);
        var metawareInterval = TimeSpan.FromMilliseconds(10);
        
        var testActuatorBldr = new Faker<Actuator>().StrictMode(false).RuleFor(a => a.received_data, f => f.Random.Number(1, 10));
        var testMetawareBldr = new Faker<MetawareData>().StrictMode(false).RuleFor(a => a.callback_data, f => f.Random.Number(1,10));
        var actuatorTask = Task.Run(async () =>
        {
            foreach(var a in testActuatorBldr.GenerateForever())
            {
                motor.ActuatorDataSubject.OnNext(a);
                await Task.Delay(actuatorInterval);
            }
        });

        var metawareTask = Task.Run(async () =>
        {
            foreach(var m in testMetawareBldr.GenerateForever())
            {
                motor.MetawareDataSubject.OnNext(m);
                await Task.Delay(metawareInterval);
            }
            
        });

        return Task.Delay(-1);
    }
}