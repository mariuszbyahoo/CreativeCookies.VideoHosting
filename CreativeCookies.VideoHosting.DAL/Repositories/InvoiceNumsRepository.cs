using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.Repositories
{
    public class InvoiceNumsRepository : IInvoiceNumsRepository
    {
        private readonly AppDbContext _ctx;

        public InvoiceNumsRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<string> GetNewNumber()
        {
            var currentDateStr = DateTime.UtcNow.ToString("ddMMyyyy");

            var todayCount = await _ctx.InvoiceNums
                                       .CountAsync(inv => inv.Id.StartsWith(currentDateStr)) + 1;

            var newInvoiceNum = $"{currentDateStr}/{todayCount}";

            var invoiceNumEntity = new InvoiceNum { Id = newInvoiceNum };
            _ctx.InvoiceNums.Add(invoiceNumEntity);
            _ctx.SaveChanges();

            return newInvoiceNum;
        }
    }
}
