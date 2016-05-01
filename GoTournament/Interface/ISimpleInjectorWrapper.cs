namespace GoTournament.Interface
{
    public interface ISimpleInjectorWrapper
    {
        TService GetInstance<TService>() where TService : class;
    }
}