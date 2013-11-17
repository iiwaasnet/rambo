using Autofac;
using rambo.Implementation;
using rambo.Interfaces;
using rambo.Messaging;
using rambo.Messaging.Inproc;
using Topshelf;

namespace rambo
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<InprocMessageHub>().As<IMessageHub>().SingleInstance();
            builder.RegisterType<MessageSerializer>().As<IMessageSerializer>().SingleInstance();
            builder.Register(c => new Logger("fileLogger")).As<ILogger>().SingleInstance();
            builder.Register(c => new Configuration(new ConfigurationIndex(0),
                                                    new[]
                                                    {
                                                        new Node(1),
                                                        new Node(2),
                                                        new Node(3)
                                                    }))
                   .As<IConfiguration>()
                   .SingleInstance();
            builder.RegisterType<RamboFactory>().As<IRamboFactory>().SingleInstance();
            builder.RegisterType<RamboService>().As<ServiceControl>().SingleInstance();
        }
    }
}