using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Interfaces;

namespace UberSystem.Service
{
    public class MailService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SendVerificationCodeAsync(string email)
        {
            var _userRepository = _unitOfWork.Repository<User>();

            var users = await _userRepository.GetAllAsync();

            var user = users.FirstOrDefault(e => e.Email == email);

            if (user == null)
                throw new Exception("Your verification code");

            var verificationCode = GenerateVerificationCode();

            var message = $@"
                            <html>
                                <body>
                                    <h2>Your verification code</h2>
                                    <p>Your verification code is <strong>{verificationCode}</strong>.</p>
                                </body>
                            </html>";

            user.VerificationCode = verificationCode;
            user.ExpiryDate = DateTime.UtcNow.AddHours(8).AddMinutes(10);

            _userRepository.UpdateAsync(user);

            await SendEmailAsync(user.Email, message);
        }

        public async Task<bool> VerifyEmailAsync(string email, string code)
        {
            var _userRepository = _unitOfWork.Repository<User>();
            var users = await _userRepository.GetAllAsync();

            var user = users.FirstOrDefault(e => e.Email == email);

            if (user == null)
                throw new Exception("User not found.");


            if (user.VerificationCode != code || user.ExpiryDate < DateTime.UtcNow)
                return false; 

            // Cập nhật trạng thái người dùng
            user.IsEmailConfirmed = true;
            user.VerificationCode = null; // Xóa mã sau khi xác thực thành công
            user.ExpiryDate = DateTime.UtcNow.AddHours(7);

            await _userRepository.UpdateAsync(user);
            return true; 
        }



        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // Mã xác thực gồm 6 chữ số
        }
        public async Task SendEmailAsync(string toEmail, string message)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("tduy152003@gmail.com"));
            email.To.Add(new MailboxAddress("", toEmail.Trim()));
            email.Subject = "Verification code";

            email.Body = new TextPart(TextFormat.Html)
            {
                Text = message
            };

            using (var smtp = new SmtpClient())
            {
                await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls); 
                await smtp.AuthenticateAsync("tduy152003@gmail.com", "uwva drio gchh rlvt"); 
                await smtp.SendAsync(email); 
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
