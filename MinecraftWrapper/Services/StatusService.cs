﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MinecraftWrapper.Data;
using MinecraftWrapper.Models;

namespace MinecraftWrapper.Services
{
    public class StatusService
    {
        private Dictionary<string, bool> _onlineUsers = new Dictionary<string, bool>();
        private DateTime _refreshTime;

        private readonly IServiceProvider _serviceProvider;

        private const int MINUTES_TO_CACHE = 20;

        public StatusService ( IServiceProvider serviceProvider )
        {
            _serviceProvider = serviceProvider;
            RefreshUserList ();
        }

        private void RefreshUserList ()
        {
            using ( var scope = _serviceProvider.CreateScope () )
            {
                var userRepository = scope.ServiceProvider.GetService<UserRepository> ();
                var allUsers = userRepository.GetAllUsers ();

                foreach ( var user in allUsers )
                {
                    if ( user.GamerTag != null && !_onlineUsers.ContainsKey ( user.GamerTag ) )
                    {
                        _onlineUsers.Add ( user.GamerTag, false );
                    }
                }

                _refreshTime = DateTime.UtcNow;
            }
        }

        public bool GetUserStatus ( string userId )
        {
            if ( _refreshTime.AddMinutes ( MINUTES_TO_CACHE ) > DateTime.UtcNow )
            {
                RefreshUserList ();
            }

            using ( var scope = _serviceProvider.CreateScope () )
            {
                var user = scope.ServiceProvider.GetService<UserRepository> ()
                    .GetAllUsers ()
                    .Where ( u => u.Id == userId )
                    .SingleOrDefault ();

                var gamerTag = user?.GamerTag;

                if ( gamerTag != null && _onlineUsers.ContainsKey ( gamerTag ) )
                {
                    return _onlineUsers [ gamerTag ];
                }
            }

            return false;
        }

        public void UpdateUserStatus ( string gamerTag, bool isOnline )
        {
            RefreshUserList ();

            if ( gamerTag != null && _onlineUsers.ContainsKey ( gamerTag ) )
            {
                _onlineUsers [ gamerTag ] = isOnline;
            }

        }
    }
}
