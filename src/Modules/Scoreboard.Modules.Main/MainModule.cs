using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Scoreboard.Modules.Main.Models;
using Scoreboard.Modules.Main.Models.Abstractions;
using Scoreboard.Modules.Main.ViewModels;
using Scoreboard.Modules.Main.Views;

namespace Scoreboard.Modules.Main;

public class MainModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion("MainRegion", typeof(MainView));
        regionManager.RegisterViewWithRegion("OscRegion", typeof(OscView));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IMainModel, MainModel>();
        ViewModelLocationProvider.Register<MainView, MainViewModel>();
        ViewModelLocationProvider.Register<OscView, MainViewModel>();
    }
}
