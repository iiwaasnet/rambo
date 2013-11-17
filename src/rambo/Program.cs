using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Configuration;
using Topshelf;
using Topshelf.HostConfigurators;

namespace rambo
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationSettingsReader());

            var container = builder.Build();

            HostFactory.New(x => ConfigureService(x, container)).Run();
        }

        private static void ConfigureService(HostConfigurator x, IContainer container)
        {
            x.Service<ServiceControl>(s =>
            {
                s.ConstructUsing(name => container.Resolve<ServiceControl>());
                s.WhenStarted((sc, hc) => sc.Start(hc));
                s.WhenStopped((sc, hc) => sc.Stop(hc));
            });
            x.RunAsLocalSystem();

            x.SetDescription("Rambo");
            x.SetDisplayName("Rambo");
            x.SetServiceName("Rambo");

            //x.AfterInstall(InstallPerfCounters);
            //x.BeforeUninstall(UninstallPerfCounters);
        }
    }
}
