using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Activation;
using Ninject.Parameters;
using OxTailHelpers;

namespace OxTail
{
    public class ExpressionFactory : IExpressionFactory
    {
        private readonly IKernel Kernel;

        public ExpressionFactory(IKernel kernel)
        {
            this.Kernel = kernel;
        }

        public IExpression CreateFile(int id, string text, string name)
        {
            IRequest req = Kernel.CreateRequest(typeof(IExpression), null, new IParameter[] { new Parameter("id", id, false), new Parameter("text", text, false), new Parameter("name", name, false) }, false, false);

            // I don't know if I've missed the point here or not, but I anticipated that each time i called Bind<> I'd get a different instance
            // of the class put in the kernel with those passed in parameter values. However I don't I get the same class back each time
            // with the same values, so the only way to create a new instance is to Unbind<> the original instance and add another with the 
            // new params?
            if (!this.Kernel.CanResolve(req))
            {
                Kernel.Bind<IExpression>().To<OxTailHelpers.Expression>().InTransientScope()
                    .WithConstructorArgument("id", id)
                    .WithConstructorArgument("text", text)
                    .WithConstructorArgument("name", name);
            }
            
            IExpression expr = this.Kernel.Get<IExpression>();
            Kernel.Unbind<IExpression>();

            return expr;
        }
    }
}
