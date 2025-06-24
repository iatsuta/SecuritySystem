namespace SecuritySystem.Configurator;

public interface IConfiguratorSetup
{
    IConfiguratorSetup AddModule(IConfiguratorModule module);
}
