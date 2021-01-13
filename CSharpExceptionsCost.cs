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
            _userService = UserService.CreateServiceForBenchmark();
        }

        [Benchmark(Description = "Attempts to fetch a non-existent user with the exception-throwing method")]
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

        [Benchmark(Baseline = true, Description = "Attempts to fetch a non-existent user with the null-returning method")]
        public bool GetUserWithDefault()
        {
            return _userService.GetUserByIdOrDefault("") != null;
        }
        
        [Benchmark(Description = "Attempts to fetch a non-existent user with the try/get method")]
        public bool GetUserWithTryGet()
        {
            return _userService.TryGetUserById("", out var foundUser);
        }
    }
    
    /// <summary>
    ///     A dummy service for fetching users.
    ///     The only reason for this to be a "UserService" (and not just an "EntityFetcher") is to make the code
    ///     a bit less abstract.
    /// </summary>
    class UserService
    {
        private Dictionary<string, User> _userRepo;

        private UserService()
        {
        }
        
        public static UserService CreateServiceForBenchmark()
        {
            return new UserService()
            {
                _userRepo = new Dictionary<string, User>
                {
                    {
                        "some-user-who-will-never-be-fetched", new User()
                        {
                            Id = "some-user-who-will-never-be-fetched",
                            Name = "User!"
                        }
                    }
                }
            };
        }

        /// <summary>
        ///     Fetches the user with the given ID. If no user is found, a UserNotFoundException will be thrown.
        /// </summary>
        /// <param name="userId">The ID of the user to fetch</param>
        /// <returns>The user with the given ID</returns>
        /// <exception cref="UserNotFoundException">Thrown if no user with the given ID can be found</exception>
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
        
        /// <summary>
        ///     Fetches the user with the given ID. If no user is found, `null` is returned.
        /// </summary>
        /// <param name="userId">The ID of the user to fetch</param>
        /// <returns>The user with the given ID - if no user is found, `null` is returned.</returns>
        public User GetUserByIdOrDefault(string userId)
        {
            return !_userRepo.TryGetValue(userId, out var foundUser) ? default : foundUser;
        }

        /// <summary>
        ///     Fetches the user with the given ID. If a user is found, `true` is returned and the user is returned in the `user` out parameter.
        ///     If no user is found, `false` is returned.
        /// </summary>
        /// <param name="userId">The ID of the user to fetch</param>
        /// <param name="user">The found user, if any</param>
        /// <returns>`true`, if a user with the given ID has been found. `false` otherwise</returns>
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

    /// <summary>
    ///     A dummy entity, making the benchmark code a bit less abstract
    /// </summary>
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