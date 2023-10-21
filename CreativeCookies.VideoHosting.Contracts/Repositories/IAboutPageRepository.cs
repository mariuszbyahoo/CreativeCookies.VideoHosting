using CreativeCookies.VideoHosting.DTOs.About;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IAboutPageRepository
    {
        /// <summary>
        /// Returns the HTML code of the About page
        /// </summary>
        /// <returns>DTO with an Inner HTML code, if none found - returns string.Empty</returns>
        Task<AboutPageDTO> GetAboutPageInnerHTML();

        /// <summary>
        /// Updates the About page's inner HTML code
        /// </summary>
        /// <param name="newInnerHtml">new HTML code to insert instead of the old one</param>
        /// <returns>If everything went good - true, otherwise - false</returns>
        Task<bool> UpsertAboutPageInnerHTML(string newInnerHtml);
    }
}
