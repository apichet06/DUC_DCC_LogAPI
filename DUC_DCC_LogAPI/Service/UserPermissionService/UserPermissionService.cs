using AutoMapper;
using Azure;
using DUC_DCC_LogAPI.Data;
using DUC_DCC_LogAPI.Models;
using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Models.Dto.User;
using Microsoft.EntityFrameworkCore;

namespace DUC_DCC_LogAPI.Service.UserPermissionService
{
    public class UserPermissionService : IUserPermissionService
    {

        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly MessageDto _message;
        private readonly ResponseDto _response;
  
        public UserPermissionService(AppDbContext dBContext, IMapper mapper  )
        {
            _db = dBContext;
            _response = new ResponseDto();
            _message = new MessageDto();
            _mapper = mapper;
   
        }

     
        public async Task<ResponseDto> GetUserById(string emp_no)
        {
            try
            {
                var query = await (from a in _db.Users_Permission
                                   join b in _db.bu_plant on a.Plant_Id equals b.Id
                                   where a.emp_no == emp_no
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
                                   }).ToListAsync();
                _response.Result = query;
            }

            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;

            }
            return _response;
        }

        public async Task<ResponseDto> GetUserList(Users_Permission request)
        {
            try
            {
                var apps = await _db.app_name.ToListAsync();
                var query = await (from a in _db.Users_Permission
                                   join b in _db.bu_plant on a.Plant_Id equals b.Id 
                                   select new UserResposeDto
                                   {
                                       Id = a.Id,
                                       Plant = b.Plant,
                                       Plant_Id = a.Plant_Id,
                                       emp_no = a.emp_no,
                                       emp_email = a.emp_email,
                                       username = a.username,
                                       firstname = a.firstname,
                                       lastname = a.lastname,
                                       fullname = $"{a.firstname} {a.lastname}",
                                       is_email = a.is_email,
                                       is_accept = a.is_accept,
                                       is_review = a.is_review,
                                       is_export = a.is_export,
                                       App_Id = a.App_Id,
                                       Status = a.Status,
                                       Plant_Name = b.Plant_Name,
                                       created_by = a.created_by,
                                       Created_date= a.Created_date,
                                       updated_by= a.updated_by,
                                       updated_date = a.updated_date,
                                   }).ToListAsync();
                foreach (var item in query)
                {
                    if (!string.IsNullOrEmpty(item.App_Id))
                    {
                        var ids = item.App_Id.Split(',').Select(int.Parse).ToList();
                        item.App_Names = string.Join(", ", apps.Where(x => ids.Contains(x.Id)).Select(x => x.App_log));
                    }
                }
                _response.Result = _mapper.Map<List<UserResposeDto>>(query);
            }

            catch (Exception ex)
            {
                _response.IsSuccess = false;
                if (ex.InnerException != null)
                    _response.Message = _message.an_error_occurred + ex.InnerException.Message;
                else
                    _response.Message = _message.an_error_occurred + ex.Message;

            }
            return _response;
        }
        public async Task<ResponseDto> CreateUserPermiss(Users_Permission request)
        {
            try
            {
                var exists = await _db.Users_Permission.AnyAsync(a => a.emp_no == request.emp_no || a.emp_email == request.emp_email);

                if (exists)
                {
                    _response.IsSuccess = false;
                    _response.Message = _message.already_exists + $" {request.emp_no} - {request.emp_email}";
                    return _response;
                }

                var obj = _mapper.Map<Users_Permission>(request);
                if (!string.IsNullOrEmpty(request.emp_email) && request.emp_email.Contains("@"))
                {
                    obj.username = request.emp_email.Split('@')[0];
                }
                else
                {
                    obj.username = request.emp_email; 
                }
                obj.Created_date = DateTime.Now;
                _db.Users_Permission.Add(obj);
                await _db.SaveChangesAsync();

                _response.Result = _mapper.Map<Users_Permission>(obj);
                _response.Message = _message.InsertMessage;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                if (ex.InnerException != null)
                    _response.Message = _message.an_error_occurred + ex.InnerException.Message;
                else
                    _response.Message = _message.an_error_occurred + ex.Message;

            }
            return _response;
        }
        public async Task<ResponseDto> UpdateUserPermiss(Users_Permission request, int Id)
        {
            try
            {
                var obj = await _db.Users_Permission.FirstOrDefaultAsync(a => a.Id == Id);
                 
                if (obj == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = _message.Not_found;
                    return _response;
                }


                var exists = await _db.Users_Permission
                  .AnyAsync(a => a.Id != Id && (a.emp_no == request.emp_no || a.emp_email == request.emp_email));

                if (exists)
                {
                    _response.IsSuccess = false;
                    _response.Message = _message.already_exists + $" {request.emp_no} - {request.emp_email}";
                    return _response;
                }

                obj.Plant_Id = request.Plant_Id;
                obj.emp_no =request.emp_no;
                obj.username =request.username;
                obj.emp_email =request.emp_email;
                obj.firstname =request.firstname;
                obj.lastname =request.lastname;
                obj.App_Id = request.App_Id;
                obj.is_email = request.is_email;
                obj.is_accept = request.is_accept;
                obj.is_review = request.is_review;
                obj.is_export = request.is_export;
                obj.updated_by = request.updated_by;
                obj.updated_date = DateTime.Now;
                obj.Status= request.Status;

                _db.Users_Permission.Update(obj);
                _db.SaveChanges();

                _response.Result = _mapper.Map<Users_Permission>(obj);
                _response.Message = _message.UpdateMessage;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;

            }
            return _response;
        }

        public async Task<ResponseDto> DeleteUserPermiss(int Id)
        {
            try { 

                var obj = await _db.Users_Permission.FirstOrDefaultAsync(x => x.Id == Id);

                if (obj == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = _message.Not_found;
                    return _response;
                }

                _db.Remove(obj);
                await _db.SaveChangesAsync();

                _response.Result = _mapper.Map<Users_Permission>(obj);
                _response.Message = _message.DeleteMessage;

            } catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;

            }
            return _response;
        }

        public async Task<ResponseDto> GetAppName()
        {

            try
            {
                var apps = await _db.app_name.ToListAsync();
                _response.Result = apps;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;

            }
            return _response;
        }

        public async Task<ResponseDto> GetBuPlant()
        {
            try
            {
                var buPlant = await _db.bu_plant.ToListAsync();
                _response.Result = buPlant;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = _message.an_error_occurred + ex.Message;

            }
            return _response;
        }
    }
}
