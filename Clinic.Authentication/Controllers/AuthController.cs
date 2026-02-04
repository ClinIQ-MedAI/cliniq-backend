using Clinic.Authentication.Contracts;
using Clinic.Authentication.Services;
using Clinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Authentication.Controllers;

/// <summary>
/// Unified authentication controller.
/// Handles registration, login, verification, and password reset.
/// </summary>
[Route("auth")]
[ApiController]
public class AuthController(
    IRegistrationService registrationService,
    IAuthService authService,
    IVerificationService verificationService,
    IPasswordService passwordService) : ControllerBase
{
    private readonly IRegistrationService _registrationService = registrationService;
    private readonly IAuthService _authService = authService;
    private readonly IVerificationService _verificationService = verificationService;
    private readonly IPasswordService _passwordService = passwordService;

    /// <summary>
    /// Register a new user.
    /// Creates ApplicationUser only - no profile is created.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _registrationService.RegisterAsync(request, cancellationToken);

        if (result.IsSucceed)
            return Ok(new { Message = "Registration successful. Please verify your email or phone to proceed." });

        return result.ToProblem();
    }

    /// <summary>
    /// Login with email and password or OTP.
    /// Requires verified email or phone.
    /// Returns JWT with patient_status and doctor_status claims.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);

        if (!result.IsSuccess)
            return Problem(detail: result.Error, statusCode: StatusCodes.Status401Unauthorized);

        return Ok(new
        {
            Token = result.Token,
            RefreshToken = result.RefreshToken,
            ExpiresAt = result.ExpiresAt,
            PatientStatus = result.PatientStatus?.ToString(),
            DoctorStatus = result.DoctorStatus?.ToString()
        });
    }

    /// <summary>
    /// Verify email with OTP code.
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await _verificationService.VerifyEmailAsync(request, cancellationToken);

        if (result.IsSucceed)
            return Ok(new { Message = "Email verified successfully. You can now login." });

        return result.ToProblem();
    }

    /// <summary>
    /// Verify phone with OTP code.
    /// </summary>
    [HttpPost("verify-phone")]
    public async Task<IActionResult> VerifyPhone([FromBody] VerifyPhoneRequest request, CancellationToken cancellationToken)
    {
        var result = await _verificationService.VerifyPhoneAsync(request, cancellationToken);

        if (result.IsSucceed)
            return Ok(new { Message = "Phone verified successfully. You can now login." });

        return result.ToProblem();
    }

    /// <summary>
    /// Send OTP code to email for verification.
    /// </summary>
    [HttpPost("send-email-otp")]
    public async Task<IActionResult> SendEmailOtp([FromBody] SendEmailOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _verificationService.SendEmailOtpAsync(request.Email, cancellationToken);

        if (result.IsSucceed)
            return Ok(new { Message = "OTP sent to email." });

        return result.ToProblem();
    }

    /// <summary>
    /// Send OTP code to phone for verification.
    /// </summary>
    [HttpPost("send-phone-otp")]
    public async Task<IActionResult> SendPhoneOtp([FromBody] SendPhoneOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _verificationService.SendPhoneOtpAsync(request.Phone, cancellationToken);

        if (result.IsSucceed)
            return Ok(new { Message = "OTP sent to phone." });

        return result.ToProblem();
    }

    /// <summary>
    /// Request password reset - sends reset code to email.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _passwordService.ForgotPasswordAsync(request, cancellationToken);

        // Always return success to not reveal user existence
        return Ok(new { Message = "If the email exists, a password reset link has been sent." });
    }

    /// <summary>
    /// Reset password using reset code.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _passwordService.ResetPasswordAsync(request, cancellationToken);

        if (result.IsSucceed)
            return Ok(new { Message = "Password reset successfully." });

        return result.ToProblem();
    }

    /// <summary>
    /// Send OTP code to email for login (passwordless authentication).
    /// </summary>
    [HttpPost("send-login-otp")]
    public async Task<IActionResult> SendLoginOtp([FromBody] SendEmailOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.SendLoginOtpAsync(request.Email, cancellationToken);

        if (result.IsSucceed)
            return Ok(new { Message = "OTP sent to email for login." });

        return result.ToProblem();
    }
}
