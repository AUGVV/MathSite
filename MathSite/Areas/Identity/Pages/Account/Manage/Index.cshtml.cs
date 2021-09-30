﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MathSite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MathSite.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private TasksContext db;

        public IndexModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, TasksContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            db = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            int AnsweredCount = db.UserTaskState.Where(x => x.UserName == userName && x.Answered == 1).Count();
            int VotedCount = db.UserTaskState.Where(x => x.UserName == userName && x.Voted == 1).Count();
            int CreatedTasks = db.Tasks.Where(x => x.Author == userName).Count();
            int SumAllRatings = db.Tasks.Where(x => x.Author == userName).Sum(x => x.SumRating);
            int SumAllVotes = db.Tasks.Where(x => x.Author == userName).Sum(x => x.SumVotes);
            int ResultRaiting = 0;
            if (SumAllRatings != 0 || SumAllVotes != 0)
            {
                 ResultRaiting = SumAllRatings / SumAllVotes;
            }
            ViewData["AnsweredCount"] = AnsweredCount;
            ViewData["VotedCount"] = VotedCount;
            ViewData["ResultRaiting"] = ResultRaiting;
            ViewData["CreatedTasks"] = CreatedTasks;
            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
