﻿using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using T3RMSWS.Data;

namespace T3RMSWS.Areas.Management.Controllers
{
    [Area("Management")]
    [Authorize(Roles = "Manager")]
    public class Reservation : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ReservationService _service;
        public Reservation(UserManager<IdentityUser> userManager, ReservationService service)
        {
            _userManager = userManager;
            _service = service;
        }
        public async Task<IActionResult> Index()
        {
            var reservationIndexViewModel = await _service.GetReservationsForManager();
            return View(reservationIndexViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
            var reservation = await _service.GetOneReservation(id);
            if (reservation == null)
                return NotFound();
            return View(reservation);
        }

        [HttpPost, ActionName(nameof(Delete))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteReservation(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            var reservation = await _service.GetOneReservation(id);
            if (reservation == null)
                return NotFound();
            return View(reservation);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ReservationRequest reservation)
        {
            if (ModelState.IsValid)
            {
                var isVaildReservation = await _service.IsValidReservation(reservation);
                if (!isVaildReservation)
                {
                    ViewBag.AvailableTimeMsg = "Sorry, the reservation end time is outside the sitting period";
                    return View(reservation);
                }
                try
                {
                    var user = await _userManager.FindByEmailAsync(User.Identity.Name);
                    var userId = user?.Id;
                    var isSuccess = await _service.EditReservation(reservation, userId);
                    if(isSuccess)
                        return RedirectToAction(nameof(Index));
                    else
                    {
                        var availableTimeRange = await _service.GetAvailableTimeRange(reservation);
                        if (availableTimeRange == null)
                            ViewBag.AvailableTimeMsg = "Sorry, there is no available time on selected date, please select another date";
                        else
                        {
                            var startTime = availableTimeRange[0];
                            ViewBag.AvailableTimeMsg =
                                $"There is no vacancy on selected time, please select {reservation.StartDateTime.ToString("yyyy-MM-dd")} {startTime}";
                        }
                        return View(reservation);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_service.IsReservationExist(reservation.Id))
                        return NoContent();
                    else
                        throw;
                }
               
            }
            return View(reservation);
        }

      

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var reservation = await _service.GetOneReservation(id);
            if (reservation == null)
                return NotFound();
            return View(reservation);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}