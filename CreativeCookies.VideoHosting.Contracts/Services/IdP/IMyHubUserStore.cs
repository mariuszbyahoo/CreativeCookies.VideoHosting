using CreativeCookies.VideoHosting.DTOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Services.IdP
{
    /// <summary>
    /// Wrapper around IUserStore of MyHubUser
    /// </summary>
    public interface IMyHubUserStore 
    {
        /// <summary>
        /// Sets the given <paramref name="userName" /> for the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user whose name should be set.</param>
        /// <param name="userName">The user name to set.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetUserNameAsync(MyHubUserDto user, string? userName, CancellationToken cancellationToken);
    }
}
