using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace ASPWeb.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;
        public string Message { get; set; }

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }


        public void OnGet()
        {
            Message = "Введите сумму";
        }

        public IActionResult OnPost()
        {
            string url = "/";
            var form = HttpContext.Request.Form;
            //if (form.ContainsKey("logout"))
            //{
            //    HttpContext.Session.SetString("logged", "false");
            //    url = "/";
            //}

            return Redirect(url);
        }
    }
}