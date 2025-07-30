using Microsoft.AspNetCore.Mvc;
using Pulse360.Data;
using Pulse360.Models;
using SelectPdf;
using System.Globalization;
using System.Net.Mail;
using System.Net;

namespace Pulse360.Controllers
{
    public class ResignController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment env;
        public ResignController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult resign(string templateName)
        {
            ViewData["SelectedTemplate"] = templateName;
            return View();
        }

        [HttpPost]
        public IActionResult resign(Resign r, string Template)
        {
            var cd = db.Organization.FirstOrDefault();
            if (cd != null)
            {
                string data;
                if (Template.Equals("templet1"))
                {
                    data = templet1(cd, r);
                    var d = ConvertHtmlToPdf(data, r.employeeEmail);
                    return View();
                }
                else if (Template.Equals("templet2"))
                {
                    data = templet2(cd, r);
                    var d = ConvertHtmlToPdf(data, r.employeeEmail);
                    TempData["success"] = "Resignation letter is successfully generated and sent on given email Id";
                    return View();
                }
                else if (Template.Equals("templet3"))
                {
                    data = templet3(cd, r);
                    var d = ConvertHtmlToPdf(data, r.employeeEmail);
                    TempData["success"] = "Resignation letter is successfully generated and sent on given email Id";
                    return View();
                }
                else if (Template.Equals("templet4"))
                {
                    data = templet4(cd, r);
                    var d = ConvertHtmlToPdf(data, r.employeeEmail);
                    TempData["success"] = "Resignation letter is successfully generated and sent on given email Id";
                    return View();
                }
            }
            else
            {
                return RedirectToAction("CompanyDetails");
            }
            return View();
        }


        public string templet1(Organization cd, Resign r)
        {
            string htmlContent = $@"
            <!DOCTYPE html>
            <html lang=""en"">
            <head>
                <style>
                    /* General Styles */
                    body {{
                        margin: 0;
                        padding: 0;
                        font-family: Arial, sans-serif;
                        background-color: #f9f6ef;
                        color: #333;
                    }}

                    /* Container */
                    .container {{
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        min-height: 100vh;
                        padding: 20px;
                    }}

                    /* Letter Styles */
                    .letter {{
                        background-color: #ffffff;
                        padding: 40px;
                        border-radius: 8px;
                        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                        max-width: 700px;
                        line-height: 1.6;
                    }}

                    h1 {{
                        font-size: 28px;
                        margin-bottom: 20px;
                        font-weight: normal;
                    }}

                    h1 .highlight {{
                        color: #c9a90f;
                        font-weight: bold;
                    }}

                    p {{
                        margin: 10px 0;
                    }}

                    p.signature {{
                        font-family: ""Georgia"", serif;
                        font-size: 18px;
                        font-style: italic;
                    }}
                </style>    
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Resignation Letter</title>
                <link rel=""stylesheet"" href=""style.css"">
            </head>
            <body>
                <div class=""container"">
                    <div class=""letter"">
                        <h1><span class=""highlight"">{r.employeeName}</span><br>Resignation Letter</h1>
                        <p><strong>{r.resignationDate}</strong></p>
                        <p>
                            {cd.OrganizationName}<br>
                            {r.departmentHead}<br>
                            
                            {cd.OrganizationAddress}<br>
                        </p>

                        <p><strong>To {cd.OrganizationName}:</strong></p>

                        <p>
                            Kindly accept this letter as my formal resignation as a staff member of {cd.OrganizationName}. My last day is expected to be on {r.lastDay}, two weeks from today.
                        </p>

                        <p>
                            I am incredibly grateful for the opportunities that I have been given in this post. I value the insights that I have learned, and I expect them to help me in my future endeavors. I would also like to thank you for being supportive of my professional growth. I have enjoyed working with you and the rest of the team.
                        </p>

                        <p>
                            Let me know how I can help in making the transition of responsibilities as seamless as possible for everyone involved. Thank you.
                        </p>

                        <p>Best wishes,</p>

                        <p class=""signature"">{r.employeeName}</p>
                        <p>{r.employeeName}</p>
                    </div>
                </div>
            </body>
            </html>
            ";

            return htmlContent;
        }

        public string templet2(Organization cd, Resign r)
        {
            string htmlContent = $@"
            <!DOCTYPE html>
            <html lang=""en"">
            <head>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        margin: 0;
                        padding: 20px;
                        background-color: #f8f8f8;
                    }}

                    .letter-container {{
                        max-width: 800px;
                        margin: auto;
                        background: white;
                        padding: 20px;
                        border: 1px solid #ddd;
                        border-radius: 8px;
                        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                    }}

                    .header {{
                        text-align: center;
                        margin-bottom: 20px;
                    }}

                    .header h1 {{
                        font-size: 24px;
                        color: #4a4a8c;
                        margin: 0;
                    }}

                    .subheading {{
                        font-size: 18px;
                        color: #7e7ebc;
                        margin: 5px 0 0;
                    }}

                    .content p {{
                        margin: 10px 0;
                    }}

                    .signature {{
                        font-style: italic;
                        color: #4a4a8c;
                        margin: 10px 0;
                    }}

                    .footer {{
                        margin-top: 30px;
                        text-align: left;
                        font-size: 14px;
                        color: #555;
                    }}

                    .footer p {{
                        margin: 5px 0;
                    }}

                    .icon {{
                        margin-right: 8px;
                        color: #7e7ebc;
                    }}
                </style>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Resignation Letter</title>
                <link rel=""stylesheet"" href=""styles.css"">
            </head>
            <body>
                <div class=""letter-container"">
                    <header class=""header"">
                        <h1>{r.employeeName}</h1>
                        <p class=""subheading"">Resignation Letter</p>
                    </header>

                    <section class=""content"">
                        <p>{r.resignationDate}</p>
                        <p>{cd.OrganizationName}</p>
                        <p>hiringManagerName</p>
                        <p>{cd.OrganizationName}</p>

                        <p>To {cd.OrganizationName}:</p>
                        <p>
                            Kindly accept this letter as my formal resignation as a staff member of {cd.OrganizationName}. 
                            My last day is expected to be on {r.lastDay}.
                        </p>
                        <p>
                            I am incredibly grateful for the opportunities that I have been given in this post. I would also like 
                            to thank you for being supportive of my professional growth. [Highlight moments or people during your tenure that you are thankful for].
                        </p>
                        <p>
                            Let me know how I can help in making the transition of responsibilities as seamless as possible for everyone involved.
                        </p>
                        <p>Best wishes,</p>
                        <p class=""signature"">{r.employeeName}</p>
                        <p>{r.employeeName}</p>
                    </section>

                    <footer class=""footer"">
                        <p><span class=""icon"">📍</span> {cd.OrganizationName}</p>
                        <p><span class=""icon"">📞</span> {cd.OrganizationPhone}</p>
                        <p><span class=""icon"">✉</span> {cd.OrganizationEmail}</p>
                    </footer>
                </div>
            </body>
            </html>
            ";
            return htmlContent;
        }

        public string templet3(Organization cd, Resign r)
        {
            string htmlcontent = $@"
                <!DOCTYPE html>
                <html lang=""en"">
                <head>
                    <style>
                        /* General Styles */
                        body {{
                            margin: 0;
                            padding: 0;
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f9;
                            color: #333;
                        }}

                        /* Container */
                        .container {{
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            min-height: 100vh;
                            padding: 20px;
                        }}

                        /* Letter Styles */
                        .letter {{
                            background-color: #ffffff;
                            padding: 40px;
                            border-radius: 8px;
                            box-shadow: 0 6px 12px rgba(0, 0, 0, 0.1);
                            max-width: 700px;
                            line-height: 1.6;
                            border: 6px solid #ff5733;  /* Colorful Border */
                        }}

                        /* Header */
                        .logo {{
                            text-align: center;
                            margin-bottom: 20px;
                        }}
                        .logo img {{
                            max-width: 150px;
                        }}

                        h1 {{
                            font-size: 30px;
                            color: #3b5998;
                            margin-bottom: 20px;
                            font-weight: bold;
                        }}

                        h1 .highlight {{
                            color: #ff5733;
                            font-weight: normal;
                        }}

                        p {{
                            margin: 15px 0;
                            font-size: 16px;
                        }}

                        p.signature {{
                            font-family: ""Georgia"", serif;
                            font-size: 18px;
                            font-style: italic;
                        }}

                        /* Footer */
                        .footer {{
                            margin-top: 40px;
                            text-align: center;
                            font-size: 14px;
                            color: #888;
                        }}
                    </style>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Resignation Letter</title>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""letter"">
                            <h1><span class=""highlight"">{r.employeeName}</span><br>Resignation Letter</h1>
                            <p><strong>Date of Resignation: {r.resignationDate}</strong></p>
                            <p>
                                {cd.OrganizationName}<br>
                                {r.departmentHead}<br>
                                {cd.OrganizationAddress}<br>
                            </p>

                            <p><strong>Dear {cd.OrganizationName},</strong></p>

                            <p>
                                I am writing to formally resign from my position at {cd.OrganizationName}, effective {r.lastDay}. This decision has not been an easy one, but after careful consideration, I believe it is the right step for my personal and professional growth.
                            </p>

                            <p>
                                I am deeply grateful for the opportunities and experiences I’ve had while being a part of the {cd.OrganizationName} team. Your guidance and the incredible support from my colleagues have played a pivotal role in my career development, and I will always cherish these memories.
                            </p>

                            <p>
                                Please let me know how I can assist in the transition process and ensure a smooth handover of my responsibilities.
                            </p>

                            <p>
                                Once again, thank you for everything, and I wish the team continued success in the future.
                            </p>

                            <p>Sincerely,</p>

                            <p class=""signature"">{r.employeeName}</p>
                            <p class=""footer"">This letter is provided in accordance with the terms of your employment agreement with {cd.OrganizationName}.</p>
                        </div>
                    </div>
                </body>
                </html>
                ";

            return htmlcontent;

        }

        public string templet4(Organization cd, Resign r)
        {
            string htmlcontent = $@"
                <!DOCTYPE html>
                <html lang=""en"">
                <head>
                    <style>
                        /* General Styles */
                        body {{
                            margin: 0;
                            padding: 0;
                            font-family: Arial, sans-serif;
                            background-color: #f8f8f8;
                            color: #333;
                        }}

                        /* Container */
                        .container {{
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            min-height: 100vh;
                            padding: 20px;
                        }}

                        /* Letter Styles */
                        .letter {{
                            background-color: #ffffff;
                            padding: 40px;
                            border-radius: 8px;
                            box-shadow: 0 6px 12px rgba(0, 0, 0, 0.1);
                            max-width: 700px;
                            line-height: 1.6;
                            border-left: 8px solid #1e90ff;  /* Single side colored border */
                        }}

                        /* Header */
                        .logo {{
                            text-align: center;
                            margin-bottom: 20px;
                        }}
                        .logo img {{
                            max-width: 120px;
                        }}

                        h1 {{
                            font-size: 28px;
                            margin-bottom: 15px;
                            color: #1e90ff;
                            font-weight: bold;
                        }}

                        h1 .highlight {{
                            color: #ff6347;
                            font-weight: normal;
                        }}

                        p {{
                            margin: 15px 0;
                            font-size: 16px;
                        }}

                        p.signature {{
                            font-family: ""Georgia"", serif;
                            font-size: 18px;
                            font-style: italic;
                        }}

                        /* Footer */
                        .footer {{
                            margin-top: 40px;
                            text-align: center;
                            font-size: 14px;
                            color: #888;
                        }}
                    </style>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Resignation Letter</title>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""letter"">
                            <h1><span class=""highlight"">{r.employeeName}</span><br>Resignation Letter</h1>
                            <p><strong>Date of Resignation: {r.resignationDate}</strong></p>
                            <p>
                                {cd.OrganizationName}<br>
                                {r.departmentHead}<br>
                                {cd.OrganizationAddress}<br>
                            </p>

                            <p><strong>Dear {cd.OrganizationName},</strong></p>

                            <p>
                                After careful consideration, I have decided to submit my formal resignation from my position at {cd.OrganizationName}, effective {r.lastDay}. This decision has been made after much reflection on my personal and professional growth.
                            </p>

                            <p>
                                I am truly grateful for the experiences, skills, and friendships I have gained during my time at {cd.OrganizationName}. I have learned a great deal, and I am thankful for the support and mentorship from my colleagues and superiors.
                            </p>

                            <p>
                                I am more than willing to assist in any way I can to ensure a smooth and seamless transition. Please let me know how I can help during this period.
                            </p>

                            <p>
                                Once again, I appreciate the opportunities provided to me, and I wish the entire team continued success in the future.
                            </p>

                            <p>Sincerely,</p>

                            <p class=""signature"">{r.employeeName}</p>
                            <p class=""footer"">This letter serves as a formal resignation notice, in accordance with the terms of your employment agreement with {cd.OrganizationName}.</p>
                        </div>
                    </div>
                </body>
                </html>
                ";

            return htmlcontent;

        }


        public IActionResult ConvertHtmlToPdf(string htmlContent, string email)
        {
            HtmlToPdf html = new HtmlToPdf();
            PdfDocument pdfDocument = html.ConvertHtmlString(htmlContent);
            byte[] pdf = pdfDocument.Save();
            pdfDocument.Close();
            Random r = new Random();
            string rn = $"Resignation-letter{r.Next(100000, 50000000).ToString()}.pdf";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Content", "resignation-letters", rn);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            System.IO.File.WriteAllBytes(filePath, pdf);
            SendEmailWithAttachment(filePath, email);
            SaveDataIntoDb(email, "Resignation-letter", rn);
            return File(pdf, "application/pdf", rn);
        }

        private void SendEmailWithAttachment(string filePath, string email)
        {
            try
            {
                var fromEmail = "gaudak30@gmail.com";
                var toEmail = email;
                var subject = "Offer Letter";
                var body = "Please find the attached offer letter PDF.";

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("gaudak30@gmail.com", "lcbbmkuhovxxomvs"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage(fromEmail, toEmail, subject, body);

                var attachment = new Attachment(filePath);
                mailMessage.Attachments.Add(attachment);
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        public void SaveDataIntoDb(string email, string docname, string docfile)
        {
            var ag = new AdminDocuments()
            {
                Email = email,
                DocName = docname,
                DocFile = docfile
            };
            db.AdminDocuments.Add(ag);
            db.SaveChanges();
        }
    }
}
