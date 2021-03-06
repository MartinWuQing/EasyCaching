﻿namespace EasyCaching.Interceptor.Castle
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using global::Castle.DynamicProxy;
    using System;
    using System.Linq;

    /// <summary>
    /// Easycaching interceptor.
    /// </summary>
    public class EasyCachingInterceptor : IInterceptor
    {
        /// <summary>
        /// The cache provider.
        /// </summary>
        private readonly IEasyCachingProvider _cacheProvider;

        /// <summary>
        /// The key generator.
        /// </summary>
        private readonly IEasyCachingKeyGenerator _keyGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Interceptor.Castle.EasyCachingInterceptor"/> class.
        /// </summary>
        /// <param name="cacheProvider">Cache provider.</param>
        /// <param name="keyGenerator">Key generator.</param>
        public EasyCachingInterceptor(IEasyCachingProvider cacheProvider, IEasyCachingKeyGenerator keyGenerator)
        {
            _cacheProvider = cacheProvider;
            _keyGenerator = keyGenerator;
        }

        /// <summary>
        /// Intercept the specified invocation.
        /// </summary>
        /// <returns>The intercept.</returns>
        /// <param name="invocation">Invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            //Process any early evictions 
            ProcessEvict(invocation, true);

            //Process any cache interceptor 
            ProceedAble(invocation);

            // Process any put requests
            ProcessPut(invocation);

            // Process any late evictions
            ProcessEvict(invocation, false);
        }

        /// <summary>
        /// Proceeds the able.
        /// </summary>
        /// <param name="invocation">Invocation.</param>
        private void ProceedAble(IInvocation invocation)
        {
            var serviceMethod = invocation.MethodInvocationTarget ?? invocation.Method;

            var attribute = serviceMethod.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(EasyCachingAbleAttribute)) as EasyCachingAbleAttribute;

            if (attribute != null)
            {
                var cacheKey = _keyGenerator.GetCacheKey(serviceMethod, invocation.Arguments, attribute.CacheKeyPrefix);

                var cacheValue = _cacheProvider.Get<object>(cacheKey);

                if (cacheValue.HasValue)
                {
                    invocation.ReturnValue = cacheValue.Value;
                }
                else
                {
                    // Invoke the method if we don't have a cache hit                    
                    invocation.Proceed();

                    if (!string.IsNullOrWhiteSpace(cacheKey))
                        _cacheProvider.Set(cacheKey, invocation.ReturnValue, TimeSpan.FromSeconds(attribute.Expiration));
                }
            }
            else
            {
                // Invoke the method if we don't have EasyCachingAbleAttribute
                invocation.Proceed();
            }
        }

        /// <summary>
        /// Processes the put.
        /// </summary>
        /// <param name="invocation">Invocation.</param>
        private void ProcessPut(IInvocation invocation)
        {
            var serviceMethod = invocation.MethodInvocationTarget ?? invocation.Method;

            var attribute = serviceMethod.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(EasyCachingPutAttribute)) as EasyCachingPutAttribute;

            if (attribute != null)
            {
                var cacheKey = _keyGenerator.GetCacheKey(serviceMethod, invocation.Arguments, attribute.CacheKeyPrefix);

                _cacheProvider.Set(cacheKey, invocation.ReturnValue, TimeSpan.FromSeconds(attribute.Expiration));
            }
        }

        /// <summary>
        /// Processes the evict.
        /// </summary>
        /// <param name="invocation">Invocation.</param>
        /// <param name="isBefore">If set to <c>true</c> is before.</param>
        private void ProcessEvict(IInvocation invocation, bool isBefore)
        {
            var serviceMethod = invocation.MethodInvocationTarget ?? invocation.Method;

            var attribute = serviceMethod.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(EasyCachingEvictAttribute)) as EasyCachingEvictAttribute;

            if (attribute != null && attribute.IsBefore == isBefore)
            {
                var cacheKey = _keyGenerator.GetCacheKey(serviceMethod, invocation.Arguments, attribute.CacheKeyPrefix);

                _cacheProvider.Remove(cacheKey);
            }
        }
    }
}
