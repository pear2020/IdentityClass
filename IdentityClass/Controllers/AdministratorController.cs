using IdentityClass.Models;
using IdentityClass.Views.Administrator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityClass.Controllers
{
    //  [Authorize(Roles = "Admin,User")]
    [Authorize(Roles = "Admin")]
    public class AdministratorController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AdministratorController(
              UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        // list all current roles, links to manage roles
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRole model)
        {
            if (ModelState.IsValid) 
            {
                IdentityRole role = new IdentityRole { Name = model.RoleName };

                var result = await roleManager.CreateAsync(role);

                if (result.Succeeded) 
                {
                    return RedirectToAction("ListRole");
                }

                foreach (var error in result.Errors) 
                {
                    ModelState.AddModelError("",error.Description);
                }
            }
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ListRole()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id) 
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null) 
            { 
                // Error Message
            }
            var model = new EditRole
            {
                Id = role.Id,
                RoleName = role.Name
            };

            foreach (var user in userManager.Users)
            {
                if (await userManager.IsInRoleAsync(user,role.Name)) 
                {
                    model.Users.Add(user.UserName);
                }
            }
            return View(model);
        
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRole model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                // Error Message
            }
            else 
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role);
                if (result.Succeeded) 
                {
                    return RedirectToAction("ListRole");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            
            }
            
            return View(model);

        }

        [HttpGet]
        public async Task<IActionResult> DeleteRole(string roleId) 
        {
            ViewBag.roleId = roleId;
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null) 
            {
                // Error message
                ViewBag.ErrorMessage = $"Role with id= {roleId} cannot be found";
            }
            var model = new List<UserRole>();
            foreach (var user in userManager.Users)
            {
                var userrole = new UserRole
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userrole.IsSelected = true;
                }
                else 
                {
                    userrole.IsSelected = false;
                }
                model.Add(userrole);
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId) 
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null) 
            {
                // 
                ViewBag.ErrorMessage = $"Role with id= {roleId} cannot be found";
            }
            var model = new List<UserRole>();
            foreach (var user in userManager.Users)
            {
                var userrole = new UserRole
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userrole.IsSelected = true;
                }
                else
                {
                    userrole.IsSelected = false;
                }
                model.Add(userrole);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRole> model,string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                // 
                ViewBag.ErrorMessage = $"Role with id= {roleId} cannot be found";
            }

            for (int i = 0; i < model.Count; i++) 
            {
                var user = await userManager.FindByIdAsync(model[i].UserId);
                IdentityResult result = null;
                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && (await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else 
                {
                    continue;
                }
                if (result.Succeeded) 
                {
                    if (i < (model.Count - 1))
                    {
                        continue;
                    }
                    else 
                    {
                        return RedirectToAction("EditRole", new { id = roleId});
                    }
                }  
            }
            return RedirectToAction("EditRole", new { id = roleId }); // 失败也返回
        }

    }
}
