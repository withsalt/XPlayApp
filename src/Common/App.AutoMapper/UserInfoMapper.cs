using AutoMapper;
using App.Data.Entity.User;
using App.Data.Model.Response.User;
using App.Util.Date;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.AutoMapper
{
    public class UserInfoMapper : Profile
    {
        public UserInfoMapper()
        {
            CreateMap<UserInfo, UserInfoModel>()
                .ForMember(dest => dest.Uid, opts => opts.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Age, opts => opts.MapFrom(src => src.Brithday == null ? 0 : ((TimeUtil.Timestamp() - src.Brithday) / 31536000)))
                .ForMember(dest => dest.Token, opts => opts.MapFrom(src => src.UserPasswd == null ? null : src.UserPasswd.Token))
                .ForMember(dest => dest.Passwd, opts => opts.MapFrom(src => src.UserPasswd == null ? null : src.UserPasswd.Password));
        }
    }
}
