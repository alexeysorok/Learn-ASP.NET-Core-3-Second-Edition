﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TicTacToe.Models;
using TicTacToe.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TicTacToe.Controllers
{
    public class GameInvitationController : Controller
    {
        private IStringLocalizer<GameInvitationController> _stringLocalizer;
        private IUserService _userService;
        public GameInvitationController(IUserService userService, IStringLocalizer<GameInvitationController> stringLocalizer)
        {
            _userService = userService;
            _stringLocalizer = stringLocalizer;
        }

        [HttpPost]
        public async Task<IActionResult> Index( GameInvitationModel gameInvitationModel, [FromServices]IEmailService emailService)
        {
            var gameInvitationService = Request.HttpContext.RequestServices.GetService<IGameInvitationService>();
            if (ModelState.IsValid)
            {
                try
                {
                    var invitationModel = new InvitationEmailModel
                    {
                        DisplayName = $"{gameInvitationModel.EmailTo}",
                        InvitedBy = await _userService.GetUserByEmail( gameInvitationModel.InvitedBy),
                        ConfirmationUrl = Url.Action("ConfirmGameInvitation", "GameInvitation", new { id = gameInvitationModel.Id }, Request.Scheme, Request.Host.ToString()),
                        InvitedDate = gameInvitationModel.ConfirmationDate
                    };
                    var emailRenderService =  HttpContext.RequestServices.GetService<IEmailTemplateRenderService>();
                    var message = await emailRenderService.RenderTemplate<InvitationEmailModel>("EmailTemplates/InvitationEmail",invitationModel, Request.Host.ToString());
                    await emailService.SendEmail(
                        gameInvitationModel.EmailTo, _stringLocalizer["Invitation for playing a Tic-Tac-Toe game"], message);
                }
                catch
                {
                }
                //var invitation = gameInvitationService.Add(gameInvitationModel).Result;
                //return RedirectToAction("GameInvitationConfirmation", new { id = gameInvitationModel.Id });
                var invitation =  gameInvitationService.Add(gameInvitationModel).Result;
                return RedirectToAction("GameInvitationConfirmation", new { id = invitation.Id });
            }
            return View(gameInvitationModel);
        }

        //[HttpGet]
        //public IActionResult ConfirmGameInvitation(Guid id, [FromServices]IGameInvitationService gameInvitationService)
        //{
        //    var gameInvitation = gameInvitationService.Get(id).Result;
        //    gameInvitation.IsConfirmed = true;
        //    gameInvitation.ConfirmationDate = DateTime.Now;
        //    gameInvitationService.Update(gameInvitation);
        //    return RedirectToAction("Index", "GameSession", new { id = id });
        //}
        [HttpGet]
        public async Task<IActionResult> ConfirmGameInvitation(Guid id,
         [FromServices]IGameInvitationService gameInvitationService)
        {
            var gameInvitation = await gameInvitationService.Get(id);
            gameInvitation.IsConfirmed = true;
            gameInvitation.ConfirmationDate = DateTime.Now;
            await gameInvitationService.Update(gameInvitation);
            Request.HttpContext.Session.SetString("email",
             gameInvitation.EmailTo);
            await _userService.RegisterUser(new UserModel
            {
                Email = gameInvitation.EmailTo,
                EmailConfirmationDate = DateTime.Now,
                EmailConfirmed = true,
                FirstName = "",
                LastName = "",
                Password = "Azerty123!",
                UserName = gameInvitation.EmailTo
            });
            return RedirectToAction("Index", "GameSession", new { id });
        }
        //[HttpGet]
        //public async Task<IActionResult> Index(string email)
        //{
        //    var gameInvitationModel = new GameInvitationModel
        //    {
        //        InvitedBy = email,
        //        Id = Guid.NewGuid()
        //    };
        //    Request.HttpContext.Session.SetString("email", email);
        //    var user = await _userService.GetUserByEmail(email);
        //    Request.HttpContext.Session.SetString("displayName",
        //    $"{user.FirstName} {user.LastName}");
        //    return View(gameInvitationModel);
        //}
        //[HttpGet]
        //public async Task<IActionResult> Index(string email)
        //{
        //    var gameInvitationModel = new GameInvitationModel { InvitedBy = email };
        //    HttpContext.Session.SetString("email", email);
        //    return View(gameInvitationModel);
        //}

        //[HttpPost]
        //public IActionResult Index( GameInvitationModel gameInvitationModel)
        //{
        //    return Content(_stringLocalizer["GameInvitationConfirmationMessage", gameInvitationModel.EmailTo]);
        //}
        //[HttpPost]
        //public IActionResult Index(GameInvitationModel gameInvitationModel, [FromServices]IEmailService emailService)
        //{
        //    var gameInvitationService = Request.HttpContext.RequestServices.GetService<IGameInvitationService>();
        //    if (ModelState.IsValid)
        //    {
        //        emailService.SendEmail(gameInvitationModel.EmailTo,
        //         _stringLocalizer["Invitation for playing a Tic-Tac-Toe game"],
        //         _stringLocalizer[$"Hello, you have been invited to play the Tic - Tac - Toe game by { 0}. For joining the game,please click here { 1} ", gameInvitationModel.InvitedBy,
        //        Url.Action("GameInvitationConfirmation", "GameInvitation", new
        //        {
        //            gameInvitationModel.InvitedBy,
        //            gameInvitationModel.EmailTo
        //        }, Request.Scheme,
        //       Request.Host.ToString())]);

        //        var invitation =
        //          gameInvitationService.Add(gameInvitationModel).Result;
        //        return RedirectToAction("GameInvitationConfirmation",
        //         new { id = invitation.Id });
        //    }
        //    return View(gameInvitationModel);
        //}

        [HttpGet]
        public async Task<IActionResult> GameInvitationConfirmation(
         Guid id, [FromServices]IGameInvitationService
         gameInvitationService)
        {
            return await Task.Run(() =>
            {
                var gameInvitation = gameInvitationService.Get(id).Result;
                return View(gameInvitation);
            });
        }
    }
}
