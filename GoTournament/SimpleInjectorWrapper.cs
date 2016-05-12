namespace GoTournament
{
    using System;

    using GoTournament.Interface;

    using SimpleInjector;

    public class SimpleInjectorWrapper : ISimpleInjectorWrapper
    {
        private readonly Container container;

        public SimpleInjectorWrapper(Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            this.container = container;
        }

        public TService GetInstance<TService>() where TService : class
        {
            return this.container.GetInstance<TService>();
        }
    }
}