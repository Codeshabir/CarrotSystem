
using CarrotSystem.Helpers;
using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using CarrotSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarrotSystem.Controllers
{
   // [Authorize(Roles = "Employee, Supervisor, Manager, SystemAdmin")]
    public class DefinitionsController : Controller
    {
        private readonly IContextService _context;
        private readonly IAPIService _api;
        private readonly IEventWriter _logger;
        private IEmailService _emailService;
        public string eventBy = "Dashboard";

        public DefinitionsController(IEventWriter logger, IContextService context, IEmailService emailservice, IAPIService api)
        {
            _logger = logger;
            _context = context;
            _emailService = emailservice;
            _api = api;
        }

        public IActionResult ProductList()
        {
            ViewDefinitions viewModel = new ViewDefinitions();

            string strShow = "All";
            string strFilter = "All";

            viewModel.productMainGroupList = _context.GetMPSContext().ProductMainGroup.ToList();

            List<Product> productList = new List<Product>();
            productList = _api.GetProductList(strShow, "All");

            List<ProductView> productViewList = new List<ProductView>();
            productViewList = GetProductViewList(productList, strFilter);

            viewModel.productViewList = productViewList;

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SelectedProductList(ViewDefinitions selectedModel)
        {
            ViewDefinitions viewModel = new ViewDefinitions();

            string strShow = selectedModel.show;
            string strFilter = selectedModel.filter;

            viewModel.productMainGroupList = _context.GetMPSContext().ProductMainGroup.ToList();

            List<Product> productList = new List<Product>();
            productList = _api.GetProductList(strShow, "All");

            List<ProductView> productViewList = new List<ProductView>();
            productViewList = GetProductViewList(productList, strFilter);

            viewModel.productViewList = productViewList;

            return View("ProductList", viewModel);
        }

        [HttpPost]
        public IActionResult SaveCompany(ViewDefinitions newModel)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var timeSync = DateTime.Now;
            Company newComp = new Company();

            if(newModel.targetModel.CompPK.Equals("New"))
            {
                newComp.CompanyName = newModel.targetModel.CompanyName.Replace('_', '&').Replace('_', '&').Replace('_', '&').ToUpper();
                newComp.Abn = newModel.targetModel.Abn;
                newComp.Type = newModel.targetModel.Type;

                if (newModel.targetModel.Active.Equals("true"))
                {
                    newComp.Inactive = false;
                }
                else
                {
                    newComp.Inactive = true;
                }

                if (!string.IsNullOrEmpty(newModel.targetModel.Comment))
                {
                    newComp.Comment = newModel.targetModel.Comment;
                }

                newComp.UpdatedBy = userName;
                newComp.UpdatedOn = timeSync;

                _context.GetMPSContext().Company.Add(newComp);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    newComp = _context.GetMPSContext().Company.Where(w => w.UpdatedOn.Equals(timeSync)).First();
                    _logger.WriteEvents(userName, "Company", "Added", "Added Company [" + newComp.CompanyName + "]");
                }
            }
            else
            {
                newComp = _context.GetMPSContext().Company.Where(w=>w.Pk.Equals(int.Parse(newModel.targetModel.CompPK))).First();
                
                //newComp.CompanyName = newModel.targetModel.CompanyName;
                newComp.Abn = newModel.targetModel.Abn;
                newComp.Type = newModel.targetModel.Type;
                    
                if(newModel.targetModel.Active.Equals("true"))
                {
                    newComp.Inactive = false;
                }
                else
                {
                    newComp.Inactive = true;
                }

                if (!string.IsNullOrEmpty(newModel.targetModel.Comment))
                {
                    newComp.Comment = newModel.targetModel.Comment;
                }

                newComp.UpdatedBy = userName;
                newComp.UpdatedOn = timeSync;

                _context.GetMPSContext().Company.Update(newComp);
                if(_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Company", "Adjustment", "Updated Company [" + newComp.CompanyName + "]");
                }
            }

            return RedirectToAction("CompanyDetails", new { id = newComp.Pk.ToString(), pageType = "Details" });
        }

        [HttpPost]
        public IActionResult SaveBillingAddress(ViewDefinitions newModel)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var timeSync = DateTime.Now;
            
            Company company = new Company();
            company = _context.GetMPSContext().Company.Where(w=>w.Pk.Equals(int.Parse(newModel.targetModel.CompPK))).First();
            Address targetAdd = new Address();
            bool isUpdate = true;

            if (_context.GetMPSContext().Address.Any(any=>any.Company.Equals(company.CompanyName) && any.Type.Equals("Billing")))
            {
                isUpdate = true;
                targetAdd = _context.GetMPSContext().Address.Where(w =>w.Company.Equals(company.CompanyName) && w.Type.Equals("Billing")).First();
            }
            else
            {
                targetAdd.Company = company.CompanyName;
                targetAdd.AddressName = "Billing";
                targetAdd.Type = "Billing";
                isUpdate = false;
            }

            if (!string.IsNullOrEmpty(newModel.billAddress.Street))
            {
                targetAdd.Street = newModel.billAddress.Street;
            }
            else
            {
                targetAdd.Street = "";
            }

            if (!string.IsNullOrEmpty(newModel.billAddress.City))
            {
                targetAdd.City = newModel.billAddress.City;
            }
            else
            {
                targetAdd.City = "";
            }

            if (!string.IsNullOrEmpty(newModel.billAddress.State))
            {
                targetAdd.State = newModel.billAddress.State;
            }
            else
            {
                targetAdd.State = "";
            }

            if (!string.IsNullOrEmpty(newModel.billAddress.Postcode))
            {
                targetAdd.Postcode = newModel.billAddress.Postcode;
            }
            else
            {
                targetAdd.Postcode = "";
            }

            if (!string.IsNullOrEmpty(newModel.billAddress.Country))
            {
                targetAdd.Country = newModel.billAddress.Country;
            }
            else
            {
                targetAdd.Country = "";
            }

            if (!string.IsNullOrEmpty(newModel.billAddress.ContactName))
            {
                targetAdd.ContactName = newModel.billAddress.ContactName;
            }
            else
            {
                targetAdd.ContactName = "";
            }

            if (!string.IsNullOrEmpty(newModel.billAddress.Phone1))
            {
                targetAdd.Phone1 = newModel.billAddress.Phone1;
            }
            else
            {
                targetAdd.Phone1 = "";
            }

            if (!string.IsNullOrEmpty(newModel.billAddress.Phone2))
            {
                targetAdd.Phone2 = newModel.billAddress.Phone2;
            }
            else
            {
                targetAdd.Phone2 = "";
            }

            if (!string.IsNullOrEmpty(newModel.billAddress.Fax))
            {
                targetAdd.Fax = newModel.billAddress.Fax;
            }
            else
            {
                targetAdd.Fax = "";
            }

            if (!string.IsNullOrEmpty(newModel.billAddress.Email))
            {
                targetAdd.Email = newModel.billAddress.Email;
            }
            else
            {
                targetAdd.Email = "";
            }

            if (!string.IsNullOrEmpty(newModel.billAddress.Www))
            {
                targetAdd.Www = newModel.billAddress.Www;
            }
            else
            {
                targetAdd.Www = "";
            }

            if (!string.IsNullOrEmpty(newModel.billAddress.Comment))
            {
                targetAdd.Comment = newModel.billAddress.Comment;
            }
            else
            {
                targetAdd.Comment = "";
            }

            targetAdd.UpdatedBy = userName;
            targetAdd.UpdatedOn = timeSync;

            var actionType = "Address";

            if(isUpdate)
            {
                _context.GetMPSContext().Address.Update(targetAdd);
                actionType = actionType + " Adjustment";
            }
            else
            {
                _context.GetMPSContext().Address.Add(targetAdd);
                actionType = actionType + " Added";
            }
            
            if(_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Company", actionType, "[" + targetAdd.Type + "] " + actionType + "ID #" + targetAdd.Id);
            }

            return RedirectToAction("CompanyDetails", new { id = company.Pk.ToString(), pageType = "Billing" });
        }

        [HttpPost]
        public IActionResult CopyBillToShip(int compPk)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var timeSync = DateTime.Now;

            Company company = new Company();
            company = _context.GetMPSContext().Company.Where(w => w.Pk.Equals(compPk)).First();

            Address billAdd = new Address();
            Address targetAdd = new Address();

            if (_context.GetMPSContext().Address.Any(any => any.Company.Equals(company.CompanyName) && any.Type.Equals("Billing")))
            {
                billAdd = _context.GetMPSContext().Address.Where(w => w.Company.Equals(company.CompanyName) && w.Type.Equals("Billing")).First();
            }

            if (!string.IsNullOrEmpty(billAdd.Street))
            {
                targetAdd.Street = billAdd.Street;
            }
            else
            {
                targetAdd.Street = "";
            }

            if (!string.IsNullOrEmpty(billAdd.City))
            {
                targetAdd.City = billAdd.City;
            }
            else
            {
                targetAdd.City = "";
            }

            if (!string.IsNullOrEmpty(billAdd.State))
            {
                targetAdd.State = billAdd.State;
            }
            else
            {
                targetAdd.State = "";
            }

            if (!string.IsNullOrEmpty(billAdd.Postcode))
            {
                targetAdd.Postcode = billAdd.Postcode;
            }
            else
            {
                targetAdd.Postcode = "";
            }

            if (!string.IsNullOrEmpty(billAdd.Country))
            {
                targetAdd.Country = billAdd.Country;
            }
            else
            {
                targetAdd.Country = "";
            }

            if (!string.IsNullOrEmpty(billAdd.ContactName))
            {
                targetAdd.ContactName = billAdd.ContactName;
            }
            else
            {
                targetAdd.ContactName = "";
            }

            if (!string.IsNullOrEmpty(billAdd.Phone1))
            {
                targetAdd.Phone1 = billAdd.Phone1;
            }
            else
            {
                targetAdd.Phone1 = "";
            }

            if (!string.IsNullOrEmpty(billAdd.Phone2))
            {
                targetAdd.Phone2 = billAdd.Phone2;
            }
            else
            {
                targetAdd.Phone2 = "";
            }

            if (!string.IsNullOrEmpty(billAdd.Fax))
            {
                targetAdd.Fax = billAdd.Fax;
            }
            else
            {
                targetAdd.Fax = "";
            }

            if (!string.IsNullOrEmpty(billAdd.Email))
            {
                targetAdd.Email = billAdd.Email;
            }
            else
            {
                targetAdd.Email = "";
            }

            if (!string.IsNullOrEmpty(billAdd.Www))
            {
                targetAdd.Www = billAdd.Www;
            }
            else
            {
                targetAdd.Www = "";
            }

            if (!string.IsNullOrEmpty(billAdd.Comment))
            {
                targetAdd.Comment = billAdd.Comment;
            }
            else
            {
                targetAdd.Comment = "";
            }

            targetAdd.Type = "Shipping";
            targetAdd.AddressName = "Shipping";
            targetAdd.Company = company.CompanyName;
            targetAdd.UpdatedBy = userName;
            targetAdd.UpdatedOn = timeSync;

            _context.GetMPSContext().Address.Add(targetAdd);
            if(_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Company", "Address Added", "[" + targetAdd.Type + "] Address Added ID #" + targetAdd.Id);
            }

            return new JsonResult("Success");
           // return RedirectToAction("CompanyDetails", new { id = company.Pk.ToString(), pageType = "Shipping" });
        }

        [HttpPost]
        public IActionResult SaveShippingAddress(ViewDefinitions newModel)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var timeSync = DateTime.Now;
            var company = _context.GetMPSContext().Company.Where(w=>w.CompanyName.Equals(newModel.shipAddress.Company)).First();

            Address targetAdd = new Address();
            targetAdd = _context.GetMPSContext().Address.Where(any => any.Id.Equals(newModel.shipAddress.Id)).First();

            if (!string.IsNullOrEmpty(newModel.shipAddress.AddressName))
            {
                targetAdd.AddressName = newModel.shipAddress.AddressName;
            }
            else
            {
                targetAdd.AddressName = "Shipping";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Street))
            {
                targetAdd.Street = newModel.shipAddress.Street;
            }
            else
            {
                targetAdd.Street = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.City))
            {
                targetAdd.City = newModel.shipAddress.City;
            }
            else
            {
                targetAdd.City = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.State))
            {
                targetAdd.State = newModel.shipAddress.State;
            }
            else
            {
                targetAdd.State = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Postcode))
            {
                targetAdd.Postcode = newModel.shipAddress.Postcode;
            }
            else
            {
                targetAdd.Postcode = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Country))
            {
                targetAdd.Country = newModel.shipAddress.Country;
            }
            else
            {
                targetAdd.Country = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.ContactName))
            {
                targetAdd.ContactName = newModel.shipAddress.ContactName;
            }
            else
            {
                targetAdd.ContactName = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Phone1))
            {
                targetAdd.Phone1 = newModel.shipAddress.Phone1;
            }
            else
            {
                targetAdd.Phone1 = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Phone2))
            {
                targetAdd.Phone2 = newModel.shipAddress.Phone2;
            }
            else
            {
                targetAdd.Phone2 = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Fax))
            {
                targetAdd.Fax = newModel.shipAddress.Fax;
            }
            else
            {
                targetAdd.Fax = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Email))
            {
                targetAdd.Email = newModel.shipAddress.Email;
            }
            else
            {
                targetAdd.Email = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Www))
            {
                targetAdd.Www = newModel.shipAddress.Www;
            }
            else
            {
                targetAdd.Www = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Comment))
            {
                targetAdd.Comment = newModel.shipAddress.Comment;
            }
            else
            {
                targetAdd.Comment = "";
            }

            targetAdd.UpdatedBy = userName;
            targetAdd.UpdatedOn = timeSync;

            _context.GetMPSContext().Address.Update(targetAdd);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Company", "Address Adjustment", "[" + targetAdd.Type + "] Address Updated ID #" + targetAdd.Id);
            }

            return RedirectToAction("CompanyDetails", new { id = company.Pk.ToString(), pageType = "Shipping" });
        }

        [HttpPost]
        public IActionResult DeleteAddress(ViewDefinitions delModel)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var address = _context.GetMPSContext().Address.Where(w=>w.Id.Equals(delModel.addressId)).First();

            _context.GetMPSContext().Address.Remove(address);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Company", "Address Deleted", "[" + address.Type + "] Address Deleted ID #" + address.Id);
            }

            //return new JsonResult("Success");
            return RedirectToAction("CompanyDetails", new { id = delModel.compPK, pageType = "Shipping" });
        }

        [HttpPost]
        public IActionResult NewShippingAddress(ViewDefinitions newModel)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var timeSync = DateTime.Now;
            
            var company = _context.GetMPSContext().Company.Where(w=>w.Pk.Equals(int.Parse(newModel.targetModel.CompPK))).First();
            Address newShipAdd = new Address();

            newShipAdd.Company = company.CompanyName;
            newShipAdd.Type = "Shipping";

            if (!string.IsNullOrEmpty(newModel.shipAddress.AddressName))
            {
                newShipAdd.AddressName = newModel.shipAddress.AddressName;
            }
            else
            {
                newShipAdd.AddressName = "Shipping";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Street))
            {
                newShipAdd.Street = newModel.shipAddress.Street;
            }
            else
            {
                newShipAdd.Street = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.City))
            {
                newShipAdd.City = newModel.shipAddress.City;
            }
            else
            {
                newShipAdd.City = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.State))
            {
                newShipAdd.State = newModel.shipAddress.State;
            }
            else
            {
                newShipAdd.State = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Postcode))
            {
                newShipAdd.Postcode = newModel.shipAddress.Postcode;
            }
            else
            {
                newShipAdd.Postcode = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Country))
            {
                newShipAdd.Country = newModel.shipAddress.Country;
            }
            else
            {
                newShipAdd.Country = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.ContactName))
            {
                newShipAdd.ContactName = newModel.shipAddress.ContactName;
            }
            else
            {
                newShipAdd.ContactName = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Phone1))
            {
                newShipAdd.Phone1 = newModel.shipAddress.Phone1;
            }
            else
            {
                newShipAdd.Phone1 = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Phone2))
            {
                newShipAdd.Phone2 = newModel.shipAddress.Phone2;
            }
            else
            {
                newShipAdd.Phone2 = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Fax))
            {
                newShipAdd.Fax = newModel.shipAddress.Fax;
            }
            else
            {
                newShipAdd.Fax = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Email))
            {
                newShipAdd.Email = newModel.shipAddress.Email;
            }
            else
            {
                newShipAdd.Email = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Www))
            {
                newShipAdd.Www = newModel.shipAddress.Www;
            }
            else
            {
                newShipAdd.Www = "";
            }

            if (!string.IsNullOrEmpty(newModel.shipAddress.Comment))
            {
                newShipAdd.Comment = newModel.shipAddress.Comment;
            }
            else
            {
                newShipAdd.Comment = "";
            }

            newShipAdd.UpdatedBy = userName;
            newShipAdd.UpdatedOn = timeSync;

            _context.GetMPSContext().Address.Add(newShipAdd);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Company", "Address Added", "[" + newShipAdd.Type + "] Address Added ID #" + newShipAdd.Id);
            }

            return RedirectToAction("CompanyDetails", new { id = company.Pk.ToString(), pageType = "Shipping" });
        }

        public IActionResult GetShipAddress(int addId)
        {
            Address targetAdd = new Address();

            if (_context.GetMPSContext().Address.Any(w => w.Id.Equals(addId)))
            {
                targetAdd = _context.GetMPSContext().Address.Where(w => w.Id.Equals(addId)).First();

                var returnModel = new ViewDefinitions();
                returnModel.shipAddress = targetAdd;

                return PartialView("_ShipAddressDetail", returnModel);
            }
            else
            {
                return new JsonResult("None");
            }
        }

        public IActionResult CompanyList()
        {
            //SyncCompanyPK();

            ViewDefinitions viewModel = new ViewDefinitions();

            string strShow = "All";
            string strStatus = "All";

            List<Company> companyList = new List<Company>();
            companyList = _api.GetCompanyList(strShow, strStatus);

            List<CompanyView> compViewList = new List<CompanyView>();
            compViewList = GetCompanyViewList(companyList);

            viewModel.companyList = compViewList;

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult IsDuplicateName(string compName)
        {
            var returnVal = true;

            var companyName = compName.Replace('_','&').Replace('_', '&').Replace('_', '&').ToUpper();

            if (_context.GetMPSContext().Company.Any(a=>a.CompanyName.Equals(companyName)))
            {
                returnVal = true;
            }
            else
            {
                returnVal = false;
            }

            return new JsonResult(returnVal);
        }

        [HttpPost]
        public IActionResult SelectedCompanyList(ViewDefinitions selectedModel)
        {
            ViewDefinitions viewModel = new ViewDefinitions();

            string strShow = selectedModel.show;
            string strStatus = selectedModel.status;

            List<Company> companyList = new List<Company>();
            companyList = _api.GetCompanyList(strShow, strStatus);

            List<CompanyView> compViewList = new List<CompanyView>();
            compViewList = GetCompanyViewList(companyList);

            viewModel.companyList = compViewList;

            return View("CompanyList", viewModel);
        }

        public IActionResult ProductsDetails(string id)
        {
            ViewProduct viewModel = new ViewProduct();
            List<ProductRecipe> recipeList = new List<ProductRecipe>();
            List<RecipeJsonView> recipeItemList = new List<RecipeJsonView>();

            Product product = new Product();
            ProductDetailView detailsView = new ProductDetailView();

            if (id.Equals("New"))
            {
                viewModel.pageTitle = "New";
                viewModel.isNew = true;
            }
            else
            {
                viewModel.pageTitle = "Edit";
                viewModel.isNew = false;
                if(_context.GetMPSContext().Product.Any(a=>a.Id.Equals(int.Parse(id))))
                {
                    product = _context.GetMPSContext().Product.Where(a => a.Id.Equals(int.Parse(id))).First();
                }

                ProductMainGroup mainGroup = new ProductMainGroup();
                ProductMinorGroup minorGroup = new ProductMinorGroup();
                ProductSubGroup subGroup = new ProductSubGroup();

                minorGroup = _context.GetMPSContext().ProductMinorGroup.Where(w=>w.Id.Equals(product.MinorGroupId)).First();
                subGroup = _context.GetMPSContext().ProductSubGroup.Where(w => w.Id.Equals(minorGroup.SubGroupId)).First();
                mainGroup = _context.GetMPSContext().ProductMainGroup.Where(w => w.Id.Equals(subGroup.MainGroupId)).First();

                detailsView.ProductId = product.Id;
                detailsView.Code = product.Code;
                detailsView.Desc = product.Desc;
                detailsView.Active = !product.Inactive.Value;
                detailsView.Unit = product.Unit;
                detailsView.Tax = product.Tax;
                detailsView.Comment = product.Comment;

                detailsView.MainGroupId = mainGroup.Id;
                detailsView.MinorGroupId = minorGroup.Id;
                detailsView.SubGroupId = subGroup.Id;

                detailsView.MainGroup = mainGroup.Type;
                detailsView.MinorGroup = minorGroup.Type;
                detailsView.SubGroup = subGroup.Type;

                detailsView.Prefix = minorGroup.Prefix;
                detailsView.Suffix = product.Code.Replace(detailsView.Prefix, "");

                if(_context.GetMPSContext().ProductRecipe.Any(a=>a.ProductCode.Equals(detailsView.Code)))
                {
                    recipeList = _context.GetMPSContext().ProductRecipe.Where(w => w.ProductCode.Equals(detailsView.Code)).ToList();
                }

                recipeItemList = GetNewRecipeItem(minorGroup.Id);
            }

            viewModel.productDetailView = detailsView;
            viewModel.recipeList = recipeList;
            viewModel.recipeItemList = recipeItemList;

            List<ProductMainGroup> mainGroupList = new List<ProductMainGroup>();
            List<ProductMinorGroup> minorGroupList = new List<ProductMinorGroup>();
            List<ProductSubGroup> subGroupList = new List<ProductSubGroup>();
            List<ProductUnit> unitList = new List<ProductUnit>();
            List<Tax> taxList = new List<Tax>();

            unitList = _context.GetMPSContext().ProductUnit.ToList();
            taxList = _context.GetMPSContext().Tax.ToList();
            mainGroupList = _context.GetMPSContext().ProductMainGroup.ToList();
            minorGroupList = _context.GetMPSContext().ProductMinorGroup.ToList();
            subGroupList = _context.GetMPSContext().ProductSubGroup.ToList();
            
            viewModel.unitList = unitList;
            viewModel.taxList = taxList;
            viewModel.mainGroupList = mainGroupList;
            viewModel.minorGroupList = minorGroupList;
            viewModel.subGroupList = subGroupList;

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult DeleteProduct(int productId)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (productId > 0)
            {
                var delProduct = _context.GetMPSContext().Product.Where(w => w.Id.Equals(productId)).First();

                _context.GetMPSContext().Product.Remove(delProduct);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Product", "Deleted", "Product [" + delProduct.Code + "] Deleted");
                }
            }

            return new JsonResult("Success");
        }

        [HttpPost]
        public IActionResult SaveProduct(ViewProduct viewModel)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var timeSync = DateTime.Now;
            var compId = viewModel.productDetailView.ProductId;

            Product targetProduct = new Product();

            if (compId > 0)
            {
                targetProduct = _context.GetMPSContext().Product.Where(w => w.Id.Equals(compId)).First();

                targetProduct.Code = viewModel.productDetailView.Code;
                targetProduct.Desc = viewModel.productDetailView.Desc;
                targetProduct.MinorGroupId = viewModel.productDetailView.MinorGroupId;
                targetProduct.Unit = viewModel.productDetailView.Unit;
                targetProduct.Tax = viewModel.productDetailView.Tax;

                targetProduct.Inactive = !viewModel.productDetailView.Active;

                if (!string.IsNullOrEmpty(viewModel.productDetailView.Comment))
                {
                    targetProduct.Comment = viewModel.productDetailView.Comment;
                }

                targetProduct.UpdatedBy = userName;
                targetProduct.UpdatedOn = timeSync;

                _context.GetMPSContext().Product.Update(targetProduct);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Product", "Adjustment", "Product [" + targetProduct.Code + "] Adjustment");
                }
            }
            else
            {
                targetProduct.Code = viewModel.productDetailView.Code;
                targetProduct.Desc = viewModel.productDetailView.Desc;
                targetProduct.MinorGroupId = viewModel.productDetailView.MinorGroupId;
                targetProduct.Unit = viewModel.productDetailView.Unit;
                targetProduct.Tax = viewModel.productDetailView.Tax;

                targetProduct.Inactive = !viewModel.productDetailView.Active;

                if (!string.IsNullOrEmpty(viewModel.productDetailView.Comment))
                {
                    targetProduct.Comment = viewModel.productDetailView.Comment;
                }

                targetProduct.UpdatedBy = userName;
                targetProduct.UpdatedOn = timeSync;

                _context.GetMPSContext().Product.Add(targetProduct);
                if(_context.GetMPSContext().SaveChanges() > 0)
                {
                    targetProduct = _context.GetMPSContext().Product.Where(w=>w.UpdatedOn.Equals(timeSync)).First();
                    _logger.WriteEvents(userName, "Product", "Added", "Product [" + targetProduct.Code + "] Added");
                }

                //SendConfirmationEmail("Product")
            }

            return RedirectToAction("ProductsDetails", new { id = targetProduct.Id.ToString() });
        }

        [HttpPost]
        public IActionResult AddRecipeItem(int productId, int repProductId, double repQty, bool isEmbeded)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (productId > 0)
            {
                ProductRecipe newRepItem = new ProductRecipe();

                newRepItem.ProductCode = _context.GetMPSContext().Product.Where(w=>w.Id.Equals(productId)).First().Code;
                newRepItem.Component = _context.GetMPSContext().Product.Where(w => w.Id.Equals(repProductId)).First().Code;
                newRepItem.Qty = repQty;
                newRepItem.Embeded = isEmbeded;

                newRepItem.UpdatedBy = userName;
                newRepItem.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().ProductRecipe.Add(newRepItem);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Product", "Recipe Item Added", "Product Recipe Item [" + newRepItem.ProductCode + "] Added");
                }
            }

            return new JsonResult("Success");
        }

        public IActionResult CompanyDetails(string id, string pageType)
        {
            ViewDefinitions viewModel = new ViewDefinitions();

            Address address = new Address();
            Address billAddress = new Address();
            List<Address> shippingAddressList = new List<Address>();
            List<MappingView> mappingList = new List<MappingView>();

            if (id.Equals("New"))
            {
                viewModel.pageTitle = "New";
            }
            else
            {
                viewModel.pageTitle = "Edit";

                var company = _context.GetMPSContext().Company.Where(w=>w.Pk.Equals(int.Parse(id))).First();
                
                if(_context.GetMPSContext().Address.Any(w => w.Company.Equals(company.CompanyName)))
                {
                    address = _context.GetMPSContext().Address.Where(w => w.Company.Equals(company.CompanyName)).First();
                }

                if(_context.GetMPSContext().Address.Any(w => w.Company.Equals(company.CompanyName) && w.Type.Equals("Billing")))
                {
                    billAddress = _context.GetMPSContext().Address.Where(w => w.Company.Equals(company.CompanyName) && w.Type.Equals("Billing")).First();
                }

                List<ProductMapping> productMappingList = new List<ProductMapping>();
                productMappingList = _context.GetMPSContext().ProductMapping.Where(w => w.Company.Equals(company.CompanyName)).ToList();

                if(productMappingList.Count > 0)
                {
                    mappingList = GetMappingList(productMappingList);
                }
                
                shippingAddressList = _context.GetMPSContext().Address.Where(w=>w.Company.Equals(company.CompanyName) && (!w.Type.Equals("Billing"))).ToList();
                viewModel.company = company;
            }

            viewModel.mappingList = mappingList;
            viewModel.dataId = id;
            viewModel.shippingAddressList = shippingAddressList;
            viewModel.address = address;
            viewModel.billAddress = billAddress;
            viewModel.pageType = pageType;

            return View(viewModel);
        }

        public IActionResult CompanyDetail(string id)
        {
            ViewDefinitions viewModel = new ViewDefinitions();

            Address address = new Address();
            Address billAddress = new Address();
            List<Address> shippingAddressList = new List<Address>();
            List<MappingView> mappingList = new List<MappingView>();

            if (id.Equals("New"))
            {
                viewModel.pageTitle = "New";
            }
            else
            {
                viewModel.pageTitle = "Edit";

                var company = _context.GetMPSContext().Company.Where(w => w.Pk.Equals(int.Parse(id))).First();

                if (_context.GetMPSContext().Address.Any(w => w.Company.Equals(company.CompanyName)))
                {
                    address = _context.GetMPSContext().Address.Where(w => w.Company.Equals(company.CompanyName)).First();
                }

                if (_context.GetMPSContext().Address.Any(w => w.Company.Equals(company.CompanyName) && w.Type.Equals("Billing")))
                {
                    billAddress = _context.GetMPSContext().Address.Where(w => w.Company.Equals(company.CompanyName) && w.Type.Equals("Billing")).First();
                }

                List<ProductMapping> productMappingList = new List<ProductMapping>();
                productMappingList = _context.GetMPSContext().ProductMapping.Where(w => w.Company.Equals(company.CompanyName)).ToList();

                if (productMappingList.Count > 0)
                {
                    mappingList = GetMappingList(productMappingList);
                }

                shippingAddressList = _context.GetMPSContext().Address.Where(w => w.Company.Equals(company.CompanyName) && (!w.Type.Equals("Billing"))).ToList();
                viewModel.company = company;
            }

            viewModel.mappingList = mappingList;
            viewModel.dataId = id;
            viewModel.shippingAddressList = shippingAddressList;
            viewModel.address = address;
            viewModel.billAddress = billAddress;
            viewModel.pageType = "Details";

            return View("CompanyDetails", viewModel);
        }

        [HttpPost]
        public IActionResult DeleteCompany(int compId)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (compId > 0)
            {
                var editCompany = _context.GetMPSContext().Company.Where(w => w.Pk.Equals(compId)).First();

                _context.GetMPSContext().Company.Remove(editCompany);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Company", "Deleted", "Company [" + editCompany.CompanyName + "] Deleted");
                }
            }

            return new JsonResult("Success");
        }

        [HttpPost]
        public IActionResult GetDetails(string content, int dataId)
        {
            if (content.Equals("shippingAddress"))
            {
                var returnData = _context.GetMPSContext().Address.Where(w=>w.Id.Equals(dataId)).First();

                return new JsonResult(returnData);
            }
            else if (content.Equals("SubGroup"))
            {
                var returnData = _context.GetMPSContext().ProductSubGroup.Where(w=>w.MainGroupId.Equals(dataId)).ToList();

                return new JsonResult(returnData);
            }
            else if (content.Equals("MinorGroup"))
            {
                var returnData = _context.GetMPSContext().ProductMinorGroup.Where(w => w.SubGroupId.Equals(dataId)).ToList();

                return new JsonResult(returnData);
            }
            else if (content.Equals("ProductDetails"))
            {
                var returnData = _context.GetMPSContext().ProductMinorGroup.Where(w => w.Id.Equals(dataId)).First();

                return new JsonResult(returnData);
            }
            else if (content.Equals("ProductRecipe"))
            {
                var returnData = _context.GetMPSContext().Product.Where(w => w.Id.Equals(dataId)).First().Code;

                return new JsonResult(returnData);
            }
            else
            {
                return new JsonResult("");
            }
        }

        [HttpPost]
        public void ProductCountable(int productId, bool isChecked)
        {
            //
            if(_context.GetMPSContext().Product.Any(a=>a.Id.Equals(productId)))
            {
                var product = _context.GetMPSContext().Product.Where(w=>w.Id.Equals(productId)).First();

                if(_context.GetMPSContext().ProductSettings.Any(x=>x.ProductId.Equals(product.Id)))
                {
                    var setting = _context.GetMPSContext().ProductSettings.Where(x => x.ProductId.Equals(product.Id)).First();
                    
                    setting.IsStockCountable = isChecked;

                    _context.GetMPSContext().ProductSettings.Update(setting);
                    _context.GetMPSContext().SaveChanges();

                }
                else
                {
                    ProductSetting setting = new ProductSetting();

                    setting.ProductCode = product.Code;
                    setting.ProductId = product.Id;
                    setting.IsStockCountable = isChecked;

                    _context.GetMPSContext().ProductSettings.Add(setting);
                    _context.GetMPSContext().SaveChanges();
                }

            }

        }

        [HttpPost]
        public IActionResult UpdatePrefix(int minorId, string prefix)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            var minorGroup = _context.GetMPSContext().ProductMinorGroup.Where(w=>w.Id.Equals(minorId)).First();

            minorGroup.Prefix = prefix;

            _context.GetMPSContext().ProductMinorGroup.Update(minorGroup);
            if (_context.GetMPSContext().SaveChanges() > 0)
            {
                _logger.WriteEvents(userName, "Product", "Adjustment", "Minor Group ID# " + minorGroup.Id + ", prefix [" + minorGroup.Prefix + "] Updated");
            }

            return new JsonResult("Success");
        }

        [HttpPost]
        public IActionResult NewGroup(ViewProduct viewModel)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            var productId = viewModel.productId;
            var groupType = viewModel.newGroupType;
            var groupName = viewModel.newGroupName;

            _logger.WriteTestLog("Type : " + groupType + ", Name : " + groupName);

            if(groupType.Equals("MainGroup"))
            {
                ProductMainGroup newMainGroup = new ProductMainGroup();

                newMainGroup.Type = groupName;
                newMainGroup.UpdatedBy = userName;
                newMainGroup.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().ProductMainGroup.Add(newMainGroup);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Product", "Added Main Group", "Main Group [" + groupName + "] Added");
                }

            }
            else if(groupType.Equals("SubGroup"))
            {
                var mainGroupId = int.Parse(viewModel.newMainGroupId);

                ProductSubGroup newSubGroup = new ProductSubGroup();

                newSubGroup.Type = groupName;
                newSubGroup.MainGroupId = mainGroupId;
                newSubGroup.UpdatedBy = userName;
                newSubGroup.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().ProductSubGroup.Add(newSubGroup);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Product", "Added Sub Group", "Sub Group [" + groupName + "] Added");
                }
            }
            else if(groupType.Equals("MinorGroup"))
            {
                var subGroupId = int.Parse(viewModel.newSubGroupId);

                ProductMinorGroup newMinorGroup = new ProductMinorGroup();

                newMinorGroup.Type = groupName;
                newMinorGroup.SubGroupId = subGroupId;
                
                if(!string.IsNullOrEmpty(viewModel.newGroupName))
                {
                    newMinorGroup.Prefix = viewModel.newGroupName;
                }
                else
                {
                    newMinorGroup.Prefix = "";
                }

                newMinorGroup.UpdatedBy = userName;
                newMinorGroup.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().ProductMinorGroup.Add(newMinorGroup);
                if (_context.GetMPSContext().SaveChanges() > 0)
                {
                    _logger.WriteEvents(userName, "Product", "Added Minor Group", "Minor Group [" + groupName + "] Added");
                }
            }

            return RedirectToAction("ProductsDetails", new { id = productId });
        }

        [HttpPost]
        public IActionResult DeleteGroup(string groupType, int groupId, string groupName)
        {
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);

            if (groupType.Equals("Main"))
            {
                if(_context.GetMPSContext().ProductMainGroup.Any(x=>x.Id.Equals(groupId)))
                {
                    var removeGroup = _context.GetMPSContext().ProductMainGroup.Where(x=>x.Id.Equals(groupId)).First();
                    
                    _context.GetMPSContext().ProductMainGroup.Remove(removeGroup);
                    
                    if (_context.GetMPSContext().SaveChanges() > 0)
                    {
                        _logger.WriteEvents(userName, "Product", "Deleted Main Group", "Main Group [" + groupName + "] Deleted");
                    }
                }
            }
            else if (groupType.Equals("Sub"))
            {
                if (_context.GetMPSContext().ProductSubGroup.Any(x => x.Id.Equals(groupId)))
                {
                    var removeGroup = _context.GetMPSContext().ProductSubGroup.Where(x => x.Id.Equals(groupId)).First();

                    _context.GetMPSContext().ProductSubGroup.Remove(removeGroup);

                    if (_context.GetMPSContext().SaveChanges() > 0)
                    {
                        _logger.WriteEvents(userName, "Product", "Deleted Sub Group", "Sub Group [" + groupName + "] Deleted");
                    }
                }
            }
            else if (groupType.Equals("Minor"))
            {
                if (_context.GetMPSContext().ProductMinorGroup.Any(x => x.Id.Equals(groupId)))
                {
                    var removeGroup = _context.GetMPSContext().ProductMinorGroup.Where(x => x.Id.Equals(groupId)).First();

                    _context.GetMPSContext().ProductMinorGroup.Remove(removeGroup);

                    if (_context.GetMPSContext().SaveChanges() > 0)
                    {
                        _logger.WriteEvents(userName, "Product", "Deleted Minor Group", "Minor Group [" + groupName + "] Deleted");
                    }
                }
            }

            return new JsonResult("Success");
        }

        // Get Methods

        public List<RecipeJsonView> GetNewRecipeItem(int minorId)
        {
            List<RecipeJsonView> recipeItemList = new List<RecipeJsonView>();
            List<Product> productList = _context.GetMPSContext().Product.Where(w=>w.Id > 0).ToList();

            if(productList.Count > 0)
            {
                foreach(var product in productList)
                {
                    RecipeJsonView newItem = new RecipeJsonView();

                    newItem.MinorId = product.MinorGroupId.Value;
                    newItem.Code = product.Code;
                    newItem.Desc = product.Desc;
                    newItem.ProductId = product.Id;

                    recipeItemList.Add(newItem);
                }
            }

            return recipeItemList;
        }

        public List<ProductView> GetProductViewList(List<Product> productList, string filter)
        {
            List<ProductView> productViewList = new List<ProductView>();

            foreach (var product in productList)
            {
                ProductView productView = new ProductView();

                productView.ProductId = product.Id;
                productView.Code = product.Code;
                productView.Desc = product.Desc;

                if (product.Inactive.Value)
                {
                    productView.Active = "Inactive";
                }
                else
                {
                    productView.Active = "Active";
                }

                if(product.Weight.HasValue)
                {
                    productView.Weight = product.Weight.Value;
                }
                else
                {
                    productView.Weight = 0;
                }

                if (product.Space.HasValue)
                {
                    productView.Space = product.Space.Value;
                }
                else
                {
                    productView.Space = 0;
                }

                if(_context.GetMPSContext().ProductSettings.Any(x=>x.ProductId.Equals(product.Id) && x.ProductCode.Equals(product.Code)))
                {
                    productView.IsCountable = _context.GetMPSContext().ProductSettings.Where(x => x.ProductId.Equals(product.Id) && x.ProductCode.Equals(product.Code)).First().IsStockCountable;
                }
                else
                {
                    productView.IsCountable = true;
                }

                productView.Tax = product.Tax;

                var minorGroup = _context.GetMPSContext().ProductMinorGroup.Where(w=>w.Id.Equals(product.MinorGroupId)).FirstOrDefault();
                productView.MinorGroup = minorGroup.Type;

                var subgroup = _context.GetMPSContext().ProductSubGroup.Where(w => w.Id.Equals(minorGroup.SubGroupId)).First();
                productView.SubGroup = subgroup.Type;

                var mainGroup = _context.GetMPSContext().ProductMainGroup.Where(w => w.Id.Equals(subgroup.MainGroupId)).First();
                productView.MainGroup = mainGroup.Type;

                productView.Comment = product.Comment;
                    
                if (product.Inactive.Value)
                {
                    productView.Active = "Inactive";
                }
                else
                {
                    productView.Active = "Active";
                }

                if(filter.Equals("All") || filter.Equals(productView.MainGroup))
                {
                    productViewList.Add(productView);
                }
            }

            return productViewList;
        }

        public List<MappingView> GetMappingList(List<ProductMapping> mappingList)
        {
            List<MappingView> returnList = new List<MappingView>();

            foreach(var item in mappingList)
            {
                MappingView mapping = new MappingView();

                mapping.MercCode = item.MercCode;
                mapping.MercDesc = _context.GetMPSContext().Product.Where(w=>w.Code.Equals(item.MercCode)).First().Desc;
                mapping.CompanyCode = item.CompanyCode;
                mapping.CompanyDesc = item.CompanyDesc;
                mapping.MappingId = item.Id;

                returnList.Add(mapping);
            }

            return returnList;
        }

        public List<CompanyView> GetCompanyViewList(List<Company> compList)
        {
            List<CompanyView> compViewList = new List<CompanyView>();

            foreach (var comp in compList)
            {
                if (_context.GetMPSContext().Address.Any(a => a.Company.Equals(comp.CompanyName)))
                {
                    var addressModel = _context.GetMPSContext().Address.Where(w => w.Company.Equals(comp.CompanyName)).First();
                    var address = addressModel.Street + " " + addressModel.City + " " + addressModel.State + " " + addressModel.Postcode;

                    CompanyView compView = new CompanyView();

                    compView.CompanyName = comp.CompanyName;
                    compView.Type = comp.Type;
                    compView.Abn = comp.Abn;
                    compView.Pk = comp.Pk;
                    compView.Comment = comp.Comment;

                    if (comp.Inactive.Value)
                    {
                        compView.Active = "Inactive";
                    }
                    else
                    {
                        compView.Active = "Active";
                    }

                    compView.Address = address;

                    compViewList.Add(compView);
                }
                else
                {
                    CompanyView compView = new CompanyView();

                    compView.CompanyName = comp.CompanyName;
                    compView.Type = comp.Type;
                    compView.Abn = comp.Abn;
                    compView.Pk = comp.Pk;
                    compView.Comment = comp.Comment;

                    if (comp.Inactive.Value)
                    {
                        compView.Active = "Inactive";
                    }
                    else
                    {
                        compView.Active = "Active";
                    }

                    compViewList.Add(compView);
                }
            }

            return compViewList;
        }

        [HttpPost]
        public IActionResult SendConfirmationEmail(string emailType, string changeName)
        {
            //Email Information
            var userName = _api.GetFullName(HttpContext.User.Identity.Name);
            string subject = "Confirmation Email -" + emailType + "(" + changeName + ")" + DateTime.Now.ToString("dd/MM/yyyy");
            string emails = "";

            if (emailType.Equals("Company"))
            {
                emails = _context.GetMPSContext().EmailGroup.Where(w => w.GroupName.Equals("NewCompany")).First().EmailAddress;
            }
            else if (emailType.Equals("Product"))
            {
                emails = _context.GetMPSContext().EmailGroup.Where(w => w.GroupName.Equals("NewProduct")).First().EmailAddress;
            }
            else
            {
                
            }

            //Don't Change from Address
            string from = "admin@myproductionsystem.au";

            string emailMessage = "This is a confirmation email for new " + emailType + "(" + changeName + ")" +
                                  "Please find the attached invoice file.<br />" +
                                  "Thank you and kind regards. <br /><br />" +
                                  "<b>[THIS IS AN AUTOMATED MESSAGE - PLEASE DO NOT REPLY DIRECTLY TO THIS EMAIL]</b>";

            _emailService.Execute(from, emails, subject, emailMessage);

            // logging and message
            _logger.WriteEvents(userName, "Email", "Sent", "Confirmation Email - " + emailType + " [" + changeName + "]");

            return new JsonResult("Success");
        }

        // End Methods
    }
}
