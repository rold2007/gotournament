namespace GoTournament.UnitTest
{
    using System;

    using GoTournament.Interface;
    using GoTournament.Service;

    using SimpleInjector;

    using Xunit;

    public class SimpleInjectorWrapperTest
    {
        [Fact]
        public void SimpleInjectorWrapperCtorTest()
        {
            ISimpleInjectorWrapper injector = null;
            try
            {
                injector = new SimpleInjectorWrapper(null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: container", ex.Message);
            }

            Container cont = new Container();
            injector = new SimpleInjectorWrapper(cont);
            Assert.NotNull(injector);
        }

        [Fact]
        public void SimpleInjectorWrapperGetInstanceTest()
        {
            Container cont = new Container();
            ISimpleInjectorWrapper injector = new SimpleInjectorWrapper(cont);
            cont.Register<IFileService, FileService>();
            var service = injector.GetInstance<IFileService>();
            Assert.NotNull(service);
            Assert.IsType(typeof(FileService), service);

            try
            {
                var notRegistered = injector.GetInstance<IJsonService>();
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ActivationException), ex);
                Assert.Equal("No registration for type IJsonService could be found.", ex.Message);
            }
        }
    }
}