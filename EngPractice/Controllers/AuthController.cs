﻿using EngPractice.Domain;
using EngPractice.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Google;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Web;
using System.Text.Json;

namespace EngPractice.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IConfiguration _config;
        public AuthController(AuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(request);

            if (!response.Success)
            {
                return BadRequest(new { response.Message });
            }

            return Ok(response);
        }
 

        [HttpGet("login")]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string code)
        {
            var clientId = _config["Authentication:Google:ClientId"];
            var clientSecret = _config["Authentication:Google:ClientSecret"];
            var redirectUri = _config["Authentication:Google:RedirectUri"];
            var decodedCode = HttpUtility.UrlDecode(code);
            // 1. Trao đổi code lấy access_token
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "code", decodedCode },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" }
        })
            };

            var tokenResponse = await _httpClient.SendAsync(tokenRequest);
            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
                return BadRequest($"Không lấy được access token: {tokenContent}");

            var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(tokenContent);

            // 2. Gọi API userinfo
            var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
            userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);

            var userResponse = await _httpClient.SendAsync(userRequest);
            var userContent = await userResponse.Content.ReadAsStringAsync();

            if (!userResponse.IsSuccessStatusCode)
                return BadRequest($"Không lấy được thông tin người dùng: {userContent}");

            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userContent);

            // TODO: lưu user / tạo JWT nếu cần
            return Ok(userInfo);
        }
    }
}
