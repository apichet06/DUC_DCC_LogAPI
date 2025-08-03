using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using DUC_DCC_LogAPI.Data; 
using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dto.Authen;
using DUC_DCC_LogAPI.Models.Dto.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
 
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DUC_DCC_LogAPI.Service.AuthenService
{
    public class AuthenService : IAuthenService
    {

        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly MessageDto _message;
        private readonly ResponseDto _response;
        private readonly ResponseAuthen _resAuthen;
        private readonly IConfiguration _configuration;

        public AuthenService(AppDbContext dBContext, IMapper mapper, IConfiguration configuration)
        {
            _db = dBContext;
            _response = new ResponseDto();
            _message = new MessageDto();
            _mapper = mapper; 
            _configuration = configuration;
            _resAuthen = new ResponseAuthen();
        }




        public async Task<ResponseAuthen> Authen(AuthenDto authen)
        {
            try
            {
                var user = await _db.Users_Permission.FirstOrDefaultAsync(x => x.username == authen.Username);

                if (user is null)
                    return new ResponseAuthen { IsSuccess = false, Message = _message.LoginNotFound };



                var claims = new[]
                 {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]!),
                        new Claim(JwtRegisteredClaimNames.Iat,
                        new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()), 
                        new Claim("UserId", user.username!)
                    };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:key"]!));
                var singIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["jwt:Issuer"],
                    _configuration["jwt:Audience"],
                claims, expires: DateTime.Now.AddHours(24),
                signingCredentials: singIn);

                var result = _mapper.Map<UserResposeDto>(user);

                _resAuthen.token = new JwtSecurityTokenHandler().WriteToken(token);
                _resAuthen.Result = result;
                _resAuthen.Message = _message.LoginSuccess;


                //var userResponse = new UserResposeDto { username = user.username, emp_no = user.emp_no };
            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;

            }
            return _resAuthen;
        }

        public async Task<ResponseAuthen> AuthenticateUserAsync(AuthenDto authen)
        {
            return await Task.Run(() =>
            {
                var user = new { username = authen.Username };
                var claims = new[]
                {
            new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            new Claim("UserId", user.username!)
        };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:key"]!));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["jwt:Issuer"],
                    _configuration["jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(60),
                    signingCredentials: signIn
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                _resAuthen.Message = _message.LoginSuccess;
                _resAuthen.token = tokenString;

                return _resAuthen;
            });
        }


    }
}
