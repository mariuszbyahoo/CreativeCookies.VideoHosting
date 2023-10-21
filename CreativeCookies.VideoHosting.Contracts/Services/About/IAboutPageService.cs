using CreativeCookies.VideoHosting.DTOs.About;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Services.About
{
    public interface IAboutPageService
    {
        /// <summary>
        /// Returns the About Page's innerHTML code
        /// </summary>
        /// <returns>A DTO object with the InnerHTML code if any found, if not - then string.Empty</returns>
        Task<AboutPageDTO> GetAboutPageContents();

        /// <summary>
        /// Upserts the innerHTML code of an About page
        /// </summary>
        /// <param name="innerHtml">InnerHTML to insert</param>
        /// <returns>If everything went good - true, otherwise - false</returns>
        Task<bool> UpsertAboutPageContents(string innerHtml);
    }
}
