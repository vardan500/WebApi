using System;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace TestApi.Helpers
{
    public class CorrelationIdEnricher : ILogEventEnricher
    {
        private IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdEnricher()
        {
            _httpContextAccessor = new HttpContextAccessor();
        }

        public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException("logEvent");
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return;
            }

            string correlationId = httpContext.Request.Headers["correlationId"];

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                return;
            }

            var correlationIdProperty = new LogEventProperty("correlationId", new ScalarValue(correlationId));
            logEvent.AddPropertyIfAbsent(correlationIdProperty);
        }
    }
}