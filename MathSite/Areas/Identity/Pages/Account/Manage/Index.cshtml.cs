using System;
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
        private TasksContext DataBase;

        public IndexModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, TasksContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            DataBase = context;
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
            GetUserInfo(GetUserTaskState(userName), GetTask(userName));

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
        }

        private IEnumerable<UserTaskModel> GetUserTaskState(string UserName)
        {
             return DataBase.UserTaskState.Where(x => x.UserName == UserName);
        }

        private IEnumerable<TasksModel> GetTask(string UserName)
        {
            return DataBase.Tasks.Where(x => x.Author == UserName);
        }


        private void GetUserInfo(IEnumerable<UserTaskModel> UserTaskState, IEnumerable<TasksModel> UserTasks)
        {
            ViewData["AnsweredCount"] = GetAnswersCount(UserTaskState);
            ViewData["VotedCount"] = GetVotedCount(UserTaskState);
            ViewData["CreatedTasks"] = GetCreatedTasks(UserTasks);
            ViewData["ResultRaiting"] = GetRating(UserTasks);
        }

        private int GetAnswersCount(IEnumerable<UserTaskModel> UserTaskState)
        {
            return UserTaskState.Where(x => x.isAnswered == true).Count();
        }

        private int GetVotedCount(IEnumerable<UserTaskModel> UserTaskState)
        {
            return UserTaskState.Where(x => x.isVoted == true).Count();
        }

        private int GetCreatedTasks(IEnumerable<TasksModel> UserTasks)
        {
            return UserTasks.Count();
        }

        private int GetRating(IEnumerable<TasksModel> UserTasks)
        {
            int AllRatings = GetAllRatings(UserTasks);
            int AllVotes = GetAllVotes(UserTasks);

            int ResultRaiting = 0;
            if (AllRatings != 0 || AllVotes != 0)
            {
                ResultRaiting = AllRatings / AllVotes;
            }
            return ResultRaiting;
        }

        private int GetAllRatings(IEnumerable<TasksModel> UserTasks)
        {
            return UserTasks.Sum(x => x.SumRating);
        }

        private int GetAllVotes(IEnumerable<TasksModel> UserTasks)
        {
            return UserTasks.Sum(x => x.SumVotes);
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
