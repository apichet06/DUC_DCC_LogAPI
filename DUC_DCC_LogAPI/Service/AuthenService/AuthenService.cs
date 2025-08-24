using AutoMapper;
using Azure;
using DocumentFormat.OpenXml.Office2010.Excel;
using DUC_DCC_LogAPI.Data; 
using DUC_DCC_LogAPI.Models;
using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dto.Authen;
using DUC_DCC_LogAPI.Models.Dto.User;
using Microsoft.AspNetCore.Http;
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
                //var user = await _db.Users_Permission.FirstOrDefaultAsync(x => x.emp_email == authen.user_name);
                var user = await (from a in _db.Users_Permission
                                  join b in _db.bu_plant on a.Plant_Id equals b.Id
                                  select new UserResposeDto
                                  {
                                      Id = a.Id,
                                      Plant = b.Plant,
                                      emp_no = a.emp_no,
                                      emp_email = a.emp_email,
                                      username = a.username,
                                      firstname = a.firstname,
                                      lastname = a.lastname,
                                      is_email = a.is_email,
                                      is_accept = a.is_accept,
                                      is_review = a.is_review,
                                      is_export = a.is_export,
                                      App_Id = a.App_Id,
                                      Status = a.Status,
                                      Plant_Name = b.Plant_Name,
                                      Plant_Id = a.Plant_Id
                                  }).FirstOrDefaultAsync(x => x.emp_email == authen.user_name);

                if (user is null)
                    return new ResponseAuthen { IsSuccess = false, Message = _message.LoginNotFound };

                 
                var claims = new[]
                 {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]!),
                        new Claim(JwtRegisteredClaimNames.Iat,
                        new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()), 
                        new Claim("UserId", user.username!)
                    };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
                var singIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                claims, expires: DateTime.Now.AddHours(24),
                signingCredentials: singIn);

                var result = _mapper.Map<UserResposeDto>(user);

                _resAuthen.token = new JwtSecurityTokenHandler().WriteToken(token);
                _resAuthen.Result = user;
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


        #region || Cookied ||
        public async Task<ResponseAuthen> AuthenticateUserAsync(AuthenDto authen)
        {
            return await Task.Run(() =>
            {
                var user = new { username = authen.user_name };
                var claims = new[]
                {
            new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            new Claim("UserId", user.username!)
        };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
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


        #endregion
         

    }
}
