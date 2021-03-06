﻿using System.Web.Http;
using Adapter.Notification.Email;
using Adapter.Persistence.MySql;
using Domain.UseCases;
using Host.WebService.Client1.BookOrders;
using Owin;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using Swashbuckle.Application;

namespace Host.WebService.Client1
{
    public class Startup
    {
        protected Container Container;

        public Startup()
        {
            Container = new Container();
        }

        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();

            RegisterPersistenceAdapter();
            RegisterNotificationAdapter();
            RegisterControllers();
            RegisterHostAdapter();

            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(Container);

            config.MapHttpAttributeRoutes();
            config.Routes.IgnoreRoute("IgnoreAxdFiles", "{resource}.axd/{*pathInfo}");

            config.EnableSwagger(c => c.SingleApiVersion("v1", "BookOrders Client 1"))
                .EnableSwaggerUi();

            config.EnsureInitialized();

            appBuilder.UseWebApi(config);
        }

        private void RegisterControllers()
        {
            Container.Register<BookOrdersController>();
        }

        /// <summary>
        /// Wire upstream ports to host implementations, in this case
        /// we are registering the ports (use cases) in the IoC container
        /// as the Controllers are the host adapter implementations as they
        /// call into the use cases
        /// </summary>
        private void RegisterHostAdapter()
        {
            Container.Register<AddBookTitleRequestUseCase>();
            Container.Register<ApproveBookOrderUseCase>();
            Container.Register<SendBookOrderUseCase>();
            Container.Register<GetAllBookOrdersUseCase>();
            Container.Register<DeleteBookOrdersUseCase>();
            Container.Register<SupplierBookOrderUpdateUseCase>();
        }

        protected virtual void RegisterNotificationAdapter()
        {
            var notificationAdapter = new Adapter.Notification.Email.NotificationAdapter(
                new NotificationAdapterSettings(
                    "localhost", 1025, bookSupplierEmail: "BookSupplierGateway@fakedomain.com"));
            notificationAdapter.Initialize();
            notificationAdapter.Register(Container);
        }

        protected virtual void RegisterPersistenceAdapter()
        {
            RegisterMySqlPersistenceAdapter();
        }

        private void RegisterMySqlPersistenceAdapter()
        {
            var persistenceAdapter = new Adapter.Persistence.MySql.PersistenceAdapter(
                new PersistenceAdapterSettings()
                {
                    ConnectionString = "server=127.0.0.1;" +
                                       "uid=bookorder_srv;" +
                                       "pwd=123;" +
                                       "database=bookorders"
                });

            persistenceAdapter.Initialize();
            persistenceAdapter.Register(Container);
        }
    }
}
