using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using X_Admin_API.Models.DB;
using X_Admin_API.Repository.Interface;
using X_Admin_API.Models.DTO.Customer;
using System.Threading.Tasks;
using X_Admin_API.Helper;
using System.Web;
using System.Net;

namespace X_Admin_API.Repository
{
    public class CustomerRepository : ICreate<CustomerNewDTO, CustomerViewDTO>,
                                      ISelectByID<CustomerViewDTO>,
                                      IEdit<CustomerEditDTO, CustomerViewDTO>,
                                      IDelete,
                                      IGetList<CustomerListDTO>,
                                      ISearch<CustomerListDTO>
    {
        private THEntities db = null;

        public CustomerRepository()
        {
            db = new THEntities();
        }

        //-> Create
        public async Task<CustomerViewDTO> Create(CustomerNewDTO newDTO)
        {
            newDTO = StringHelper.TrimStringProperties(newDTO);
            var checkName = await db.tblCustomers.FirstOrDefaultAsync(x => x.cust_Deleted == null && x.name == newDTO.name); // check whether itemgroup name exist or not
            if (checkName != null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "This customer name already exsits.");

            tblCustomer customer = (tblCustomer)Helper.Helper.MapDTOToDBClass<CustomerNewDTO, tblCustomer>(newDTO, new tblCustomer());
            customer.cust_CreatedDate = DateTime.Now;
            db.tblCustomers.Add(customer);
            await db.SaveChangesAsync();
            db.Entry(customer).Reload();
            return await SelectByID(customer.id);
        }

        //-> SelectByID
        public async Task<CustomerViewDTO> SelectByID(int id)
        {
            var customer = await db.tblCustomers.FirstOrDefaultAsync(r => r.cust_Deleted == null && r.id == id);
            if (customer == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "NotFound");
            return Helper.Helper.MapDBClassToDTO<tblCustomer, CustomerViewDTO>(customer);
        }

        //-> Edit
        public async Task<CustomerViewDTO> Edit(CustomerEditDTO editDTO)
        {
            editDTO = StringHelper.TrimStringProperties(editDTO);

            tblCustomer customer = await db.tblCustomers.FirstOrDefaultAsync(r => r.cust_Deleted == null && r.id == editDTO.id);
            if (customer == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "This record has been deleted");

            var checkName = await db.tblCustomers.FirstOrDefaultAsync(x => x.cust_Deleted == null && x.name == editDTO.name && x.id != editDTO.id);
            if (checkName != null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "This name already exsits");

            customer = (tblCustomer)Helper.Helper.MapDTOToDBClass<CustomerEditDTO, tblCustomer>(editDTO, customer);
            customer.cust_UpdatedDate = DateTime.Now;
            await db.SaveChangesAsync();
            return await SelectByID(customer.id);
        }

        //-> Delete
        public async Task<Boolean> Delete(int id)
        {
            var customer = await db.tblCustomers.FirstOrDefaultAsync(r => r.cust_Deleted == null && r.id == id);
            if (customer == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "NotFound");
            customer.cust_Deleted = "1";
            await db.SaveChangesAsync();
            return true;
        }

        //-> GetList
        public async Task<CustomerListDTO> GetList(int currentPage)
        {
            IQueryable<tblCustomer> customers = from r in db.tblCustomers
                                                where r.cust_Deleted == null
                                                orderby r.name ascending
                                                select r;
            return await Listing(currentPage, customers);
        }

        //-> Search
        public async Task<CustomerListDTO> Search(int currentPage, string search)
        {
            IQueryable<tblCustomer> customers = from r in db.tblCustomers
                                                where r.cust_Deleted == null && (r.name.Contains(search) || r.code.Contains(search))
                                                orderby r.name ascending
                                                select r;
            return await Listing(currentPage, customers, search);
        }

        //-- private method --//
        private async Task<CustomerListDTO> Listing(int currentPage, IQueryable<tblCustomer> customers, string search = null)
        {
            int startRow = Helper.Helper.GetStartRow(currentPage);
            var customerViewList = new List<CustomerViewDTO>();
            var myCustomers = await customers.Skip(startRow).Take(Helper.Helper.pageSize).ToListAsync();
            foreach (var customer in myCustomers)
            {
                customerViewList.Add(await SelectByID(customer.id));
            }
            CustomerListDTO customerList = new CustomerListDTO();
            customerList.metaData = await Helper.Helper.GetMetaData(currentPage, customers, "name", "asc", search);
            customerList.results = customerViewList;
            return customerList;
        }
    }
}