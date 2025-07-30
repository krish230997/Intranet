using Microsoft.AspNetCore.Mvc;
using Pulse360.Data;
using Pulse360.Models;
using System.Globalization;
using System.Net.Mail;
using System.Net;

namespace Pulse360.Controllers
{
    public class OfferLetterController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment env;

        public OfferLetterController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult OfferLetter(string templateName)
        {
            var cd = db.Organization.FirstOrDefault();
            if (cd != null)
            {
                ViewData["SelectedTemplate"] = templateName;
                return View();
            }
            else
            {
                return RedirectToAction("AddOrganisation", "Home");
            }
        }

        [HttpPost]
        public IActionResult OfferLetter(OfferLetter ol, string Template)
        {
            var cd = db.Organization.FirstOrDefault();
            if (cd != null)
            {
                string data;
                if (Template.Equals("templet1"))
                {
                    data = Templete1(cd, ol);
                    var d = ConvertHtmlToPdf(data, ol.employeeEmail);
                    TempData["success"] = "Offer letter is generated successfully and sent on given email Id";
                    return View();
                }
                else if (Template.Equals("templet2"))
                {
                    data = Templete2(cd, ol);
                    var d = ConvertHtmlToPdf(data, ol.employeeEmail);
                    TempData["success"] = "Offer letter is generated successfully and sent on given email Id";
                    return View();
                }
                else if (Template.Equals("templet3"))
                {
                    data = Templete3(cd, ol);
                    var d = ConvertHtmlToPdf(data, ol.employeeEmail);
                    TempData["success"] = "Offer letter is generated successfully and sent on given email Id";
                    return View();
                }
                else if (Template.Equals("templet4"))
                {
                    data = Templete4(cd, ol);
                    var d = ConvertHtmlToPdf(data, ol.employeeEmail);
                    TempData["success"] = "Offer letter is generated successfully and sent on given email Id";
                    return View();
                }
            }
            else
            {
                return RedirectToAction("AddCompanyInfo", "Admin");
            }
            return View();
        }

        public string Templete1(Organization cd, OfferLetter ol)
        {
            string path = env.WebRootPath;
            string relpath = cd.OrganizationLogo;
            string fullpath = path + relpath;

            string htmlContent = $@"
            <!DOCTYPE html>
            <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Offer Letter</title>
                <style>
                    @page {{
                        size: A4;
                        margin: 20mm;
                    }}
                    body {{
                        font-family: 'Arial', sans-serif;
                        margin: 0;
                        padding: 0;
                        color: #333;
                    }}
                    .container {{
                        margin: 40px auto;
                        padding: 30px;
                        background-color: #ffffff;
                        border-left: 6px solid #004a99;
                    }}
                    .logo {{
                        text-align: left;
                        margin-bottom: 30px;
                    }}
                    .logo img {{
                        width: 120px;
                    }}
                    .header {{
                        text-align: center;
                        font-size: 26px;
                        color: #004a99;
                        margin-bottom: 15px;
                        font-weight: 600;
                    }}
                    .content {{
                        font-size: 16px;
                        line-height: 1.6;
                        color: #333;
                    }}
                    .highlight {{
                        color: #004a99;
                        font-weight: bold;
                    }}
                    p {{
                        margin: 12px 0;
                    }}
                    .footer {{
                        margin-top: 30px;
                        text-align: center;
                        font-size: 14px;
                        color: #666;
                    }}
                    .footer a {{
                        color: #004a99;
                        text-decoration: none;
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <!-- Company Logo -->
                    <div class=""logo"">
                        <img src=""{fullpath}"" alt=""Company Logo"">
                    </div>

                    <!-- Header -->
                    <div class=""header"">Offer of Employment</div>

                    <!-- Content -->
                    <div class=""content"">
                        <p>Date: <span class=""highlight"">{DateTime.Now.ToString("dd-MM-yyyy")}</span></p>
                        <p>To,</p>
                        <p><span class=""highlight"">{ol.employeeName}</span></p>
                        <p><span class=""highlight"">{ol.address}</span></p>

                        <p>Dear <span class=""highlight"">{ol.employeeName}</span>,</p>

                        <p>We are pleased to extend an offer for you to join <span class=""highlight"">{cd.OrganizationName}</span> as a <span class=""highlight"">{ol.position}</span>.</p>

                        <p>Please find below the details of your employment:</p>

                        <ul>
                            <li><strong>Position:</strong> <span class=""highlight"">{ol.position}</span></li>
                            <li><strong>Start Date:</strong> <span class=""highlight"">{ol.startDate}</span></li>
                            <li><strong>Salary:</strong> <span class=""highlight"">{ol.salary} per annum</span></li>
                            <li><strong>Location:</strong> <span class=""highlight"">{cd.OrganizationAddress}</span></li>
                        </ul>

                        <p>We are confident that your skills and expertise will be a valuable addition to our team. We look forward to having you with us and hope for a long and successful association.</p>

                        <p>Please confirm your acceptance of this offer by signing and returning this letter by <span class=""highlight"">{ol.acceptanceDeadline}</span>.</p>

                        <p>If you have any questions, feel free to reach out to our HR team at <a href=""mailto:hr@company.com"" class=""highlight"">hr@company.com</a>.</p>

                        <p>Best regards,</p>

                        <div class=""signature"">
                           
                            <p>{cd.OrganizationName}</p>
                        </div>
                    </div>

                    <!-- Footer -->
                    <div class=""footer"">
                        <p>&copy; {DateTime.Now.Year.ToString()} {cd.OrganizationName}. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
            ";
            return htmlContent;
        }

        public string Templete2(Organization cd, OfferLetter ol)
        {
            string path = env.WebRootPath;
            string relpath = cd.OrganizationLogo;
            string fullpath = path + relpath;
            string htmlcontent = $@"
            <!DOCTYPE html>
            <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Offer Letter</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        margin: 0;
                        padding: 0;
                        color: #2c2c2c;
                    }}

                    .container {{
                        margin: 30px auto;
                        padding: 20px 30px;
                        background: linear-gradient(135deg, #1e3c72, #2a5298);
                        color: #ffffff;
                        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.2);
                    }}

                    .logo {{
                        text-align: center;
                        margin-bottom: 30px;
                    }}

                    .logo img {{
                        max-width: 180px;
                        border-radius: 8px;
                    }}

                    .header {{
                        text-align: center;
                        margin-bottom: 20px;
                        font-size: 24px;
                        font-weight: bold;
                    }}

                    .content {{
                        background-color: #ffffff;
                        padding: 20px;
                        border-radius: 8px;
                        color: #333333;
                    }}

                    .content p {{
                        margin: 10px 0;
                    }}

                    .highlight {{
                        color: #1e3c72;
                        font-weight: bold;
                    }}

                    ul {{
                        padding-left: 20px;
                        margin: 10px 0;
                    }}

                    ul li {{
                        margin: 5px 0;
                    }}

                    .signature {{
                        margin-top: 30px;
                    }}

                    .signature p {{
                        margin: 5px 0;
                    }}

                    .footer {{
                        margin-top: 30px;
                        text-align: center;
                        font-size: 14px;
                        color: #ddd;
                    }}

                    .footer a {{
                        color: #ffffff;
                        text-decoration: underline;
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <!-- Company Logo -->
                    <div class=""logo"">
                        <img src=""{fullpath}"" alt=""Company Logo"">
                    </div>

                    <!-- Header -->
                    <div class=""header"">Offer of Employment</div>

                    <!-- Content -->
                    <div class=""content"">
                        <p>Date: <span class=""highlight"">{DateTime.Now.ToString("dd-MM-yyyy")}</span></p>
                        <p>To,</p>
                        <p><span class=""highlight"">{ol.employeeName}</span></p>
                        <p><span class=""highlight"">{ol.address}</span></p>

                        <p>Dear <span class=""highlight"">{ol.employeeName}</span>,</p>

                        <p>We are delighted to offer you the position of <span class=""highlight"">{ol.position}</span> at <span class=""highlight"">{cd.OrganizationName}</span>. Below are the details of your employment:</p>

                        <ul>
                            <li><strong>Job Title:</strong> <span class=""highlight"">{ol.position}</span></li>
                            <li><strong>Start Date:</strong> <span class=""highlight"">{ol.startDate}</span></li>
                            <li><strong>Location:</strong> <span class=""highlight"">{cd.OrganizationAddress}</span></li>
                            <li><strong>Salary:</strong> <span class=""highlight"">{ol.salary} per annum</span></li>
                            <li><strong>Benefits:</strong> Health, dental, and vision insurance, retirement plans, and performance bonuses.</li>
                        </ul>

                        <p>We believe that your skills and experience will be an excellent fit for this role and our organization. If you accept this offer, kindly sign and return a copy of this letter by <span class=""highlight"">{ol.acceptanceDeadline}</span>.</p>

                        <p>Please feel free to contact us at <a href=""mailto:hr@company.com"" class=""highlight"">hr@company.com</a> if you have any questions.</p>

                        <p>Looking forward to welcoming you to our team!</p>

                        <p>Sincerely,</p>

                        <div class=""signature"">
                          
                            <p>{cd.OrganizationName}</p>
                        </div>
                    </div>

                    <!-- Footer -->
                    <div class=""footer"">
                        <p>&copy; {DateTime.Now.Year} {cd.OrganizationName}. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
            ";

            return htmlcontent;
        }

        public string Templete3(Organization cd, OfferLetter ol)
        {
            string path = env.WebRootPath;
            string relpath = cd.OrganizationLogo;
            string fullpath = path + relpath;
            string htmlcontent = $@"
            <!DOCTYPE html>
            <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Offer Letter</title>
                <style>
                    body {{
                        font-family: 'Verdana', sans-serif;
                        margin: 0;
                        padding: 0;
                        color: #333333;
                    }}

                    .container {{
                        margin: 30px auto;
                        padding: 20px 30px;
                        background-color: #ffffff;
                        border-radius: 10px;
                        border-top: 10px solid #3c9e74;
                        box-shadow: 0 3px 8px rgba(0, 0, 0, 0.15);
                    }}

                    .logo {{
                        text-align: center;
                        margin-bottom: 25px;
                    }}

                    .logo img {{
                        max-width: 150px;
                    }}

                    .header {{
                        text-align: center;
                        font-size: 24px;
                        color: #3c9e74;
                        margin-bottom: 15px;
                        font-weight: bold;
                    }}

                    .content {{
                        font-size: 16px;
                        line-height: 1.6;
                    }}

                    .content p {{
                        margin: 10px 0;
                    }}

                    .highlight {{
                        color: #3c9e74;
                        font-weight: bold;
                    }}

                    ul {{
                        padding-left: 20px;
                        margin: 15px 0;
                    }}

                    ul li {{
                        margin: 5px 0;
                    }}

                    .signature {{
                        margin-top: 40px;
                    }}

                    .signature p {{
                        margin: 5px 0;
                    }}

                    .footer {{
                        margin-top: 30px;
                        text-align: center;
                        font-size: 14px;
                        color: #777777;
                    }}

                    .footer a {{
                        color: #3c9e74;
                        text-decoration: none;
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <!-- Company Logo -->
                    <div class=""logo"">
                        <img src=""{fullpath}"" alt=""Company Logo"">
                    </div>

                    <!-- Header -->
                    <div class=""header"">Offer of Employment</div>

                    <!-- Content -->
                    <div class=""content"">
                        <p>Date: <span class=""highlight"">{DateTime.Now.Date}</span></p>
                        <p>To,</p>
                        <p><span class=""highlight"">{ol.employeeName}</span></p>
                        <p><span class=""highlight"">{ol.address}</span></p>

                        <p>Dear <span class=""highlight"">{ol.employeeName}</span>,</p>

                        <p>We are excited to offer you the position of <span class=""highlight"">{ol.position}</span> at <span class=""highlight"">{cd.OrganizationName}</span>. Below are the terms of your employment:</p>

                        <ul>
                            <li><strong>Job Title:</strong> <span class=""highlight"">{ol.position}</span></li>
                            <li><strong>Start Date:</strong> <span class=""highlight"">{ol.startDate}</span></li>
                            <li><strong>Location:</strong> <span class=""highlight"">{cd.OrganizationAddress}</span></li>
                            <li><strong>Salary:</strong> <span class=""highlight"">{ol.salary} per annum</span></li>
                            <li><strong>Benefits:</strong> Health insurance, retirement plans, and paid time off.</li>
                        </ul>

                        <p>We believe your expertise will make a significant impact on our team. Please confirm your acceptance by signing and returning this letter by <span class=""highlight"">{ol.acceptanceDeadline}</span>.</p>

                        <p>If you have any questions, feel free to reach out to <a href=""mailto:hr@company.com"" class=""highlight"">hr@company.com</a>.</p>

                        <p>We look forward to welcoming you on board!</p>

                        <p>Warm regards,</p>

                        <div class=""signature"">
                            
                            <p>{cd.OrganizationName}</p>
                        </div>
                    </div>

                    <!-- Footer -->
                    <div class=""footer"">
                        <p>&copy; {DateTime.Now.Date} {cd.OrganizationName}. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
            ";
            return htmlcontent;
        }

        public string Templete4(Organization cd, OfferLetter ol)
        {
            string path = env.WebRootPath;
            string relpath = cd.OrganizationLogo;
            string fullpath = path + relpath;
            string htmlcontent = $@"
                <!DOCTYPE html>
                <html lang=""en"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Offer Letter</title>
                    <style>
                        body {{
                            font-family: 'Arial', sans-serif;
                            background-color: #f1f1f1;
                            margin: 0;
                            padding: 40px;
                        }}
                        .container {{
                            max-width: 800px;
                            background-color: #ffffff;
                            padding: 40px;
                            border-radius: 12px;
                            box-shadow: 0px 5px 20px rgba(0, 0, 0, 0.1);
                            border-left: 8px solid #f39c12; /* Orange border */
                        }}
                        .header {{
                            text-align: center;
                            font-size: 30px;
                            font-weight: bold;
                            color: #2c3e50;
                            margin-bottom: 25px;
                        }}
                        .content {{
                            font-size: 17px;
                            line-height: 1.7;
                            color: #34495e;
                            margin-bottom: 30px;
                        }}
                        .highlight {{
                            color: #f39c12;
                            font-weight: bold;
                        }}
                        ul {{
                            list-style-type: none;
                            padding: 0;
                        }}
                        ul li {{
                            margin: 8px 0;
                        }}
                        .signature {{
                            margin-top: 40px;
                            font-size: 18px;
                            text-align: right;
                            color: #2c3e50;
                        }}
                        .footer {{
                            margin-top: 25px;
                            text-align: center;
                            font-size: 14px;
                            color: #7f8c8d;
                        }}
                        .footer a {{
                            color: #f39c12;
                            text-decoration: none;
                        }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <!-- Company Logo -->
                    <div class=""logo"">
                        <img src=""{fullpath}"" alt=""Company Logo"">
                    </div>
                        <div class=""header"">Exciting Career Opportunity - Offer Letter</div>
                        <div class=""content"">
                            <p><strong>Date:</strong> <span class=""highlight"">{DateTime.Now:dd-MM-yyyy}</span></p>
                            <p>Dear <span class=""highlight"">{ol.employeeName}</span>,</p>
                            <p>We are delighted to extend to you an offer to join <span class=""highlight"">{cd.OrganizationName}</span> as a <span class=""highlight"">{ol.position}</span>. Your skills and expertise are exactly what we need to continue building our success.</p>
            
                            <p><strong>Key Details of the Offer:</strong></p>
                            <ul>
                                <li><strong>Position:</strong> {ol.position}</li>
                                <li><strong>Start Date:</strong> {ol.startDate}</li>
                                <li><strong>Annual Salary:</strong> {ol.salary}</li>
                                <li><strong>Work Location:</strong> {cd.OrganizationAddress}</li>
                            </ul>
            
                            <p>We are confident that your contribution will help elevate our team, and we look forward to the potential of working together.</p>

                            <p>Kindly confirm your acceptance by signing and returning this letter by <span class=""highlight"">{ol.acceptanceDeadline}</span>. If you have any questions or need more information, do not hesitate to reach out to us.</p>

                            <p>You can contact us at <a href=""mailto:hr@company.com"">hr@company.com</a> for further queries.</p>

                            <p class=""signature"">Best regards, <br />{cd.OrganizationName}</p>
                        </div>
                        <div class=""footer"">
                            <p>&copy; {DateTime.Now.Year} {cd.OrganizationName}. All rights reserved.</p>
                            
                        </div>
                    </div>
                </body>
                </html>";
            return htmlcontent;
        }


        public IActionResult ConvertHtmlToPdf(string htmlContent, string email)
        {
            SelectPdf.HtmlToPdf html = new SelectPdf.HtmlToPdf();
            SelectPdf.PdfDocument pdfDocument = html.ConvertHtmlString(htmlContent);
            byte[] pdf = pdfDocument.Save();
            pdfDocument.Close();
            Random r = new Random();
            string rn = $"Offer-letter{r.Next(100000, 50000000).ToString()}.pdf";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Content", "offer-letters", rn);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            System.IO.File.WriteAllBytes(filePath, pdf);
            SendEmailWithAttachment(filePath, email);
            SaveDataIntoDb(email, "Offer-letter", rn);
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
