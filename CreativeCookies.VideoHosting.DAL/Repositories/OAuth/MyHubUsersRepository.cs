

using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.AspNetCore.Identity;

namespace CreativeCookies.VideoHosting.DAL.Repositories.OAuth
{
    public class MyHubUsersRepository : IMyHubUsersRepository
    {
        private readonly AppDbContext _ctx;
        private readonly UserManager<MyHubUser> _userManager;

        public MyHubUsersRepository(AppDbContext ctx, UserManager<MyHubUser> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
        }

        public Task<MyHubUserDto> GetUser(Guid id)
        {
            throw new NotImplementedException();

            // HACK: Tu jest jebana niezgodność typów i się pluje kompilator - tu trzeba najpierw zmienić cały DAL tak aby pracował na klasie MyHubUser i dopiero
            // wtedy będzie można się bawić w jakieś repozytoria czy inne pierdoły.

            //var dao = _ctx.Users.Where(u => u.Id.ToUpperInvariant().Equals(id.ToString().ToUpperInvariant())).FirstOrDefault() as MyHubUser;
            //_userManager.GetRolesAsync(dao)
            //var res = new MyHubUserDto(Guid.Parse(dao.Id), dao.Email, , dao.EmailConfirmed);
            //return res;
        }

        public Task<MyHubUserDto> GetUser(string email)
        {
            throw new NotImplementedException();
        }

        public Task<MyHubUserDto> GetUsers(string role)
        {
            throw new NotImplementedException();
        }

        public Task<MyHubUserDto> GetUsers()
        {
            throw new NotImplementedException();
        }
    }
}
