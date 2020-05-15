﻿using System;
using System.Net.Http;

using Microsoft.Extensions.DependencyInjection;

namespace Refit
{
    public static class HttpClientFactoryExtensions
    {
        /// <summary>
        /// Adds a Refit client to the DI container
        /// </summary>
        /// <typeparam name="T">Type of the Refit interface</typeparam>
        /// <param name="services">container</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddRefitClient<T>(this IServiceCollection services) where T : class
            => AddRefitClient<T>(services, provider => null);

        /// <summary>
        /// Adds a Refit client to the DI container
        /// </summary>
        /// <typeparam name="T">Type of the Refit interface</typeparam>
        /// <param name="services">container</param>
        /// <param name="settings">Optional. Settings to configure the instance with</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddRefitClient<T>(this IServiceCollection services, RefitSettings settings) where T : class
            => AddRefitClient<T>(services, provider => settings);

        /// <summary>
        /// Adds a Refit client to the DI container
        /// </summary>
        /// <typeparam name="T">Type of the Refit interface</typeparam>
        /// <param name="services">container</param>
        /// <param name="settingsFactory">Optional. Settings to configure the instance with</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddRefitClient<T>(this IServiceCollection services, Func<IServiceProvider, RefitSettings> settingsFactory) where T : class
        {
            services.AddSingleton(provider => RequestBuilder.ForType<T>(settingsFactory(provider)));

            return services.AddHttpClient(UniqueName.ForType<T>())
                           .ConfigureHttpMessageHandlerBuilder(builder =>
                           {
                               // check to see if user provided custom auth token
                               HttpMessageHandler innerHandler = null;
                               var settings = settingsFactory(builder.Services);
                               if (settings != null)
                               {
                                   if (settings.HttpMessageHandlerFactory != null)
                                   {
                                       innerHandler = settings.HttpMessageHandlerFactory();
                                   }

                                   if (settings.AuthorizationHeaderValueGetter != null)
                                   {
                                       innerHandler = new AuthenticatedHttpClientHandler(settings.AuthorizationHeaderValueGetter, innerHandler);
                                   }
                                   else if (settings.AuthorizationHeaderValueWithParamGetter != null)
                                   {
                                       innerHandler = new AuthenticatedParameterizedHttpClientHandler(settings.AuthorizationHeaderValueWithParamGetter, innerHandler);
                                   }
                               }

                               if (innerHandler != null)
                               {
                                   builder.PrimaryHandler = innerHandler;
                               }

                           })
                           .AddTypedClient((client, serviceProvider) => RestService.For<T>(client, serviceProvider.GetService<IRequestBuilder<T>>()));
        }

        /// <summary>
        /// Adds a Refit client to the DI container
        /// </summary>
        /// <param name="services">container</param>
        /// <param name="refitInterfaceType">Type of the Refit interface</typeparam>
        /// <returns></returns>
        public static IHttpClientBuilder AddRefitClient(this IServiceCollection services, Type refitInterfaceType)
            => AddRefitClient(services, refitInterfaceType, provider => null);

        /// <summary>
        /// Adds a Refit client to the DI container
        /// </summary>
        /// <param name="services">container</param>
        /// <param name="refitInterfaceType">Type of the Refit interface</typeparam>
        /// <param name="settings">Optional. Settings to configure the instance with</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddRefitClient(this IServiceCollection services, Type refitInterfaceType, RefitSettings settings)
            => AddRefitClient(services, refitInterfaceType, provider => settings);

        /// <summary>
        /// Adds a Refit client to the DI container
        /// </summary>
        /// <param name="services">container</param>
        /// <param name="refitInterfaceType">Type of the Refit interface</typeparam>
        /// <param name="settings">Optional. Settings to configure the instance with</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddRefitClient(this IServiceCollection services, Type refitInterfaceType, Func<IServiceProvider, RefitSettings> settingsFactory)
        {
            return services.AddHttpClient(UniqueName.ForType(refitInterfaceType))
                            .ConfigureHttpMessageHandlerBuilder(builder =>
                            {
                                // check to see if user provided custom auth token
                                HttpMessageHandler innerHandler = null;
                                var settings = settingsFactory(builder.Services);
                                if (settings != null)
                                {
                                    if (settings.HttpMessageHandlerFactory != null)
                                    {
                                        innerHandler = settings.HttpMessageHandlerFactory();
                                    }

                                    if (settings.AuthorizationHeaderValueGetter != null)
                                    {
                                        innerHandler = new AuthenticatedHttpClientHandler(settings.AuthorizationHeaderValueGetter, innerHandler);
                                    }
                                    else if (settings.AuthorizationHeaderValueWithParamGetter != null)
                                    {
                                        innerHandler = new AuthenticatedParameterizedHttpClientHandler(settings.AuthorizationHeaderValueWithParamGetter, innerHandler);
                                    }
                                }

                                if (innerHandler != null)
                                {
                                    builder.PrimaryHandler = innerHandler;
                                }

                            })
                           .AddTypedClient((client, serviceProvider) => RestService.For(refitInterfaceType, client, settingsFactory(serviceProvider)));
        }
    }
}
