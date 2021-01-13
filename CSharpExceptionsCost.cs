using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace CSharpExceptionsCost
{
    [SimpleJob(RuntimeMoniker.Net48, launchCount: LaunchCount, warmupCount: WarmupCount, targetCount: TargetCount)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21, launchCount: LaunchCount, warmupCount: WarmupCount, targetCount: TargetCount)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31, launchCount: LaunchCount, warmupCount: WarmupCount, targetCount: TargetCount)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50, launchCount: LaunchCount, warmupCount: WarmupCount, targetCount: TargetCount)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MarkdownExporterAttribute.GitHub]
    public class ErrorCodesVsExceptionsBenchmark
    {
        private const int LaunchCount = 3;
        private const int WarmupCount = 10;
        private const int TargetCount = 30;
        
        private readonly UserService _userService;

        public ErrorCodesVsExceptionsBenchmark()
        {
            _userService = new UserService();
        }

        [Benchmark]
        public bool GetUserWithExceptions()
        {
            try
            {
                return _userService.GetUserByIdOrThrowException("") != null;
            }
            catch (UserNotFoundException)
            {
                return false;
            }
        }

        [Benchmark(Baseline = true)]
        public bool GetUserWithDefault()
        {
            return _userService.GetUserByIdOrDefault("") != null;
        }
        
        [Benchmark]
        public bool GetUserWithTryGet()
        {
            return _userService.TryGetUserById("", out var foundUser);
        }
    }
    
    class UserService
    {
        private readonly Dictionary<string, User> _userRepo;

        public UserService()
        {
            _userRepo = new Dictionary<string, User>
            {
                {"some-user", new User()
                {
                    Id = "some-user", 
                    Name = "User!"
                }}
            };
        }

        public User GetUserByIdOrThrowException(string userId)
        {
            try
            {
                return _userRepo[userId];
            }
            catch (KeyNotFoundException e)
            {
                throw new UserNotFoundException(userId, e);
            }
        }
        
        public User GetUserByIdOrDefault(string userId)
        {
            return !_userRepo.TryGetValue(userId, out var foundUser) ? default : foundUser;
        }

        public bool TryGetUserById(string userId, out User user)
        {
            if (_userRepo.TryGetValue(userId, out var foundUser))
            {
                user = foundUser;
                return true;
            }
            user = default;
            return false;
        }
    }

    internal class UserNotFoundException : Exception
    {
        private string UserId { get; }

        public UserNotFoundException(string userId, Exception innerException) : base($"User with ID {userId} was not found", innerException)
        {
            UserId = userId;
        }
    }

    internal class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ErrorCodesVsExceptionsBenchmark>();
        }
    }
}