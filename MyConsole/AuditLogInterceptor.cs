using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Castle.DynamicProxy;
using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    internal class AuditLogInterceptor : IInterceptor
    {
        private readonly ILogger _logger;
        private readonly IContext _context;

        public AuditLogInterceptor(ILogger logger, IContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void Intercept(IInvocation invocation)
        {
            if (!(Attribute.GetCustomAttribute(invocation.Method, typeof(AuditLogAttribute)) is AuditLogAttribute
                auditLogAttribute))
            {
                invocation.Proceed();
            }
            else
            {
                var currentUser = _context.GetCurrentUser();
                var parameters = string.Join("|", invocation.Arguments.Select(x => (x ?? "").ToString()));

                _logger.Info($"user:{currentUser.Name} invoke with parameters:{parameters}");
                invocation.Proceed();
                var returnValue = invocation.ReturnValue;

                _logger.Info(returnValue.ToString());
            }
        }
    }

    internal interface IContext
    {
        Account GetCurrentUser();
        void SetCurrentUser(Account account);
    }

    public class Account
    {
        public string Name { get; set; }
    }
}