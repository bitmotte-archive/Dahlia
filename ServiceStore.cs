public static class ServiceStore
{
    private static Dictionary<Type, IService> _serviceStore = [];

    public static T StoreService<T>(T service) where T: IService
    {
        var serviceType = typeof(T);
        var serviceClass = serviceType;
        _serviceStore.Add(serviceClass, service);
        service.Init();
        return service;
    }

    public static T GetService<T>() where T: IService
    {
        var serviceType = typeof(T);
        return (T)_serviceStore[serviceType];
    }
}