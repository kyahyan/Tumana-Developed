using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using TumanaDeveloped.Core.interfaces;
using TumanaDeveloped.Core.ViewModel;
using Umbraco.Web.Mvc;

namespace TumanaDeveloped.Core.Controller
{
    public class RegisterController : SurfaceController
    {

        private const string PARTIAL_VIEW_FOLDER = "~/Views/Partials/";
        private IEmailService _emailService;

        #region Register Form

        public RegisterController(IEmailService emailService)
        {
            _emailService = emailService;
        }

         public ActionResult RenderRegister()
        {
            var vm = new RegisterViewModel();
            return PartialView(PARTIAL_VIEW_FOLDER + "Register.cshtml", vm);
                
        }

        public ActionResult HandleRegister(RegisterViewModel vm)
        {
            if(!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            var existingMember = Services.MemberService.GetByEmail(vm.Email);

            if(existingMember != null)
            {
                ModelState.AddModelError("Account Error", "There's already a user with that email address.");
                return CurrentUmbracoPage();
            }

            existingMember = Services.MemberService.GetByUsername(vm.Username);

            if (existingMember != null)
            {
                ModelState.AddModelError("Account Error", "There's already a user with that username.");
                return CurrentUmbracoPage();
            }

            var newMember = Services.MemberService.CreateMember(vm.Username, vm.Email, $"{vm.FirstName} {vm.LastName}", "Member");

            newMember.PasswordQuestion = "";
            newMember.RawPasswordAnswerValue = "";

            Services.MemberService.Save(newMember);
            Services.MemberService.SavePassword(newMember, vm.Password);

            Services.MemberService.AssignRole(newMember.Id, "Normal User");


            var token = Guid.NewGuid().ToString();
            newMember.SetValue("emailVerifyToken", token);
            Services.MemberService.Save(newMember);

            _emailService.SendVerifyEmailAddressNotification(newMember.Email, token);


            TempData["status"] = "OK";
            return RedirectToCurrentUmbracoPage();
        }

        #endregion

        #region Verification

        public ActionResult RenderEmailVerification(string token)
        {
            var member = Services.MemberService.GetMembersByPropertyValue("emailVerifyToken", token).SingleOrDefault();
            
            if (member != null)
            {
                var alreadyVerified = member.GetValue<bool>("emailVerified");
                if(alreadyVerified)
                {
                    ModelState.AddModelError("Verified", "You've already verified your email address thanks!");
                    return CurrentUmbracoPage();

                }
                member.SetValue("emailVerified", true);
                member.SetValue("emailVerifiedDate", DateTime.Now);
                Services.MemberService.Save(member);

                TempData["status"] = "OK";
                return PartialView(PARTIAL_VIEW_FOLDER + "EmailVerification.cshtml");


            }
            ModelState.AddModelError("Error", "Apologies there has been some problem!");
            return PartialView(PARTIAL_VIEW_FOLDER + "EmailVerification.cshtml");
        }

        #endregion
    }
}
