using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TumanaDeveloped.Core.interfaces;
using TumanaDeveloped.Core.Services;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace TumanaDeveloped.Core
{
    public class RegisterServicesComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<IEmailService, EmailService>(Lifetime.Request);
        }
    }
}
